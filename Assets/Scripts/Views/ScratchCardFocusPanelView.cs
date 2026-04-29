using System.Collections.Generic;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Focus panel view. Only renders pattern rows.
/// </summary>
public class ScratchCardFocusPanelView : MonoBehaviour
{
    [Header("Panel Layout")]
    [SerializeField] private Vector2 _panelAnchorMin = new Vector2(1f, 0.5f);
    [SerializeField] private Vector2 _panelAnchorMax = new Vector2(1f, 0.5f);
    [SerializeField] private Vector2 _panelPivot = new Vector2(1f, 0.5f);
    [SerializeField] private Vector2 _panelAnchoredPosition = new Vector2(-160f, 0f);
    [SerializeField] private Vector2 _panelSize = new Vector2(500f, 400f);

    [Header("Rows Root Layout")]
    [SerializeField] private Vector2 _rowsAnchorMin = new Vector2(0f, 1f);
    [SerializeField] private Vector2 _rowsAnchorMax = new Vector2(1f, 1f);
    [SerializeField] private Vector2 _rowsPivot = new Vector2(0.5f, 1f);
    [SerializeField] private Vector2 _rowsAnchoredPosition = new Vector2(0f, -82f);
    [SerializeField] private Vector2 _rowsSize = new Vector2(0f, 310f);
    [SerializeField] private RectOffset _rowsPadding;
    [SerializeField] private float _rowSpacing = 6f;
    [SerializeField] private TextAnchor _rowsAlignment = TextAnchor.UpperCenter;

    [Header("Row Layout")]
    [SerializeField] private float _rowHeight = 54f;
    [SerializeField] private RectOffset _rowPadding;
    [SerializeField] private float _columnSpacing = 8f;
    [SerializeField] private Vector2 _iconSize = new Vector2(60f, 60f);
    [SerializeField] private float _nameWidth = 80f;
    [SerializeField] private float _probabilityWidth = 80f;
    [SerializeField] private float _scoreWidth = 80f;

    [Header("Text")]
    [SerializeField] private int _nameFontSize = 30;
    [SerializeField] private int _descriptionFontSize = 24;
    [SerializeField] private int _probabilityFontSize = 30;
    [SerializeField] private int _scoreFontSize = 30;
    [SerializeField] private FontStyles _nameFontStyle = FontStyles.Bold;
    [SerializeField] private FontStyles _probabilityFontStyle = FontStyles.Bold;
    [SerializeField] private FontStyles _scoreFontStyle = FontStyles.Bold;
    [SerializeField] private TextAlignmentOptions _nameAlignment = TextAlignmentOptions.Left;
    [SerializeField] private TextAlignmentOptions _probabilityAlignment = TextAlignmentOptions.Right;
    [SerializeField] private TextAlignmentOptions _scoreAlignment = TextAlignmentOptions.Right;

    [Header("Style")]
    [SerializeField] private Color _rowColor = new Color(1f, 1f, 1f, 0.08f);
    [SerializeField] private Color _nameTextColor = new Color(0.96f, 0.96f, 0.92f, 1f);
    [SerializeField] private Color _probabilityTextColor = new Color(1f, 0.78f, 0.28f, 1f);
    [SerializeField] private Color _scoreTextColor = new Color(0.96f, 0.96f, 0.92f, 1f);
    [SerializeField] private Color _enhancedScoreTextColor = new Color(0.35f, 0.65f, 1f, 1f);

    [Header("Animation")]
    [SerializeField] private float _showFadeDuration = 0.16f;
    [SerializeField] private float _hideFadeDuration = 0.12f;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private TextMeshProUGUI _descriptionText;
    private RectTransform _rowsRoot;
    private Tween _fadeTween;
    private readonly List<RowView> _rows = new List<RowView>();

    private class RowView
    {
        public GameObject Root;
        public Image Background;
        public HorizontalLayoutGroup Layout;
        public Image Icon;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ProbabilityText;
        public TextMeshProUGUI ScoreText;
    }

    private void Awake()
    {
        EnsureSerializedDefaults();
        EnsureViewBuilt();
    }

    private void OnValidate()
    {
        EnsureSerializedDefaults();
        ApplyLayoutSettings();
    }

