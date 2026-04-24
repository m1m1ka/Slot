using System;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 轻量资源加载入口。
    /// 当前基于 Resources 实现，并提供简单缓存，后续可平滑替换为 Addressables。
    /// </summary>
    public static class AssetProvider
    {
        private static readonly Dictionary<string, UnityEngine.Object> Cache = new Dictionary<string, UnityEngine.Object>();
        private static readonly Dictionary<string, Sprite[]> SpriteAtlasCache = new Dictionary<string, Sprite[]>();

        public static T Load<T>(string resourcesPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
            {
                Debug.LogWarning($"[AssetProvider] Load<{typeof(T).Name}> failed: resources path is empty.");
                return null;
            }

            if (Cache.TryGetValue(resourcesPath, out UnityEngine.Object cachedObject))
            {
                if (cachedObject is T typedCached)
                {
                    return typedCached;
                }

                Debug.LogWarning($"[AssetProvider] Cached asset type mismatch at path: Resources/{resourcesPath}");
                return null;
            }

            T asset = Resources.Load<T>(resourcesPath);
            if (asset == null)
            {
                Debug.LogWarning($"[AssetProvider] Failed to load {typeof(T).Name} at Resources/{resourcesPath}");
                return null;
            }

            Cache[resourcesPath] = asset;
            return asset;
        }

        public static GameObject LoadPrefab(string resourcesPath)
        {
            return Load<GameObject>(resourcesPath);
        }

        public static AudioClip LoadAudioClip(string resourcesPath)
        {
            return Load<AudioClip>(resourcesPath);
        }

        public static TextAsset LoadTextAsset(string resourcesPath)
        {
            return Load<TextAsset>(resourcesPath);
        }

        public static Sprite[] LoadSprites(string resourcesPath)
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
            {
                Debug.LogWarning("[AssetProvider] LoadSprites failed: resources path is empty.");
                return Array.Empty<Sprite>();
            }

            if (SpriteAtlasCache.TryGetValue(resourcesPath, out Sprite[] cachedSprites))
            {
                return cachedSprites;
            }

            Sprite[] sprites = Resources.LoadAll<Sprite>(resourcesPath);
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"[AssetProvider] Failed to load sprite atlas at Resources/{resourcesPath}");
                return Array.Empty<Sprite>();
            }

            SpriteAtlasCache[resourcesPath] = sprites;
            return sprites;
        }

        public static Sprite LoadSpriteFromAtlas(string atlasPath, string spriteName)
        {
            if (string.IsNullOrWhiteSpace(spriteName))
            {
                Debug.LogWarning("[AssetProvider] LoadSpriteFromAtlas failed: sprite name is empty.");
                return null;
            }

            Sprite[] sprites = LoadSprites(atlasPath);
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null && string.Equals(sprites[i].name, spriteName, StringComparison.Ordinal))
                {
                    return sprites[i];
                }
            }

            Debug.LogWarning($"[AssetProvider] Sprite '{spriteName}' not found in atlas Resources/{atlasPath}");
            return null;
        }

        public static GameObject InstantiatePrefab(string resourcesPath, Transform parent = null)
        {
            GameObject prefab = LoadPrefab(resourcesPath);
            if (prefab == null)
            {
                return null;
            }

            return UnityEngine.Object.Instantiate(prefab, parent);
        }

        public static GameObject InstantiatePrefab(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[AssetProvider] InstantiatePrefab failed: prefab is null.");
                return null;
            }

            return UnityEngine.Object.Instantiate(prefab, parent);
        }

        public static void Unload(string resourcesPath)
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
            {
                return;
            }

            Cache.Remove(resourcesPath);
        }

        public static void ClearCache()
        {
            Cache.Clear();
            SpriteAtlasCache.Clear();
        }
    }
}
