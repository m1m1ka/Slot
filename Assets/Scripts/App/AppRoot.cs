using Core;
using UI;
using UnityEngine;

/// <summary>
/// 全局运行时根节点，负责持有常驻的核心管理器引用。
/// 这是一个轻量容器，不承担复杂业务逻辑。
/// </summary>
public class AppRoot : MonoBehaviour
{
    public static AppRoot Instance { get; private set; }

    [Header("Core Managers")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private PoolManager _poolManager;
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private AudioManager _audioManager;

    public UIManager UIManager => _uiManager;
    public PoolManager PoolManager => _poolManager;
    public TimeManager TimeManager => _timeManager;
    public AudioManager AudioManager => _audioManager;
    public PlayerContext PlayerContext { get; private set; }
    public GameSession GameSession { get; private set; }

    public bool HasAllManagers =>
        _uiManager != null &&
        _poolManager != null &&
        _timeManager != null &&
        _audioManager != null;

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

    public void BindManagers(UIManager uiManager, PoolManager poolManager, TimeManager timeManager, AudioManager audioManager)
    {
        _uiManager = uiManager;
        _poolManager = poolManager;
        _timeManager = timeManager;
        _audioManager = audioManager;
    }

    public void InitializeContexts(PlayerContext playerContext, GameSession gameSession)
    {
        PlayerContext = playerContext;
        GameSession = gameSession;
    }
}
