using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    /// <summary>
    /// 全局强类型事件总线，用于解耦各层的通信。
    /// 优势：可以通过IDE的“查找所有引用”，轻松找到事件的所有派发者和监听者
    /// </summary>
    public static class EventBus
    {
        // 核心字典存储：Key为事件的类型(Type)，Value为该类型事件的回调委托列表
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T">具体的事件类型结构体</typeparam>
        /// <param name="handler">回调方法</param>
        public static void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            
            if (!_subscribers.TryGetValue(eventType, out var handlerList))
            {
                handlerList = new List<Delegate>();
                _subscribers[eventType] = handlerList;
            }

            // 避免重复注册同一个方法
            if (!handlerList.Contains(handler))
            {
                handlerList.Add(handler);
            }
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        /// <typeparam name="T">具体的事件类型结构体</typeparam>
        /// <param name="handler">回调方法</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            
            if (_subscribers.TryGetValue(eventType, out var handlerList))
            {
                handlerList.Remove(handler);
            }
        }

        /// <summary>
        /// 派发（触发）事件
        /// </summary>
        /// <typeparam name="T">具体的事件类型结构体</typeparam>
        /// <param name="eventData">事件的实例数据</param>
        public static void Publish<T>(T eventData) where T : IEvent
        {
            var eventType = typeof(T);
            
            if (_subscribers.TryGetValue(eventType, out var handlerList))
            {
                // 先转换为一个快照（例如ToList()）遍历执行，目的是防止在回调函数内部修改订阅列表（报错：集合被修改现象）
                foreach (var handler in handlerList.ToList())
                {
                    if (handler is Action<T> action)
                    {
                        action.Invoke(eventData);
                    }
                }
            }
        }
        
        /// <summary>
        /// 可以在游戏重启、返回主菜单时清空所有订阅
        /// </summary>
        public static void ClearAll()
        {
            _subscribers.Clear();
        }
    }
}