    public void Bind(ScratchCardFocusPanelModel model)
    {
        EnsureViewBuilt();
        ApplyLayoutSettings();

        if (model == null)
        {
            Hide(true);
            return;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = model.WinDescription;
        }

        IReadOnlyList<ScratchCardFocusPatternInfo> patterns = model.Patterns;
        int count = patterns != null ? patterns.Count : 0;
        EnsureRowCount(count);

        for (int i = 0; i < _rows.Count; i++)
        {
            RowView row = _rows[i];
            bool visible = i < count;
            row.Root.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            ScratchCardFocusPatternInfo pattern = patterns[i];
            row.NameText.text = pattern.PatternName;
            row.ProbabilityText.text = $"{pattern.Probability * 100f:0.#}%";
            row.ScoreText.text = $"+{pattern.BaseScore}";
            row.ProbabilityText.color = pattern.IsProbabilityEnhanced ? _enhancedScoreTextColor : _probabilityTextColor;
            row.ScoreText.color = pattern.IsBaseScoreEnhanced ? _enhancedScoreTextColor : _scoreTextColor;

            Sprite sprite = AssetProvider.LoadSpriteFromAtlas(pattern.AtlasPath, pattern.SpriteName);
            row.Icon.sprite = sprite;
            row.Icon.enabled = sprite != null;

            ApplyRowSettings(row);
        }
    }

    public void Show()
    {
        EnsureViewBuilt();
        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = false;

        _fadeTween?.Kill();
        _fadeTween = _canvasGroup
            .DOFade(1f, _showFadeDuration)
            .SetUpdate(true)
            .OnComplete(() => _fadeTween = null);
    }

    public void Hide(bool instant = false)
    {
        if (_canvasGroup == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _fadeTween?.Kill();
        _canvasGroup.blocksRaycasts = false;

        if (instant)
        {
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            return;
        }

        _fadeTween = _canvasGroup
            .DOFade(0f, _hideFadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _fadeTween = null;
            });
    }

