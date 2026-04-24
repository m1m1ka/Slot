using System.Collections.Generic;
using Core;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 全局 UI 管理器，负责所有 UI 面板的加载、显示、隐藏与层级管理。
    /// 使用栈（Stack）来管理全屏/弹窗界面，方便实现“返回上一级”或“关闭按 ESC”功能。
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI 根节点")]
        [Tooltip("Canvas 节点，所有 UI 面板都会生成在这里。")]
        public Transform canvasRoot;

        // 面板缓存字典：Type -> 面板实例
        private readonly Dictionary<System.Type, UIPanel> _panelCache = new Dictionary<System.Type, UIPanel>();
        
        // 活跃面板栈（常用于全屏界面和弹窗的管理）
        private readonly Stack<UIPanel> _panelStack = new Stack<UIPanel>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            if (canvasRoot == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null) canvasRoot = canvas.transform;
            }
        }

        /// <summary>
        /// 显示一个指定的 UI 面板
        /// </summary>
        /// <typeparam name="T">继承自 UIPanel 的具体面板类型</typeparam>
        /// <param name="closeOthers">是否关闭栈中其他全屏面板（常用于切换主界面）</param>
        /// <param name="data">传给该面板的任意初始化数据</param>
        public T ShowPanel<T>(bool closeOthers = false, object data = null) where T : UIPanel
        {
            if (closeOthers)
            {
                CloseAllPanels();
            }

            T panel = GetOrLoadPanel<T>();
            if (panel == null) return null;

            // 处理栈逻辑
            if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
            {
                // 如果当前已经在栈顶，只是想重新激活或传参
                panel.Show(data);
                return panel;
            }

            // 新面板入栈
            _panelStack.Push(panel);
            panel.transform.SetAsLastSibling(); // 保证它显示在最前面
            panel.Show(data);

            return panel;
        }

        /// <summary>
        /// 关闭当前处于栈顶的面板（例如按下返回键或点关闭按钮）
        /// </summary>
        public void CloseTopPanel()
        {
            if (_panelStack.Count > 0)
            {
                UIPanel topPanel = _panelStack.Pop();
                topPanel.Hide();

                // 唤醒栈中下一个面板（如果有的话）
                if (_panelStack.Count > 0)
                {
                    UIPanel nextPanel = _panelStack.Peek();
                    nextPanel.OnResume(); // 通知它回到了栈顶
                }
            }
        }

        /// <summary>
        /// 指定关闭某个面板
        /// </summary>
        public void ClosePanel<T>() where T : UIPanel
        {
            var type = typeof(T);
            if (_panelCache.TryGetValue(type, out var panel) && panel.gameObject.activeSelf)
            {
                // 特殊处理：如果它正好在栈顶，走正常的弹栈逻辑
                if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
                {
                    CloseTopPanel();
                }
                else
                {
                    // 否则只是普通的隐藏（比如它是一个非压栈的小挂件层）
                    panel.Hide();
                    
                    // 注意：如果是复杂栈叠放，通常需要重建栈。这里作为简化框架直接隐藏
                }
            }
        }

        /// <summary>
        /// 关闭所有栈中的面板
        /// </summary>
        public void CloseAllPanels()
        {
            while (_panelStack.Count > 0)
            {
                UIPanel panel = _panelStack.Pop();
                panel.Hide();
            }
        }

        // ======================= 内部加载逻辑 ======================= //

        private T GetOrLoadPanel<T>() where T : UIPanel
        {
            var type = typeof(T);

            // 1. 如果缓存里已经有了，直接返回
            if (_panelCache.TryGetValue(type, out var existPanel))
            {
                return existPanel as T;
            }

            // 2. 如果没有，则去 Resources 里加载预制体。
            // 规范：预制体名字必须和类名一样！比如 LoginPanel.prefab 对应的脚本就是 LoginPanel.cs
            // 路径固定在 Resources/UI/ 下
            string prefabPath = $"UI/{type.Name}";
            GameObject prefab = AssetProvider.LoadPrefab(prefabPath);
            
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] 加载面板失败！未能在 Resources/{prefabPath} 找到预制体。");
                return null;
            }

            // 3. 实例化到 Canvas 下
            GameObject instance = AssetProvider.InstantiatePrefab(prefab, canvasRoot);
            instance.name = type.Name; // 去掉 "(Clone)" 后缀
            
            T panelComp = instance.GetComponent<T>();
            if (panelComp == null)
            {
                Debug.LogError($"[UIManager] 面板预制体 {type.Name} 身上没有挂载对应的 {type.Name} 脚本！");
                Destroy(instance);
                return null;
            }

            // 4. 加入缓存并初次初始化
            _panelCache.Add(type, panelComp);
            panelComp.Init();
            // 默认隐藏
            instance.SetActive(false);

            return panelComp;
        }
    }
}
