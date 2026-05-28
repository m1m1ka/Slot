using UnityEngine;
using TMPro; // 鎺ㄨ崘浣跨敤 TextMeshPro 鏉ユ樉绀烘枃鏈?
using System;
using System.Collections.Generic;
using Configs;
using Core;
using DG.Tweening;
using UI; // 寮曞叆浣犲凡鏈夌殑 UI 鍛藉悕绌洪棿锛屽寘鍚?UIPanel 鍩虹被
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

    [Header("Rogue Cards")]
    [SerializeField] private RectTransform _rogueOwnedCardsRoot;
    [SerializeField] private RectTransform _rogueChoiceOverlayRoot;

    [Header("Win Panel")]
    [SerializeField] private RectTransform _winPanelRoot;
    [SerializeField] private Button _rogueCardRewardButton;
    [SerializeField] private Button _scratchToolRewardButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private float _scratchToolRewardChoiceScale = 3.5f;
    [SerializeField] private float _scratchToolRewardChoiceSpacing = 300f;

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
    [SerializeField] private ScratchCardFocusPanelView _configuredFocusPanelView;
    private ScratchCardFocusPanelView _focusPanelView;
    private GameObject _rogueChoiceOverlayObject;
    private Transform _rogueChoiceContentRoot;
    private readonly List<GameObject> _rogueChoiceObjects = new List<GameObject>();
    private GameObject _scratchToolChoiceOverlayObject;
    private Transform _scratchToolChoiceContentRoot;
    private readonly List<GameObject> _scratchToolChoiceObjects = new List<GameObject>();
    private readonly List<GameObject> _ownedRogueCardObjects = new List<GameObject>();

    public event Action<int> OnRogueRewardCardSelected;
    public event Action OnRogueRewardRequested;
    public event Action<int> OnScratchToolRewardSelected;
    public event Action OnScratchToolRewardRequested;
    public event Action OnWinContinueRequested;

    private void Awake()
    {
        EnsureWinPanel();
        HideWinPanel();
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
            _coinText.text = currentCoins.ToString("N0");
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
            _levelGoalText.text = $"{currentCoins:N0} / {levelModel.RequiredCoins:N0}";
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

    public void ShowRogueCardChoices(
        IReadOnlyList<RogueCardConfig> choices,
        IReadOnlyList<RogueCardInventoryEntryModel> ownedCards = null)
    {
        EnsureRogueChoiceOverlay();
        ClearRogueChoiceObjects();

        if (_rogueChoiceOverlayObject == null || _rogueChoiceContentRoot == null)
        {
            return;
        }

        _rogueChoiceOverlayObject.SetActive(true);
        _rogueChoiceOverlayObject.transform.SetAsLastSibling();
        int count = choices != null ? choices.Count : 0;
        for (int i = 0; i < count; i++)
        {
            RogueCardConfig cardConfig = choices[i];
            if (cardConfig == null)
            {
                continue;
            }

            int currentLevel = GetOwnedRogueCardLevel(ownedCards, cardConfig.Id);
            GameObject cardObject = CreateRogueChoiceCard(cardConfig, _rogueChoiceContentRoot, currentLevel);
            _rogueChoiceObjects.Add(cardObject);
        }
    }

    public void HideRogueCardChoices()
    {
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
    }

    public void HideScratchToolChoices()
    {
        if (_scratchToolChoiceOverlayObject != null)
        {
            _scratchToolChoiceOverlayObject.SetActive(false);
        }
    }

    public void ShowWinPanel(bool canRequestRogueReward, bool canContinue, bool canRequestScratchToolReward = false)
    {
        EnsureWinPanel();
        if (_winPanelRoot == null)
        {
            return;
        }

        _winPanelRoot.gameObject.SetActive(true);
        _winPanelRoot.SetAsLastSibling();
        SetWinPanelButtonState(_rogueCardRewardButton, canRequestRogueReward, canRequestRogueReward);
        SetWinPanelButtonState(_scratchToolRewardButton, canRequestScratchToolReward, canRequestScratchToolReward);
        SetWinPanelButtonState(_continueButton, canContinue, canContinue);
    }

    public void HideWinPanel()
    {
        EnsureWinPanel();
        if (_winPanelRoot != null)
        {
            _winPanelRoot.gameObject.SetActive(false);
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

    public void SetWinPanelContinueButtonVisible(bool visible, bool interactable = true)
    {
        EnsureWinPanel();
        SetWinPanelButtonState(_continueButton, visible, interactable);
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

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child != null && child.name == childName)
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

        Image overlayImage = _rogueChoiceOverlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.68f);
        overlayImage.raycastTarget = true;

        GameObject contentObject = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(_rogueChoiceOverlayObject.transform, false);

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup layout = contentObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 28f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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

        if (_continueButton == null)
        {
            Transform foundContinueButton = FindChildRecursive(_winPanelRoot, "ContinueBtn");
            _continueButton = foundContinueButton != null ? foundContinueButton.GetComponent<Button>() : null;
        }

        if (_rogueCardRewardButton == null)
        {
            _rogueCardRewardButton = CreateWinPanelButton("RogueCardRewardBtn", "选择奖励", new Vector2(0f, 120f), new Vector2(347f, 108f));
        }

        if (_scratchToolRewardButton == null)
        {
            _scratchToolRewardButton = CreateWinPanelButton("ScratchToolRewardBtn", "选择刮具", new Vector2(0f, -60f), new Vector2(347f, 108f));
        }

        if (_continueButton == null)
        {
            _continueButton = CreateWinPanelButton("ContinueBtn", "下一关", new Vector2(0f, -373f), new Vector2(346f, 153f));
        }

        _rogueCardRewardButton.onClick.RemoveListener(HandleRogueCardRewardButtonClicked);
        _rogueCardRewardButton.onClick.AddListener(HandleRogueCardRewardButtonClicked);
        _scratchToolRewardButton.onClick.RemoveListener(HandleScratchToolRewardButtonClicked);
        _scratchToolRewardButton.onClick.AddListener(HandleScratchToolRewardButtonClicked);

        _continueButton.onClick.RemoveListener(HandleWinContinueButtonClicked);
        _continueButton.onClick.AddListener(HandleWinContinueButtonClicked);
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

    private void HandleWinContinueButtonClicked()
    {
        OnWinContinueRequested?.Invoke();
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

    private GameObject CreateRogueChoiceCard(RogueCardConfig cardConfig, Transform parent, int currentLevel)
    {
        int maxLevel = cardConfig != null ? cardConfig.GetMaxLevel() : 1;
        int previewLevel = currentLevel > 0 ? Mathf.Min(currentLevel + 1, maxLevel) : 1;
        string levelText = currentLevel >= maxLevel ? $"Lv.{maxLevel} 已满级" : currentLevel > 0 ? $"升级至 Lv.{previewLevel}" : $"Lv.{previewLevel}";
        GameObject cardObject = CreateRogueCardVisual(cardConfig, parent, levelText, previewLevel);
        Button button = cardObject != null ? cardObject.GetComponent<Button>() : null;
        if (button == null && cardObject != null)
        {
            button = cardObject.AddComponent<Button>();
        }

        if (button == null)
        {
            return cardObject;
        }

        int selectedCardId = cardConfig.Id;
        button.onClick.AddListener(() => OnRogueRewardCardSelected?.Invoke(selectedCardId));
        return cardObject;
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
        for (int i = 0; i < _scratchToolChoiceObjects.Count; i++)
        {
            if (_scratchToolChoiceObjects[i] != null)
            {
                Destroy(_scratchToolChoiceObjects[i]);
            }
        }

        _scratchToolChoiceObjects.Clear();
    }

    private void ClearOwnedRogueCardObjects()
    {
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

        _focusOverlayTween?.Kill();
        _levelGoalSliderTween?.Kill();
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