    private void EnsureViewBuilt()
    {
        if (_rectTransform != null)
        {
            return;
        }

        _rectTransform = transform as RectTransform;
        if (_rectTransform == null)
        {
            _rectTransform = gameObject.AddComponent<RectTransform>();
        }

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        GameObject descriptionObject = new GameObject("WinDescription", typeof(RectTransform), typeof(TextMeshProUGUI));
        descriptionObject.transform.SetParent(transform, false);
        _descriptionText = descriptionObject.GetComponent<TextMeshProUGUI>();
        PrepareText(_descriptionText);
        _descriptionText.enableWordWrapping = true;
        _descriptionText.overflowMode = TextOverflowModes.Ellipsis;
        _descriptionText.fontSize = _descriptionFontSize;
        _descriptionText.fontStyle = FontStyles.Bold;
        _descriptionText.color = _nameTextColor;
        _descriptionText.alignment = TextAlignmentOptions.TopLeft;

        GameObject rowsRootObject = new GameObject("Rows", typeof(RectTransform), typeof(VerticalLayoutGroup));
        rowsRootObject.transform.SetParent(transform, false);
        _rowsRoot = rowsRootObject.GetComponent<RectTransform>();

        ApplyLayoutSettings();
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private void EnsureRowCount(int count)
    {
        while (_rows.Count < count)
        {
            _rows.Add(CreateRow(_rowsRoot));
        }
    }

    private RowView CreateRow(Transform parent)
    {
        GameObject root = new GameObject("PatternRow", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        root.transform.SetParent(parent, false);

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(root.transform, false);

        GameObject nameObject = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameObject.transform.SetParent(root.transform, false);

        GameObject probabilityObject = new GameObject("Probability", typeof(RectTransform), typeof(TextMeshProUGUI));
        probabilityObject.transform.SetParent(root.transform, false);

        GameObject scoreObject = new GameObject("Score", typeof(RectTransform), typeof(TextMeshProUGUI));
        scoreObject.transform.SetParent(root.transform, false);

        RowView row = new RowView
        {
            Root = root,
            Background = root.GetComponent<Image>(),
            Layout = root.GetComponent<HorizontalLayoutGroup>(),
            Icon = iconObject.GetComponent<Image>(),
            NameText = nameObject.GetComponent<TextMeshProUGUI>(),
            ProbabilityText = probabilityObject.GetComponent<TextMeshProUGUI>(),
            ScoreText = scoreObject.GetComponent<TextMeshProUGUI>()
        };

        row.Icon.preserveAspect = true;
        row.Icon.raycastTarget = false;

        PrepareText(row.NameText);
        PrepareText(row.ProbabilityText);
        PrepareText(row.ScoreText);

        ApplyRowSettings(row);
        return row;
    }

    private void ApplyLayoutSettings()
    {
        if (_rectTransform != null)
        {
            _rectTransform.anchorMin = _panelAnchorMin;
            _rectTransform.anchorMax = _panelAnchorMax;
            _rectTransform.pivot = _panelPivot;
            _rectTransform.anchoredPosition = _panelAnchoredPosition;
            _rectTransform.sizeDelta = _panelSize;
        }

        if (_rowsRoot == null)
        {
            return;
        }

        if (_descriptionText != null)
        {
            RectTransform descriptionRect = _descriptionText.rectTransform;
            descriptionRect.anchorMin = new Vector2(0f, 1f);
            descriptionRect.anchorMax = new Vector2(1f, 1f);
            descriptionRect.pivot = new Vector2(0.5f, 1f);
            descriptionRect.anchoredPosition = new Vector2(0f, -8f);
            descriptionRect.sizeDelta = new Vector2(-16f, 68f);
        }

        VerticalLayoutGroup rowsLayout = _rowsRoot.GetComponent<VerticalLayoutGroup>();
        if (rowsLayout == null)
        {
            rowsLayout = _rowsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        _rowsRoot.anchorMin = _rowsAnchorMin;
        _rowsRoot.anchorMax = _rowsAnchorMax;
        _rowsRoot.pivot = _rowsPivot;
        _rowsRoot.anchoredPosition = _rowsAnchoredPosition;
        _rowsRoot.sizeDelta = _rowsSize;

        rowsLayout.padding = _rowsPadding;
        rowsLayout.spacing = _rowSpacing;
        rowsLayout.childAlignment = _rowsAlignment;
        rowsLayout.childControlWidth = true;
        rowsLayout.childControlHeight = false;
        rowsLayout.childForceExpandWidth = true;
        rowsLayout.childForceExpandHeight = false;
    }

    private void ApplyRowSettings(RowView row)
    {
        if (row == null)
        {
            return;
        }

        if (row.Background == null)
        {
            row.Background = row.Root != null ? row.Root.GetComponent<Image>() : null;
        }

        if (row.Layout == null)
        {
            row.Layout = row.Root != null ? row.Root.GetComponent<HorizontalLayoutGroup>() : null;
        }

        if (row.Background != null)
        {
            row.Background.color = _rowColor;
            row.Background.raycastTarget = false;
        }

        if (row.Layout != null)
        {
            row.Layout.padding = _rowPadding;
            row.Layout.spacing = _columnSpacing;
            row.Layout.childAlignment = TextAnchor.MiddleLeft;
            row.Layout.childControlWidth = true;
            row.Layout.childControlHeight = true;
            row.Layout.childForceExpandWidth = false;
            row.Layout.childForceExpandHeight = false;
        }

        SetLayout(row.Root, -1f, _rowHeight);
        if (row.Icon != null)
        {
            SetLayout(row.Icon.gameObject, _iconSize.x, _iconSize.y);
        }

        ApplyTextStyle(row.NameText, _nameWidth, _nameFontSize, _nameFontStyle, _nameTextColor, _nameAlignment);
        ApplyTextStyle(row.ProbabilityText, _probabilityWidth, _probabilityFontSize, _probabilityFontStyle, row.ProbabilityText.color, _probabilityAlignment);
        ApplyTextStyle(row.ScoreText, _scoreWidth, _scoreFontSize, _scoreFontStyle, row.ScoreText.color, _scoreAlignment);
    }

    private static void PrepareText(TextMeshProUGUI text)
    {
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        AssetProvider.ApplyDefaultTmpFont(text);
    }

    private void ApplyTextStyle(
        TextMeshProUGUI text,
        float preferredWidth,
        int fontSize,
        FontStyles fontStyle,
        Color color,
        TextAlignmentOptions alignment)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        SetLayout(text.gameObject, preferredWidth, _rowHeight);
    }

    private void EnsureSerializedDefaults()
    {
        if (_rowsPadding == null)
        {
            _rowsPadding = new RectOffset(0, 0, 0, 0);
        }

        if (_rowPadding == null)
        {
            _rowPadding = new RectOffset(8, 8, 6, 6);
        }
    }

    private static void SetLayout(GameObject target, float preferredWidth, float preferredHeight)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = target.AddComponent<LayoutElement>();
        }

        if (preferredWidth >= 0f)
        {
            layoutElement.preferredWidth = preferredWidth;
        }

        if (preferredHeight >= 0f)
        {
            layoutElement.preferredHeight = preferredHeight;
        }
    }
}
