using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Lightweight global audio manager.
    /// Business code should prefer PlayCue so clip paths stay centralized here.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public enum AudioCueKind
        {
            Sfx,
            Music
        }

        [System.Serializable]
        public class AudioCueDefinition
        {
            public AudioCueId Id;
            public AudioCueKind Kind = AudioCueKind.Sfx;
            public string ResourcesPath;
            [Range(0f, 1f)] public float VolumeScale = 1f;
            public float Pitch = 1f;
            public float Cooldown = 0f;
            public float FadeDuration = 0.5f;
            public bool Loop = true;
        }

        public static AudioManager Instance { get; private set; }

        [Header("Cue Library")]
        [SerializeField] private List<AudioCueDefinition> _cueLibrary = new List<AudioCueDefinition>();
        [SerializeField] private bool _useDefaultCueLibrary = true;
        [SerializeField] private bool _logMissingCueAssets = true;

        [Header("Music")]
        [SerializeField] private float _musicVolume = 1f;
        [SerializeField] private bool _muteMusic;

        [Header("Sfx")]
        [SerializeField] private float _sfxVolume = 1f;
        [SerializeField] private bool _muteSfx;
        [SerializeField] private int _initialSfxSourceCount = 6;
        [SerializeField] private int _maxSfxSourceCount = 24;

        private AudioSource _musicSource;
        private AudioSource _loopSfxSource;
        private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
        private readonly Dictionary<AudioCueId, AudioCueDefinition> _cueLookup = new Dictionary<AudioCueId, AudioCueDefinition>();
        private readonly Dictionary<AudioCueId, float> _nextCuePlayTime = new Dictionary<AudioCueId, float>();
        private readonly HashSet<AudioCueId> _missingCueIds = new HashSet<AudioCueId>();
        private readonly HashSet<string> _reportedMissingAssets = new HashSet<string>();
        private Coroutine _musicFadeCoroutine;
        private AudioCueId _currentLoopCueId = AudioCueId.None;

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
            BuildCueLookup();
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
            ApplyLoopSfxVolume();
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
            ApplyLoopSfxVolume();
        }

        public void PlayCue(AudioCueId cueId)
        {
            if (cueId == AudioCueId.None)
            {
                return;
            }

            if (!_cueLookup.TryGetValue(cueId, out AudioCueDefinition cue) || cue == null)
            {
                Debug.LogWarning($"[AudioManager] Cue '{cueId}' is not registered.");
                return;
            }

            if (cue.Cooldown > 0f &&
                _nextCuePlayTime.TryGetValue(cueId, out float nextTime) &&
                Time.unscaledTime < nextTime)
            {
                return;
            }

            if (cue.Cooldown > 0f)
            {
                _nextCuePlayTime[cueId] = Time.unscaledTime + cue.Cooldown;
            }

            AudioClip clip = LoadCueClip(cue);
            if (clip == null)
            {
                return;
            }

            if (cue.Kind == AudioCueKind.Music)
            {
                PlayMusic(clip, cue.FadeDuration, cue.Loop);
                return;
            }

            PlaySfx(clip, cue.VolumeScale, cue.Pitch);
        }

        public void PlayLoopCue(AudioCueId cueId)
        {
            if (cueId == AudioCueId.None)
            {
                return;
            }

            if (!_cueLookup.TryGetValue(cueId, out AudioCueDefinition cue) || cue == null)
            {
                Debug.LogWarning($"[AudioManager] Loop cue '{cueId}' is not registered.");
                return;
            }

            AudioClip clip = LoadCueClip(cue);
            if (clip == null)
            {
                return;
            }

            EnsureLoopSfxSource();
            if (_loopSfxSource == null)
            {
                return;
            }

            if (_currentLoopCueId == cueId && _loopSfxSource.isPlaying && _loopSfxSource.clip == clip)
            {
                ApplyLoopSfxVolume();
                return;
            }

            _currentLoopCueId = cueId;
            _loopSfxSource.clip = clip;
            _loopSfxSource.pitch = cue.Pitch;
            _loopSfxSource.loop = true;
            _loopSfxSource.volume = Mathf.Clamp01(_sfxVolume * cue.VolumeScale);
            _loopSfxSource.Play();
            ApplyLoopSfxVolume();
        }

        public void StopLoopCue(AudioCueId cueId)
        {
            if (_loopSfxSource == null || !_loopSfxSource.isPlaying)
            {
                return;
            }

            if (cueId != AudioCueId.None && _currentLoopCueId != cueId)
            {
                return;
            }

            _loopSfxSource.Stop();
            _loopSfxSource.clip = null;
            _currentLoopCueId = AudioCueId.None;
        }

        public float GetCueDuration(AudioCueId cueId)
        {
            if (cueId == AudioCueId.None)
            {
                return 0f;
            }

            if (!_cueLookup.TryGetValue(cueId, out AudioCueDefinition cue) || cue == null)
            {
                return 0f;
            }

            AudioClip clip = LoadCueClip(cue);
            if (clip == null)
            {
                return 0f;
            }

            float pitch = Mathf.Abs(cue.Pitch);
            return pitch > 0.001f ? clip.length / pitch : clip.length;
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

        private void EnsureLoopSfxSource()
        {
            if (_loopSfxSource != null)
            {
                return;
            }

            GameObject loopObject = new GameObject("LoopSfxSource");
            loopObject.transform.SetParent(transform, false);
            _loopSfxSource = loopObject.AddComponent<AudioSource>();
            _loopSfxSource.playOnAwake = false;
            _loopSfxSource.loop = true;
            ApplyLoopSfxVolume();
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

        private AudioClip LoadCueClip(AudioCueDefinition cue)
        {
            if (cue == null || string.IsNullOrWhiteSpace(cue.ResourcesPath))
            {
                Debug.LogWarning("[AudioManager] Cue load failed: resources path is empty.");
                return null;
            }

            if (_missingCueIds.Contains(cue.Id))
            {
                return null;
            }

            AudioClip clip = AssetProvider.LoadAudioClip(cue.ResourcesPath);
            if (clip == null)
            {
                _missingCueIds.Add(cue.Id);
                if (_logMissingCueAssets && _reportedMissingAssets.Add(cue.ResourcesPath))
                {
                    Debug.LogWarning($"[AudioManager] Cue '{cue.Id}' has no AudioClip at Resources/{cue.ResourcesPath}.");
                }
            }

            return clip;
        }

        private void BuildCueLookup()
        {
            _cueLookup.Clear();

            if (_useDefaultCueLibrary)
            {
                RegisterDefaultCueLibrary();
            }

            for (int i = 0; i < _cueLibrary.Count; i++)
            {
                RegisterCue(_cueLibrary[i]);
            }
        }

        private void RegisterDefaultCueLibrary()
        {
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.UiClick, ResourcesPath = "Audio/Sfx/UI_Click", VolumeScale = 0.75f, Cooldown = 0.03f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.UiDenied, ResourcesPath = "Audio/Sfx/UI_Denied", VolumeScale = 0.9f, Cooldown = 0.1f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.ScratchCardPurchased, ResourcesPath = "Audio/Sfx/ScratchCard_Purchased", VolumeScale = 0.9f, Cooldown = 0.05f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.ScratchCardSpawned, ResourcesPath = "Audio/Sfx/ScratchCard_Spawned", VolumeScale = 0.8f, Cooldown = 0.05f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.ScratchCardFocused, ResourcesPath = "Audio/Sfx/ScratchCard_Focused", VolumeScale = 0.8f, Cooldown = 0.08f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.ScratchCardScratching, ResourcesPath = "Audio/Sfx/ScratchCard_Scratching", VolumeScale = 0.55f, Cooldown = 0.08f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.ScratchCardCompleted, ResourcesPath = "Audio/Sfx/ScratchCard_Completed", VolumeScale = 1f, Cooldown = 0.1f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.ScratchCardRewardClaimed, ResourcesPath = "Audio/Sfx/ScratchCard_RewardClaimed", VolumeScale = 1f, Cooldown = 0.1f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.BuyScratchCard, ResourcesPath = "Audio/Sfx/BuyScrathCard", VolumeScale = 0.95f, Cooldown = 0.05f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.GainMoney, ResourcesPath = "Audio/Sfx/GainMoney", VolumeScale = 1f, Cooldown = 0.05f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.Sratching, ResourcesPath = "Audio/Sfx/Scratching", VolumeScale = 0.8f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.ScratchRight, ResourcesPath = "Audio/Sfx/Scratch_Right", VolumeScale = 0.8f, Cooldown = 0.05f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.ScratchLeft, ResourcesPath = "Audio/Sfx/Scratch_Left", VolumeScale = 0.8f, Cooldown = 0.05f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.Ding, ResourcesPath = "Audio/Sfx/Ding", VolumeScale = 0.9f, Cooldown = 0.04f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.LevelPassCharging, ResourcesPath = "Audio/Sfx/charging", VolumeScale = 0.9f, Cooldown = 0.05f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.LevelPassWin, ResourcesPath = "Audio/Sfx/win", VolumeScale = 1f, Cooldown = 0.05f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.CoinPouring, ResourcesPath = "Audio/Sfx/CoinPouring", VolumeScale = 1f, Cooldown = 0.15f });
            RegisterCue(new AudioCueDefinition { Id = AudioCueId.MainMusic, Kind = AudioCueKind.Music, ResourcesPath = "Audio/Music/Main", VolumeScale = 1f, FadeDuration = 0.6f, Loop = true });
        }

        private void RegisterCue(AudioCueDefinition cue)
        {
            if (cue == null || cue.Id == AudioCueId.None)
            {
                return;
            }

            _cueLookup[cue.Id] = cue;
        }

        private void ApplyLoopSfxVolume()
        {
            if (_loopSfxSource == null)
            {
                return;
            }

            if (_muteSfx)
            {
                _loopSfxSource.volume = 0f;
                return;
            }

            if (_currentLoopCueId != AudioCueId.None &&
                _cueLookup.TryGetValue(_currentLoopCueId, out AudioCueDefinition cue) &&
                cue != null)
            {
                _loopSfxSource.volume = Mathf.Clamp01(_sfxVolume * cue.VolumeScale);
            }
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
