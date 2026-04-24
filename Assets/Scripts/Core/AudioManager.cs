using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 轻量全局音频管理器。
    /// 支持 BGM、SFX、多音效并发播放，以及 BGM 淡入淡出。
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private float _musicVolume = 1f;
        [SerializeField] private bool _muteMusic;

        [Header("Sfx")]
        [SerializeField] private float _sfxVolume = 1f;
        [SerializeField] private bool _muteSfx;
        [SerializeField] private int _initialSfxSourceCount = 6;
        [SerializeField] private int _maxSfxSourceCount = 24;

        private AudioSource _musicSource;
        private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
        private Coroutine _musicFadeCoroutine;

        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;
        public bool IsMusicMuted => _muteMusic;
        public bool IsSfxMuted => _muteSfx;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureMusicSource();
            PrewarmSfxSources();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _muteMusic ? 0f : _musicVolume;
            }
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicMute(bool mute)
        {
            _muteMusic = mute;
            if (_musicSource != null)
            {
                _musicSource.volume = _muteMusic ? 0f : _musicVolume;
            }
        }

        public void SetSfxMute(bool mute)
        {
            _muteSfx = mute;
        }

        public void PlayMusic(AudioClip clip, float fadeDuration = 0.5f, bool loop = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] PlayMusic failed: clip is null.");
                return;
            }

            EnsureMusicSource();

            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
            }

            _musicFadeCoroutine = StartCoroutine(FadeInMusicRoutine(clip, fadeDuration, loop));
        }

        public void PlayMusic(string resourcesPath, float fadeDuration = 0.5f, bool loop = true)
        {
            AudioClip clip = AssetProvider.LoadAudioClip(resourcesPath);
            if (clip != null)
            {
                PlayMusic(clip, fadeDuration, loop);
            }
        }

        public void StopMusic(float fadeDuration = 0.5f)
        {
            if (_musicSource == null || !_musicSource.isPlaying)
            {
                return;
            }

            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
            }

            _musicFadeCoroutine = StartCoroutine(FadeOutMusicRoutine(fadeDuration));
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] PlaySfx failed: clip is null.");
                return;
            }

            if (_muteSfx)
            {
                return;
            }

            AudioSource source = GetAvailableSfxSource();
            if (source == null)
            {
                Debug.LogWarning("[AudioManager] No available SFX source. Consider increasing max pool size.");
                return;
            }

            source.pitch = pitch;
            source.volume = Mathf.Clamp01(_sfxVolume * volumeScale);
            source.clip = clip;
            source.loop = false;
            source.Play();
        }

        public void PlaySfx(string resourcesPath, float volumeScale = 1f, float pitch = 1f)
        {
            AudioClip clip = AssetProvider.LoadAudioClip(resourcesPath);
            if (clip != null)
            {
                PlaySfx(clip, volumeScale, pitch);
            }
        }

        private IEnumerator FadeInMusicRoutine(AudioClip newClip, float fadeDuration, bool loop)
        {
            fadeDuration = Mathf.Max(0f, fadeDuration);

            if (_musicSource.isPlaying)
            {
                yield return FadeMusicVolume(_musicSource.volume, 0f, fadeDuration * 0.5f);
            }

            _musicSource.Stop();
            _musicSource.clip = newClip;
            _musicSource.loop = loop;
            _musicSource.volume = 0f;
            _musicSource.Play();

            float targetVolume = _muteMusic ? 0f : _musicVolume;
            yield return FadeMusicVolume(0f, targetVolume, fadeDuration);
            _musicFadeCoroutine = null;
        }

        private IEnumerator FadeOutMusicRoutine(float fadeDuration)
        {
            fadeDuration = Mathf.Max(0f, fadeDuration);
            float startVolume = _musicSource.volume;

            yield return FadeMusicVolume(startVolume, 0f, fadeDuration);

            _musicSource.Stop();
            _musicSource.clip = null;
            _musicFadeCoroutine = null;
        }

        private IEnumerator FadeMusicVolume(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                _musicSource.volume = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                _musicSource.volume = Mathf.Lerp(from, to, progress);
                yield return null;
            }

            _musicSource.volume = to;
        }

        private void EnsureMusicSource()
        {
            if (_musicSource != null)
            {
                return;
            }

            GameObject musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform, false);
            _musicSource = musicObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = _muteMusic ? 0f : _musicVolume;
        }

        private void PrewarmSfxSources()
        {
            int targetCount = Mathf.Max(1, _initialSfxSourceCount);
            for (int i = _sfxSources.Count; i < targetCount; i++)
            {
                CreateSfxSource(i);
            }
        }

        private AudioSource GetAvailableSfxSource()
        {
            for (int i = 0; i < _sfxSources.Count; i++)
            {
                if (!_sfxSources[i].isPlaying)
                {
                    return _sfxSources[i];
                }
            }

            if (_sfxSources.Count >= _maxSfxSourceCount)
            {
                return null;
            }

            return CreateSfxSource(_sfxSources.Count);
        }

        private AudioSource CreateSfxSource(int index)
        {
            GameObject sfxObject = new GameObject($"SfxSource_{index}");
            sfxObject.transform.SetParent(transform, false);

            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;

            _sfxSources.Add(source);
            return source;
        }
    }
}
