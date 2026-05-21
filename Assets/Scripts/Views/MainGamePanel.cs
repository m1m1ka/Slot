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
    private const string FloatingTextPrefabPath = "UI/FloatingText";

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

    [Header("Focused Score")]
    [SerializeField] private RectTransform _scorePanelRoot;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _scoreMultiplierText;
    [SerializeField] private float _scorePulseScale = 1.12f;
    [SerializeField] private float _scorePulseDuration = 0.16f;
    [SerializeField] private float _scoreChangePulseScale = 1.28f;
    [SerializeField] private float _scoreChangePulseDuration = 0.36f;
    [SerializeField] private float _scoreFloatDistance = 52f;
    [SerializeField] private float _scoreFloatDuration = 0.75f;
    [SerializeField] private Vector2 _scoreChangeFloatStartOffset = Vector2.zero;
    [SerializeField] private float _scoreChangeFloatIntroDuration = 0.18f;
    [SerializeField] private float _scoreChangeFloatHoldDuration = 0.28f;
    [SerializeField] private float _scoreChangeFloatMoveDuration = 0.28f;
    [SerializeField] private int _scoreFloatFontSize = 28;
    [SerializeField] private Color _scoreFloatTextColor = new Color(0.96f, 0.96f, 0.92f, 1f);
    [SerializeField] private Color _enhancedScoreFloatTextColor = new Color(0.35f, 0.65f, 1f, 1f);

    [Header("Rogue Cards")]
    [SerializeField] private RectTransform _rogueOwnedCardsRoot;
    [SerializeField] private RectTransform _rogueChoiceOverlayRoot;

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
    private Tween _scoreTextPulseTween;
    private Tween _scoreMultiplierPulseTween;
    private bool _hasFocusedScoreSnapshot;
    private int _lastFocusedScore;
    private double _lastFocusedRewardMultiplier;
    [SerializeField] private ScratchCardFocusPanelView _configuredFocusPanelView;
    private ScratchCardFocusPanelView _focusPanelView;
    private GameObject _rogueChoiceOverlayObject;
    private Transform _rogueChoiceContentRoot;
    private readonly List<GameObject> _rogueChoiceObjects = new List<GameObject>();
    private readonly List<GameObject> _ownedRogueCardObjects = new List<GameObject>();

    public event Action<int> OnRogueRewardCardSelected;

    private void Awake()
    {
        EnsureScorePanel();
        SetScorePanelVisible(false);
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
            _purchaseLimitText.text = $"{levelModel.RemainingScratchCardPurchases:N0} / {levelModel.ScratchCardPurchaseLimit:N0}";
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
        SetScorePanelVisible(false);
    }

    public void UpdateFocusedScore(int reward, double rewardMultiplier, bool visible)
    {
        EnsureScorePanel();
        if (!visible)
        {
            SetScorePanelVisible(false);
            _hasFocusedScoreSnapshot = false;
            return;
        }

        SetScorePanelVisible(true);

        if (_hasFocusedScoreSnapshot)
        {
            PlayFocusedScoreChangeFloats(reward, rewardMultiplier);
        }

        if (_scoreText != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_scoreText);
            _scoreText.text = reward.ToString();
        }

        if (_scoreMultiplierText != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_scoreMultiplierText);
            _scoreMultiplierText.text = FormatRewardMultiplier(rewardMultiplier);
        }

        _lastFocusedScore = reward;
        _lastFocusedRewardMultiplier = rewardMultiplier;
        _hasFocusedScoreSnapshot = true;
    }

    public void PlayFocusedScoreReveal(RectTransform sourceRect, int score, bool isEnhanced, double scoreMultiplier)
    {
        EnsureScorePanel();
        RectTransform parentRect = _scorePanelRoot != null
            ? _scorePanelRoot
            : transform as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        PlayScorePanelPulse();

        Vector3 startPosition = sourceRect != null ? sourceRect.position : parentRect.position;
        int displayScore = ScratchSettlementResult.ApplyMultiplier(score, scoreMultiplier);
        PlayFloatingText(parentRect, startPosition, displayScore.ToString(), isEnhanced ? _enhancedScoreFloatTextColor : _scoreFloatTextColor);
    }

    private void PlayFocusedScoreChangeFloats(int reward, double rewardMultiplier)
    {
        int scoreDelta = reward - _lastFocusedScore;
        if (scoreDelta != 0 && _scoreText != null)
        {
            _scoreTextPulseTween = PlayScoreTextPulse(
                _scoreText.rectTransform,
                _scoreTextPulseTween,
                _scoreChangePulseScale,
                _scoreChangePulseDuration);
            PlayScoreChangeFloatingText(
                _scoreText.rectTransform,
                FormatSignedInt(scoreDelta),
                _scoreFloatTextColor);
        }

        double multiplierDelta = rewardMultiplier - _lastFocusedRewardMultiplier;
        if (Math.Abs(multiplierDelta) > 0.0001d && _scoreMultiplierText != null)
        {
            _scoreMultiplierPulseTween = PlayScoreTextPulse(
                _scoreMultiplierText.rectTransform,
                _scoreMultiplierPulseTween,
                _scoreChangePulseScale,
                _scoreChangePulseDuration);
            PlayScoreChangeFloatingText(
                _scoreMultiplierText.rectTransform,
                FormatSignedDouble(multiplierDelta),
                _scoreFloatTextColor);
        }
    }

    private void PlayScoreChangeFloatingText(RectTransform sourceRect, string value, Color color)
    {
        if (sourceRect == null)
        {
            PlayFloatingText(
                _scorePanelRoot,
                _scorePanelRoot != null ? _scorePanelRoot.position : transform.position,
                value,
                color,
                _scoreChangeFloatHoldDuration,
                _scoreChangeFloatMoveDuration);
            return;
        }

        PlayFloatingText(
            _scorePanelRoot,
            sourceRect.position,
            value,
            color,
            _scoreChangeFloatHoldDuration,
            _scoreChangeFloatMoveDuration,
            GetScoreChangeFloatOffsetPosition(sourceRect),
            _scoreChangeFloatIntroDuration);
    }

    private Vector3 GetScoreChangeFloatOffsetPosition(RectTransform sourceRect)
    {
        if (sourceRect == null)
        {
            return _scorePanelRoot != null ? _scorePanelRoot.position : transform.position;
        }

        return sourceRect.TransformPoint(_scoreChangeFloatStartOffset);
    }

    private void PlayFloatingText(
        RectTransform parentRect,
        Vector3 startPosition,
        string value,
        Color color,
        float holdDuration = 0f,
        float moveDuration = -1f,
        Vector3? introEndPosition = null,
        float introDuration = 0f)
    {
        if (parentRect == null)
        {
            return;
        }

        GameObject textObject = AssetProvider.InstantiatePrefab(FloatingTextPrefabPath, parentRect);
        if (textObject == null)
        {
            return;
        }

        textObject.transform.SetAsLastSibling();

        RectTransform textRect = textObject.transform as RectTransform;
        if (textRect == null)
        {
            Destroy(textObject);
            return;
        }

        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.position = startPosition;

        CanvasGroup canvasGroup = textObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textObject.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (text == null)
        {
            Destroy(textObject);
            return;
        }

        AssetProvider.ApplyDefaultTmpFont(text);
        text.text = value;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;

        Vector3 originalScale = textRect.localScale;
        float resolvedIntroDuration = Mathf.Max(0f, introDuration);
        float resolvedHoldDuration = Mathf.Max(0f, holdDuration);
        float resolvedMoveDuration = moveDuration > 0f ? moveDuration : _scoreFloatDuration;
        Vector2 floatStartAnchoredPosition = textRect.anchoredPosition;
        if (introEndPosition.HasValue)
        {
            Vector3 localIntroEndPosition = parentRect.InverseTransformPoint(introEndPosition.Value);
            floatStartAnchoredPosition = new Vector2(localIntroEndPosition.x, localIntroEndPosition.y);
        }

        Sequence floatSequence = DOTween.Sequence().SetUpdate(true);
        if (introEndPosition.HasValue && resolvedIntroDuration > 0f)
        {
            textRect.localScale = Vector3.zero;
            floatSequence
                .Append(textRect.DOMove(introEndPosition.Value, resolvedIntroDuration).SetEase(Ease.OutCubic))
                .Join(textRect.DOScale(originalScale, resolvedIntroDuration).SetEase(Ease.OutBack));
        }
        else if (introEndPosition.HasValue)
        {
            textRect.position = introEndPosition.Value;
        }

        if (resolvedHoldDuration > 0f)
        {
            floatSequence.AppendInterval(resolvedHoldDuration);
        }

        Vector2 endPosition = floatStartAnchoredPosition + Vector2.up * _scoreFloatDistance;
        floatSequence
            .Append(textRect.DOAnchorPos(endPosition, resolvedMoveDuration).SetEase(Ease.InCubic))
            .Join(canvasGroup.DOFade(0f, resolvedMoveDuration).SetEase(Ease.InCubic))
            .OnComplete(() => Destroy(textObject));
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
        if (cardObject != null && cardObject.GetComponent<RogueCardHoverView>() == null)
        {
            cardObject.AddComponent<RogueCardHoverView>();
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
        _focusOverlayTween?.Kill();
        _levelGoalSliderTween?.Kill();
        _scoreTextPulseTween?.Kill();
        _scoreMultiplierPulseTween?.Kill();
    }

    private void EnsureScorePanel()
    {
        if (_scorePanelRoot == null)
        {
            Transform foundPanel = FindChildRecursive(transform, "ScorePanel");
            if (foundPanel != null)
            {
                _scorePanelRoot = foundPanel as RectTransform;
            }
        }

        if (_scorePanelRoot == null)
        {
            return;
        }

        if (_scoreText == null)
        {
            Transform foundScoreText = FindChildRecursive(_scorePanelRoot, "ScoreText");
            if (foundScoreText != null)
            {
                _scoreText = foundScoreText.GetComponent<TextMeshProUGUI>();
            }
        }

        if (_scoreMultiplierText == null)
        {
            _scoreMultiplierText = FindScorePanelTextByName("MultiplierText")
                ?? FindScorePanelTextByName("RewardMultiplierText")
                ?? FindScorePanelTextByName("Multiply")
                ?? FindScorePanelTextByName("CoinValue")
                ?? FindScorePanelValueText();
        }
    }

    private TextMeshProUGUI FindScorePanelTextByName(string childName)
    {
        if (_scorePanelRoot == null)
        {
            return null;
        }

        Transform child = FindChildRecursive(_scorePanelRoot, childName);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    private TextMeshProUGUI FindScorePanelValueText()
    {
        if (_scorePanelRoot == null)
        {
            return null;
        }

        TextMeshProUGUI[] texts = _scorePanelRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null || text == _scoreText)
            {
                continue;
            }

            string value = text.text;
            if (string.Equals(value, "分数", StringComparison.Ordinal) ||
                string.Equals(value, "倍率", StringComparison.Ordinal))
            {
                continue;
            }

            return text;
        }

        return null;
    }

    private void SetScorePanelVisible(bool visible)
    {
        EnsureScorePanel();
        if (_scorePanelRoot != null)
        {
            _scorePanelRoot.gameObject.SetActive(visible);
            if (visible)
            {
                _scorePanelRoot.SetAsLastSibling();
            }
        }
    }

    private void PlayScorePanelPulse()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        _scoreTextPulseTween = PlayScoreTextPulse(
            _scoreText != null ? _scoreText.rectTransform : null,
            _scoreTextPulseTween,
            _scorePulseScale,
            _scorePulseDuration);
        _scoreMultiplierPulseTween = PlayScoreTextPulse(
            _scoreMultiplierText != null ? _scoreMultiplierText.rectTransform : null,
            _scoreMultiplierPulseTween,
            _scorePulseScale,
            _scorePulseDuration);
    }

    private Tween PlayScoreTextPulse(RectTransform target, Tween currentTween, float pulseScale, float pulseDuration)
    {
        if (target == null)
        {
            return currentTween;
        }

        currentTween?.Kill();
        target.localScale = Vector3.one;
        float resolvedPulseScale = Mathf.Max(1f, pulseScale);
        float halfDuration = Mathf.Max(0.01f, pulseDuration * 0.5f);
        return DOTween.Sequence()
            .SetUpdate(true)
            .Append(target.DOScale(Vector3.one * resolvedPulseScale, halfDuration).SetEase(Ease.OutCubic))
            .Append(target.DOScale(Vector3.one, halfDuration).SetEase(Ease.OutCubic));
    }

    private static string FormatRewardMultiplier(double rewardMultiplier)
    {
        double normalizedMultiplier = rewardMultiplier > 0d ? rewardMultiplier : 1d;
        return $"{normalizedMultiplier:0.##}";
    }

    private static string FormatSignedInt(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private static string FormatSignedDouble(double value)
    {
        return value > 0d ? $"+{value:0.##}" : $"{value:0.##}";
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
