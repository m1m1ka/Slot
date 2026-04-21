using UnityEngine;

/// <summary>
/// 游戏全局生命周期入口与管理器。
/// 负责全局设置、核心系统初始化及游戏首屏流程。
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private void Awake()
    {
        // 保证全局唯一，跨场景不销毁
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeGlobalSettings();
    }

    private void Start()
    {
        // 所有设定初始化完毕后，启动游戏主流程
        LaunchApp();
    }

    /// <summary>
    /// 初始化游戏底层通用设置与核心系统
    /// </summary>
    private void InitializeGlobalSettings()
    {
        // 1. 挂机类2D游戏建议锁帧以防设备发热（例如60帧）
        Application.targetFrameRate = 60;

        // 2. 屏幕常亮设置（部分挂机游戏可能需要）
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 3. 初始化所有核心系统配置 (EventBus, ClassPool, ConfigManager预加载等)
        // 例如：ConfigManager.LoadAll();
        
        Debug.Log("[GameManager] 全局设定与基础框架初始化完成。");
    }

    /// <summary>
    /// 加载游戏核心逻辑并弹出主界面
    /// </summary>
    private void LaunchApp()
    {
        Debug.Log("[GameManager] 正在启动主界面并载入游戏逻辑...");

        // 方案一：如果你的 UIManager 支持通过名称从 Resources/UI/ 动态加载面板：
        // UIManager.Instance.OpenPanel("MainGamePanel");

        // 方案二：作为MVC的中枢入口，我们动态创建全局逻辑控制器 MainGameController
        // 然后交由 MainGameController 去利用 UIManager 或者资源池请求它的 View (MainGamePanel)
        GameObject controllerObj = new GameObject("MainGameController");
        controllerObj.transform.SetParent(this.transform);
        
        // 挂载我们之前写好的主控制器
        MainGameController mainGameController = controllerObj.AddComponent<MainGameController>();
        
        // （视你当前 UIManager 封装实现而定，如果需要也可以直接在此处调用 UIManager 开启界面的接口）
    }
}
