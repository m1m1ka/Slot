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
    private bool _secondTutorialShown;
    private bool _thirdTutorialShown;
    private bool _settlementClickedTutorialShown;
    private bool _fifthTutorialShown;
    private bool _fifthTutorialCompletionHandled;
    private bool _sixthTutorialShown;
    private bool _seventhTutorialShown;

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

    private void OnDestroy()
    {
        EventBus.Unsubscribe<TutorialEvent>(HandleTutorialEvent);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            UIManager.Instance?.ShowPanel<TutorialPanel>(data: "你好，欢迎光临");
        }
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
            _appRoot.PlayerContext ?? new PlayerContext(initialCoins: 45),
            _appRoot.GameSession ?? new GameSession()
        );

        ConfigManager.Instance.InitAllConfigs();

        _isBootstrapped = true;
        EventBus.Subscribe<TutorialEvent>(HandleTutorialEvent);
        Debug.Log("[GameBootstrapper] Core managers, runtime contexts, and configs initialized.");

        if (_launchMainPanelOnBootstrap && uiManager != null)
        {
            uiManager.ShowPanel<GameStartPanel>(closeOthers: true);
            Debug.Log("[GameBootstrapper] GameStartPanel launched.");
        }
    }

    private void HandleTutorialEvent(TutorialEvent tutorialEvent)
    {
        if (!_secondTutorialShown && tutorialEvent.Type == TutorialEventType.ScratchCardClicked)
        {
            _secondTutorialShown = true;
            UIManager.Instance?.ShowPanel<TutorialPanel>(data: TutorialPanel.CreateSecondTutorialSequence());
            return;
        }

        if (!_thirdTutorialShown && tutorialEvent.Type == TutorialEventType.ScratchCardCompleted)
        {
            _thirdTutorialShown = true;
            UIManager.Instance?.ShowPanel<TutorialPanel>(data: TutorialPanel.CreateThirdTutorialSequence());
            return;
        }

        if (!_settlementClickedTutorialShown && tutorialEvent.Type == TutorialEventType.SettlementButtonClicked)
        {
            _settlementClickedTutorialShown = true;
            UIManager.Instance?.ShowPanel<TutorialPanel>(data: TutorialPanel.CreateSettlementClickedTutorialSequence());
            return;
        }

        if (!_fifthTutorialShown && tutorialEvent.Type == TutorialEventType.CannotAffordAnyScratchCardAfterSettlement)
        {
            _fifthTutorialShown = true;
            UIManager.Instance?.ShowPanel<TutorialPanel>(data: TutorialPanel.CreateFifthTutorialSequence());
            return;
        }

        if (!_fifthTutorialCompletionHandled &&
            tutorialEvent.Type == TutorialEventType.TutorialCompleted &&
            tutorialEvent.IntValue == 5)
        {
            _fifthTutorialCompletionHandled = true;
            GrantFifthTutorialCompletionRewards();
            AudioManager.Instance?.PlayBgmFromFolder();
            return;
        }

        if (!_sixthTutorialShown && tutorialEvent.Type == TutorialEventType.FirstLevelPassed)
        {
            _sixthTutorialShown = true;
            UIManager.Instance?.ShowPanel<TutorialPanel>(data: TutorialPanel.CreateSixthTutorialSequence());
            return;
        }

        if (!_seventhTutorialShown && tutorialEvent.Type == TutorialEventType.SecondLevelPassed)
        {
            _seventhTutorialShown = true;
            UIManager.Instance?.ShowPanel<TutorialPanel>(data: TutorialPanel.CreateSeventhTutorialSequence());
        }
    }

    private void GrantFifthTutorialCompletionRewards()
    {
        PlayerContext playerContext = _appRoot != null ? _appRoot.PlayerContext : null;
        if (playerContext == null)
        {
            return;
        }

        AddStarterScratchTools(playerContext.ScratchTools);
        playerContext.Player?.AddCoins(30);
    }

    private void AddStarterScratchTools(ScratchToolInventoryModel scratchTools)
    {
        if (scratchTools == null)
        {
            return;
        }

        var starterTools = ScratchToolDefaultsProvider.GetStarterTools();
        for (int i = 0; i < starterTools.Count; i++)
        {
            scratchTools.AddTool(starterTools[i]);
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
