using UnityEngine;
using TMPro; // 推荐使用 TextMeshPro 来显示文本
using System;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using UI; // 引入你已有的 UI 命名空间，包含 UIPanel 基类
using UnityEngine.UI;
/// <summary>
/// 视图层：只负责呈现UI并收集用户点击输入，不包含任何业务逻辑判断。
/// 注意：这里继承自你已有的 UIPanel 基类。
/// </summary>
public class MainGamePanel : UIPanel
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _coinText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _levelGoalText;
    [SerializeField] private TextMeshProUGUI _purchaseLimitText;
    [SerializeField] private TextMeshProUGUI _levelStateText;

    [Header("Shop & Upgrade Lists Root")]
    [SerializeField] private Transform _slotListRoot;       // 挂载左侧购买老虎机按钮的父节点
    [SerializeField] private Transform _upgradeListRoot;    // 挂载右侧升级按钮的父节点

    [Header("Scratch Card Root")]
    [SerializeField] private RectTransform _scratchCardRoot; // 挂载动态生成的彩票表现层

    [Header("Focus Overlay Root")]
    [SerializeField] private RectTransform _focusOverlayRoot; // 挂载聚焦遮罩层的单独容器

    [Header("Focus Overlay")]
    [SerializeField] private Color _focusOverlayColor = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] private float _focusOverlayFadeDuration = 0.18f;

    [Header("Rogue Cards")]
    [SerializeField] private RectTransform _rogueOwnedCardsRoot;
    [SerializeField] private RectTransform _rogueChoiceOverlayRoot;

    // 对外暴露列表的父节点供 Controller 挂载子物体引用
    public Transform SlotListRoot => _slotListRoot;
    public Transform UpgradeListRoot => _upgradeListRoot;
    public RectTransform ScratchCardRoot => _scratchCardRoot;
    public RectTransform FocusOverlayRoot => _focusOverlayRoot;

    private CanvasGroup _focusOverlayCanvasGroup;
    private Image _focusOverlayImage;
    private Tween _focusOverlayTween;
    [SerializeField] private ScratchCardFocusPanelView _configuredFocusPanelView;
    private ScratchCardFocusPanelView _focusPanelView;
    private GameObject _rogueChoiceOverlayObject;
    private Transform _rogueChoiceContentRoot;
    private readonly List<GameObject> _rogueChoiceObjects = new List<GameObject>();
    private readonly List<GameObject> _ownedRogueCardObjects = new List<GameObject>();

    public event Action<int> OnRogueRewardCardSelected;

    // 如果面板上有通用的点击按钮，可以通过事件向Controller抛出
    // public event Action OnSomeGlobalButtonClicked;

    /// <summary>
    /// 供 Controller 调用，用于刷新金币显示的表现
    /// </summary>
    /// <param name="currentCoins">玩家当前金币数量</param>
    public void UpdateCoinDisplay(double currentCoins)
    {
        if (_coinText != null)
        {
            // 增量游戏通常需要格式化庞大数字 (如 1.2K, 3.5M)
            // 这里暂且使用字符串格式化展示
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
            _levelText.text = levelModel.LevelName;
        }

        if (_levelGoalText != null)
        {
            _levelGoalText.text = $"{currentCoins:N0} / {levelModel.RequiredCoins:N0}";
        }

        if (_purchaseLimitText != null)
        {
            _purchaseLimitText.text = $"{levelModel.RemainingScratchCardPurchases:N0} / {levelModel.ScratchCardPurchaseLimit:N0}";
        }

        if (_levelStateText != null)
        {
            _levelStateText.text = levelModel.IsPassed ? "Passed" : "In Progress";
        }
    }

    public void ShowRogueCardChoices(IReadOnlyList<RogueCardConfig> choices)
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

            GameObject cardObject = CreateRogueChoiceCard(cardConfig, _rogueChoiceContentRoot);
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

    public void RefreshOwnedRogueCards(IReadOnlyList<RogueCardConfig> ownedCards)
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
            RogueCardConfig cardConfig = ownedCards[i];
            if (cardConfig == null)
            {
                continue;
            }

            GameObject cardObject = CreateOwnedRogueCard(cardConfig, _rogueOwnedCardsRoot);
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

    private GameObject CreateRogueChoiceCard(RogueCardConfig cardConfig, Transform parent)
    {
        GameObject cardObject = CreateRogueCardVisual(cardConfig, parent, new Vector2(220f, 300f));
        Button button = cardObject.AddComponent<Button>();
        int selectedCardId = cardConfig.Id;
        button.onClick.AddListener(() => OnRogueRewardCardSelected?.Invoke(selectedCardId));
        return cardObject;
    }

    private GameObject CreateOwnedRogueCard(RogueCardConfig cardConfig, Transform parent)
    {
        return CreateRogueCardVisual(cardConfig, parent, new Vector2(150f, 58f));
    }

    private GameObject CreateRogueCardVisual(RogueCardConfig cardConfig, Transform parent, Vector2 size)
    {
        GameObject cardObject = new GameObject($"RogueCard_{cardConfig.Id}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        cardObject.transform.SetParent(parent, false);

        RectTransform rect = cardObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image image = cardObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.15f, 0.16f, 0.96f);

        VerticalLayoutGroup layout = cardObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 10, 10);
        layout.spacing = 6f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateRogueCardText("Name", cardConfig.Name, cardObject.transform, 20f, FontStyles.Bold);
        CreateRogueCardText("Rarity", cardConfig.Rarity, cardObject.transform, 13f, FontStyles.Normal);
        CreateRogueCardText("Description", cardConfig.Description, cardObject.transform, 14f, FontStyles.Normal);
        return cardObject;
    }

    private void CreateRogueCardText(string objectName, string text, Transform parent, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = string.IsNullOrWhiteSpace(text) ? "-" : text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.enableWordWrapping = true;
        label.raycastTarget = false;
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

    // 后续可以增加动态实例化子项的方法
    // public void AddSlotShopItem(SlotShopItemView itemPrefab) { ... }
}
