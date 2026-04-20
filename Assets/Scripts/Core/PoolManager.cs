using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool; // 依赖Unity 2021+ 原生的超高性能池类

namespace Core
{
    /// <summary>
    /// 全局高性能 Prefab 对象池管理器。适用于 View 表现层或挂载了MonoBehaviour的控制器。
    /// 基于 Unity.Pool 原生API重新包装，屏蔽烦人的池对象管理和资源泄漏问题。
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Tooltip("池对象的默认初始容量")]
        public int defaultCapacity = 10;
        
        [Tooltip("单个 Prefab 池的允许最大数量(超出此数量的回收对象会被Destroy掉)")]
        public int maxSize = 500;

        // 核心字典存储：使用Prefab实例作为Key，记录对应的池
        private Dictionary<GameObject, IObjectPool<GameObject>> _prefabPools = new Dictionary<GameObject, IObjectPool<GameObject>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // 建议挂载在常驻内存场景下的空节点，或者启用 DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 从池中生成一个预制体
        /// </summary>
        /// <param name="prefab">目标预制体 (必须确保非空)</param>
        /// <param name="position">需要放置的世界坐标</param>
        /// <param name="rotation">需要放置的旋转</param>
        /// <param name="parent">父节点(可选)</param>
        /// <returns>池子里的 GameObject (一定处于 Active 状态)</returns>
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;

            // 1. 如果没有针对这个预制体的对象池，我们就建立一个新的。
            if (!_prefabPools.TryGetValue(prefab, out var pool))
            {
                pool = CreatePoolForPrefab(prefab, parent);
                _prefabPools[prefab] = pool;
            }

            // 2. 从池中取出
            GameObject instance = pool.Get();

            // 3. 复位基础 Transform 数据
            instance.transform.SetPositionAndRotation(position, rotation);
            if (parent != null)
            {
                instance.transform.SetParent(parent, true); // true保证世界缩放相对正确
            }

            return instance;
        }

        public GameObject Spawn(GameObject prefab, Transform parent = null)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, parent);
        }

        /// <summary>
        /// 无论此物体是如何生成的，都可以统一用这个方法回收
        /// 不管是在外部调它，还是在它内部调用自身的 PoolObject.Release()，其效果和性能一致
        /// </summary>
        public void Despawn(GameObject instance)
        {
            if (instance == null) return;

            // 查找组件，如果有的话就释放回专属池
            if (instance.TryGetComponent<PoolObject>(out var poolObj))
            {
                poolObj.ReleaseToPool();
            }
            else
            {
                Destroy(instance); // 如果它是硬生生 Instantiate 出去没打上烙印的，或者压根不是池生成的，这里直接保底销毁
            }
        }

        // ======================= 内部逻辑 ======================= //

        private IObjectPool<GameObject> CreatePoolForPrefab(GameObject prefab, Transform initialParent)
        {
            IObjectPool<GameObject> newPool = null;

            // 借助闭包特性将 pre-created 的对象指向自己专属的池
            newPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    // Action 1: 当池子里空了，如何"真创建"一个新的？
                    GameObject obj = Instantiate(prefab, initialParent);
                    // 一定要挂上识别器！哪怕 prefab 上本来有也会被覆盖或直接服用
                    var poolObj = obj.GetComponent<PoolObject>();
                    if (poolObj == null) poolObj = obj.AddComponent<PoolObject>();
                    poolObj.Setup(newPool); // 这里是神来之笔：物体被强制记录了它来自哪个池！

                    return obj;
                },
                actionOnGet: (obj) =>
                {
                    // Action 2: 当对象被发出去之前，做点什么？（显示它）
                    obj.SetActive(true);
                },
                actionOnRelease: (obj) =>
                {
                    // Action 3: 当对象被强制回收塞进池子之前，做点什么？（隐藏它，并可选设置父物体回归总管理）
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform, false);
                },
                actionOnDestroy: (obj) =>
                {
                    // Action 4: 万一池子装满了，被踢多余出来的对象去哪里？
                    Destroy(obj);
                },
                // 关闭 collectionCheck 以追求极致性能（前提是不允许你手动在代码外面去删已经隐藏起来的池物体，我们通常不会这么做）
                collectionCheck: false, 
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );

            return newPool;
        }
    }
}