using UnityEngine;
using UnityEngine.Pool;

namespace Core
{
    /// <summary>
    /// 挂载在被池化的 GameObject 上，用于记录它属于哪个池。
    /// 可以极大提升回收性能：不再需要字典查找来销毁对象，也不需要传递预制体引用。
    /// </summary>
    public class PoolObject : MonoBehaviour
    {
        private IObjectPool<GameObject> _pool;

        /// <summary>
        /// 当对象被池创建时进行初始化
        /// </summary>
        public void Setup(IObjectPool<GameObject> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// 对象要求把自己释放回对象池
        /// </summary>
        public void ReleaseToPool()
        {
            if (_pool != null)
            {
                _pool.Release(this.gameObject);
            }
            else
            {
                // 退后方案：如果是编辑器手动拖出来的，没有绑定池子，直接删掉
                Destroy(this.gameObject); 
            }
        }
    }
}