using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 全局时间管理器。
    /// 提供高性能的无GC计时器系统（基于上一步的 ClassPool），并管理全局的时间缩放（TimeScale）。
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        /// <summary>
        /// 内部的计时器数据块。实现 IPoolable 接口以支持无 GC 的高效回收。
        /// </summary>
        private class TimerData : IPoolable
        {
            public int Id;
            public float Duration;
            public float Elapsed;
            public Action OnComplete;
            public Action<float> OnUpdate;
            public bool IsLoop;
            public bool IgnoreTimeScale;

            public void OnRecycled()
            {
                Id = 0;
                Duration = 0f;
                Elapsed = 0f;
                OnComplete = null;
                OnUpdate = null;
                IsLoop = false;
                IgnoreTimeScale = false;
            }
        }

        // 活跃设定的计时器列表
        private readonly List<TimerData> _activeTimers = new List<TimerData>();
        
        // 递增的计时器唯一ID
        private int _nextId = 1;
        
        // 记录暂停前的时间缩放值
        private float _previousTimeScale = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // 倒序遍历，安全地在遍历时移除完成的计时器
            for (int i = _activeTimers.Count - 1; i >= 0; i--)
            {
                var timer = _activeTimers[i];
                float deltaTime = timer.IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                timer.Elapsed += deltaTime;

                // 每帧回调（比如用来做UI倒计时展示）
                timer.OnUpdate?.Invoke(timer.Duration - timer.Elapsed);

                if (timer.Elapsed >= timer.Duration)
                {
                    // 触发完成回调
                    timer.OnComplete?.Invoke();

                    if (timer.IsLoop)
                    {
                        timer.Elapsed = 0f; // 重置进度继续循环
                    }
                    else
                    {
                        // 不循环，进入回收流程
                        _activeTimers.RemoveAt(i);
                        ClassPool<TimerData>.Release(timer);
                    }
                }
            }
        }

        // ========================== 计时器系统 ========================== //

        /// <summary>
        /// 添加一个计时器
        /// </summary>
        /// <param name="duration">间隔时长</param>
        /// <param name="onComplete">完成时回调</param>
        /// <param name="isLoop">是否循环运行</param>
        /// <param name="ignoreTimeScale">是否忽略时间缩放（游戏受击暂停时，UI动画可能仍需要运行）</param>
        /// <param name="onUpdate">每帧回调剩余时间</param>
        /// <returns>计时器唯一ID，可用于提前取消</returns>
        public int AddTimer(float duration, Action onComplete, bool isLoop = false, bool ignoreTimeScale = false, Action<float> onUpdate = null)
        {
            // 从我们写好的 C# 对象池中获取，无 GC 消耗
            var timer = ClassPool<TimerData>.Get();
            timer.Id = _nextId++;
            timer.Duration = duration;
            timer.Elapsed = 0f;
            timer.OnComplete = onComplete;
            timer.IsLoop = isLoop;
            timer.IgnoreTimeScale = ignoreTimeScale;
            timer.OnUpdate = onUpdate;

            _activeTimers.Add(timer);
            return timer.Id;
        }

        /// <summary>
        /// 提前取消/移除一个计时器
        /// </summary>
        /// <param name="timerId">计时器的ID</param>
        public void CancelTimer(int timerId)
        {
            for (int i = 0; i < _activeTimers.Count; i++)
            {
                if (_activeTimers[i].Id == timerId)
                {
                    var timer = _activeTimers[i];
                    _activeTimers.RemoveAt(i);
                    ClassPool<TimerData>.Release(timer);
                    break;
                }
            }
        }

        /// <summary>
        /// 取消所有进行中的计时器
        /// </summary>
        public void CancelAllTimers()
        {
            foreach (var timer in _activeTimers)
            {
                ClassPool<TimerData>.Release(timer);
            }
            _activeTimers.Clear();
        }

        // ========================== 时间缩放系统 ========================== //

        /// <summary>
        /// 设置游戏全局时间缩放（常用于子弹时间/慢动作）
        /// </summary>
        /// <param name="scale">比如 0.5 就是半速慢动作</param>
        public void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0f, 100f);
        }

        /// <summary>
        /// 暂停游戏，并记住暂停前的时间流速
        /// </summary>
        public void PauseGame()
        {
            if (Time.timeScale > 0f)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }

        /// <summary>
        /// 恢复游戏到暂停前的时间流速
        /// </summary>
        public void ResumeGame()
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = _previousTimeScale > 0f ? _previousTimeScale : 1f;
            }
        }
    }
}