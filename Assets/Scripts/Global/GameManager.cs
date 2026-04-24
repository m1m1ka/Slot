using UnityEngine;
/// <summary>
/// 游戏全局生命周期入口与管理器。
/// 负责全局设置，并将运行时装配交给 GameBootstrapper。
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
        // 所有设定初始化完毕后，交给统一启动器完成后续装配
        EnsureBootstrapper().Bootstrap();
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

    private GameBootstrapper EnsureBootstrapper()
    {
        if (GameBootstrapper.Instance != null)
        {
            return GameBootstrapper.Instance;
        }

        GameBootstrapper bootstrapper = FindObjectOfType<GameBootstrapper>(true);
        if (bootstrapper != null)
        {
            return bootstrapper;
        }

        GameObject bootstrapperObject = new GameObject("GameBootstrapper");
        return bootstrapperObject.AddComponent<GameBootstrapper>();
    }
}
