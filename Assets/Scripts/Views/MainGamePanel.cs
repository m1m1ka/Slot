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

    // 瀵瑰鏆撮湶鍒楄〃鐨勭埗鑺傜偣渚?Controller 鎸傝浇瀛愮墿浣撳紩鐢?
    public Transform SlotListRoot => _slotListRoot;
    public Transform UpgradeListRoot => _upgradeListRoot;
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
    private readonly List<GameObject> _ownedRogueCardObjects = new List<GameObject>();

    public event Action<int> OnRogueRewardCardSelected;

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
        return CreateRogueCardVisual(ownedCard.Config, parent, $"Lv.{ownedCard.Level}", ownedCard.Level);
    }

    private GameObject CreateRogueCardVisual(RogueCardConfig cardConfig, Transform parent, string levelText, int level)
    {
        GameObject cardPrefab = AssetProvider.LoadPrefab("UI/Card");
        if (cardPrefab == null)
        {
            Debug.LogError("[MainGamePanel] Failed to load UI/Card prefab for rogue card display.");
            return null;
        }

        GameObject cardObject = Instantiate(cardPrefab, parent, false);
        cardObject.name = $"RogueCard_{cardConfig.Id}";

        SetRogueCardText(cardObject.transform, "CardName", cardConfig.Name);
        SetRogueCardText(cardObject.transform, "Rare", $"{cardConfig.GetRarityDisplayName()}  {levelText}");
        SetRogueCardText(cardObject.transform, "Description", cardConfig.GetDescriptionForLevel(level));
        return cardObject;
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
    }

    private void EnsureFocusPanel()
    {
        if (_focusPanelView != null || _focusOverlayRoot == null)
        {
            return;
        }

        if (_configuredFocusPanelView != null)
        {
            _focusPanelView = _configuredFocusPanelView;
            return;
        }

        _focusPanelView = _focusOverlayRoot.GetComponentInChildren<ScratchCardFocusPanelView>(true);
        if (_focusPanelView != null)
        {
            return;
        }

        GameObject panelObject = new GameObject("ScratchCardFocusPanel", typeof(RectTransform), typeof(CanvasGroup));
        panelObject.transform.SetParent(_focusOverlayRoot, false);
        _focusPanelView = panelObject.AddComponent<ScratchCardFocusPanelView>();
        _focusPanelView.Hide(true);
    }

    // 鍚庣画鍙互澧炲姞鍔ㄦ€佸疄渚嬪寲瀛愰」鐨勬柟娉?
    // public void AddSlotShopItem(SlotShopItemView itemPrefab) { ... }
}
