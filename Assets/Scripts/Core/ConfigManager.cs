using System;
using System.Collections.Generic;
using UnityEngine;
using Configs;

namespace Core
{
    /// <summary>
    /// 全局配置表管理器（单例），遵循纯 C# 逻辑设计。
    /// 负责在游戏启动时把 JSON/CSV 数据反序列化成强类型的字典并驻留内存。
    /// </summary>
    public class ConfigManager
    {
        private static ConfigManager _instance;
        public static ConfigManager Instance => _instance ??= new ConfigManager();

        // 核心缓存缓存字典：外层Key是配置类的Type，内层Key是数据的独立ID
        private readonly Dictionary<Type, object> _caches = new Dictionary<Type, object>();

        private ConfigManager() { }

        /// <summary>
        /// 提供给外部业务查询配表的泛型接口
        /// 用法: var cfg = ConfigManager.Instance.GetConfig<CharacterConfig>(1001);
        /// </summary>
        public T GetConfig<T>(int id) where T : class, IConfig
        {
            var type = typeof(T);
            if (_caches.TryGetValue(type, out var dictObj))
            {
                var dict = dictObj as Dictionary<int, T>;
                if (dict != null && dict.TryGetValue(id, out var config))
                {
                    return config;
                }
            }
            Debug.LogWarning($"[ConfigManager] 未找到类型 {type.Name} 或 ID {id} 的配表数据！");
            return null;
        }

        /// <summary>
        /// 获取某张表的全部数据（常用于UI列表展示等）
        /// </summary>
        public Dictionary<int, T> GetAllConfigs<T>() where T : class, IConfig
        {
            var type = typeof(T);
            if (_caches.TryGetValue(type, out var dictObj))
            {
                return dictObj as Dictionary<int, T>;
            }
            return new Dictionary<int, T>();
        }

        // ======================= 初始化与加载 ======================= //

        /// <summary>
        /// 在游戏启动时（GameRoot）调用，一次性加载所有需要的表
        /// </summary>
        public void InitAllConfigs()
        {
            _caches.Clear();

            // 在这里注册你的所有表
            // 例如：LoadConfig<CharacterConfig>("Configs/CharacterConfig");
            // LoadConfig<ItemConfig>("Configs/ItemConfig");
            
            Debug.Log("[ConfigManager] 所有配表加载完成！");
        }

        /// <summary>
        /// 核心解析逻辑：读取 JSON 文本，转换成对应的实体类字典
        /// </summary>
        /// <param name="resourcePath">Resources 文件夹下的路径，如 "Configs/ItemConfig"</param>
        private void LoadConfig<T>(string resourcePath) where T : class, IConfig
        {
            // 这是从 Resources 加载，如果你后期用 Addressables，可以改成异步加载
            TextAsset textAsset = AssetProvider.LoadTextAsset(resourcePath);
            if (textAsset == null)
            {
                Debug.LogError($"[ConfigManager] 找不到配表文件：Resources/{resourcePath}");
                return;
            }

            // 注意：Unity自带的 JsonUtility 不支持直接解析顶层是数组的JSON，如果使用 JsonUtility，需要包装一层
            // 工业界实际上通常使用 Newtonsoft.Json (Json.Net) 解析以下结构：
            // List<T> list = JsonConvert.DeserializeObject<List<T>>(textAsset.text);
            
            // 为了演示暂用这套伪代码流程（你可以随时换装 Newtonsoft.Json 或你习惯的序列化库）
            List<T> list = ParseJsonToList<T>(textAsset.text); 

            var dict = new Dictionary<int, T>();
            foreach (var item in list)
            {
                if (!dict.ContainsKey(item.Id))
                {
                    dict.Add(item.Id, item);
                }
            }

            _caches[typeof(T)] = dict;
        }

        // --- 序列化解析占位（建议引入 com.unity.modules.jsonserialize 包或 Newtonsoft.Json） ---
        private List<T> ParseJsonToList<T>(string json)
        {
            // TODO: 使用 Newtonsoft.Json
            // return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(json);
            return new List<T>();
        }
    }
}
