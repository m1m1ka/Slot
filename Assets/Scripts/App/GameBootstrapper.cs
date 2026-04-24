using Core;
using UI;
using UnityEngine;

/// <summary>
/// 轻量启动器：负责查找或创建核心管理器，初始化基础系统，并打开首屏 UI。
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    public static GameBootstrapper Instance { get; private set; }

    [Header("Bootstrap")]
    [SerializeField] private AppRoot _appRoot;
    [SerializeField] private bool _launchMainPanelOnBootstrap = true;

    private bool _isBootstrapped;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Bootstrap();
    }

    public void Bootstrap()
    {
        if (_isBootstrapped)
        {
            return;
        }

        _appRoot = EnsureAppRoot();

        UIManager uiManager = EnsureManager(_appRoot.UIManager, "UIManager");
        PoolManager poolManager = EnsureManager(_appRoot.PoolManager, "PoolManager");
        TimeManager timeManager = EnsureManager(_appRoot.TimeManager, "TimeManager");
        AudioManager audioManager = EnsureManager(_appRoot.AudioManager, "AudioManager");

        _appRoot.BindManagers(uiManager, poolManager, timeManager, audioManager);
        _appRoot.InitializeContexts(
            _appRoot.PlayerContext ?? new PlayerContext(initialCoins: 1000),
            _appRoot.GameSession ?? new GameSession()
        );

        ConfigManager.Instance.InitAllConfigs();

        _isBootstrapped = true;
        Debug.Log("[GameBootstrapper] Core managers, runtime contexts, and configs initialized.");

        if (_launchMainPanelOnBootstrap && uiManager != null)
        {
            uiManager.ShowPanel<MainGamePanel>(closeOthers: true);
            Debug.Log("[GameBootstrapper] MainGamePanel launched.");
        }
    }

    private AppRoot EnsureAppRoot()
    {
        if (_appRoot != null)
        {
            return _appRoot;
        }

        AppRoot existingRoot = FindExistingObject<AppRoot>();
        if (existingRoot != null)
        {
            return existingRoot;
        }

        GameObject rootObject = new GameObject("AppRoot");
        return rootObject.AddComponent<AppRoot>();
    }

    private T EnsureManager<T>(T current, string objectName) where T : MonoBehaviour
    {
        if (current != null)
        {
            return current;
        }

        T existing = FindExistingObject<T>();
        if (existing != null)
        {
            return existing;
        }

        GameObject managerObject = new GameObject(objectName);
        managerObject.transform.SetParent(_appRoot.transform, false);
        return managerObject.AddComponent<T>();
    }

    private static T FindExistingObject<T>() where T : Object
    {
        T[] objects = FindObjectsOfType<T>(true);
        return objects.Length > 0 ? objects[0] : null;
    }
}
