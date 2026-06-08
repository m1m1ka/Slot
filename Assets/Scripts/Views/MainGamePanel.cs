using UnityEngine;
using TMPro; // 鎺ㄨ崘浣跨敤 TextMeshPro 鏉ユ樉绀烘枃鏈?
using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Core;
using DG.Tweening;
using UI; // 寮曞叆浣犲凡鏈夌殑 UI 鍛藉悕绌洪棿锛屽寘鍚?UIPanel 鍩虹被
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 瑙嗗浘灞傦細鍙礋璐ｅ憟鐜癠I骞舵敹闆嗙敤鎴风偣鍑昏緭鍏ワ紝涓嶅寘鍚换浣曚笟鍔￠€昏緫鍒ゆ柇銆?
/// 娉ㄦ剰锛氳繖閲岀户鎵胯嚜浣犲凡鏈夌殑 UIPanel 鍩虹被銆?
/// </summary>
public class MainGamePanel : UIPanel
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _coinText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _levelGoalText;
    [SerializeField] private TextMeshProUGUI _purchaseLimitText;
    [SerializeField] private TextMeshProUGUI _levelStateText;
    [SerializeField] private Slider _levelGoalSlider;

    [Header("Shop & Upgrade Lists Root")]
    [SerializeField] private Transform _slotListRoot;       // 鎸傝浇宸︿晶璐拱鑰佽檸鏈烘寜閽殑鐖惰妭鐐?
    [SerializeField] private Transform _upgradeListRoot;    // 鎸傝浇鍙充晶鍗囩骇鎸夐挳鐨勭埗鑺傜偣
    [SerializeField] private Transform _scratchToolsListRoot;

    [Header("Scratch Card Root")]
    [SerializeField] private RectTransform _scratchCardRoot; // 鎸傝浇鍔ㄦ€佺敓鎴愮殑褰╃エ琛ㄧ幇灞?

    [Header("Focus Overlay Root")]
    [SerializeField] private RectTransform _focusOverlayRoot; // 鎸傝浇鑱氱劍閬僵灞傜殑鍗曠嫭瀹瑰櫒

    [Header("Focus Overlay")]
    [SerializeField] private Color _focusOverlayColor = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] private float _focusOverlayFadeDuration = 0.18f;
    [SerializeField] private float _levelGoalSliderTweenDuration = 0.2f;
    [SerializeField] private float _levelGoalWinOutlineDuration = 0.65f;
    [SerializeField] private float _levelGoalWinOutlineTargetScaleX = 1f;
    [SerializeField] private float _levelGoalWinOutlineFadeDuration = 0.5f;

    [Header("Rogue Cards")]
    [SerializeField] private RectTransform _rogueOwnedCardsRoot;
    [SerializeField] private RectTransform _rogueChoiceOverlayRoot;
    [SerializeField] private float _rogueRewardChoiceCardScale = 1f;
    [SerializeField] private float _rogueRewardChoiceSpacing = 28f;
    [SerializeField] private float _rogueRewardSelectUnselectedScaleDuration = 0.18f;
    [SerializeField] private float _rogueRewardSelectMoveDuration = 0.28f;
    [SerializeField] private float _rogueRewardOwnedAppearDuration = 0.32f;
    [SerializeField] private float _rogueRewardOwnedAppearOvershoot = 1.35f;

    [Header("Win Panel")]
    [SerializeField] private RectTransform _winPanelRoot;
    [SerializeField] private Button _rogueCardRewardButton;
    [SerializeField] private Button _scratchToolRewardButton;
    [SerializeField] private Button _scratchCardRewardButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private RectTransform _newLevelPanelRoot;
    [SerializeField] private TextMeshProUGUI _newLevelTargetText;
    [SerializeField] private TextMeshProUGUI _newLevelInfoText;
    [SerializeField] private Button _newLevelStartButton;
    [SerializeField] private float _winPanelRibbonScaleDuration = 0.22f;
    [SerializeField] private float _winPanelBgScaleDuration = 0.22f;
    [SerializeField] private float _winPanelRewardButtonDelay = 0.08f;
    [SerializeField] private float _winPanelRewardButtonPopDuration = 0.16f;
    [SerializeField] private float _newLevelRibbonScaleDuration = 0.22f;
    [SerializeField] private float _newLevelBgScaleDuration = 0.22f;
    [SerializeField] private float _newLevelStartButtonDelay = 0.08f;
    [SerializeField] private float _newLevelStartButtonPopDuration = 0.16f;
    [SerializeField] private float _scratchCardRewardChoiceScale = 1.4f;
    [SerializeField] private float _scratchCardRewardChoiceSpacing = 48f;
    [SerializeField] private float _scratchCardRewardChoiceShowDuration = 0.28f;
    [SerializeField] private float _scratchCardRewardChoiceShowStagger = 0.06f;
    [SerializeField] private float _scratchCardRewardChoiceHoverScale = 1.6f;
    [SerializeField] private float _scratchCardRewardChoiceHoverOffsetY = 60f;
    [SerializeField] private float _scratchCardRewardChoiceHoverDuration = 0.16f;
    [SerializeField] private float _scratchToolRewardChoiceScale = 3.5f;
    [SerializeField] private float _scratchToolRewardChoiceSpacing = 300f;

    [Header("Coin Rain Effect")]
    [SerializeField] private RectTransform _coinRainRoot;
    [SerializeField] private int _coinRainIconCount = 80;
    [SerializeField] private int _coinRainPoolPrewarmCount = 260;
    [SerializeField] private float _coinRainDuration = 3.5f;
    [SerializeField] private int _coinRainWaveCount = 5;
    [SerializeField] private Vector2 _coinRainIconSizeRange = new Vector2(42f, 86f);
    [SerializeField] private Vector2 _coinRainFallDurationRange = new Vector2(0.75f, 1.15f);
    [SerializeField] private Vector2 _coinRainHorizontalDriftRange = new Vector2(-260f, 260f);
    [SerializeField] private Vector2 _coinRainFallOvershootRange = new Vector2(90f, 240f);
    [SerializeField] private float _coinRainJackpotPopDuration = 0.75f;
    [SerializeField] private float _coinRainJackpotHoldDuration = 1.8f;
    [SerializeField] private float _coinRainJackpotShrinkDuration = 0.65f;
    [SerializeField] private float _coinRainJackpotTargetScale = 1.65f;
    [SerializeField] private float _coinRainJackpotFontSize = 112f;
    [SerializeField] private Color _coinRainJackpotTextColor = new Color(1f, 0.84f, 0.18f, 1f);

    [Header("Game Over Effect")]
    [SerializeField] private float _gameOverMaskFadeDuration = 1.2f;

    // 瀵瑰鏆撮湶鍒楄〃鐨勭埗鑺傜偣渚?Controller 鎸傝浇瀛愮墿浣撳紩鐢?
    public Transform SlotListRoot => _slotListRoot;
    public Transform UpgradeListRoot => _upgradeListRoot;
    public Transform ScratchToolsListRoot
    {
        get
        {
            EnsureScratchToolsListRoot();
            return _scratchToolsListRoot;
        }
    }
    public RectTransform ScratchCardRoot => _scratchCardRoot;
    public RectTransform FocusOverlayRoot => _focusOverlayRoot;

    private CanvasGroup _focusOverlayCanvasGroup;
    private Image _focusOverlayImage;
    private Tween _focusOverlayTween;
    private Tween _levelGoalSliderTween;
    private Tween _levelGoalWinOutlineTween;
    private Sequence _winPanelShowSequence;
    private Sequence _newLevelPanelShowSequence;
    private Sequence _rogueChoiceShowSequence;
    private Sequence _scratchToolChoiceShowSequence;
    private Sequence _scratchCardChoiceShowSequence;
    private Sequence _rogueChoiceSelectSequence;
    private Sequence _rogueOwnedAppearSequence;
    private Coroutine _rogueChoiceShowCoroutine;
    private Coroutine _scratchToolChoiceShowCoroutine;
    private Coroutine _scratchCardChoiceShowCoroutine;
    private bool _isRogueChoiceShowAnimating;
    private bool _isRogueChoiceSelectAnimating;
    private int _pendingOwnedRogueCardAppearId = -1;
    [SerializeField] private ScratchCardFocusPanelView _configuredFocusPanelView;
    private ScratchCardFocusPanelView _focusPanelView;
    private Transform _winPanelRibbon;
    private Transform _winPanelBg;
    private Transform _levelGoalWinOutline;
    private Graphic _levelGoalWinOutlineGraphic;
    private Transform _newLevelRibbon;
    private Transform _newLevelBg;
    private Vector3 _winPanelRibbonDefaultScale = Vector3.one;
    private Vector3 _winPanelBgDefaultScale = Vector3.one;
    private Vector3 _levelGoalWinOutlineDefaultScale = Vector3.one;
    private float _levelGoalWinOutlineDefaultAlpha = 1f;
    private Vector3 _newLevelRibbonDefaultScale = Vector3.one;
    private Vector3 _newLevelBgDefaultScale = Vector3.one;
    private Vector3 _newLevelStartButtonDefaultScale = Vector3.one;
    private bool _hasNewLevelStartButtonDefaultScale;
    private readonly Dictionary<Button, Vector3> _winPanelButtonDefaultScales = new Dictionary<Button, Vector3>();
    private GameObject _rogueChoiceOverlayObject;
    private Image _rogueChoiceOverlayImage;
    private Transform _rogueChoiceContentRoot;
    private readonly List<GameObject> _rogueChoiceObjects = new List<GameObject>();
    private TextMeshProUGUI _rogueExchangePromptText;
    private bool _isRogueExchangeMode;
    private int _selectedRogueExchangeCardId = -1;
    private int _selectedRogueExchangeCardLevel = 1;
    private GameObject _selectedRogueExchangeCardObject;
    private Vector2 _selectedRogueExchangeRestingPosition;
    private Vector3 _selectedRogueExchangeRestingScale = Vector3.one;
    private GameObject _scratchToolChoiceOverlayObject;
    private Transform _scratchToolChoiceContentRoot;
    private readonly List<GameObject> _scratchToolChoiceObjects = new List<GameObject>();
    private GameObject _scratchCardChoiceOverlayObject;
    private Transform _scratchCardChoiceContentRoot;
    private readonly List<GameObject> _scratchCardChoiceObjects = new List<GameObject>();
    private readonly List<GameObject> _ownedRogueCardObjects = new List<GameObject>();
    private readonly List<CoinRainIcon> _coinRainPool = new List<CoinRainIcon>();
    private readonly List<CoinRainIcon> _activeCoinRainIcons = new List<CoinRainIcon>();
    private Sprite[] _coinRainSprites;
    private GameObject _coinRainJackpotTextObject;
    private Sequence _coinRainJackpotTextSequence;
    private Image _gameOverMaskImage;
    private Coroutine _gameOverSkullEffectRoutine;

    public event Action<int, int> OnRogueRewardCardSelected;
    public event Action<int> OnRogueOwnedCardSelected;
    public event Action OnRogueRewardRequested;
    public event Action<int> OnScratchToolRewardSelected;
    public event Action OnScratchToolRewardRequested;
    public event Action<int> OnScratchCardRewardSelected;
    public event Action OnScratchCardRewardRequested;
    public event Action OnWinContinueRequested;
    public event Action OnNewLevelStartRequested;

    public int SelectedRogueExchangeCardId => _selectedRogueExchangeCardId;
    public int SelectedRogueExchangeCardLevel => _selectedRogueExchangeCardLevel;

    public void QueueOwnedRogueCardAppear(int cardId)
    {
        _pendingOwnedRogueCardAppearId = cardId;
    }

    private void Awake()
    {
        EnsureWinPanel();
        HideWinPanel();
        EnsureNewLevelPanel();
        HideNewLevelPanel();
    }

    private void Update()
    {
        if (!_isRogueExchangeMode || _selectedRogueExchangeCardId <= 0)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TrySelectOwnedRogueCardAtScreenPoint(Input.mousePosition, GetUiEventCamera());
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            TrySelectOwnedRogueCardAtScreenPoint(Input.GetTouch(0).position, GetUiEventCamera());
        }
    }

    // 濡傛灉闈㈡澘涓婃湁閫氱敤鐨勭偣鍑绘寜閽紝鍙互閫氳繃浜嬩欢鍚慍ontroller鎶涘嚭
    // public event Action OnSomeGlobalButtonClicked;

    /// <summary>
    /// 渚?Controller 璋冪敤锛岀敤浜庡埛鏂伴噾甯佹樉绀虹殑琛ㄧ幇
    /// </summary>
    /// <param name="currentCoins">鐜╁褰撳墠閲戝竵鏁伴噺</param>
    public void UpdateCoinDisplay(double currentCoins)
    {
        if (_coinText != null)
        {
            // 澧為噺娓告垙閫氬父闇€瑕佹牸寮忓寲搴炲ぇ鏁板瓧 (濡?1.2K, 3.5M)
            // 杩欓噷鏆備笖浣跨敤瀛楃涓叉牸寮忓寲灞曠ず
            AssetProvider.ApplyDefaultTmpFont(_coinText);
            _coinText.text = NumberFormatter.FormatCompact(currentCoins);
        }
    }

    public void UpdateLevelDisplay(LevelProgressModel levelModel, double currentCoins)
    {
        if (levelModel == null)
        {
            return;
        }

        if (_levelText != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_levelText);
            _levelText.text = levelModel.LevelName;
        }

        if (_levelGoalText != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_levelGoalText);
            _levelGoalText.text = $"{NumberFormatter.FormatCompact(currentCoins)} / {NumberFormatter.FormatCompact(levelModel.RequiredCoins)}";
        }

        UpdateLevelGoalSlider(levelModel.RequiredCoins, currentCoins);

        if (_purchaseLimitText != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_purchaseLimitText);
            _purchaseLimitText.text = "无限";
        }

        if (_levelStateText != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_levelStateText);
            _levelStateText.text = levelModel.IsPassed ? "已通关" : "进行中";
        }
    }

    private void UpdateLevelGoalSlider(double targetCoins, double currentCoins)
    {
        if (_levelGoalSlider == null)
        {
            return;
        }

        float maxValue = targetCoins > 0d ? (float)targetCoins : 1f;
        float nextValue = Mathf.Clamp((float)currentCoins, 0f, maxValue);

        _levelGoalSlider.maxValue = maxValue;
        _levelGoalSliderTween?.Kill();

        if (!gameObject.activeInHierarchy)
        {
            _levelGoalSlider.value = nextValue;
            return;
        }

        _levelGoalSliderTween = _levelGoalSlider
            .DOValue(nextValue, _levelGoalSliderTweenDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => _levelGoalSliderTween = null);
    }

    public void PlayLevelGoalWinEffect(Action onComplete)
    {
        EnsureLevelGoalWinEffectTarget();
        _levelGoalWinOutlineTween?.Kill();

        if (_levelGoalWinOutline == null)
        {
            onComplete?.Invoke();
            return;
        }

        Vector3 targetScale = GetLevelGoalWinOutlineTargetScale();
        RestoreLevelGoalWinOutlineAlpha();
        _levelGoalWinOutline.gameObject.SetActive(true);
        _levelGoalWinOutline.localScale = new Vector3(0f, targetScale.y, targetScale.z);

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(_levelGoalWinOutline
            .DOScaleX(targetScale.x, _levelGoalWinOutlineDuration)
            .SetEase(Ease.OutCubic));
        sequence.AppendCallback(() =>
        {
            _levelGoalWinOutline.localScale = targetScale;
            onComplete?.Invoke();
        });

        if (_levelGoalWinOutlineGraphic != null)
        {
            sequence.Append(_levelGoalWinOutlineGraphic
                .DOFade(0f, _levelGoalWinOutlineFadeDuration)
                .SetEase(Ease.OutCubic));
        }

        sequence.OnComplete(() =>
            {
                _levelGoalWinOutlineTween = null;
                if (_levelGoalWinOutline != null)
                {
                    _levelGoalWinOutline.gameObject.SetActive(false);
                }
            });
        _levelGoalWinOutlineTween = sequence;
    }

    public void PlayCoinRainEffect(string jackpotText = "头奖")
    {
        EnsureCoinRainRoot();
        EnsureCoinRainSprites();
        PrewarmCoinRainPool();

        if (_coinRainRoot == null || _coinRainSprites == null || _coinRainSprites.Length == 0)
        {
            return;
        }

        _coinRainRoot.gameObject.SetActive(true);
        _coinRainRoot.SetAsLastSibling();
        AudioManager.Instance?.PlayCue(AudioCueId.CoinPouring);

        Rect rect = _coinRainRoot.rect;
        float duration = Mathf.Max(0.1f, _coinRainDuration);
        int iconsPerWave = Mathf.Max(1, _coinRainIconCount);
        int waveCount = Mathf.Max(1, _coinRainWaveCount);
        float maxFallDuration = Mathf.Max(0.1f, Mathf.Max(_coinRainFallDurationRange.x, _coinRainFallDurationRange.y));
        float waveInterval = waveCount > 1 ? Mathf.Max(0.05f, (duration - maxFallDuration) / waveCount) : 0f;
        for (int waveIndex = 0; waveIndex < waveCount; waveIndex++)
        {
            float waveDelay = waveInterval * waveIndex;
            for (int i = 0; i < iconsPerWave; i++)
            {
                CoinRainIcon icon = GetCoinRainIcon();
                if (icon == null)
                {
                    continue;
                }

                PlayCoinRainIcon(icon, rect, waveDelay, i, iconsPerWave);
            }
        }

        PlayCoinRainJackpotText(jackpotText);
    }

    public void PlayGameOverSkullEffect()
    {
        if (_gameOverSkullEffectRoutine != null)
        {
            StopCoroutine(_gameOverSkullEffectRoutine);
        }

        _gameOverSkullEffectRoutine = StartCoroutine(PlayGameOverSkullEffectRoutine());
    }

    private IEnumerator PlayGameOverSkullEffectRoutine()
    {
        EnsureGameOverMask();
        if (_gameOverMaskImage == null)
        {
            yield break;
        }

        _gameOverMaskImage.gameObject.SetActive(true);
        _gameOverMaskImage.transform.SetAsLastSibling();
        SetGameOverMaskAlpha(0f);

        AudioManager.Instance?.PlayCue(AudioCueId.CoverUp);
        float coverDuration = Mathf.Max(
            Mathf.Max(0.01f, _gameOverMaskFadeDuration),
            AudioManager.Instance != null ? AudioManager.Instance.GetCueDuration(AudioCueId.CoverUp) : 0f);

        float elapsed = 0f;
        while (elapsed < coverDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetGameOverMaskAlpha(Mathf.Clamp01(elapsed / Mathf.Max(0.01f, _gameOverMaskFadeDuration)));
            yield return null;
        }

        SetGameOverMaskAlpha(1f);
        AudioManager.Instance?.PlayCue(AudioCueId.Cock);
        yield return new WaitForSecondsRealtime(AudioManager.Instance != null
            ? AudioManager.Instance.GetCueDuration(AudioCueId.Cock)
            : 0f);

        AudioManager.Instance?.PlayCue(AudioCueId.Shot);
        _gameOverSkullEffectRoutine = null;
    }

    private void EnsureGameOverMask()
    {
        RectTransform parent = GetCoinRainRootParent();
        if (parent == null)
        {
            return;
        }

        if (_gameOverMaskImage == null)
        {
            GameObject maskObject = new GameObject("GameOverMask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            maskObject.transform.SetParent(parent, false);
            _gameOverMaskImage = maskObject.GetComponent<Image>();
            _gameOverMaskImage.raycastTarget = true;
        }
        else if (_gameOverMaskImage.transform.parent != parent)
        {
            _gameOverMaskImage.transform.SetParent(parent, false);
        }

        RectTransform rectTransform = _gameOverMaskImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    private void SetGameOverMaskAlpha(float alpha)
    {
        if (_gameOverMaskImage != null)
        {
            _gameOverMaskImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
        }
    }

    private void PlayCoinRainIcon(CoinRainIcon icon, Rect rootRect, float waveDelay, int iconIndex, int iconsPerWave)
    {
        Sprite sprite = _coinRainSprites[UnityEngine.Random.Range(0, _coinRainSprites.Length)];
        float size = UnityEngine.Random.Range(
            Mathf.Max(1f, _coinRainIconSizeRange.x),
            Mathf.Max(_coinRainIconSizeRange.x, _coinRainIconSizeRange.y));
        float delay = waveDelay + UnityEngine.Random.Range(0f, 0.42f);
        float fallDuration = UnityEngine.Random.Range(
            Mathf.Max(0.1f, _coinRainFallDurationRange.x),
            Mathf.Max(_coinRainFallDurationRange.x, _coinRainFallDurationRange.y));
        float segmentT = iconsPerWave > 1
            ? (iconIndex + UnityEngine.Random.Range(-0.35f, 0.35f)) / (iconsPerWave - 1f)
            : UnityEngine.Random.value;
        float startX = Mathf.Lerp(rootRect.xMin, rootRect.xMax, Mathf.Clamp01(segmentT));
        float startY = rootRect.yMax + UnityEngine.Random.Range(24f, 180f);
        float driftX = UnityEngine.Random.Range(_coinRainHorizontalDriftRange.x, _coinRainHorizontalDriftRange.y);
        float endY = rootRect.yMin - UnityEngine.Random.Range(_coinRainFallOvershootRange.x, _coinRainFallOvershootRange.y);
        float startRotation = UnityEngine.Random.Range(0f, 360f);
        float rotation = UnityEngine.Random.Range(240f, 900f) * (UnityEngine.Random.value < 0.5f ? -1f : 1f);

        icon.GameObject.SetActive(true);
        icon.Transform.SetAsLastSibling();
        icon.Transform.sizeDelta = new Vector2(size, size);
        icon.Transform.anchoredPosition = new Vector2(startX, startY);
        icon.Transform.localScale = Vector3.one;
        icon.Transform.localRotation = Quaternion.Euler(0f, 0f, startRotation);
        icon.Image.sprite = sprite;
        icon.Image.raycastTarget = false;
        icon.CanvasGroup.alpha = 0f;
        icon.CanvasGroup.blocksRaycasts = false;
        icon.CanvasGroup.interactable = false;

        icon.Sequence?.Kill(false);
        icon.Sequence = DOTween.Sequence().SetUpdate(true);
        icon.Sequence.SetDelay(delay);
        icon.Sequence.AppendCallback(icon.ShowCallback);
        icon.Sequence.Join(icon.Transform
            .DOAnchorPos(new Vector2(startX + driftX, endY), fallDuration)
            .SetEase(Ease.InQuad));
        icon.Sequence.Join(icon.Transform
            .DORotate(new Vector3(0f, 0f, startRotation + rotation), fallDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear));
        icon.Sequence.OnComplete(icon.RecycleCallback);
    }

    private void EnsureLevelGoalWinEffectTarget()
    {
        if (_levelGoalWinOutline != null)
        {
            return;
        }

        if (_levelGoalSlider == null)
        {
            return;
        }

        Transform background = FindChildRecursive(_levelGoalSlider.transform, "Background");
        Transform outline = background != null
            ? FindChildRecursive(background, "Outline")
            : FindChildRecursive(_levelGoalSlider.transform, "Outline");
        _levelGoalWinOutline = outline;
        _levelGoalWinOutlineDefaultScale = _levelGoalWinOutline != null ? _levelGoalWinOutline.localScale : Vector3.one;
        _levelGoalWinOutlineGraphic = _levelGoalWinOutline != null ? _levelGoalWinOutline.GetComponent<Graphic>() : null;
        _levelGoalWinOutlineDefaultAlpha = _levelGoalWinOutlineGraphic != null ? _levelGoalWinOutlineGraphic.color.a : 1f;
    }

    private Vector3 GetLevelGoalWinOutlineTargetScale()
    {
        Vector3 targetScale = _levelGoalWinOutlineDefaultScale;
        targetScale.x = Mathf.Approximately(targetScale.x, 0f)
            ? _levelGoalWinOutlineTargetScaleX
            : targetScale.x;
        return targetScale;
    }

    private void RestoreLevelGoalWinOutlineAlpha()
    {
        if (_levelGoalWinOutlineGraphic == null)
        {
            return;
        }

        Color color = _levelGoalWinOutlineGraphic.color;
        color.a = _levelGoalWinOutlineDefaultAlpha;
        _levelGoalWinOutlineGraphic.color = color;
    }

    public void ShowRogueCardChoices(
        IReadOnlyList<RogueCardRewardChoiceModel> choices,
        IReadOnlyList<RogueCardInventoryEntryModel> ownedCards = null,
        bool exchangeMode = false)
    {
        EnsureRogueChoiceOverlay();
        ClearRogueChoiceObjects();

        if (_rogueChoiceOverlayObject == null || _rogueChoiceContentRoot == null)
        {
            return;
        }

        _isRogueExchangeMode = exchangeMode;
        ClearRogueExchangeSelection();
        SetRogueExchangePromptVisible(false);
        if (_rogueChoiceOverlayImage != null)
        {
            _rogueChoiceOverlayImage.raycastTarget = true;
        }

        _rogueChoiceOverlayObject.SetActive(true);
        _rogueChoiceOverlayObject.transform.SetAsLastSibling();
        int count = choices != null ? choices.Count : 0;
        for (int i = 0; i < count; i++)
        {
            RogueCardRewardChoiceModel choice = choices[i];
            RogueCardConfig cardConfig = choice != null ? choice.CardConfig : null;
            if (cardConfig == null)
            {
                continue;
            }

            int currentLevel = GetOwnedRogueCardLevel(ownedCards, cardConfig.Id);
            GameObject cardObject = CreateRogueChoiceCard(choice, _rogueChoiceContentRoot, currentLevel);
            _rogueChoiceObjects.Add(cardObject);
        }

        _isRogueChoiceShowAnimating = true;
        SetRewardChoiceRaycasts(_rogueChoiceObjects, false);
        SetRogueChoiceHoverEnabled(false);
        _rogueChoiceShowCoroutine = StartRewardChoiceShowAnimation(
            _rogueChoiceContentRoot,
            _rogueChoiceObjects,
            sequence => _rogueChoiceShowSequence = sequence,
            () =>
            {
                _rogueChoiceShowSequence = null;
                _rogueChoiceShowCoroutine = null;
                _isRogueChoiceShowAnimating = false;
                SetRewardChoiceRaycasts(_rogueChoiceObjects, true);
                SetRewardChoiceButtonsInteractable(_rogueChoiceObjects, true);
                SetRogueChoiceHoverEnabled(true);
                RefreshRogueChoiceHoverState();
            },
            false);
    }

    public void HideRogueCardChoices()
    {
        _rogueChoiceSelectSequence?.Kill(false);
        _rogueChoiceSelectSequence = null;
        _isRogueChoiceSelectAnimating = false;
        StopRewardChoiceShowAnimation(ref _rogueChoiceShowSequence, ref _rogueChoiceShowCoroutine, _rogueChoiceContentRoot);
        _isRogueChoiceShowAnimating = false;
        _isRogueExchangeMode = false;
        ClearRogueExchangeSelection();
        SetRogueExchangePromptVisible(false);
        SetRogueChoiceHoverEnabled(true);
        if (_rogueChoiceOverlayImage != null)
        {
            _rogueChoiceOverlayImage.raycastTarget = true;
        }

        if (_rogueChoiceOverlayObject != null)
        {
            _rogueChoiceOverlayObject.SetActive(false);
        }
    }

    public void ShowScratchToolChoices(IReadOnlyList<ScratchToolConfig> choices)
    {
        EnsureScratchToolChoiceOverlay();
        ClearScratchToolChoiceObjects();

        if (_scratchToolChoiceOverlayObject == null || _scratchToolChoiceContentRoot == null)
        {
            return;
        }

        _scratchToolChoiceOverlayObject.SetActive(true);
        _scratchToolChoiceOverlayObject.transform.SetAsLastSibling();
        int count = choices != null ? choices.Count : 0;
        for (int i = 0; i < count; i++)
        {
            ScratchToolConfig toolConfig = choices[i];
            if (toolConfig == null)
            {
                continue;
            }

            GameObject toolObject = CreateScratchToolChoice(toolConfig, _scratchToolChoiceContentRoot);
            _scratchToolChoiceObjects.Add(toolObject);
        }

        _scratchToolChoiceShowCoroutine = StartRewardChoiceShowAnimation(
            _scratchToolChoiceContentRoot,
            _scratchToolChoiceObjects,
            sequence => _scratchToolChoiceShowSequence = sequence,
            () =>
            {
                _scratchToolChoiceShowSequence = null;
                _scratchToolChoiceShowCoroutine = null;
            });
    }

    public void HideScratchToolChoices()
    {
        StopRewardChoiceShowAnimation(ref _scratchToolChoiceShowSequence, ref _scratchToolChoiceShowCoroutine, _scratchToolChoiceContentRoot);

        if (_scratchToolChoiceOverlayObject != null)
        {
            _scratchToolChoiceOverlayObject.SetActive(false);
        }
    }

    public void ShowScratchCardChoices(IReadOnlyList<ScratchCardTypeConfig> choices)
    {
        EnsureScratchCardChoiceOverlay();
        ClearScratchCardChoiceObjects();

        if (_scratchCardChoiceOverlayObject == null || _scratchCardChoiceContentRoot == null)
        {
            return;
        }

        _scratchCardChoiceOverlayObject.SetActive(true);
        _scratchCardChoiceOverlayObject.transform.SetAsLastSibling();
        int count = choices != null ? choices.Count : 0;
        for (int i = 0; i < count; i++)
        {
            ScratchCardTypeConfig cardTypeConfig = choices[i];
            if (cardTypeConfig == null)
            {
                continue;
            }

            GameObject cardObject = CreateScratchCardChoice(cardTypeConfig, _scratchCardChoiceContentRoot);
            _scratchCardChoiceObjects.Add(cardObject);
        }

        SetScratchCardChoiceHoverEnabled(false);
        PlayScratchCardChoiceShowAnimation();
    }

    public void HideScratchCardChoices()
    {
        StopRewardChoiceShowAnimation(ref _scratchCardChoiceShowSequence, ref _scratchCardChoiceShowCoroutine, _scratchCardChoiceContentRoot);

        if (_scratchCardChoiceOverlayObject != null)
        {
            _scratchCardChoiceOverlayObject.SetActive(false);
        }
    }

    public void ShowWinPanel(bool canRequestRogueReward, bool canContinue, bool canRequestScratchToolReward = false, bool canRequestScratchCardReward = false)
    {
        EnsureWinPanel();
        if (_winPanelRoot == null)
        {
            return;
        }

        bool shouldPlayShowAnimation = !_winPanelRoot.gameObject.activeSelf;
        _winPanelRoot.gameObject.SetActive(true);
        _winPanelRoot.SetAsLastSibling();
        if (shouldPlayShowAnimation)
        {
            PlayWinPanelShowAnimation(canRequestRogueReward, canContinue, canRequestScratchToolReward, canRequestScratchCardReward);
            return;
        }

        _winPanelShowSequence?.Kill();
        _winPanelShowSequence = null;
        RestoreWinPanelAnimatedTargets();
        ApplyWinPanelButtonStates(canRequestRogueReward, canContinue, canRequestScratchToolReward, canRequestScratchCardReward);
    }

    public void HideWinPanel()
    {
        EnsureWinPanel();
        _winPanelShowSequence?.Kill();
        _winPanelShowSequence = null;
        if (_winPanelRoot != null)
        {
            _winPanelRoot.gameObject.SetActive(false);
        }
    }

    public void ShowNewLevelPanel(LevelConfig levelConfig)
    {
        EnsureNewLevelPanel();
        if (_newLevelPanelRoot == null)
        {
            return;
        }

        if (_newLevelTargetText != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_newLevelTargetText);
            _newLevelTargetText.text = levelConfig != null ? NumberFormatter.FormatCompact(levelConfig.RequiredCoins) : "-";
        }

        if (_newLevelInfoText != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_newLevelInfoText);
            _newLevelInfoText.text = GetLevelDisplayName(levelConfig);
        }

        bool shouldPlayShowAnimation = !_newLevelPanelRoot.gameObject.activeSelf;
        _newLevelPanelRoot.gameObject.SetActive(true);
        _newLevelPanelRoot.SetAsLastSibling();
        if (shouldPlayShowAnimation)
        {
            PlayNewLevelPanelShowAnimation(levelConfig != null);
            return;
        }

        _newLevelPanelShowSequence?.Kill();
        _newLevelPanelShowSequence = null;
        RestoreNewLevelPanelAnimatedTargets();
        SetNewLevelStartButtonState(levelConfig != null);
    }

    public void HideNewLevelPanel()
    {
        EnsureNewLevelPanel();
        _newLevelPanelShowSequence?.Kill();
        _newLevelPanelShowSequence = null;
        RestoreNewLevelPanelAnimatedTargets();
        if (_newLevelPanelRoot != null)
        {
            _newLevelPanelRoot.gameObject.SetActive(false);
        }
    }

    public void SetWinPanelRewardButtonVisible(bool visible, bool interactable = true)
    {
        EnsureWinPanel();
        SetWinPanelButtonState(_rogueCardRewardButton, visible, interactable);
    }

    public void SetWinPanelScratchToolRewardButtonVisible(bool visible, bool interactable = true)
    {
        EnsureWinPanel();
        SetWinPanelButtonState(_scratchToolRewardButton, visible, interactable);
    }

    public void SetWinPanelScratchCardRewardButtonVisible(bool visible, bool interactable = true)
    {
        EnsureWinPanel();
        SetWinPanelButtonState(_scratchCardRewardButton, visible, interactable);
    }

    public void SetWinPanelContinueButtonVisible(bool visible, bool interactable = true)
    {
        EnsureWinPanel();
        SetWinPanelButtonState(_continueButton, visible, interactable);
    }

    private void ApplyWinPanelButtonStates(
        bool canRequestRogueReward,
        bool canContinue,
        bool canRequestScratchToolReward,
        bool canRequestScratchCardReward)
    {
        SetWinPanelButtonState(_rogueCardRewardButton, canRequestRogueReward, canRequestRogueReward);
        SetWinPanelButtonState(_scratchToolRewardButton, canRequestScratchToolReward, canRequestScratchToolReward);
        SetWinPanelButtonState(_scratchCardRewardButton, canRequestScratchCardReward, canRequestScratchCardReward);
        SetWinPanelButtonState(_continueButton, canContinue, canContinue);
        RestoreWinPanelButtonScale(_rogueCardRewardButton);
        RestoreWinPanelButtonScale(_scratchToolRewardButton);
        RestoreWinPanelButtonScale(_scratchCardRewardButton);
        RestoreWinPanelButtonScale(_continueButton);
    }

    private void PlayWinPanelShowAnimation(
        bool canRequestRogueReward,
        bool canContinue,
        bool canRequestScratchToolReward,
        bool canRequestScratchCardReward)
    {
        EnsureWinPanelAnimationTargets();
        _winPanelShowSequence?.Kill();

        SetWinPanelButtonState(_rogueCardRewardButton, false, false);
        SetWinPanelButtonState(_scratchToolRewardButton, false, false);
        SetWinPanelButtonState(_scratchCardRewardButton, false, false);
        SetWinPanelButtonState(_continueButton, false, false);

        ResetWinPanelAnimatedScale(_winPanelRibbon, _winPanelRibbonDefaultScale, true);
        ResetWinPanelAnimatedScale(_winPanelBg, _winPanelBgDefaultScale, false);

        _winPanelShowSequence = DOTween.Sequence().SetUpdate(true);
        if (_winPanelRibbon != null)
        {
            _winPanelShowSequence.Append(_winPanelRibbon.DOScaleX(_winPanelRibbonDefaultScale.x, _winPanelRibbonScaleDuration).SetEase(Ease.OutBack));
        }

        if (_winPanelBg != null)
        {
            _winPanelShowSequence.Append(_winPanelBg.DOScaleY(_winPanelBgDefaultScale.y, _winPanelBgScaleDuration).SetEase(Ease.OutBack));
        }

        AppendWinPanelRewardButtonShow(_rogueCardRewardButton, canRequestRogueReward);
        AppendWinPanelRewardButtonShow(_scratchToolRewardButton, canRequestScratchToolReward);
        AppendWinPanelRewardButtonShow(_scratchCardRewardButton, canRequestScratchCardReward);

        _winPanelShowSequence.OnComplete(() =>
        {
            _winPanelShowSequence = null;
            RestoreWinPanelAnimatedTargets();
            SetWinPanelButtonState(_continueButton, canContinue, canContinue);
            RestoreWinPanelButtonScale(_continueButton);
        });
    }

    private void AppendWinPanelRewardButtonShow(Button button, bool visible)
    {
        if (button == null || !visible)
        {
            return;
        }

        Vector3 defaultScale = GetWinPanelButtonDefaultScale(button);
        _winPanelShowSequence.AppendInterval(_winPanelRewardButtonDelay);
        _winPanelShowSequence.AppendCallback(() =>
        {
            button.gameObject.SetActive(true);
            button.interactable = true;
            button.transform.localScale = Vector3.zero;
        });
        _winPanelShowSequence.Append(button.transform.DOScale(defaultScale, _winPanelRewardButtonPopDuration).SetEase(Ease.OutBack));
    }

    private void EnsureWinPanelAnimationTargets()
    {
        EnsureWinPanel();
        if (_winPanelRoot == null)
        {
            return;
        }

        if (_winPanelRibbon == null)
        {
            _winPanelRibbon = FindChildRecursive(_winPanelRoot, "Ribbon");
            _winPanelRibbonDefaultScale = _winPanelRibbon != null ? _winPanelRibbon.localScale : Vector3.one;
        }

        if (_winPanelBg == null)
        {
            _winPanelBg = FindChildRecursive(_winPanelRoot, "BG");
            _winPanelBgDefaultScale = _winPanelBg != null ? _winPanelBg.localScale : Vector3.one;
        }

        GetWinPanelButtonDefaultScale(_rogueCardRewardButton);
        GetWinPanelButtonDefaultScale(_scratchToolRewardButton);
        GetWinPanelButtonDefaultScale(_scratchCardRewardButton);
        GetWinPanelButtonDefaultScale(_continueButton);
    }

    private static void ResetWinPanelAnimatedScale(Transform target, Vector3 defaultScale, bool collapseX)
    {
        if (target == null)
        {
            return;
        }

        target.localScale = collapseX
            ? new Vector3(0f, defaultScale.y, defaultScale.z)
            : new Vector3(defaultScale.x, 0f, defaultScale.z);
    }

    private void RestoreWinPanelAnimatedTargets()
    {
        if (_winPanelRibbon != null)
        {
            _winPanelRibbon.localScale = _winPanelRibbonDefaultScale;
        }

        if (_winPanelBg != null)
        {
            _winPanelBg.localScale = _winPanelBgDefaultScale;
        }
    }

    private Vector3 GetWinPanelButtonDefaultScale(Button button)
    {
        if (button == null)
        {
            return Vector3.one;
        }

        if (_winPanelButtonDefaultScales.TryGetValue(button, out Vector3 defaultScale))
        {
            return defaultScale;
        }

        defaultScale = button.transform.localScale;
        _winPanelButtonDefaultScales[button] = defaultScale;
        return defaultScale;
    }

    private void RestoreWinPanelButtonScale(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.transform.localScale = GetWinPanelButtonDefaultScale(button);
    }

    private void PlayNewLevelPanelShowAnimation(bool canStart)
    {
        EnsureNewLevelPanelAnimationTargets();
        _newLevelPanelShowSequence?.Kill();

        SetNewLevelStartButtonState(false);
        ResetWinPanelAnimatedScale(_newLevelRibbon, _newLevelRibbonDefaultScale, true);
        ResetWinPanelAnimatedScale(_newLevelBg, _newLevelBgDefaultScale, false);

        _newLevelPanelShowSequence = DOTween.Sequence().SetUpdate(true);
        if (_newLevelRibbon != null)
        {
            _newLevelPanelShowSequence.Append(_newLevelRibbon.DOScaleX(_newLevelRibbonDefaultScale.x, _newLevelRibbonScaleDuration).SetEase(Ease.OutBack));
        }

        if (_newLevelBg != null)
        {
            _newLevelPanelShowSequence.Append(_newLevelBg.DOScaleY(_newLevelBgDefaultScale.y, _newLevelBgScaleDuration).SetEase(Ease.OutBack));
        }

        if (_newLevelStartButton != null)
        {
            _newLevelPanelShowSequence.AppendInterval(_newLevelStartButtonDelay);
            _newLevelPanelShowSequence.AppendCallback(() =>
            {
                _newLevelStartButton.gameObject.SetActive(true);
                _newLevelStartButton.interactable = canStart;
                _newLevelStartButton.transform.localScale = Vector3.zero;
            });
            _newLevelPanelShowSequence.Append(_newLevelStartButton.transform.DOScale(_newLevelStartButtonDefaultScale, _newLevelStartButtonPopDuration).SetEase(Ease.OutBack));
        }

        _newLevelPanelShowSequence.OnComplete(() =>
        {
            _newLevelPanelShowSequence = null;
            RestoreNewLevelPanelAnimatedTargets();
            SetNewLevelStartButtonState(canStart);
        });
    }

    private void EnsureNewLevelPanelAnimationTargets()
    {
        EnsureNewLevelPanel();
        if (_newLevelPanelRoot == null)
        {
            return;
        }

        if (_newLevelRibbon == null)
        {
            _newLevelRibbon = FindChildRecursive(_newLevelPanelRoot, "Ribbon");
            _newLevelRibbonDefaultScale = _newLevelRibbon != null ? _newLevelRibbon.localScale : Vector3.one;
        }

        if (_newLevelBg == null)
        {
            _newLevelBg = FindChildRecursive(_newLevelPanelRoot, "BG");
            _newLevelBgDefaultScale = _newLevelBg != null ? _newLevelBg.localScale : Vector3.one;
        }

        if (_newLevelStartButton != null && !_hasNewLevelStartButtonDefaultScale)
        {
            _newLevelStartButtonDefaultScale = _newLevelStartButton.transform.localScale;
            _hasNewLevelStartButtonDefaultScale = true;
        }
    }

    private void RestoreNewLevelPanelAnimatedTargets()
    {
        if (_newLevelRibbon != null)
        {
            _newLevelRibbon.localScale = _newLevelRibbonDefaultScale;
        }

        if (_newLevelBg != null)
        {
            _newLevelBg.localScale = _newLevelBgDefaultScale;
        }

        if (_newLevelStartButton != null)
        {
            _newLevelStartButton.transform.localScale = _newLevelStartButtonDefaultScale;
        }
    }

    private void SetNewLevelStartButtonState(bool visibleAndInteractable)
    {
        if (_newLevelStartButton == null)
        {
            return;
        }

        _newLevelStartButton.gameObject.SetActive(visibleAndInteractable);
        _newLevelStartButton.interactable = visibleAndInteractable;
        if (visibleAndInteractable)
        {
            _newLevelStartButton.transform.localScale = _newLevelStartButtonDefaultScale;
        }
    }

    public void RefreshOwnedRogueCards(IReadOnlyList<RogueCardInventoryEntryModel> ownedCards)
    {
        EnsureRogueOwnedCardsRoot();
        ClearOwnedRogueCardObjects();

        if (_rogueOwnedCardsRoot == null)
        {
            return;
        }

        int count = ownedCards != null ? ownedCards.Count : 0;
        for (int i = 0; i < count; i++)
        {
            RogueCardInventoryEntryModel ownedCard = ownedCards[i];
            if (ownedCard == null)
            {
                continue;
            }

            GameObject cardObject = CreateOwnedRogueCard(ownedCard, _rogueOwnedCardsRoot);
            _ownedRogueCardObjects.Add(cardObject);
            if (ownedCard.CardId == _pendingOwnedRogueCardAppearId)
            {
                PlayOwnedRogueCardAppearAnimation(cardObject);
                _pendingOwnedRogueCardAppearId = -1;
            }
        }
    }

    public void ShowScratchCardFocusOverlay(RectTransform focusedCard, ScratchCardFocusPanelModel focusPanelModel = null)
    {
        EnsureFocusOverlay();
        if (_focusOverlayCanvasGroup == null)
        {
            return;
        }

        _focusOverlayCanvasGroup.gameObject.SetActive(true);
        _focusOverlayCanvasGroup.blocksRaycasts = true;

        _focusOverlayTween?.Kill();
        _focusOverlayTween = _focusOverlayCanvasGroup
            .DOFade(1f, _focusOverlayFadeDuration)
            .SetUpdate(true)
            .OnComplete(() => _focusOverlayTween = null);

        if (focusedCard != null)
        {
            MoveScratchCardToOverlayLayer(focusedCard);
        }

        if (focusPanelModel != null)
        {
            ShowScratchCardFocusPanel(focusPanelModel);
        }
    }

    public void HideScratchCardFocusOverlay()
    {
        if (_focusOverlayCanvasGroup == null)
        {
            return;
        }

        _focusOverlayCanvasGroup.blocksRaycasts = false;
        _focusOverlayTween?.Kill();
        _focusOverlayTween = _focusOverlayCanvasGroup
            .DOFade(0f, _focusOverlayFadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _focusOverlayCanvasGroup.gameObject.SetActive(false);
                _focusOverlayTween = null;
        });

        HideScratchCardFocusPanel();
    }

    public void PlayOwnedRogueCardEffect(int cardId)
    {
        for (int i = 0; i < _ownedRogueCardObjects.Count; i++)
        {
            GameObject cardObject = _ownedRogueCardObjects[i];
            RogueCardHoverView cardView = cardObject != null ? cardObject.GetComponent<RogueCardHoverView>() : null;
            if (cardView != null && cardView.CardId == cardId)
            {
                cardView.PlayEffectTriggeredAnimation();
                return;
            }
        }
    }

    public void ShowScratchCardFocusPanel(ScratchCardFocusPanelModel model)
    {
        EnsureFocusOverlay();
        EnsureFocusPanel();
        if (_focusPanelView == null)
        {
            return;
        }

        _focusPanelView.Bind(model);
        _focusPanelView.Show();
        _focusPanelView.transform.SetAsLastSibling();
    }

    public void HideScratchCardFocusPanel()
    {
        if (_focusPanelView != null)
        {
            _focusPanelView.Hide();
        }
    }

    public Vector2 GetRandomScratchCardAnchoredPosition(float margin = 120f)
    {
        if (_scratchCardRoot == null)
        {
            return Vector2.zero;
        }

        Rect rect = _scratchCardRoot.rect;
        float minX = rect.xMin + margin;
        float maxX = rect.xMax - margin;
        float minY = rect.yMin + rect.height * 0.2f;
        float maxY = rect.yMax - rect.height * 0.2f;

        if (minX > maxX)
        {
            float centerX = (rect.xMin + rect.xMax) * 0.5f;
            minX = centerX;
            maxX = centerX;
        }

        if (minY > maxY)
        {
            float centerY = (rect.yMin + rect.yMax) * 0.5f;
            minY = centerY;
            maxY = centerY;
        }

        return new Vector2(
            UnityEngine.Random.Range(minX, maxX),
            UnityEngine.Random.Range(minY, maxY));
    }

    public Vector2 GetScratchCardSpawnFromTop(float targetX, float extraHeight = 240f)
    {
        if (_scratchCardRoot == null)
        {
            return Vector2.zero;
        }

        Rect rect = _scratchCardRoot.rect;
        return new Vector2(targetX, rect.yMax + extraHeight);
    }

    public void MoveScratchCardToOverlayLayer(RectTransform scratchCard)
    {
        if (scratchCard == null || _focusOverlayRoot == null)
        {
            return;
        }

        scratchCard.SetParent(_focusOverlayRoot, false);
        scratchCard.SetAsLastSibling();
    }

    public void RestoreScratchCardToDefaultLayer(RectTransform scratchCard)
    {
        if (scratchCard == null || _scratchCardRoot == null)
        {
            return;
        }

        scratchCard.SetParent(_scratchCardRoot, false);
        scratchCard.SetAsLastSibling();
    }

    private void EnsureFocusOverlay()
    {
        if (_focusOverlayCanvasGroup != null || _focusOverlayRoot == null)
        {
            return;
        }

        GameObject overlayObject = new GameObject("ScratchCardFocusOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        overlayObject.transform.SetParent(_focusOverlayRoot, false);

        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.SetAsFirstSibling();

        _focusOverlayCanvasGroup = overlayObject.GetComponent<CanvasGroup>();
        _focusOverlayCanvasGroup.alpha = 0f;
        _focusOverlayCanvasGroup.blocksRaycasts = false;

        _focusOverlayImage = overlayObject.GetComponent<Image>();
        _focusOverlayImage.color = _focusOverlayColor;
        _focusOverlayImage.raycastTarget = true;

        overlayObject.SetActive(false);
    }

    private void EnsureScratchToolsListRoot()
    {
        if (_scratchToolsListRoot != null)
        {
            return;
        }

        Transform foundRoot = FindChildRecursive(transform, "ScratchToolsList");
        if (foundRoot != null)
        {
            _scratchToolsListRoot = foundRoot;
        }
    }

    private void EnsureCoinRainRoot()
    {
        if (_coinRainRoot != null)
        {
            RectTransform fullScreenParent = GetCoinRainRootParent();
            if (fullScreenParent != null && _coinRainRoot.parent != fullScreenParent)
            {
                _coinRainRoot.SetParent(fullScreenParent, false);
            }

            StretchCoinRainRoot();
            return;
        }

        RectTransform parent = GetCoinRainRootParent();
        if (parent == null)
        {
            return;
        }

        GameObject rootObject = new GameObject("CoinRainRoot", typeof(RectTransform), typeof(CanvasGroup));
        rootObject.transform.SetParent(parent, false);

        _coinRainRoot = rootObject.GetComponent<RectTransform>();
        StretchCoinRainRoot();

        CanvasGroup canvasGroup = rootObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private RectTransform GetCoinRainRootParent()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null
            ? canvas.transform as RectTransform
            : transform as RectTransform;
    }

    private void StretchCoinRainRoot()
    {
        if (_coinRainRoot == null)
        {
            return;
        }

        _coinRainRoot.anchorMin = Vector2.zero;
        _coinRainRoot.anchorMax = Vector2.one;
        _coinRainRoot.offsetMin = Vector2.zero;
        _coinRainRoot.offsetMax = Vector2.zero;
        _coinRainRoot.pivot = new Vector2(0.5f, 0.5f);
        _coinRainRoot.SetAsLastSibling();
    }

    private void EnsureCoinRainSprites()
    {
        if (_coinRainSprites != null && _coinRainSprites.Length > 0)
        {
            return;
        }

        var sprites = new List<Sprite>(4);
        AddCoinRainSprite(sprites, "Icons/PatternIcons/Diamond", 2);
        AddCoinRainSprite(sprites, "Icons/PatternIcons/Coin", 7);
        AddCoinRainSprite(sprites, "Icons/PatternIcons/Emerald", 2);
        AddCoinRainSprite(sprites, "Icons/PatternIcons/Star", 2);
        _coinRainSprites = sprites.ToArray();
    }

    private static void AddCoinRainSprite(List<Sprite> sprites, string resourcesPath, int weight)
    {
        Sprite sprite = AssetProvider.Load<Sprite>(resourcesPath);
        int repeatCount = Mathf.Max(1, weight);
        for (int i = 0; sprite != null && i < repeatCount; i++)
        {
            sprites.Add(sprite);
        }
    }

    private void PrewarmCoinRainPool()
    {
        if (_coinRainRoot == null)
        {
            return;
        }

        int targetCount = Mathf.Max(_coinRainIconCount * Mathf.Max(1, _coinRainWaveCount), _coinRainPoolPrewarmCount);
        while (_coinRainPool.Count < targetCount)
        {
            _coinRainPool.Add(CreateCoinRainIcon());
        }
    }

    private CoinRainIcon GetCoinRainIcon()
    {
        for (int i = 0; i < _coinRainPool.Count; i++)
        {
            CoinRainIcon icon = _coinRainPool[i];
            if (icon != null && !icon.IsActive)
            {
                icon.IsActive = true;
                _activeCoinRainIcons.Add(icon);
                return icon;
            }
        }

        CoinRainIcon newIcon = CreateCoinRainIcon();
        newIcon.IsActive = true;
        _coinRainPool.Add(newIcon);
        _activeCoinRainIcons.Add(newIcon);
        return newIcon;
    }

    private CoinRainIcon CreateCoinRainIcon()
    {
        GameObject iconObject = new GameObject("CoinRainIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        iconObject.transform.SetParent(_coinRainRoot, false);
        iconObject.SetActive(false);

        RectTransform rectTransform = iconObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Image image = iconObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;

        CanvasGroup canvasGroup = iconObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0f;

        return new CoinRainIcon(this, iconObject, rectTransform, image, canvasGroup);
    }

    private void RecycleCoinRainIcon(CoinRainIcon icon)
    {
        if (icon == null)
        {
            return;
        }

        icon.Sequence?.Kill(false);
        icon.Sequence = null;
        icon.IsActive = false;
        icon.CanvasGroup.alpha = 0f;
        icon.GameObject.SetActive(false);
        _activeCoinRainIcons.Remove(icon);
    }

    private void PlayCoinRainJackpotText(string value)
    {
        ClearCoinRainJackpotText();
        if (_coinRainRoot == null)
        {
            return;
        }

        _coinRainJackpotTextObject = AssetProvider.InstantiatePrefab("UI/FloatingText", _coinRainRoot);
        if (_coinRainJackpotTextObject == null)
        {
            return;
        }

        _coinRainJackpotTextObject.transform.SetAsLastSibling();

        RectTransform textRect = _coinRainJackpotTextObject.transform as RectTransform;
        if (textRect == null)
        {
            ClearCoinRainJackpotText();
            return;
        }

        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.localRotation = Quaternion.identity;
        textRect.localScale = Vector3.zero;

        CanvasGroup canvasGroup = _coinRainJackpotTextObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = _coinRainJackpotTextObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        TextMeshProUGUI text = _coinRainJackpotTextObject.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = _coinRainJackpotTextObject.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (text == null)
        {
            ClearCoinRainJackpotText();
            return;
        }

        AssetProvider.ApplyDefaultTmpFont(text);
        text.text = string.IsNullOrWhiteSpace(value) ? "头奖" : value;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = Mathf.Max(1f, _coinRainJackpotFontSize);
        text.color = _coinRainJackpotTextColor;
        text.raycastTarget = false;

        float targetScale = Mathf.Max(0.01f, _coinRainJackpotTargetScale);
        float popDuration = Mathf.Max(0.01f, _coinRainJackpotPopDuration);
        float shrinkDuration = Mathf.Max(0.01f, _coinRainJackpotShrinkDuration);
        float holdDuration = Mathf.Max(0f, _coinRainJackpotHoldDuration);
        _coinRainJackpotTextSequence = DOTween.Sequence().SetUpdate(true);
        _coinRainJackpotTextSequence
            .Append(textRect
                .DOScale(targetScale, popDuration)
                .SetEase(Ease.OutCubic))
            .AppendInterval(holdDuration)
            .Append(textRect
                .DOScale(0f, shrinkDuration)
                .SetEase(Ease.InCubic))
            .OnComplete(CompleteCoinRainJackpotText);
    }

    private void CompleteCoinRainJackpotText()
    {
        _coinRainJackpotTextSequence = null;

        if (_coinRainJackpotTextObject != null)
        {
            Destroy(_coinRainJackpotTextObject);
            _coinRainJackpotTextObject = null;
        }
    }

    private void ClearCoinRainJackpotText()
    {
        _coinRainJackpotTextSequence?.Kill(false);
        _coinRainJackpotTextSequence = null;

        if (_coinRainJackpotTextObject != null)
        {
            Destroy(_coinRainJackpotTextObject);
            _coinRainJackpotTextObject = null;
        }
    }

    private void ClearCoinRainPool()
    {
        for (int i = 0; i < _coinRainPool.Count; i++)
        {
            CoinRainIcon icon = _coinRainPool[i];
            if (icon == null)
            {
                continue;
            }

            icon.Sequence?.Kill(false);
            if (icon.GameObject != null)
            {
                Destroy(icon.GameObject);
            }
        }

        _activeCoinRainIcons.Clear();
        _coinRainPool.Clear();
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child != null && string.Equals(child.name?.Trim(), childName, StringComparison.Ordinal))
            {
                return child;
            }

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void EnsureRogueChoiceOverlay()
    {
        if (_rogueChoiceOverlayObject != null)
        {
            return;
        }

        RectTransform parent = _rogueChoiceOverlayRoot != null ? _rogueChoiceOverlayRoot : transform as RectTransform;
        if (parent == null)
        {
            return;
        }

        _rogueChoiceOverlayObject = new GameObject("RogueCardChoiceOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        _rogueChoiceOverlayObject.transform.SetParent(parent, false);

        RectTransform overlayRect = _rogueChoiceOverlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.SetAsLastSibling();

        _rogueChoiceOverlayImage = _rogueChoiceOverlayObject.GetComponent<Image>();
        _rogueChoiceOverlayImage.color = new Color(0f, 0f, 0f, 0.68f);
        _rogueChoiceOverlayImage.raycastTarget = true;

        EventTrigger clickTrigger = _rogueChoiceOverlayObject.AddComponent<EventTrigger>();
        EventTrigger.Entry clickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        clickEntry.callback.AddListener(data => HandleRogueExchangeOverlayClick(data as PointerEventData));
        clickTrigger.triggers.Add(clickEntry);

        GameObject contentObject = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(_rogueChoiceOverlayObject.transform, false);

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup layout = contentObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = _rogueRewardChoiceSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject promptObject = new GameObject("ExchangePrompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        promptObject.transform.SetParent(_rogueChoiceOverlayObject.transform, false);
        RectTransform promptRect = promptObject.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        promptRect.anchoredPosition = new Vector2(0f, -260f);
        promptRect.sizeDelta = new Vector2(720f, 64f);

        _rogueExchangePromptText = promptObject.GetComponent<TextMeshProUGUI>();
        AssetProvider.ApplyDefaultTmpFont(_rogueExchangePromptText);
        _rogueExchangePromptText.alignment = TextAlignmentOptions.Center;
        _rogueExchangePromptText.fontSize = 34f;
        _rogueExchangePromptText.color = Color.white;
        _rogueExchangePromptText.text = "选择已有的一张卡牌进行交换";
        _rogueExchangePromptText.raycastTarget = false;
        _rogueExchangePromptText.gameObject.SetActive(false);

        _rogueChoiceContentRoot = contentObject.transform;
        _rogueChoiceOverlayObject.SetActive(false);
    }

    private void EnsureRogueOwnedCardsRoot()
    {
        if (_rogueOwnedCardsRoot != null)
        {
            return;
        }

        RectTransform parent = transform as RectTransform;
        if (parent == null)
        {
            return;
        }

        GameObject rootObject = new GameObject("RogueOwnedCardsRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rootObject.transform.SetParent(parent, false);

        _rogueOwnedCardsRoot = rootObject.GetComponent<RectTransform>();
        _rogueOwnedCardsRoot.anchorMin = new Vector2(0f, 0f);
        _rogueOwnedCardsRoot.anchorMax = new Vector2(1f, 0f);
        _rogueOwnedCardsRoot.pivot = new Vector2(0.5f, 0f);
        _rogueOwnedCardsRoot.offsetMin = new Vector2(24f, 18f);
        _rogueOwnedCardsRoot.offsetMax = new Vector2(-24f, 94f);

        HorizontalLayoutGroup layout = rootObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void EnsureScratchToolChoiceOverlay()
    {
        if (_scratchToolChoiceOverlayObject != null)
        {
            return;
        }

        RectTransform parent = _rogueChoiceOverlayRoot != null ? _rogueChoiceOverlayRoot : transform as RectTransform;
        if (parent == null)
        {
            return;
        }

        _scratchToolChoiceOverlayObject = new GameObject("ScratchToolChoiceOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        _scratchToolChoiceOverlayObject.transform.SetParent(parent, false);

        RectTransform overlayRect = _scratchToolChoiceOverlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.SetAsLastSibling();

        Image overlayImage = _scratchToolChoiceOverlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.68f);
        overlayImage.raycastTarget = true;

        GameObject contentObject = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(_scratchToolChoiceOverlayObject.transform, false);

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup layout = contentObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = _scratchToolRewardChoiceSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _scratchToolChoiceContentRoot = contentObject.transform;
        _scratchToolChoiceOverlayObject.SetActive(false);
    }

    private void EnsureScratchCardChoiceOverlay()
    {
        if (_scratchCardChoiceOverlayObject != null)
        {
            return;
        }

        RectTransform parent = _rogueChoiceOverlayRoot != null ? _rogueChoiceOverlayRoot : transform as RectTransform;
        if (parent == null)
        {
            return;
        }

        _scratchCardChoiceOverlayObject = new GameObject("ScratchCardChoiceOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        _scratchCardChoiceOverlayObject.transform.SetParent(parent, false);

        RectTransform overlayRect = _scratchCardChoiceOverlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.SetAsLastSibling();

        Image overlayImage = _scratchCardChoiceOverlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.68f);
        overlayImage.raycastTarget = true;

        GameObject contentObject = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(_scratchCardChoiceOverlayObject.transform, false);

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup layout = contentObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = _scratchCardRewardChoiceSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _scratchCardChoiceContentRoot = contentObject.transform;
        _scratchCardChoiceOverlayObject.SetActive(false);
    }

    private void EnsureWinPanel()
    {
        if (_winPanelRoot == null)
        {
            Transform foundPanel = FindChildRecursive(transform, "WinPanel");
            if (foundPanel != null)
            {
                _winPanelRoot = foundPanel as RectTransform;
            }
        }

        if (_winPanelRoot == null)
        {
            return;
        }

        if (_rogueCardRewardButton == null)
        {
            Transform foundRewardButton = FindChildRecursive(_winPanelRoot, "RogueCardRewardBtn");
            _rogueCardRewardButton = foundRewardButton != null ? foundRewardButton.GetComponent<Button>() : null;
        }

        if (_scratchToolRewardButton == null)
        {
            Transform foundScratchToolRewardButton = FindChildRecursive(_winPanelRoot, "ScratchToolRewardBtn");
            _scratchToolRewardButton = foundScratchToolRewardButton != null ? foundScratchToolRewardButton.GetComponent<Button>() : null;
        }

        if (_scratchCardRewardButton == null)
        {
            Transform foundScratchCardRewardButton = FindChildRecursive(_winPanelRoot, "ScratchCardRewardBtn");
            _scratchCardRewardButton = foundScratchCardRewardButton != null ? foundScratchCardRewardButton.GetComponent<Button>() : null;
        }

        if (_continueButton == null)
        {
            Transform foundContinueButton = FindChildRecursive(_winPanelRoot, "ContinueBtn");
            _continueButton = foundContinueButton != null ? foundContinueButton.GetComponent<Button>() : null;
        }

        if (_rogueCardRewardButton == null)
        {
            _rogueCardRewardButton = CreateWinPanelButton("RogueCardRewardBtn", "选择奖励", new Vector2(0f, 180f), new Vector2(347f, 108f));
        }

        if (_scratchToolRewardButton == null)
        {
            _scratchToolRewardButton = CreateWinPanelButton("ScratchToolRewardBtn", "选择刮具", new Vector2(0f, 40f), new Vector2(347f, 108f));
        }

        if (_continueButton == null)
        {
            _continueButton = CreateWinPanelButton("ContinueBtn", "下一关", new Vector2(0f, -373f), new Vector2(346f, 153f));
        }

        _rogueCardRewardButton.onClick.RemoveListener(HandleRogueCardRewardButtonClicked);
        _rogueCardRewardButton.onClick.AddListener(HandleRogueCardRewardButtonClicked);
        _scratchToolRewardButton.onClick.RemoveListener(HandleScratchToolRewardButtonClicked);
        _scratchToolRewardButton.onClick.AddListener(HandleScratchToolRewardButtonClicked);
        if (_scratchCardRewardButton != null)
        {
            _scratchCardRewardButton.onClick.RemoveListener(HandleScratchCardRewardButtonClicked);
            _scratchCardRewardButton.onClick.AddListener(HandleScratchCardRewardButtonClicked);
        }
        else
        {
            Debug.LogWarning("[MainGamePanel] WinPanel 下未找到 ScratchCardRewardBtn，刮刮卡卡包奖励按钮不会响应。");
        }

        _continueButton.onClick.RemoveListener(HandleWinContinueButtonClicked);
        _continueButton.onClick.AddListener(HandleWinContinueButtonClicked);
    }

    private void EnsureNewLevelPanel()
    {
        if (_newLevelPanelRoot == null)
        {
            Transform foundPanel = FindChildRecursive(transform, "NewLevelPanel");
            _newLevelPanelRoot = foundPanel as RectTransform;
        }

        if (_newLevelPanelRoot == null)
        {
            return;
        }

        if (_newLevelTargetText == null)
        {
            Transform foundTarget = FindChildRecursive(_newLevelPanelRoot, "Target");
            _newLevelTargetText = foundTarget != null ? foundTarget.GetComponent<TextMeshProUGUI>() : null;
        }

        if (_newLevelInfoText == null)
        {
            Transform foundLevelInfo = FindChildRecursive(_newLevelPanelRoot, "LevelInfo");
            _newLevelInfoText = foundLevelInfo != null ? foundLevelInfo.GetComponent<TextMeshProUGUI>() : null;
        }

        if (_newLevelStartButton == null)
        {
            Transform foundStartButton = FindChildRecursive(_newLevelPanelRoot, "StartBtn");
            _newLevelStartButton = foundStartButton != null ? foundStartButton.GetComponent<Button>() : null;
        }

        if (_newLevelStartButton != null)
        {
            _newLevelStartButton.onClick.RemoveListener(HandleNewLevelStartButtonClicked);
            _newLevelStartButton.onClick.AddListener(HandleNewLevelStartButtonClicked);
        }
        else
        {
            Debug.LogWarning("[MainGamePanel] NewLevelPanel 下未找到 StartBtn，下一关开始按钮不会响应。");
        }
    }

    private static string GetLevelDisplayName(LevelConfig levelConfig)
    {
        if (levelConfig == null)
        {
            return "-";
        }

        return !string.IsNullOrWhiteSpace(levelConfig.Name)
            ? levelConfig.Name
            : $"第{levelConfig.Id}关";
    }

    private Button CreateWinPanelButton(string objectName, string labelText, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (_winPanelRoot == null)
        {
            return null;
        }

        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(_winPanelRoot, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = sizeDelta;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.18f, 0.18f, 0.85f);

        GameObject textObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        AssetProvider.ApplyDefaultTmpFont(label);
        label.text = labelText;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 50f;
        label.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        return button;
    }

    private static void SetWinPanelButtonState(Button button, bool visible, bool interactable)
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(visible);
        button.interactable = interactable;
    }

    private void HandleRogueCardRewardButtonClicked()
    {
        OnRogueRewardRequested?.Invoke();
    }

    private void HandleScratchToolRewardButtonClicked()
    {
        OnScratchToolRewardRequested?.Invoke();
    }

    private void HandleScratchCardRewardButtonClicked()
    {
        OnScratchCardRewardRequested?.Invoke();
    }

    private void HandleWinContinueButtonClicked()
    {
        OnWinContinueRequested?.Invoke();
    }

    private void HandleNewLevelStartButtonClicked()
    {
        OnNewLevelStartRequested?.Invoke();
    }

    private GameObject CreateScratchCardChoice(ScratchCardTypeConfig cardTypeConfig, Transform parent)
    {
        if (cardTypeConfig == null)
        {
            return null;
        }

        GameObject choiceRoot = CreateScratchCardChoiceRoot(parent, cardTypeConfig.Id);
        GameObject scratchCardPrefab = AssetProvider.LoadPrefab(cardTypeConfig.PrefabPath);
        GameObject cardObject = scratchCardPrefab != null
            ? Instantiate(scratchCardPrefab, choiceRoot.transform, false)
            : CreateScratchCardChoiceFallback(choiceRoot.transform);
        cardObject.name = $"ScratchCardContent_{cardTypeConfig.Id}";
        cardObject.transform.localScale = Vector3.one;
        ConfigureScratchCardChoicePreview(choiceRoot, cardObject, cardTypeConfig);
        ConfigureScratchCardChoiceDescription(cardObject, cardTypeConfig);
        ConfigureScratchCardChoiceHover(choiceRoot, cardObject);

        Button button = choiceRoot.GetComponent<Button>();
        if (button != null)
        {
            int selectedCardTypeId = cardTypeConfig.Id;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HandleScratchCardChoiceClicked(selectedCardTypeId));
        }

        return choiceRoot;
    }

    private GameObject CreateScratchCardChoiceRoot(Transform parent, int cardTypeId)
    {
        GameObject rootObject = new GameObject($"ScratchCard_{cardTypeId}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        rootObject.transform.SetParent(parent, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = Vector2.zero;

        Image image = rootObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        Button button = rootObject.GetComponent<Button>();
        button.targetGraphic = image;
        PreserveButtonDisabledVisual(button);

        float scale = Mathf.Max(0.01f, _scratchCardRewardChoiceScale);
        rootObject.transform.localScale = Vector3.one * scale;
        return rootObject;
    }

    private void ConfigureScratchCardChoicePreview(GameObject choiceRoot, GameObject cardObject, ScratchCardTypeConfig cardTypeConfig)
    {
        if (choiceRoot == null || cardObject == null || cardTypeConfig == null)
        {
            return;
        }

        RectTransform rootRect = choiceRoot.transform as RectTransform;
        RectTransform cardRect = cardObject.transform as RectTransform;
        if (cardRect != null)
        {
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;

            if (rootRect != null)
            {
                Vector2 previewSize = cardRect.sizeDelta;
                if (previewSize.x <= 0f || previewSize.y <= 0f)
                {
                    previewSize = cardRect.rect.size;
                }

                if (previewSize.x <= 0f || previewSize.y <= 0f)
                {
                    previewSize = new Vector2(260f, 320f);
                }

                rootRect.sizeDelta = previewSize;
            }
        }

        CanvasGroup[] canvasGroups = cardObject.GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            if (canvasGroups[i] == null)
            {
                continue;
            }

            canvasGroups[i].alpha = 1f;
            canvasGroups[i].interactable = true;
            canvasGroups[i].blocksRaycasts = false;
        }

        ScratchCardController controller = cardObject.GetComponent<ScratchCardController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        ScratchCardView scratchCardView = cardObject.GetComponent<ScratchCardView>();
        if (scratchCardView != null)
        {
            ScratchAreaTemplateConfig areaTemplateConfig = ScratchCardDefaultsProvider.GetAreaTemplate(cardTypeConfig.AreaTemplateId);
            IReadOnlyList<ScratchCellModel> cells = ScratchCardGenerator.GenerateCells(cardTypeConfig, areaTemplateConfig);
            scratchCardView.BindCardData(cells);
            scratchCardView.SetupInitialVisual();
            scratchCardView.SetFocused(false, true);
            scratchCardView.enabled = false;
            return;
        }

        SetScratchCardChoiceFallbackText(cardObject.transform, cardTypeConfig);
    }

    private void ConfigureScratchCardChoiceDescription(GameObject cardObject, ScratchCardTypeConfig cardTypeConfig)
    {
        Transform description = FindScratchCardNormalDescription(cardObject);
        if (description == null || cardTypeConfig == null)
        {
            return;
        }

        SetScratchCardChoiceDescriptionText(description, "PatterCount", $"可刮个数：{BuildScratchCardPatternCountText(cardTypeConfig)}");
        SetScratchCardChoiceDescriptionText(description, "WinRule", $"中奖规则：{BuildScratchCardWinRuleText(cardTypeConfig)}");
        SetScratchCardChoiceDescriptionText(description, "SpecialEffect", $"特殊效果：{BuildScratchCardSpecialEffectText(cardTypeConfig)}");
        description.gameObject.SetActive(false);
    }

    private static Transform FindScratchCardNormalDescription(GameObject cardObject)
    {
        if (cardObject == null)
        {
            return null;
        }

        Transform normal = cardObject.transform.Find("Normal");
        return normal != null
            ? FindChildRecursive(normal, "Description")
            : FindChildRecursive(cardObject.transform, "Description");
    }

    private void SetScratchCardChoiceDescriptionText(Transform descriptionRoot, string childName, string value)
    {
        if (descriptionRoot == null)
        {
            return;
        }

        Transform child = FindChildRecursive(descriptionRoot, childName);
        TextMeshProUGUI text = child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        if (text == null)
        {
            return;
        }

        AssetProvider.ApplyDefaultTmpFont(text);
        text.text = string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string BuildScratchCardPatternCountText(ScratchCardTypeConfig cardTypeConfig)
    {
        ScratchAreaTemplateConfig areaTemplate = cardTypeConfig != null
            ? ScratchCardDefaultsProvider.GetAreaTemplate(cardTypeConfig.AreaTemplateId)
            : null;
        if (areaTemplate == null)
        {
            return "-";
        }

        return $"{areaTemplate.Height}×{areaTemplate.Width}";
    }

    private static string BuildScratchCardWinRuleText(ScratchCardTypeConfig cardTypeConfig)
    {
        string description = cardTypeConfig != null ? cardTypeConfig.WinDescription : null;
        if (!string.IsNullOrWhiteSpace(description))
        {
            string[] parts = description.Split('；', ';');
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                return parts[0].Trim();
            }
        }

        ScratchCardWinRuleConfig rule = cardTypeConfig != null && cardTypeConfig.WinRules != null && cardTypeConfig.WinRules.Count > 0
            ? cardTypeConfig.WinRules[0]
            : null;
        if (rule == null)
        {
            return "-";
        }

        switch (rule.RuleType)
        {
            case ScratchCardWinRuleType.SpecificPatternCount:
                return $"刮出{rule.RequiredCount}个{GetScratchPatternDisplayName(rule.TargetPatternId)}获奖";
            case ScratchCardWinRuleType.AllFruitPatterns:
                return "全部图案为水果图案即中奖";
            case ScratchCardWinRuleType.AllMineralPatterns:
                return "全部图案为矿物图案即中奖";
            case ScratchCardWinRuleType.AllPlanetPatterns:
                return "全部图案为星球图案即中奖";
            case ScratchCardWinRuleType.ScoreEveryRevealedPattern:
                return "刮开即中奖";
            case ScratchCardWinRuleType.AllGoodFaceJokerPatterns:
                return "全部图案为好脸小丑即中奖";
            case ScratchCardWinRuleType.GameOver:
                return "不会获得金币";
            case ScratchCardWinRuleType.SamePatternCount:
                return rule.RequiredCount <= 1 ? "刮开即中奖" : $"刮开{rule.RequiredCount}个相同图案获奖";
            case ScratchCardWinRuleType.None:
            default:
                return "-";
        }
    }

    private static string BuildScratchCardSpecialEffectText(ScratchCardTypeConfig cardTypeConfig)
    {
        string description = cardTypeConfig != null ? cardTypeConfig.SpecialDescription : null;
        if (!string.IsNullOrWhiteSpace(description))
        {
            return description.Trim();
        }

        if (cardTypeConfig == null)
        {
            return "-";
        }

        if (cardTypeConfig.PatternPoolId == ScratchCardDefaultsProvider.FruitPatternPoolId)
        {
            return "初始只有水果图案";
        }

        if (cardTypeConfig.ExtraEffects != null && cardTypeConfig.ExtraEffects.Count > 0)
        {
            List<string> descriptions = new List<string>();
            for (int i = 0; i < cardTypeConfig.ExtraEffects.Count; i++)
            {
                string effectText = BuildScratchCardExtraEffectText(cardTypeConfig.ExtraEffects[i]);
                if (!string.IsNullOrWhiteSpace(effectText))
                {
                    descriptions.Add(effectText);
                }
            }

            if (descriptions.Count > 0)
            {
                return string.Join("；", descriptions);
            }
        }

        ScratchCardWinRuleConfig rule = cardTypeConfig.WinRules != null && cardTypeConfig.WinRules.Count > 0
            ? cardTypeConfig.WinRules[0]
            : null;
        if (rule != null && rule.ScoreMultiplier > 1d)
        {
            return $"奖励倍率×{rule.ScoreMultiplier:0.##}";
        }

        return "-";
    }

    private static string BuildScratchCardExtraEffectText(ScratchCardExtraEffectConfig effectConfig)
    {
        if (effectConfig == null || effectConfig.EffectType == ScratchCardExtraEffectType.None)
        {
            return null;
        }

        switch (effectConfig.EffectType)
        {
            case ScratchCardExtraEffectType.MultiplyPatternWeight:
                return BuildPatternWeightEffectText(effectConfig);
            case ScratchCardExtraEffectType.MultiplyCellScoreMultiplier:
                return "特定位置图案获得额外效果";
            case ScratchCardExtraEffectType.AddRewardMultiplierOnSettlement:
                return $"每次结算额外增加{effectConfig.Value:0.##}倍率";
            case ScratchCardExtraEffectType.RestrictPatternType:
                return $"只出现{GetPatternTypeDisplayName(effectConfig.TargetPatternType)}图案";
            case ScratchCardExtraEffectType.None:
            default:
                return null;
        }
    }

    private static string GetPatternTypeDisplayName(string patternType)
    {
        if (string.IsNullOrWhiteSpace(patternType))
        {
            return "指定类型";
        }

        switch (patternType.Trim())
        {
            case "Fruit":
                return "水果";
            case "Mineral":
                return "矿物";
            case "Planet":
                return "星球";
            case "Joker":
                return "小丑";
            case "Number":
                return "数字";
            case "Multiplier":
                return "倍率";
            default:
                return patternType.Trim();
        }
    }

    private static string BuildPatternWeightEffectText(ScratchCardExtraEffectConfig effectConfig)
    {
        if (effectConfig.TargetPatternIds != null &&
            effectConfig.TargetPatternIds.Count == 4 &&
            effectConfig.TargetPatternIds.Contains(5) &&
            effectConfig.TargetPatternIds.Contains(6) &&
            effectConfig.TargetPatternIds.Contains(8) &&
            effectConfig.TargetPatternIds.Contains(9))
        {
            return "金属图案概率翻倍";
        }

        if (effectConfig.TargetPatternIds == null || effectConfig.TargetPatternIds.Count == 0)
        {
            return "图案概率提升";
        }

        if (effectConfig.TargetPatternIds.Count == 1)
        {
            return $"{GetScratchPatternDisplayName(effectConfig.TargetPatternIds[0])}图案概率翻倍";
        }

        return "指定图案概率翻倍";
    }

    private static string GetScratchPatternDisplayName(int patternId)
    {
        ScratchPatternConfig patternConfig = ScratchPatternDefaultProvider.GetById(patternId);
        return patternConfig != null && !string.IsNullOrWhiteSpace(patternConfig.Name)
            ? patternConfig.Name
            : $"图案{patternId}";
    }

    private void ConfigureScratchCardChoiceHover(GameObject choiceRoot, GameObject cardObject)
    {
        if (choiceRoot == null)
        {
            return;
        }

        ScratchCardRewardChoiceHoverView hoverView = choiceRoot.GetComponent<ScratchCardRewardChoiceHoverView>();
        if (hoverView == null)
        {
            hoverView = choiceRoot.AddComponent<ScratchCardRewardChoiceHoverView>();
        }

        Transform outline = cardObject != null ? FindChildRecursive(cardObject.transform, "Outline") : null;
        Transform description = FindScratchCardNormalDescription(cardObject);
        hoverView.Configure(
            _scratchCardRewardChoiceHoverScale,
            _scratchCardRewardChoiceHoverOffsetY,
            _scratchCardRewardChoiceHoverDuration,
            outline != null ? outline.gameObject : null,
            description != null ? description.gameObject : null);
        hoverView.CaptureCurrentTransformAsRestingState();
    }

    private void HandleScratchCardChoiceClicked(int cardTypeId)
    {
        OnScratchCardRewardSelected?.Invoke(cardTypeId);
    }

    private void PlayScratchCardChoiceShowAnimation()
    {
        _scratchCardChoiceShowCoroutine = StartRewardChoiceShowAnimation(
            _scratchCardChoiceContentRoot,
            _scratchCardChoiceObjects,
            sequence => _scratchCardChoiceShowSequence = sequence,
            () =>
            {
                _scratchCardChoiceShowSequence = null;
                _scratchCardChoiceShowCoroutine = null;
                CaptureScratchCardChoiceHoverRestingStates();
                SetScratchCardChoiceHoverEnabled(true);
                RefreshScratchCardChoiceHoverState();
            });
    }

    private Coroutine StartRewardChoiceShowAnimation(
        Transform contentRoot,
        IReadOnlyList<GameObject> choiceObjects,
        Action<Sequence> onSequenceCreated,
        Action onComplete,
        bool setButtonsInteractableDuringAnimation = true)
    {
        if (contentRoot == null || choiceObjects == null || choiceObjects.Count <= 0)
        {
            onComplete?.Invoke();
            return null;
        }

        return StartCoroutine(PlayRewardChoiceShowAnimationRoutine(
            contentRoot,
            choiceObjects,
            onSequenceCreated,
            onComplete,
            setButtonsInteractableDuringAnimation));
    }

    private IEnumerator PlayRewardChoiceShowAnimationRoutine(
        Transform contentRoot,
        IReadOnlyList<GameObject> choiceObjects,
        Action<Sequence> onSequenceCreated,
        Action onComplete,
        bool setButtonsInteractableDuringAnimation)
    {
        RectTransform contentRect = contentRoot as RectTransform;
        if (contentRect == null || choiceObjects == null || choiceObjects.Count <= 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        RestoreRewardChoiceLayout(contentRoot);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        int count = choiceObjects.Count;
        Vector3[] targetScales = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            RectTransform choiceRect = choiceObjects[i] != null
                ? choiceObjects[i].transform as RectTransform
                : null;
            if (choiceRect == null)
            {
                continue;
            }

            choiceRect.DOKill(false);
            targetScales[i] = choiceRect.localScale;
            choiceRect.localScale = Vector3.zero;
        }

        if (setButtonsInteractableDuringAnimation)
        {
            SetRewardChoiceButtonsInteractable(choiceObjects, false);
        }

        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        Sequence sequence = PlayRewardChoiceShowAnimation(
            contentRoot,
            choiceObjects,
            targetScales,
            onComplete,
            setButtonsInteractableDuringAnimation);
        onSequenceCreated?.Invoke(sequence);
    }

    private Sequence PlayRewardChoiceShowAnimation(
        Transform contentRoot,
        IReadOnlyList<GameObject> choiceObjects,
        Vector3[] targetScaleOverrides,
        Action onComplete,
        bool setButtonsInteractableDuringAnimation = true)
    {
        RectTransform contentRect = contentRoot as RectTransform;
        if (contentRect == null || choiceObjects == null || choiceObjects.Count <= 0)
        {
            onComplete?.Invoke();
            return null;
        }

        RestoreRewardChoiceLayout(contentRoot);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        int count = choiceObjects.Count;
        Vector2[] targetPositions = new Vector2[count];
        Vector3[] targetScales = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            RectTransform choiceRect = choiceObjects[i] != null
                ? choiceObjects[i].transform as RectTransform
                : null;
            if (choiceRect == null)
            {
                continue;
            }

            choiceRect.DOKill(false);
            targetPositions[i] = choiceRect.anchoredPosition;
            targetScales[i] = targetScaleOverrides != null && i < targetScaleOverrides.Length
                ? targetScaleOverrides[i]
                : choiceRect.localScale;
        }

        SetRewardChoiceLayoutEnabled(contentRoot, false);
        if (setButtonsInteractableDuringAnimation)
        {
            SetRewardChoiceButtonsInteractable(choiceObjects, false);
        }

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        float duration = Mathf.Max(0.01f, _scratchCardRewardChoiceShowDuration);
        float stagger = Mathf.Max(0f, _scratchCardRewardChoiceShowStagger);
        for (int i = 0; i < count; i++)
        {
            RectTransform choiceRect = choiceObjects[i] != null
                ? choiceObjects[i].transform as RectTransform
                : null;
            if (choiceRect == null)
            {
                continue;
            }

            choiceRect.anchoredPosition = Vector2.zero;
            choiceRect.localScale = Vector3.zero;
            float insertTime = i * stagger;
            sequence.Insert(insertTime, choiceRect.DOAnchorPos(targetPositions[i], duration).SetEase(Ease.OutCubic));
            sequence.Insert(insertTime, choiceRect.DOScale(targetScales[i], duration).SetEase(Ease.OutCubic));
        }

        sequence.OnComplete(() =>
        {
            for (int i = 0; i < count; i++)
            {
                RectTransform choiceRect = choiceObjects[i] != null
                    ? choiceObjects[i].transform as RectTransform
                    : null;
                if (choiceRect == null)
                {
                    continue;
                }

                choiceRect.anchoredPosition = targetPositions[i];
                choiceRect.localScale = targetScales[i];
            }

            CaptureRewardChoiceRestingStates(choiceObjects);
            if (setButtonsInteractableDuringAnimation)
            {
                SetRewardChoiceButtonsInteractable(choiceObjects, true);
            }

            onComplete?.Invoke();
        });

        return sequence;
    }

    private void RestoreRewardChoiceLayout(Transform contentRoot)
    {
        SetRewardChoiceLayoutEnabled(contentRoot, true);
    }

    private static void CaptureRewardChoiceRestingStates(IReadOnlyList<GameObject> choiceObjects)
    {
        if (choiceObjects == null)
        {
            return;
        }

        for (int i = 0; i < choiceObjects.Count; i++)
        {
            RogueCardHoverView[] hoverViews = choiceObjects[i] != null
                ? choiceObjects[i].GetComponentsInChildren<RogueCardHoverView>(true)
                : null;
            if (hoverViews == null)
            {
                continue;
            }

            for (int hoverIndex = 0; hoverIndex < hoverViews.Length; hoverIndex++)
            {
                if (hoverViews[hoverIndex] != null)
                {
                    hoverViews[hoverIndex].CaptureCurrentTransformAsRestingState();
                }
            }
        }
    }

    private void StopRewardChoiceShowAnimation(ref Sequence sequence, ref Coroutine coroutine, Transform contentRoot)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (sequence != null)
        {
            sequence.Kill(false);
            sequence = null;
        }

        RestoreRewardChoiceLayout(contentRoot);
    }

    private void SetRewardChoiceLayoutEnabled(Transform contentRoot, bool enabled)
    {
        if (contentRoot == null)
        {
            return;
        }

        HorizontalLayoutGroup layout = contentRoot.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.enabled = enabled;
        }

        ContentSizeFitter fitter = contentRoot.GetComponent<ContentSizeFitter>();
        if (fitter != null)
        {
            fitter.enabled = enabled;
        }
    }

    private static void SetRewardChoiceButtonsInteractable(IReadOnlyList<GameObject> choiceObjects, bool interactable)
    {
        if (choiceObjects == null)
        {
            return;
        }

        for (int i = 0; i < choiceObjects.Count; i++)
        {
            Button[] buttons = choiceObjects[i] != null
                ? choiceObjects[i].GetComponentsInChildren<Button>(true)
                : null;
            if (buttons == null)
            {
                continue;
            }

            for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
            {
                if (buttons[buttonIndex] != null)
                {
                    buttons[buttonIndex].interactable = interactable;
                }
            }
        }
    }

    private static void SetRewardChoiceRaycasts(IReadOnlyList<GameObject> choiceObjects, bool enabled)
    {
        if (choiceObjects == null)
        {
            return;
        }

        for (int i = 0; i < choiceObjects.Count; i++)
        {
            GameObject choiceObject = choiceObjects[i];
            if (choiceObject == null)
            {
                continue;
            }

            CanvasGroup canvasGroup = choiceObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = choiceObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = enabled;
        }
    }

    private void SetRogueChoiceHoverEnabled(bool enabled)
    {
        SetRogueChoiceHoverEnabled(_rogueChoiceObjects, enabled);
    }

    private void RefreshRogueChoiceHoverState()
    {
        Camera eventCamera = GetUiEventCamera();
        Vector2 screenPosition = Input.mousePosition;
        for (int i = 0; i < _rogueChoiceObjects.Count; i++)
        {
            RogueCardHoverView[] hoverViews = _rogueChoiceObjects[i] != null
                ? _rogueChoiceObjects[i].GetComponentsInChildren<RogueCardHoverView>(true)
                : null;
            if (hoverViews == null)
            {
                continue;
            }

            for (int hoverIndex = 0; hoverIndex < hoverViews.Length; hoverIndex++)
            {
                if (hoverViews[hoverIndex] != null)
                {
                    hoverViews[hoverIndex].RefreshHoverState(screenPosition, eventCamera);
                }
            }
        }
    }

    private Camera GetUiEventCamera()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
    }

    private void SetScratchCardChoiceHoverEnabled(bool enabled)
    {
        if (_scratchCardChoiceObjects == null)
        {
            return;
        }

        for (int i = 0; i < _scratchCardChoiceObjects.Count; i++)
        {
            ScratchCardRewardChoiceHoverView hoverView = _scratchCardChoiceObjects[i] != null
                ? _scratchCardChoiceObjects[i].GetComponent<ScratchCardRewardChoiceHoverView>()
                : null;
            if (hoverView != null)
            {
                hoverView.SetInteractionEnabled(enabled);
            }
        }
    }

    private void CaptureScratchCardChoiceHoverRestingStates()
    {
        if (_scratchCardChoiceObjects == null)
        {
            return;
        }

        for (int i = 0; i < _scratchCardChoiceObjects.Count; i++)
        {
            ScratchCardRewardChoiceHoverView hoverView = _scratchCardChoiceObjects[i] != null
                ? _scratchCardChoiceObjects[i].GetComponent<ScratchCardRewardChoiceHoverView>()
                : null;
            if (hoverView != null)
            {
                hoverView.CaptureCurrentTransformAsRestingState();
            }
        }
    }

    private void RefreshScratchCardChoiceHoverState()
    {
        if (_scratchCardChoiceObjects == null)
        {
            return;
        }

        Camera eventCamera = GetUiEventCamera();
        Vector2 screenPosition = Input.mousePosition;
        for (int i = 0; i < _scratchCardChoiceObjects.Count; i++)
        {
            ScratchCardRewardChoiceHoverView hoverView = _scratchCardChoiceObjects[i] != null
                ? _scratchCardChoiceObjects[i].GetComponent<ScratchCardRewardChoiceHoverView>()
                : null;
            if (hoverView != null)
            {
                hoverView.RefreshHoverState(screenPosition, eventCamera);
            }
        }
    }

    private static void SetRogueChoiceHoverEnabled(IReadOnlyList<GameObject> choiceObjects, bool enabled)
    {
        if (choiceObjects == null)
        {
            return;
        }

        for (int i = 0; i < choiceObjects.Count; i++)
        {
            RogueCardHoverView[] hoverViews = choiceObjects[i] != null
                ? choiceObjects[i].GetComponentsInChildren<RogueCardHoverView>(true)
                : null;
            if (hoverViews == null)
            {
                continue;
            }

            for (int hoverIndex = 0; hoverIndex < hoverViews.Length; hoverIndex++)
            {
                if (hoverViews[hoverIndex] != null)
                {
                    hoverViews[hoverIndex].SetPointerInteractionEnabled(enabled);
                }
            }
        }
    }

    private GameObject CreateScratchCardChoiceFallback(Transform parent)
    {
        GameObject cardObject = new GameObject("ScratchCardChoice", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        cardObject.transform.SetParent(parent, false);

        RectTransform rect = cardObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(260f, 320f);

        Image image = cardObject.GetComponent<Image>();
        image.color = new Color(0.16f, 0.16f, 0.18f, 0.95f);

        GameObject nameObject = new GameObject("Name", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObject.transform.SetParent(cardObject.transform, false);
        RectTransform nameRect = nameObject.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.62f);
        nameRect.anchorMax = new Vector2(1f, 0.92f);
        nameRect.offsetMin = new Vector2(16f, 0f);
        nameRect.offsetMax = new Vector2(-16f, 0f);

        GameObject descObject = new GameObject("Description", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        descObject.transform.SetParent(cardObject.transform, false);
        RectTransform descRect = descObject.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0f, 0.12f);
        descRect.anchorMax = new Vector2(1f, 0.62f);
        descRect.offsetMin = new Vector2(18f, 0f);
        descRect.offsetMax = new Vector2(-18f, 0f);

        return cardObject;
    }

    private void SetScratchCardChoiceFallbackText(Transform root, ScratchCardTypeConfig cardTypeConfig)
    {
        TextMeshProUGUI[] texts = root != null ? root.GetComponentsInChildren<TextMeshProUGUI>(true) : null;
        if (texts == null)
        {
            return;
        }

        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null)
            {
                continue;
            }

            AssetProvider.ApplyDefaultTmpFont(text);
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            if (text.name == "Name")
            {
                text.fontSize = 34f;
                text.text = cardTypeConfig.Name;
            }
            else
            {
                text.fontSize = 22f;
                text.text = cardTypeConfig.WinDescription;
            }
        }
    }

    private GameObject CreateScratchToolChoice(ScratchToolConfig toolConfig, Transform parent)
    {
        if (toolConfig == null)
        {
            return null;
        }

        GameObject choiceRoot = CreateScratchToolChoiceRoot(parent, toolConfig.Id);
        GameObject toolPrefab = AssetProvider.LoadPrefab("UI/ScratchToolView");
        GameObject toolObject = toolPrefab != null
            ? Instantiate(toolPrefab, choiceRoot.transform, false)
            : CreateScratchToolChoiceFallback(choiceRoot.transform);
        toolObject.name = $"ScratchToolContent_{toolConfig.Id}";
        toolObject.transform.localScale = Vector3.one;

        ScratchToolView toolView = toolObject.GetComponent<ScratchToolView>();
        if (toolView != null)
        {
            toolView.UseRewardDescription();
            toolView.Bind(toolConfig);
        }
        else
        {
            SetScratchToolChoiceFallbackText(toolObject.transform, toolConfig);
        }

        Button button = toolObject.GetComponent<Button>();
        if (button == null)
        {
            button = toolObject.AddComponent<Button>();
        }

        int selectedToolId = toolConfig.Id;
        button.onClick.AddListener(() => OnScratchToolRewardSelected?.Invoke(selectedToolId));
        return choiceRoot;
    }

    private GameObject CreateScratchToolChoiceRoot(Transform parent, int toolId)
    {
        GameObject rootObject = new GameObject($"ScratchTool_{toolId}", typeof(RectTransform), typeof(Button));
        rootObject.transform.SetParent(parent, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = Vector2.zero;

        float scale = Mathf.Max(0.01f, _scratchToolRewardChoiceScale);
        rootObject.transform.localScale = Vector3.one * scale;
        return rootObject;
    }

    private GameObject CreateScratchToolChoiceFallback(Transform parent)
    {
        GameObject toolObject = new GameObject("ScratchToolChoice", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        toolObject.transform.SetParent(parent, false);

        RectTransform rect = toolObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(260f, 320f);

        Image image = toolObject.GetComponent<Image>();
        image.color = new Color(0.16f, 0.16f, 0.18f, 0.95f);

        GameObject nameObject = new GameObject("Name", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObject.transform.SetParent(toolObject.transform, false);
        RectTransform nameRect = nameObject.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.62f);
        nameRect.anchorMax = new Vector2(1f, 0.92f);
        nameRect.offsetMin = new Vector2(16f, 0f);
        nameRect.offsetMax = new Vector2(-16f, 0f);

        GameObject descObject = new GameObject("Description", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        descObject.transform.SetParent(toolObject.transform, false);
        RectTransform descRect = descObject.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0f, 0.12f);
        descRect.anchorMax = new Vector2(1f, 0.62f);
        descRect.offsetMin = new Vector2(18f, 0f);
        descRect.offsetMax = new Vector2(-18f, 0f);

        return toolObject;
    }

    private void SetScratchToolChoiceFallbackText(Transform root, ScratchToolConfig toolConfig)
    {
        TextMeshProUGUI[] texts = root != null ? root.GetComponentsInChildren<TextMeshProUGUI>(true) : null;
        if (texts == null)
        {
            return;
        }

        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null)
            {
                continue;
            }

            AssetProvider.ApplyDefaultTmpFont(text);
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            if (text.name == "Name")
            {
                text.fontSize = 34f;
                text.text = toolConfig.Name;
            }
            else
            {
                text.fontSize = 22f;
                text.text = toolConfig.Description;
            }
        }
    }

    private GameObject CreateRogueChoiceCard(RogueCardRewardChoiceModel choice, Transform parent, int currentLevel)
    {
        RogueCardConfig cardConfig = choice != null ? choice.CardConfig : null;
        int maxLevel = cardConfig != null ? cardConfig.GetMaxLevel() : 1;
        int previewLevel = choice != null ? Mathf.Clamp(choice.Level, 1, maxLevel) : 1;
        string levelText = currentLevel >= previewLevel ? $"Lv.{currentLevel} 已拥有" : currentLevel > 0 ? $"升级至 Lv.{previewLevel}" : $"Lv.{previewLevel}";
        GameObject cardObject = CreateRogueCardVisual(cardConfig, parent, levelText, previewLevel);
        if (cardObject != null)
        {
            cardObject.transform.localScale = Vector3.one * Mathf.Max(0.01f, _rogueRewardChoiceCardScale);
        }

        Button button = cardObject != null ? cardObject.GetComponent<Button>() : null;
        if (button == null && cardObject != null)
        {
            button = cardObject.AddComponent<Button>();
        }

        if (button == null)
        {
            return cardObject;
        }

        PreserveButtonDisabledVisual(button);
        int selectedCardId = cardConfig.Id;
        int selectedLevel = previewLevel;
        button.onClick.AddListener(() =>
        {
            if (_isRogueChoiceShowAnimating || _isRogueChoiceSelectAnimating)
            {
                return;
            }

            if (_isRogueExchangeMode && currentLevel <= 0)
            {
                ToggleRogueExchangeSelection(cardObject, selectedCardId, selectedLevel);
                return;
            }

            PlayRogueRewardSelectAnimation(cardObject, selectedCardId, selectedLevel);
        });
        return cardObject;
    }

    private void ToggleRogueExchangeSelection(GameObject selectedCardObject, int selectedCardId, int selectedLevel)
    {
        if (selectedCardObject == null)
        {
            return;
        }

        if (_selectedRogueExchangeCardObject == selectedCardObject && _selectedRogueExchangeCardId == selectedCardId)
        {
            ClearRogueExchangeSelection();
            SetRogueExchangePromptVisible(false);
            SetRogueChoiceHoverEnabled(true);
            RefreshRogueChoiceHoverState();
            return;
        }

        ClearRogueExchangeSelection();
        ResetRogueChoiceHoverToResting(selectedCardObject);
        _selectedRogueExchangeCardObject = selectedCardObject;
        _selectedRogueExchangeCardId = selectedCardId;
        _selectedRogueExchangeCardLevel = selectedLevel;

        RectTransform selectedRect = selectedCardObject.transform as RectTransform;
        if (selectedRect != null)
        {
            _selectedRogueExchangeRestingPosition = selectedRect.anchoredPosition;
            _selectedRogueExchangeRestingScale = selectedRect.localScale;
            selectedRect.DOKill(false);
            selectedRect
                .DOAnchorPosY(_selectedRogueExchangeRestingPosition.y + 80f, 0.16f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
            selectedRect
                .DOScale(_selectedRogueExchangeRestingScale * 1.08f, 0.16f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        SetRogueChoiceHoverEnabled(false);
        SetRogueExchangePromptVisible(true);
    }

    private void HandleRogueExchangeOverlayClick(PointerEventData eventData)
    {
        if (!_isRogueExchangeMode || _selectedRogueExchangeCardId <= 0 || eventData == null)
        {
            return;
        }

        Camera eventCamera = eventData.pressEventCamera != null ? eventData.pressEventCamera : GetUiEventCamera();
        TrySelectOwnedRogueCardAtScreenPoint(eventData.position, eventCamera);
    }

    private bool TrySelectOwnedRogueCardAtScreenPoint(Vector2 screenPoint, Camera eventCamera)
    {
        for (int i = 0; i < _ownedRogueCardObjects.Count; i++)
        {
            GameObject ownedCardObject = _ownedRogueCardObjects[i];
            if (!ContainsRogueCardScreenPoint(ownedCardObject, screenPoint, eventCamera))
            {
                continue;
            }

            int ownedCardId = GetOwnedRogueCardId(ownedCardObject);
            if (ownedCardId > 0)
            {
                OnRogueOwnedCardSelected?.Invoke(ownedCardId);
                return true;
            }

            return false;
        }

        return false;
    }

    private static bool ContainsRogueCardScreenPoint(GameObject cardObject, Vector2 screenPoint, Camera eventCamera)
    {
        RectTransform[] rectTransforms = cardObject != null
            ? cardObject.GetComponentsInChildren<RectTransform>(true)
            : null;
        if (rectTransforms == null)
        {
            return false;
        }

        for (int i = 0; i < rectTransforms.Length; i++)
        {
            RectTransform rectTransform = rectTransforms[i];
            if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, eventCamera) ||
                (eventCamera != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, null)))
            {
                return true;
            }
        }

        return false;
    }

    private static int GetOwnedRogueCardId(GameObject cardObject)
    {
        RogueCardHoverView hoverView = cardObject != null ? cardObject.GetComponent<RogueCardHoverView>() : null;
        return hoverView != null ? hoverView.CardId : 0;
    }

    private static void ResetRogueChoiceHoverToResting(GameObject cardObject)
    {
        RogueCardHoverView[] hoverViews = cardObject != null
            ? cardObject.GetComponentsInChildren<RogueCardHoverView>(true)
            : null;
        if (hoverViews == null)
        {
            return;
        }

        for (int i = 0; i < hoverViews.Length; i++)
        {
            if (hoverViews[i] != null)
            {
                hoverViews[i].ResetToRestingTransform();
            }
        }
    }

    private static void PreserveButtonDisabledVisual(Button button)
    {
        if (button == null)
        {
            return;
        }

        ColorBlock colors = button.colors;
        colors.disabledColor = colors.normalColor;
        button.colors = colors;
    }

    private void PlayRogueRewardSelectAnimation(GameObject selectedCardObject, int selectedCardId, int selectedLevel)
    {
        if (selectedCardObject == null || _isRogueChoiceSelectAnimating)
        {
            return;
        }

        _isRogueChoiceSelectAnimating = true;
        SetRewardChoiceButtonsInteractable(_rogueChoiceObjects, false);
        SetRogueChoiceHoverEnabled(false);
        _rogueChoiceSelectSequence?.Kill(false);
        _rogueChoiceSelectSequence = DOTween.Sequence().SetUpdate(true);

        float unselectedDuration = Mathf.Max(0.01f, _rogueRewardSelectUnselectedScaleDuration);
        float moveDuration = Mathf.Max(0.01f, _rogueRewardSelectMoveDuration);
        for (int i = 0; i < _rogueChoiceObjects.Count; i++)
        {
            GameObject choiceObject = _rogueChoiceObjects[i];
            if (choiceObject == null || choiceObject == selectedCardObject)
            {
                continue;
            }

            RectTransform choiceRect = choiceObject.transform as RectTransform;
            if (choiceRect == null)
            {
                continue;
            }

            choiceRect.DOKill(false);
            _rogueChoiceSelectSequence.Join(choiceRect
                .DOScaleY(0f, unselectedDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    if (choiceObject != null)
                    {
                        choiceObject.SetActive(false);
                    }
                }));
        }

        RectTransform selectedRect = selectedCardObject.transform as RectTransform;
        if (selectedRect == null)
        {
            _rogueChoiceSelectSequence = null;
            _isRogueChoiceSelectAnimating = false;
            _pendingOwnedRogueCardAppearId = selectedCardId;
            OnRogueRewardCardSelected?.Invoke(selectedCardId, selectedLevel);
            return;
        }

        selectedRect.DOKill(false);
        float targetY = selectedRect.anchoredPosition.y + GetRogueChoiceMoveOutOffset(selectedRect);
        _rogueChoiceSelectSequence.Append(selectedRect
            .DOAnchorPosY(targetY, moveDuration)
            .SetEase(Ease.InCubic));
        _rogueChoiceSelectSequence.OnComplete(() =>
        {
            _rogueChoiceSelectSequence = null;
            _isRogueChoiceSelectAnimating = false;
            _pendingOwnedRogueCardAppearId = selectedCardId;
            OnRogueRewardCardSelected?.Invoke(selectedCardId, selectedLevel);
        });
    }

    private void ClearRogueExchangeSelection()
    {
        if (_selectedRogueExchangeCardObject != null)
        {
            RectTransform selectedRect = _selectedRogueExchangeCardObject.transform as RectTransform;
            if (selectedRect != null)
            {
                selectedRect.DOKill(false);
                selectedRect.anchoredPosition = _selectedRogueExchangeRestingPosition;
                selectedRect.localScale = _selectedRogueExchangeRestingScale;
            }
        }

        _selectedRogueExchangeCardObject = null;
        _selectedRogueExchangeCardId = -1;
        _selectedRogueExchangeCardLevel = 1;
        _selectedRogueExchangeRestingPosition = Vector2.zero;
        _selectedRogueExchangeRestingScale = Vector3.one;
    }

    private void SetRogueExchangePromptVisible(bool visible)
    {
        if (_rogueExchangePromptText != null)
        {
            _rogueExchangePromptText.gameObject.SetActive(visible);
        }
    }

    private float GetRogueChoiceMoveOutOffset(RectTransform selectedRect)
    {
        RectTransform root = transform as RectTransform;
        float rootHeight = root != null ? root.rect.height : Screen.height;
        float cardHeight = selectedRect != null ? selectedRect.rect.height * Mathf.Abs(selectedRect.lossyScale.y) : 300f;
        return rootHeight * 0.5f + cardHeight + 120f;
    }

    private void PlayOwnedRogueCardAppearAnimation(GameObject cardObject)
    {
        RectTransform cardRect = cardObject != null ? cardObject.transform as RectTransform : null;
        if (cardRect == null)
        {
            return;
        }

        Vector3 targetScale = cardRect.localScale;
        _rogueOwnedAppearSequence?.Kill(false);
        cardRect.DOKill(false);
        cardRect.localScale = Vector3.zero;
        _rogueOwnedAppearSequence = DOTween.Sequence().SetUpdate(true);
        _rogueOwnedAppearSequence
            .Append(cardRect
                .DOScale(targetScale, Mathf.Max(0.01f, _rogueRewardOwnedAppearDuration))
                .SetEase(Ease.OutBack, Mathf.Max(0f, _rogueRewardOwnedAppearOvershoot)))
            .OnComplete(() =>
            {
                cardRect.localScale = targetScale;
                _rogueOwnedAppearSequence = null;
            });
    }

    private GameObject CreateOwnedRogueCard(RogueCardInventoryEntryModel ownedCard, Transform parent)
    {
        GameObject cardObject = CreateRogueCardVisual(ownedCard.Config, parent, $"Lv.{ownedCard.Level}", ownedCard.Level);
        RogueCardHoverView hoverView = cardObject != null ? cardObject.GetComponent<RogueCardHoverView>() : null;
        if (cardObject != null && hoverView == null)
        {
            hoverView = cardObject.AddComponent<RogueCardHoverView>();
        }

        if (hoverView != null)
        {
            hoverView.BindCardId(ownedCard.CardId);
        }

        Button button = cardObject != null ? cardObject.GetComponent<Button>() : null;
        if (button == null && cardObject != null)
        {
            button = cardObject.AddComponent<Button>();
        }

        if (button != null)
        {
            PreserveButtonDisabledVisual(button);
            int ownedCardId = ownedCard.CardId;
            button.onClick.AddListener(() =>
            {
                if (!_isRogueExchangeMode || _selectedRogueExchangeCardId <= 0)
                {
                    return;
                }

                OnRogueOwnedCardSelected?.Invoke(ownedCardId);
            });
        }

        return cardObject;
    }

    private GameObject CreateRogueCardVisual(RogueCardConfig cardConfig, Transform parent, string levelText, int level)
    {
        if (cardConfig == null)
        {
            return null;
        }

        RogueCardRarity displayRarity = cardConfig.GetRarityForLevel(level);
        string prefabPath = GetRogueCardPrefabPath(displayRarity);
        GameObject cardPrefab = AssetProvider.LoadPrefab(prefabPath);
        if (cardPrefab == null && !string.Equals(prefabPath, "UI/RogueCard_Common"))
        {
            Debug.LogWarning($"[MainGamePanel] Failed to load {prefabPath}; fallback to UI/RogueCard_Common.");
            cardPrefab = AssetProvider.LoadPrefab("UI/RogueCard_Common");
        }

        if (cardPrefab == null)
        {
            Debug.LogError("[MainGamePanel] Failed to load rogue card prefab for display.");
            return null;
        }

        GameObject cardObject = Instantiate(cardPrefab, parent, false);
        cardObject.name = $"RogueCard_{cardConfig.Id}";

        SetRogueCardText(cardObject.transform, "CardName", cardConfig.Name);
        SetRogueCardText(cardObject.transform, "Rare", $"{RogueCardConfig.GetRarityDisplayName(displayRarity)}  {levelText}");
        SetRogueCardText(cardObject.transform, "Description", cardConfig.GetDescriptionForLevel(level));
        return cardObject;
    }

    private static string GetRogueCardPrefabPath(RogueCardRarity rarity)
    {
        switch (rarity)
        {
            case RogueCardRarity.Rare:
                return "UI/RogueCard_Rare";
            case RogueCardRarity.Epic:
                return "UI/RogueCard_Epic";
            case RogueCardRarity.Legendary:
                return "UI/RogueCard_Legendary";
            case RogueCardRarity.Common:
            default:
                return "UI/RogueCard_Common";
        }
    }

    private static int GetOwnedRogueCardLevel(IReadOnlyList<RogueCardInventoryEntryModel> ownedCards, int cardId)
    {
        int count = ownedCards != null ? ownedCards.Count : 0;
        for (int i = 0; i < count; i++)
        {
            RogueCardInventoryEntryModel ownedCard = ownedCards[i];
            if (ownedCard != null && ownedCard.CardId == cardId)
            {
                return ownedCard.Level;
            }
        }

        return 0;
    }

    private void SetRogueCardText(Transform root, string childName, string text)
    {
        if (root == null)
        {
            return;
        }

        Transform child = root.Find(childName);
        if (child == null)
        {
            Debug.LogWarning($"[MainGamePanel] Rogue card prefab is missing child '{childName}'.");
            return;
        }

        TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>();
        if (label == null)
        {
            Debug.LogWarning($"[MainGamePanel] Child '{childName}' does not have a TextMeshProUGUI component.");
            return;
        }

        AssetProvider.ApplyDefaultTmpFont(label);
        label.text = string.IsNullOrWhiteSpace(text) ? "-" : text;
    }

    private void ClearRogueChoiceObjects()
    {
        _rogueChoiceSelectSequence?.Kill(false);
        _rogueChoiceSelectSequence = null;
        _isRogueChoiceSelectAnimating = false;
        StopRewardChoiceShowAnimation(ref _rogueChoiceShowSequence, ref _rogueChoiceShowCoroutine, _rogueChoiceContentRoot);
        _isRogueChoiceShowAnimating = false;
        SetRogueChoiceHoverEnabled(true);

        for (int i = 0; i < _rogueChoiceObjects.Count; i++)
        {
            if (_rogueChoiceObjects[i] != null)
            {
                Destroy(_rogueChoiceObjects[i]);
            }
        }

        _rogueChoiceObjects.Clear();
    }

    private void ClearScratchToolChoiceObjects()
    {
        StopRewardChoiceShowAnimation(ref _scratchToolChoiceShowSequence, ref _scratchToolChoiceShowCoroutine, _scratchToolChoiceContentRoot);

        for (int i = 0; i < _scratchToolChoiceObjects.Count; i++)
        {
            if (_scratchToolChoiceObjects[i] != null)
            {
                Destroy(_scratchToolChoiceObjects[i]);
            }
        }

        _scratchToolChoiceObjects.Clear();
    }

    private void ClearScratchCardChoiceObjects()
    {
        StopRewardChoiceShowAnimation(ref _scratchCardChoiceShowSequence, ref _scratchCardChoiceShowCoroutine, _scratchCardChoiceContentRoot);

        for (int i = 0; i < _scratchCardChoiceObjects.Count; i++)
        {
            if (_scratchCardChoiceObjects[i] != null)
            {
                Destroy(_scratchCardChoiceObjects[i]);
            }
        }

        _scratchCardChoiceObjects.Clear();
    }

    private void ClearOwnedRogueCardObjects()
    {
        _rogueOwnedAppearSequence?.Kill(false);
        _rogueOwnedAppearSequence = null;

        for (int i = 0; i < _ownedRogueCardObjects.Count; i++)
        {
            if (_ownedRogueCardObjects[i] != null)
            {
                Destroy(_ownedRogueCardObjects[i]);
            }
        }

        _ownedRogueCardObjects.Clear();
    }

    private void OnDestroy()
    {
        if (_rogueCardRewardButton != null)
        {
            _rogueCardRewardButton.onClick.RemoveListener(HandleRogueCardRewardButtonClicked);
        }

        if (_continueButton != null)
        {
            _continueButton.onClick.RemoveListener(HandleWinContinueButtonClicked);
        }

        if (_scratchToolRewardButton != null)
        {
            _scratchToolRewardButton.onClick.RemoveListener(HandleScratchToolRewardButtonClicked);
        }

        if (_scratchCardRewardButton != null)
        {
            _scratchCardRewardButton.onClick.RemoveListener(HandleScratchCardRewardButtonClicked);
        }

        if (_newLevelStartButton != null)
        {
            _newLevelStartButton.onClick.RemoveListener(HandleNewLevelStartButtonClicked);
        }

        _focusOverlayTween?.Kill();
        _levelGoalSliderTween?.Kill();
        _winPanelShowSequence?.Kill();
        _newLevelPanelShowSequence?.Kill();
        _rogueChoiceSelectSequence?.Kill(false);
        _rogueOwnedAppearSequence?.Kill(false);
        StopRewardChoiceShowAnimation(ref _rogueChoiceShowSequence, ref _rogueChoiceShowCoroutine, _rogueChoiceContentRoot);
        _isRogueChoiceSelectAnimating = false;
        _isRogueChoiceShowAnimating = false;
        SetRogueChoiceHoverEnabled(true);
        StopRewardChoiceShowAnimation(ref _scratchToolChoiceShowSequence, ref _scratchToolChoiceShowCoroutine, _scratchToolChoiceContentRoot);
        StopRewardChoiceShowAnimation(ref _scratchCardChoiceShowSequence, ref _scratchCardChoiceShowCoroutine, _scratchCardChoiceContentRoot);
        if (_gameOverSkullEffectRoutine != null)
        {
            StopCoroutine(_gameOverSkullEffectRoutine);
            _gameOverSkullEffectRoutine = null;
        }

        ClearCoinRainJackpotText();
        ClearCoinRainPool();
    }

    private class CoinRainIcon
    {
        public CoinRainIcon(MainGamePanel owner, GameObject gameObject, RectTransform transform, Image image, CanvasGroup canvasGroup)
        {
            _owner = owner;
            GameObject = gameObject;
            Transform = transform;
            Image = image;
            CanvasGroup = canvasGroup;
            ShowCallback = Show;
            RecycleCallback = Recycle;
        }

        private readonly MainGamePanel _owner;

        public GameObject GameObject { get; }
        public RectTransform Transform { get; }
        public Image Image { get; }
        public CanvasGroup CanvasGroup { get; }
        public Sequence Sequence { get; set; }
        public bool IsActive { get; set; }
        public TweenCallback ShowCallback { get; }
        public TweenCallback RecycleCallback { get; }

        private void Show()
        {
            if (GameObject != null)
            {
                CanvasGroup.alpha = 1f;
            }
        }

        private void Recycle()
        {
            _owner?.RecycleCoinRainIcon(this);
        }
    }

    private void EnsureFocusPanel()
    {
        if (_focusPanelView != null)
        {
            return;
        }

        if (_configuredFocusPanelView != null)
        {
            _focusPanelView = _configuredFocusPanelView;
            return;
        }

        if (_focusOverlayRoot != null)
        {
            _focusPanelView = _focusOverlayRoot.GetComponentInChildren<ScratchCardFocusPanelView>(true);
        }

        if (_focusPanelView != null)
        {
            return;
        }

        _focusPanelView = GetComponentInChildren<ScratchCardFocusPanelView>(true);
        if (_focusPanelView != null)
        {
            return;
        }

        _focusPanelView = FindObjectOfType<ScratchCardFocusPanelView>(true);
        if (_focusPanelView == null)
        {
            Debug.LogWarning("[MainGamePanel] ScratchCardFocusPanelView 未配置，图案列表不会显示。请在场景常驻 Scroll View 上挂载该组件并拖入引用。");
        }
    }

    // 鍚庣画鍙互澧炲姞鍔ㄦ€佸疄渚嬪寲瀛愰」鐨勬柟娉?
    // public void AddSlotShopItem(SlotShopItemView itemPrefab) { ... }
}
