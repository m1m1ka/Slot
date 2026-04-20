using UnityEngine;

namespace UI
{
    /// <summary>
    /// 所有的 UI 面板基类（继承 MonoBehaviour）。
    /// 负责处理生命周期回调：Init (资源就绪) -> Show (打开) -> Hide (关闭) 等。
    /// 可以由子类挂载在 UI 预制体上（如 MainPanel.cs 挂载在 Resources/UI/MainPanel 预制体上）。
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        [Tooltip("如果它是主界面等底层背景，勾上此项")]
        public bool isBackgroundLayer = false;
        
        [Tooltip("面板类型分类（全屏、弹窗等）可选，用于控制遮罩等特效")]
        // public PanelType panelType = PanelType.FullScreen; 

        // 数据载体，传给本面板的数据会被保存在这
        protected object PanelData;

        /// <summary>
        /// 当此面板第一次被 UIManager 加载并实例化时调用（只会执行一次）。
        /// 适合做 Find()，获取组件引用，绑定持久点击事件监听等操作。
        /// </summary>
        public virtual void Init()
        {
            // 例如：Button.onClick.AddListener(...)
        }

        /// <summary>
        /// 外部调用，带有传递的数据
        /// </summary>
        public void Show(object data = null)
        {
            PanelData = data;
            
            // 每次打开前刷新需要的数据绑定
            OnShow(PanelData);
            
            this.gameObject.SetActive(true);
            
            // 注意层级：被激活的始终显示在最高层（如果是全屏覆盖或弹窗）
            if (!isBackgroundLayer)
            {
                transform.SetAsLastSibling(); 
            }
        }

        /// <summary>
        /// 面板被真正打开时调用。每次打开都会执行。子类可复写。
        /// 适合刷新 UI 上的数值、播放入场动画等。
        /// </summary>
        protected virtual void OnShow(object data)
        {
        }

        /// <summary>
        /// 供外部或自身调用关闭面板
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
            OnHide();
        }

        /// <summary>
        /// 面板被隐藏时调用。每次关闭都会执行。子类可复写。
        /// 适合释放临时监听事件、停止动画特效等。
        /// </summary>
        protected virtual void OnHide()
        {
        }

        /// <summary>
        /// 当一个盖在它上面的窗口（弹窗）被关闭后，它重新成为栈顶可见窗口时调用。
        /// 适合刷新由于被遮盖期间发生的数据变化（如：关掉了装备强化弹窗，发现主界面的金币变少了，需要重绘）。
        /// </summary>
        public virtual void OnResume()
        {
            // 重新刷新数据
            OnShow(PanelData); 
        }
        
        /// <summary>
        /// 当点击关闭按钮时（可以给按钮绑定此方法）
        /// </summary>
        public virtual void OnClickCloseButton()
        {
            // 让 UIManager 去弹栈处理，而不是自己把自己关掉
            UIManager.Instance.CloseTopPanel();
        }
    }
}