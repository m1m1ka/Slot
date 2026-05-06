using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// 可选：给需要被放入类对象池的对象实现此接口，以便在回收时自动清理脏数据
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 回收进对象池时调用，用于重置状态
        /// </summary>
        void OnRecycled();
    }

    /// <summary>
    /// 纯C#类的对象池。
    /// 用于缓存 Model 层的数据类、命令、逻辑对象或 EventBus 的事件实例，避免 GC 垃圾回收。
    /// </summary>
    public static class ClassPool<T> where T : class, new()
    {
        private static readonly Stack<T> _pool = new Stack<T>(32);

        /// <summary>
        /// 从池中获取一个纯C#对象
        /// </summary>
        public static T Get()
        {
            return _pool.Count > 0 ? _pool.Pop() : new T();
        }

        /// <summary>
        /// 回收纯C#对象到池中
        /// </summary>
        public static void Release(T obj)
        {
            if (obj == null) return;

            if (obj is IPoolable poolable)
            {
                poolable.OnRecycled();
            }

            _pool.Push(obj);
        }

        public static void Clear()
        {
            _pool.Clear();
        }
    }
}