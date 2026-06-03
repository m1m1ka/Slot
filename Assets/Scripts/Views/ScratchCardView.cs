using System;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 彩票表现层。
/// 负责入场动画、点击聚焦和刮奖输入，不包含业务结算逻辑。
/// </summary>
public class ScratchCardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler
{
    private const string FloatingTextPrefabPath = "UI/FloatingText";
    private const string PatternRevealHighlightAmountProperty = "_HighlightAmount";
    private const float MaxPatternRevealHighlightDuration = 0.25f;

    [Header("Card Root")]
    [SerializeField] private RectTransform _cardTransform;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("View State")]
    [SerializeField] private GameObject _normalRoot;
    [SerializeField] private GameObject _focusRoot;
    [SerializeField] private GameObject _normalHoverOutline;
    [SerializeField] private TextMeshProUGUI _focusDescriptionText;

    [Header("Scratch Visual")]
    [SerializeField] private Image[] _patternImages;
    [SerializeField] private RawImage[] _scratchCoverImages;
    [SerializeField] private Image _scratchCoverFill;
    [SerializeField] private Slider _scratchProgressSlider;
    [SerializeField] private Button _claimRewardButton;
    [SerializeField] private TextMeshProUGUI _claimRewardText;
    [SerializeField] private TextMeshProUGUI _claimRewardMultiplierText;

    [Header("Scratch Settings")]
    [SerializeField] private int _scratchTextureWidth = 256;
    [SerializeField] private int _scratchTextureHeight = 256;
    [SerializeField] private int _brushRadius = 126;
    [SerializeField] [Range(0.1f, 1f)] private float _brushSoftness = 0.85f;
    [SerializeField] private float _autoClearThreshold = 0.5f;
    [SerializeField] [Range(0.05f, 1f)] private float _sharedMaskAutoClearThreshold = 0.75f;
    [SerializeField] [Range(0.05f, 1f)] private float _cellRevealThreshold = 0.55f;
    [SerializeField] private Color _scratchCoverColor = new Color(0.78f, 0.78f, 0.78f, 1f);
    [SerializeField] private string _questionMaskSpritePath = "Icons/Question";
    [SerializeField] [Range(0.2f, 1.2f)] private float _questionMaskSizeRatio = 0.72f;
    [SerializeField] private Color _questionMaskTint = Color.white;

    [Header("Animation")]
    [SerializeField] private float _spawnDuration = 0.7f;
    [SerializeField] private float _focusScale = 4f;
    [SerializeField] private float _focusDuration = 0.28f;
    [SerializeField] private Vector2 _focusAnchoredPosition = Vector2.zero;
    [SerializeField] private float _focusScaleOvershoot = 1.18f;
    [SerializeField] private float _normalHoverRotationAngle = 15f;
    [SerializeField] private float _normalHoverRotationDuration = 0.18f;
    [SerializeField] private float _scorePulseScale = 1.25f;
    [SerializeField] private float _scorePulseDuration = 0.22f;
    [SerializeField] private float _scoreFloatDistance = 52f;
    [SerializeField] private float _scoreFloatDuration = 0.75f;
    [SerializeField] private int _scoreFloatFontSize = 28;
    [SerializeField] private Color _scoreFloatTextColor = new Color(0.96f, 0.96f, 0.92f, 1f);
    [SerializeField] private Color _enhancedScoreFloatTextColor = new Color(0.35f, 0.65f, 1f, 1f);
    [SerializeField] private string _patternRevealHighlightShaderPath = "Shaders/UIWhiteHighlight";
    [SerializeField] private float _patternRevealHighlightDuration = 0.45f;
    [SerializeField] private string _giantFruitMaterialPath = "Materials/RainbowOutline";
    [SerializeField] private float _giantFruitScale = 1.5f;
    [SerializeField] private float _claimRewardButtonBreathScale = 1.08f;
    [SerializeField] private float _claimRewardButtonBreathDuration = 0.62f;

    public event Action OnCardClicked;
    public event Action<float, float> OnScratchDragged;
    public event Action OnScratchLayerCleared;
    public event Action<int> OnScratchCellRevealed;
    public event Action OnSpawnAnimationFinished;
    public event Action OnClaimRewardClicked;

    private Tween _spawnTween;
    private Tween _scaleTween;
    private Tween _moveTween;
    private Tween _rotationTween;
    private Tween _claimRewardButtonBreathTween;
    private Vector3 _claimRewardButtonDefaultScale = Vector3.one;
    private bool _hasClaimRewardButtonDefaultScale;
    private Vector3 _defaultScale = Vector3.one;
    private Quaternion _defaultRotation = Quaternion.identity;
    private bool _isFocused;
    private bool _scratchInputEnabled;
    private Vector2 _restingAnchoredPosition;
    private Canvas _parentCanvas;
    private Vector2 _lastHoverScreenPosition = new Vector2(float.MinValue, float.MinValue);
    private IReadOnlyList<ScratchCellModel> _boundCells;
    private string _cardDescription;
    private bool _isPointerHovering;
    private float _currentHoverRotationDirection = 1f;
    private Texture2D _questionMaskTexture;
    private Color32[] _questionMaskPixels;
    private Rect _questionMaskTextureRect;
    private readonly List<ScratchSurfaceRuntime> _scratchSurfaces = new List<ScratchSurfaceRuntime>();
    private readonly HashSet<int> _revealedCellIndices = new HashSet<int>();
    private readonly Dictionary<RawImage, Texture> _scratchCoverSourceTextures = new Dictionary<RawImage, Texture>();
    private readonly Dictionary<Image, Color> _patternImageDefaultColors = new Dictionary<Image, Color>();
    private readonly Dictionary<Image, Vector3> _patternImageDefaultScales = new Dictionary<Image, Vector3>();
    private readonly Dictionary<Image, Material> _patternImageDefaultMaterials = new Dictionary<Image, Material>();
    private readonly Dictionary<Image, Material> _patternRevealHighlightMaterials = new Dictionary<Image, Material>();
    private readonly Dictionary<Image, Material> _giantFruitMaterials = new Dictionary<Image, Material>();
    private readonly Dictionary<Image, Tween> _patternRevealHighlightTweens = new Dictionary<Image, Tween>();
    private readonly HashSet<Image> _giantFruitPatternImages = new HashSet<Image>();
    private Shader _patternRevealHighlightShader;
    private Material _giantFruitMaterial;
    private bool _scratchSurfaceNeedsLayoutRefresh;

    private class ScratchSurfaceRuntime
    {
        public RawImage CoverImage;
        public RectTransform CoverRect;
        public Texture2D Texture;
        public Color32[] Pixels;
        public int Width;
        public int Height;
        public int TotalPixelCount;
        public int CellIndex;
        public float ClearedAlphaAmount;
        public float MaxAlphaAmount;
        public bool IsFullyCleared;
    }

    private void Awake()
    {
        if (_cardTransform == null)
        {
            _cardTransform = transform as RectTransform;
        }

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        _parentCanvas = GetComponentInParent<Canvas>();
        _defaultScale = _cardTransform != null ? _cardTransform.localScale : Vector3.one;
        _defaultRotation = _cardTransform != null ? _cardTransform.localRotation : Quaternion.identity;
        ResolveViewStateRoots();
        EnsureFocusDescriptionText();
        UpdateNormalHoverOutline();
        DisableTextRaycastTargets();
        EnsureClaimRewardButton();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ResolveViewStateRoots();
        EnsureFocusDescriptionText();
    }
#endif

    public void BindCardData(IReadOnlyList<ScratchCellModel> cells, string description = null)
    {
        _boundCells = cells;
        _cardDescription = description;
        BindPatternImages(cells);
        UpdateFocusDescription();
        _scratchSurfaceNeedsLayoutRefresh = true;
    }

    public void SetupInitialVisual()
    {
        SetFocused(false, instant: true);
        RefreshScratchLayoutNow();
        InitializeScratchSurface();
        SetScratchProgress(0f);
        SetClaimRewardMultiplier(1d);
        SetCurrentRewardText(0, false);
        SetScratchInputEnabled(true);
        HideClaimRewardButton();
    }

    public void PlaySpawnAnimation(Vector2 fromAnchoredPosition, Vector2 toAnchoredPosition)
    {
        if (_cardTransform == null)
        {
            return;
        }

        KillTweens();
        _restingAnchoredPosition = toAnchoredPosition;

        _cardTransform.anchoredPosition = fromAnchoredPosition;
        _cardTransform.localScale = _defaultScale;
        _cardTransform.localRotation = _defaultRotation;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, _spawnDuration).SetUpdate(true);
        }

        _spawnTween = _cardTransform
            .DOAnchorPos(toAnchoredPosition, _spawnDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _spawnTween = null;
                OnSpawnAnimationFinished?.Invoke();
            });
    }

    public void SetFocused(bool focused, bool instant = false)
    {
        _isFocused = focused;
        _scratchInputEnabled = focused;
        SetViewStateVisible(focused);
        if (focused && _scratchSurfaceNeedsLayoutRefresh)
        {
            RefreshScratchLayoutNow();
            InitializeScratchSurface();
            _scratchSurfaceNeedsLayoutRefresh = false;
        }

        UpdateNormalHoverOutline();
        UpdateNormalHoverRotation(instant);

        if (_cardTransform == null)
        {
            return;
        }

        _scaleTween?.Kill();

        if (instant)
        {
            _cardTransform.localScale = focused ? _defaultScale * _focusScale : _defaultScale;
            _cardTransform.anchoredPosition = focused ? _focusAnchoredPosition : _restingAnchoredPosition;
            _cardTransform.localRotation = _defaultRotation;
            return;
        }

        _moveTween?.Kill();
        if (focused)
        {
            Vector3 targetScale = _defaultScale * _focusScale;
            Vector3 overshootScale = targetScale * Mathf.Max(1f, _focusScaleOvershoot);
            float settleDuration = Mathf.Max(0.08f, _focusDuration * 0.45f);

            _scaleTween = DOTween.Sequence()
                .SetUpdate(true)
                .Append(_cardTransform.DOScale(overshootScale, _focusDuration).SetEase(Ease.OutCubic))
                .Append(_cardTransform.DOScale(targetScale, settleDuration).SetEase(Ease.InOutSine))
                .OnComplete(() => _scaleTween = null);
        }
        else
        {
            _scaleTween = _cardTransform
                .DOScale(_defaultScale, _focusDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() => _scaleTween = null);
        }

        _moveTween = _cardTransform
            .DOAnchorPos(focused ? _focusAnchoredPosition : _restingAnchoredPosition, _focusDuration)
            .SetEase(focused ? Ease.OutCubic : Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() => _moveTween = null);
    }

    public void SetScratchProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (_scratchCoverFill != null)
        {
            _scratchCoverFill.fillAmount = 1f - progress;
        }

        if (_scratchProgressSlider != null)
        {
            _scratchProgressSlider.value = progress;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerHovering = true;
        _currentHoverRotationDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        UpdateNormalHoverOutline();
        UpdateNormalHoverRotation(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerHovering = false;
        UpdateNormalHoverOutline();
        UpdateNormalHoverRotation(false);
    }

    private void ResolveViewStateRoots()
    {
        if (_normalRoot == null)
        {
            Transform normal = transform.Find("Normal");
            if (normal != null)
            {
                _normalRoot = normal.gameObject;
            }
        }

        if (_focusRoot == null)
        {
            Transform focus = transform.Find("Focus");
            if (focus != null)
            {
                _focusRoot = focus.gameObject;
            }
        }
    }

    private void EnsureFocusDescriptionText()
    {
        if (_focusDescriptionText != null)
        {
            return;
        }

        ResolveViewStateRoots();
        Transform searchRoot = _focusRoot != null ? _focusRoot.transform : transform;
        Transform descriptionTransform = FindChildRecursive(searchRoot, "Description");
        if (descriptionTransform != null)
        {
            _focusDescriptionText = descriptionTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    private void UpdateFocusDescription()
    {
        EnsureFocusDescriptionText();
        if (_focusDescriptionText == null)
        {
            return;
        }

        AssetProvider.ApplyDefaultTmpFont(_focusDescriptionText);
        _focusDescriptionText.text = string.IsNullOrWhiteSpace(_cardDescription) ? string.Empty : _cardDescription;
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

    private void SetViewStateVisible(bool focused)
    {
        if (_normalRoot != null)
        {
            _normalRoot.SetActive(!focused);
        }

        if (_focusRoot != null)
        {
            _focusRoot.SetActive(focused);
        }
    }

    private void UpdateNormalHoverOutline()
    {
        if (_normalHoverOutline == null)
        {
            return;
        }

        _normalHoverOutline.SetActive(!_isFocused && _isPointerHovering);
    }

    private void UpdateNormalHoverRotation(bool instant)
    {
        if (_cardTransform == null)
        {
            return;
        }

        _rotationTween?.Kill();

        bool shouldRotate = !_isFocused && _isPointerHovering;
        Vector3 targetEuler = _defaultRotation.eulerAngles;
        if (shouldRotate)
        {
            targetEuler.z += _normalHoverRotationAngle * _currentHoverRotationDirection;
        }

        if (instant)
        {
            _cardTransform.localRotation = Quaternion.Euler(targetEuler);
            return;
        }

        _rotationTween = _cardTransform
            .DOLocalRotate(targetEuler, _normalHoverRotationDuration, RotateMode.Fast)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => _rotationTween = null);
    }

    public void ShowClaimRewardButton(int displayScore, double rewardMultiplier = 1d)
    {
        EnsureClaimRewardButton();
        if (_claimRewardButton == null)
        {
            return;
        }

        if (_claimRewardText != null)
        {
            _claimRewardText.text = FormatRewardAmount(displayScore);
            _claimRewardText.gameObject.SetActive(true);
        }

        SetClaimRewardMultiplier(rewardMultiplier, true);
        _claimRewardButton.gameObject.SetActive(true);
        _claimRewardButton.interactable = true;
        PlayClaimRewardButtonBreath();
    }

    public void ShowSettleButton(double rewardMultiplier = 1d)
    {
        EnsureClaimRewardButton();
        if (_claimRewardButton == null)
        {
            return;
        }

        if (_claimRewardText != null)
        {
            _claimRewardText.text = "\u7ed3\u7b97";
            _claimRewardText.gameObject.SetActive(true);
        }

        SetClaimRewardMultiplier(rewardMultiplier, true);
        _claimRewardButton.gameObject.SetActive(true);
        _claimRewardButton.interactable = true;
        StopClaimRewardButtonBreath();
    }

    public void ShowSettlementInProgressButton(int displayScore, double rewardMultiplier = 1d)
    {
        EnsureClaimRewardButton();
        if (_claimRewardButton == null)
        {
            return;
        }

        if (_claimRewardText != null)
        {
            _claimRewardText.text = FormatRewardAmount(displayScore);
            _claimRewardText.gameObject.SetActive(true);
        }

        SetClaimRewardMultiplier(rewardMultiplier, true);
        _claimRewardButton.gameObject.SetActive(true);
        _claimRewardButton.interactable = false;
        StopClaimRewardButtonBreath();
    }

    public void HideClaimRewardButton()
    {
        StopClaimRewardButtonBreath();
        if (_claimRewardButton == null)
        {
            return;
        }

        _claimRewardButton.interactable = false;
        _claimRewardButton.gameObject.SetActive(false);
        SetClaimRewardMultiplier(1d, false);
    }

    public void SetCurrentRewardText(int reward, bool visible)
    {
        SetCurrentRewardText(reward, visible, null);
    }

    public void SetCurrentRewardText(int reward, bool visible, double? rewardMultiplier)
    {
        if (_claimRewardText != null)
        {
            _claimRewardText.text = FormatRewardAmount(reward);
            _claimRewardText.gameObject.SetActive(visible);
        }

        if (rewardMultiplier.HasValue)
        {
            SetClaimRewardMultiplier(rewardMultiplier.Value, visible);
        }
        else if (_claimRewardMultiplierText != null)
        {
            _claimRewardMultiplierText.gameObject.SetActive(false);
        }
    }

    public void SetClaimRewardMultiplier(double rewardMultiplier)
    {
        SetClaimRewardMultiplier(rewardMultiplier, true);
    }

    public void SetClaimRewardMultiplier(double rewardMultiplier, bool visible)
    {
        EnsureClaimRewardMultiplierText();
        if (_claimRewardMultiplierText == null)
        {
            return;
        }

        _claimRewardMultiplierText.text = FormatRewardMultiplier(rewardMultiplier);
        _claimRewardMultiplierText.gameObject.SetActive(visible && ShouldShowRewardMultiplier(rewardMultiplier));
    }

    public void SetScratchInputEnabled(bool enabled)
    {
        _scratchInputEnabled = enabled;
    }

    public RectTransform PlayPatternScorePulse(int cellIndex)
    {
        if (_patternImages == null || cellIndex < 0 || cellIndex >= _patternImages.Length)
        {
            return null;
        }

        Image targetImage = _patternImages[cellIndex];
        if (targetImage == null || !targetImage.gameObject.activeInHierarchy)
        {
            return null;
        }

        RectTransform targetRect = targetImage.rectTransform;
        Vector3 originalScale = targetRect.localScale;
        Quaternion originalRotation = targetRect.localRotation;

        targetRect.DOKill();
        targetRect.localScale = originalScale;
        targetRect.localRotation = originalRotation;

        Sequence patternSequence = DOTween.Sequence().SetUpdate(true);
        patternSequence
            .Append(targetRect.DOScale(originalScale * _scorePulseScale, _scorePulseDuration * 0.5f).SetEase(Ease.OutBack))
            .Join(targetRect.DORotate(new Vector3(0f, 0f, 360f), _scorePulseDuration, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic))
            .Append(targetRect.DOScale(originalScale, _scorePulseDuration * 0.5f).SetEase(Ease.OutCubic))
            .OnComplete(() =>
            {
                targetRect.localScale = originalScale;
                targetRect.localRotation = originalRotation;
            });

        return targetRect;
    }

    public void PlayPatternRevealHighlight(int cellIndex)
    {
        if (_patternImages == null || cellIndex < 0 || cellIndex >= _patternImages.Length)
        {
            return;
        }

        Image targetImage = _patternImages[cellIndex];
        if (targetImage == null || !targetImage.gameObject.activeInHierarchy)
        {
            return;
        }

        Material highlightMaterial = _giantFruitPatternImages.Contains(targetImage)
            ? EnsureGiantFruitMaterial(targetImage)
            : EnsurePatternRevealHighlightMaterial(targetImage);
        if (highlightMaterial == null)
        {
            return;
        }

        if (_patternRevealHighlightTweens.TryGetValue(targetImage, out Tween activeTween))
        {
            activeTween?.Kill();
            _patternRevealHighlightTweens.Remove(targetImage);
            ClearPatternRevealHighlight(targetImage);
        }

        targetImage.material = highlightMaterial;
        SetPatternRevealHighlightAmount(highlightMaterial, 1f);
        _patternRevealHighlightTweens[targetImage] = DOTween
            .To(
                () => GetPatternRevealHighlightAmount(highlightMaterial),
                amount =>
                {
                    SetPatternRevealHighlightAmount(highlightMaterial, amount);
                    targetImage.SetMaterialDirty();
                },
                0f,
                GetPatternRevealHighlightDuration())
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                SetPatternRevealHighlightAmount(highlightMaterial, 0f);
                targetImage.SetMaterialDirty();
                _patternRevealHighlightTweens.Remove(targetImage);
                RestorePatternRevealMaterial(targetImage);
            });
    }

    public void RefreshPatternVisual(int cellIndex, ScratchCellModel cell)
    {
        if (cell == null || !TryGetPatternImage(cellIndex, out Image targetImage))
        {
            return;
        }

        var patternConfig = ScratchPatternDefaultProvider.GetById(cell.PatternId);
        Sprite sprite = patternConfig != null
            ? AssetProvider.LoadPatternSprite(patternConfig)
            : null;
        targetImage.sprite = sprite;
        targetImage.enabled = sprite != null;
        targetImage.color = GetPatternImageDefaultColor(targetImage);
        RestorePatternMaterial(targetImage);

        if (cell.IsGiantFruit)
        {
            ApplyGiantFruitPatternVisual(targetImage);
        }
    }

    public void SetGiantFruitPatternVisual(int cellIndex, bool isGiant)
    {
        if (!TryGetPatternImage(cellIndex, out Image targetImage))
        {
            return;
        }

        if (isGiant)
        {
            ApplyGiantFruitPatternVisual(targetImage);
            return;
        }

        ClearGiantFruitPatternVisual(targetImage);
    }

    public void ApplyGiantFruitPatternVisual(int cellIndex)
    {
        SetGiantFruitPatternVisual(cellIndex, true);
    }

    public void ClearGiantFruitPatternVisual(int cellIndex)
    {
        SetGiantFruitPatternVisual(cellIndex, false);
    }

    public void PlayPatternScoreReveal(int cellIndex, int score, bool isEnhanced, double scoreMultiplier = 1d)
    {
        RectTransform targetRect = PlayPatternScorePulse(cellIndex);
        if (targetRect == null)
        {
            return;
        }

        int displayScore = ScratchSettlementResult.ApplyMultiplier(score, scoreMultiplier);
        PlayFloatingText(targetRect, NumberFormatter.FormatCompact(displayScore), isEnhanced ? _enhancedScoreFloatTextColor : _scoreFloatTextColor);
    }

    public void PlayPatternEffectTextReveal(int cellIndex, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        RectTransform targetRect = PlayPatternScorePulse(cellIndex);
        if (targetRect == null)
        {
            return;
        }

        PlayFloatingText(targetRect, text, _scoreFloatTextColor);
    }

    public bool ContainsScreenPoint(Vector2 screenPoint)
    {
        if (_cardTransform == null)
        {
            return false;
        }

        Camera eventCamera = null;
        if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = _parentCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(_cardTransform, screenPoint, eventCamera);
    }

    public bool ContainsClaimRewardButtonScreenPoint(Vector2 screenPoint)
    {
        if (_claimRewardButton == null || !_claimRewardButton.gameObject.activeInHierarchy)
        {
            return false;
        }

        RectTransform buttonRect = _claimRewardButton.transform as RectTransform;
        if (buttonRect == null)
        {
            return false;
        }

        Camera eventCamera = null;
        if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = _parentCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPoint, eventCamera);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isFocused || !_scratchInputEnabled)
        {
            return;
        }

        TryScratchAt(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isFocused || !_scratchInputEnabled)
        {
            return;
        }

        TryScratchAt(eventData);
    }

    private void Update()
    {
        if (!_isFocused || !_scratchInputEnabled || _scratchSurfaces.Count == 0)
        {
            return;
        }

        Vector2 screenPoint = Input.mousePosition;
        if (!ContainsScreenPoint(screenPoint))
        {
            _lastHoverScreenPosition = new Vector2(float.MinValue, float.MinValue);
            return;
        }

        if (Vector2.Distance(_lastHoverScreenPosition, screenPoint) < 1f)
        {
            return;
        }

        float horizontalDelta = _lastHoverScreenPosition.x > float.MinValue
            ? screenPoint.x - _lastHoverScreenPosition.x
            : 0f;
        _lastHoverScreenPosition = screenPoint;
        TryScratchAt(screenPoint, horizontalDelta);
    }

    private void OnDisable()
    {
        KillTweens();
    }

    private void OnDestroy()
    {
        if (_claimRewardButton != null)
        {
            _claimRewardButton.onClick.RemoveListener(HandleClaimRewardButtonClicked);
        }

        for (int i = 0; i < _scratchSurfaces.Count; i++)
        {
            if (_scratchSurfaces[i].Texture != null)
            {
                Destroy(_scratchSurfaces[i].Texture);
                _scratchSurfaces[i].Texture = null;
            }
        }

        if (_questionMaskTexture != null)
        {
            Destroy(_questionMaskTexture);
            _questionMaskTexture = null;
            _questionMaskPixels = null;
        }

        KillPatternRevealTweens();
        DestroyPatternRevealMaterials();
    }

    private void InitializeScratchSurface()
    {
        for (int i = 0; i < _scratchSurfaces.Count; i++)
        {
            if (_scratchSurfaces[i].Texture != null)
            {
                Destroy(_scratchSurfaces[i].Texture);
            }
        }

        _scratchSurfaces.Clear();
        _revealedCellIndices.Clear();

        if (_scratchCoverImages == null || _scratchCoverImages.Length == 0)
        {
            return;
        }

        for (int i = 0; i < _scratchCoverImages.Length; i++)
        {
            RawImage coverImage = _scratchCoverImages[i];
            if (coverImage == null)
            {
                continue;
            }

            if (!coverImage.gameObject.activeSelf)
            {
                continue;
            }

            if (!_scratchCoverSourceTextures.TryGetValue(coverImage, out Texture sourceTexture))
            {
                sourceTexture = coverImage.texture;
                _scratchCoverSourceTextures[coverImage] = sourceTexture;
            }

            var surface = new ScratchSurfaceRuntime
            {
                CoverImage = coverImage,
                CoverRect = coverImage.rectTransform,
                Width = _scratchTextureWidth,
                Height = _scratchTextureHeight,
                TotalPixelCount = _scratchTextureWidth * _scratchTextureHeight,
                CellIndex = i,
                ClearedAlphaAmount = 0f,
                IsFullyCleared = false,
            };

            surface.Texture = CreateScratchTextureCopy(sourceTexture, surface.Width, surface.Height);
            surface.Pixels = surface.Texture.GetPixels32();
            CompositeQuestionMarkers(surface);
            surface.MaxAlphaAmount = CalculateMaxAlphaAmount(surface.Pixels);

            surface.Texture.SetPixels32(surface.Pixels);
            surface.Texture.Apply();
            surface.CoverImage.texture = surface.Texture;

            _scratchSurfaces.Add(surface);
        }
    }

    private void RefreshScratchLayoutNow()
    {
        Canvas.ForceUpdateCanvases();

        RectTransform rootRect = _cardTransform != null ? _cardTransform : transform as RectTransform;
        if (rootRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
        }

        if (_focusRoot != null)
        {
            RectTransform focusRect = _focusRoot.transform as RectTransform;
            if (focusRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(focusRect);
            }
        }

        Canvas.ForceUpdateCanvases();
    }

    private Texture2D CreateScratchTextureCopy(Texture sourceTexture, int width, int height)
    {
        Texture2D runtimeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        if (sourceTexture == null)
        {
            FillTexture(runtimeTexture, _scratchCoverColor);
            return runtimeTexture;
        }

        RenderTexture previousRenderTexture = RenderTexture.active;
        RenderTexture temporaryRenderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

        Graphics.Blit(sourceTexture, temporaryRenderTexture);
        RenderTexture.active = temporaryRenderTexture;
        runtimeTexture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        runtimeTexture.Apply();

        RenderTexture.active = previousRenderTexture;
        RenderTexture.ReleaseTemporary(temporaryRenderTexture);
        return runtimeTexture;
    }

    private void CompositeQuestionMarkers(ScratchSurfaceRuntime surface)
    {
        if (surface == null || surface.Pixels == null || _patternImages == null || _boundCells == null)
        {
            return;
        }

        if (!EnsureQuestionMaskTexture())
        {
            return;
        }

        bool usesSharedScratchCover = IsUsingSingleSerializedScratchCover();
        for (int cellIndex = 0; cellIndex < _patternImages.Length; cellIndex++)
        {
            if (!usesSharedScratchCover && cellIndex != surface.CellIndex)
            {
                continue;
            }

            if (cellIndex >= _boundCells.Count || _boundCells[cellIndex] == null || !_boundCells[cellIndex].IsScratchable)
            {
                continue;
            }

            Image patternImage = _patternImages[cellIndex];
            if (patternImage == null || !patternImage.gameObject.activeInHierarchy)
            {
                continue;
            }

            Rect patternPixelRect = GetPatternPixelRect(patternImage.rectTransform, surface);
            if (patternPixelRect.width <= 0f || patternPixelRect.height <= 0f)
            {
                continue;
            }

            DrawQuestionMask(surface, patternPixelRect);
        }
    }

    private bool EnsureQuestionMaskTexture()
    {
        if (_questionMaskPixels != null && _questionMaskTexture != null)
        {
            return true;
        }

        Sprite questionSprite = AssetProvider.Load<Sprite>(_questionMaskSpritePath);
        if (questionSprite == null || questionSprite.texture == null)
        {
            return false;
        }

        _questionMaskTexture = CreateReadableTextureCopy(questionSprite.texture);
        if (_questionMaskTexture == null)
        {
            return false;
        }

        _questionMaskPixels = _questionMaskTexture.GetPixels32();
        _questionMaskTextureRect = questionSprite.textureRect;
        return _questionMaskPixels != null && _questionMaskPixels.Length > 0;
    }

    private Texture2D CreateReadableTextureCopy(Texture sourceTexture)
    {
        if (sourceTexture == null)
        {
            return null;
        }

        Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        RenderTexture previousRenderTexture = RenderTexture.active;
        RenderTexture temporaryRenderTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);

        Graphics.Blit(sourceTexture, temporaryRenderTexture);
        RenderTexture.active = temporaryRenderTexture;
        readableTexture.ReadPixels(new Rect(0f, 0f, sourceTexture.width, sourceTexture.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previousRenderTexture;
        RenderTexture.ReleaseTemporary(temporaryRenderTexture);
        return readableTexture;
    }

    private void DrawQuestionMask(ScratchSurfaceRuntime surface, Rect patternPixelRect)
    {
        float questionAspect = _questionMaskTextureRect.width > 0f && _questionMaskTextureRect.height > 0f
            ? _questionMaskTextureRect.width / _questionMaskTextureRect.height
            : 1f;
        float displayCompensatedAspect = GetTextureAspectForDisplayAspect(surface, questionAspect);
        float maxDrawWidth = patternPixelRect.width * _questionMaskSizeRatio;
        float maxDrawHeight = patternPixelRect.height * _questionMaskSizeRatio;
        float drawWidth = maxDrawWidth;
        float drawHeight = drawWidth / displayCompensatedAspect;

        if (drawHeight > maxDrawHeight)
        {
            drawHeight = maxDrawHeight;
            drawWidth = drawHeight * displayCompensatedAspect;
        }

        int drawPixelWidth = Mathf.RoundToInt(drawWidth);
        int drawPixelHeight = Mathf.RoundToInt(drawHeight);
        if (drawPixelWidth <= 0 || drawPixelHeight <= 0)
        {
            return;
        }

        int drawMinX = Mathf.RoundToInt(patternPixelRect.center.x - drawPixelWidth * 0.5f);
        int drawMinY = Mathf.RoundToInt(patternPixelRect.center.y - drawPixelHeight * 0.5f);
        int drawMaxX = drawMinX + drawPixelWidth - 1;
        int drawMaxY = drawMinY + drawPixelHeight - 1;

        int minX = Mathf.Clamp(drawMinX, 0, surface.Width - 1);
        int maxX = Mathf.Clamp(drawMaxX, 0, surface.Width - 1);
        int minY = Mathf.Clamp(drawMinY, 0, surface.Height - 1);
        int maxY = Mathf.Clamp(drawMaxY, 0, surface.Height - 1);

        if (maxX < minX || maxY < minY)
        {
            return;
        }

        Color32 tint = _questionMaskTint;
        for (int y = minY; y <= maxY; y++)
        {
            float normalizedY = drawPixelHeight > 1 ? (float)(y - drawMinY) / (drawPixelHeight - 1) : 0.5f;
            int sourceY = Mathf.Clamp(
                Mathf.RoundToInt(_questionMaskTextureRect.yMin + normalizedY * (_questionMaskTextureRect.height - 1f)),
                0,
                _questionMaskTexture.height - 1);

            for (int x = minX; x <= maxX; x++)
            {
                float normalizedX = drawPixelWidth > 1 ? (float)(x - drawMinX) / (drawPixelWidth - 1) : 0.5f;
                int sourceX = Mathf.Clamp(
                    Mathf.RoundToInt(_questionMaskTextureRect.xMin + normalizedX * (_questionMaskTextureRect.width - 1f)),
                    0,
                    _questionMaskTexture.width - 1);

                Color32 source = _questionMaskPixels[sourceY * _questionMaskTexture.width + sourceX];
                if (source.a == 0)
                {
                    continue;
                }

                int targetIndex = y * surface.Width + x;
                surface.Pixels[targetIndex] = BlendQuestionPixel(surface.Pixels[targetIndex], source, tint);
            }
        }
    }

    private static float GetTextureAspectForDisplayAspect(ScratchSurfaceRuntime surface, float displayAspect)
    {
        if (surface?.CoverRect == null || surface.Width <= 0 || surface.Height <= 0)
        {
            return Mathf.Max(0.001f, displayAspect);
        }

        Rect coverRect = surface.CoverRect.rect;
        float displayedWidth = Mathf.Abs(coverRect.width);
        float displayedHeight = Mathf.Abs(coverRect.height);
        if (displayedWidth <= 0.001f || displayedHeight <= 0.001f)
        {
            return Mathf.Max(0.001f, displayAspect);
        }

        float textureToDisplayScaleX = displayedWidth / surface.Width;
        float textureToDisplayScaleY = displayedHeight / surface.Height;
        return Mathf.Max(0.001f, displayAspect * textureToDisplayScaleY / textureToDisplayScaleX);
    }

    private static Color32 BlendQuestionPixel(Color32 destination, Color32 source, Color32 tint)
    {
        float sourceAlpha = (source.a / 255f) * (tint.a / 255f);
        float inverseSourceAlpha = 1f - sourceAlpha;
        byte outAlpha = (byte)Mathf.Clamp(Mathf.RoundToInt(source.a * (tint.a / 255f) + destination.a * inverseSourceAlpha), 0, 255);

        return new Color32(
            (byte)Mathf.Clamp(Mathf.RoundToInt(source.r * (tint.r / 255f) * sourceAlpha + destination.r * inverseSourceAlpha), 0, 255),
            (byte)Mathf.Clamp(Mathf.RoundToInt(source.g * (tint.g / 255f) * sourceAlpha + destination.g * inverseSourceAlpha), 0, 255),
            (byte)Mathf.Clamp(Mathf.RoundToInt(source.b * (tint.b / 255f) * sourceAlpha + destination.b * inverseSourceAlpha), 0, 255),
            outAlpha);
    }

    private void FillTexture(Texture2D texture, Color color)
    {
        if (texture == null)
        {
            return;
        }

        Color32 coverColor = color;
        Color32[] pixels = new Color32[texture.width * texture.height];
        for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
        {
            pixels[pixelIndex] = coverColor;
        }

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private static float CalculateMaxAlphaAmount(Color32[] pixels)
    {
        if (pixels == null || pixels.Length == 0)
        {
            return 0f;
        }

        float alphaAmount = 0f;
        for (int i = 0; i < pixels.Length; i++)
        {
            alphaAmount += pixels[i].a;
        }

        return alphaAmount;
    }

    private void TryScratchAt(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        TryScratchAt(eventData.position, eventData.delta.x);
    }

    private void TryScratchAt(Vector2 screenPoint, float horizontalDelta = 0f)
    {
        ScratchSurfaceRuntime hoveredSurface = FindHoveredScratchSurface(screenPoint);
        if (hoveredSurface == null)
        {
            return;
        }

        Camera eventCamera = null;
        if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = _parentCanvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(hoveredSurface.CoverRect, screenPoint, eventCamera, out Vector2 localPoint))
        {
            return;
        }

        Rect rect = hoveredSurface.CoverRect.rect;
        float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        int pixelX = Mathf.RoundToInt(normalizedX * (hoveredSurface.Width - 1));
        int pixelY = Mathf.RoundToInt(normalizedY * (hoveredSurface.Height - 1));

        float deltaProgress = EraseCircle(hoveredSurface, pixelX, pixelY);
        if (deltaProgress > 0f)
        {
            TryNotifyRevealedCells(hoveredSurface);
            TryAutoClearScratchSurface(hoveredSurface, GetAutoClearThreshold());
            TryNotifyRevealedCells(hoveredSurface);
            TryNotifyScratchLayerCleared(hoveredSurface);
            TryFinalizeScratchCompletion();
            TryNotifyRevealedCells(hoveredSurface);
            OnScratchDragged?.Invoke(GetCurrentScratchProgress(), horizontalDelta);
        }
    }

    private float EraseCircle(ScratchSurfaceRuntime surface, int centerX, int centerY)
    {
        if (surface == null || surface.Pixels == null || surface.Pixels.Length == 0)
        {
            return 0f;
        }

        float clearedAlphaThisStroke = 0f;
        int radiusSquared = _brushRadius * _brushRadius;

        int minX = Mathf.Max(0, centerX - _brushRadius);
        int maxX = Mathf.Min(surface.Width - 1, centerX + _brushRadius);
        int minY = Mathf.Max(0, centerY - _brushRadius);
        int maxY = Mathf.Min(surface.Height - 1, centerY + _brushRadius);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int deltaX = x - centerX;
                int deltaY = y - centerY;
                if (deltaX * deltaX + deltaY * deltaY > radiusSquared)
                {
                    continue;
                }

                float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
                float normalizedDistance = Mathf.Clamp01(distance / _brushRadius);
                float eraseStrength = 1f - Mathf.Pow(normalizedDistance, Mathf.Lerp(1.5f, 3.5f, _brushSoftness));
                byte alphaReduction = (byte)Mathf.Clamp(Mathf.RoundToInt(eraseStrength * 255f), 0, 255);
                if (alphaReduction <= 0)
                {
                    continue;
                }

                int pixelIndex = y * surface.Width + x;
                byte oldAlpha = surface.Pixels[pixelIndex].a;
                if (oldAlpha == 0)
                {
                    continue;
                }

                Color32 pixel = surface.Pixels[pixelIndex];
                int newAlpha = Mathf.Max(0, oldAlpha - alphaReduction);
                pixel.a = (byte)newAlpha;
                surface.Pixels[pixelIndex] = pixel;
                clearedAlphaThisStroke += oldAlpha - pixel.a;
            }
        }

        if (clearedAlphaThisStroke <= 0f)
        {
            return 0f;
        }

        surface.ClearedAlphaAmount += clearedAlphaThisStroke;
        surface.Texture.SetPixels32(surface.Pixels);
        surface.Texture.Apply(false);

        float totalMaxAlphaAmount = GetTotalMaxAlphaAmount();
        return totalMaxAlphaAmount > 0f ? clearedAlphaThisStroke / totalMaxAlphaAmount : 0f;
    }

    private float TryAutoClearScratchSurface(ScratchSurfaceRuntime surface, float threshold)
    {
        if (surface == null || surface.Pixels == null || surface.MaxAlphaAmount <= 0f)
        {
            return 0f;
        }

        float clearedRatio = surface.ClearedAlphaAmount / surface.MaxAlphaAmount;
        if (clearedRatio < threshold)
        {
            return 0f;
        }

        float clearedAlphaThisAutoClear = 0f;
        for (int i = 0; i < surface.Pixels.Length; i++)
        {
            if (surface.Pixels[i].a == 0)
            {
                continue;
            }

            Color32 pixel = surface.Pixels[i];
            clearedAlphaThisAutoClear += pixel.a;
            pixel.a = 0;
            surface.Pixels[i] = pixel;
        }

        if (clearedAlphaThisAutoClear <= 0f)
        {
            return 0f;
        }

        surface.ClearedAlphaAmount = surface.MaxAlphaAmount;
        surface.Texture.SetPixels32(surface.Pixels);
        surface.Texture.Apply(false);

        float totalMaxAlphaAmount = GetTotalMaxAlphaAmount();
        return totalMaxAlphaAmount > 0f ? clearedAlphaThisAutoClear / totalMaxAlphaAmount : 0f;
    }

    private float GetAutoClearThreshold()
    {
        return Mathf.Clamp01(IsUsingSingleSharedScratchCover()
            ? _sharedMaskAutoClearThreshold
            : _autoClearThreshold);
    }

    private float GetTotalMaxAlphaAmount()
    {
        float total = 0f;
        for (int i = 0; i < _scratchSurfaces.Count; i++)
        {
            if (_scratchSurfaces[i] != null)
            {
                total += _scratchSurfaces[i].MaxAlphaAmount;
            }
        }

        return total;
    }

    private float GetTotalClearedAlphaAmount()
    {
        float total = 0f;
        for (int i = 0; i < _scratchSurfaces.Count; i++)
        {
            if (_scratchSurfaces[i] != null)
            {
                total += _scratchSurfaces[i].ClearedAlphaAmount;
            }
        }

        return total;
    }

    private float GetCurrentScratchProgress()
    {
        float totalMaxAlphaAmount = GetTotalMaxAlphaAmount();
        if (totalMaxAlphaAmount <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(GetTotalClearedAlphaAmount() / totalMaxAlphaAmount);
    }

    private float TryFinalizeScratchCompletion()
    {
        float totalMaxAlphaAmount = GetTotalMaxAlphaAmount();
        if (totalMaxAlphaAmount <= 0f)
        {
            return 0f;
        }

        float totalClearedAlphaAmount = GetTotalClearedAlphaAmount();
        float clearedRatio = totalClearedAlphaAmount / totalMaxAlphaAmount;
        if (clearedRatio < 0.99f)
        {
            return 0f;
        }

        float finalizedAlphaAmount = 0f;
        for (int surfaceIndex = 0; surfaceIndex < _scratchSurfaces.Count; surfaceIndex++)
        {
            ScratchSurfaceRuntime surface = _scratchSurfaces[surfaceIndex];
            if (surface == null || surface.Pixels == null)
            {
                continue;
            }

            for (int i = 0; i < surface.Pixels.Length; i++)
            {
                if (surface.Pixels[i].a == 0)
                {
                    continue;
                }

                Color32 pixel = surface.Pixels[i];
                finalizedAlphaAmount += pixel.a;
                pixel.a = 0;
                surface.Pixels[i] = pixel;
            }

            surface.ClearedAlphaAmount = surface.MaxAlphaAmount;
            surface.Texture.SetPixels32(surface.Pixels);
            surface.Texture.Apply(false);
        }

        return finalizedAlphaAmount > 0f ? finalizedAlphaAmount / totalMaxAlphaAmount : 0f;
    }

    private void TryNotifyScratchLayerCleared(ScratchSurfaceRuntime surface)
    {
        if (surface == null || surface.IsFullyCleared)
        {
            return;
        }

        if (surface.ClearedAlphaAmount < surface.MaxAlphaAmount)
        {
            return;
        }

        surface.IsFullyCleared = true;
        OnScratchLayerCleared?.Invoke();

        if (!IsUsingSingleSharedScratchCover())
        {
            TryNotifyScratchCellRevealed(surface.CellIndex);
        }
    }

    private void TryNotifyRevealedCells(ScratchSurfaceRuntime surface)
    {
        if (surface == null || _patternImages == null || _boundCells == null)
        {
            return;
        }

        for (int cellIndex = 0; cellIndex < _patternImages.Length; cellIndex++)
        {
            if (_revealedCellIndices.Contains(cellIndex))
            {
                continue;
            }

            if (cellIndex >= _boundCells.Count)
            {
                continue;
            }

            ScratchCellModel cell = _boundCells[cellIndex];
            if (cell == null || !cell.IsScratchable)
            {
                continue;
            }

            Image patternImage = _patternImages[cellIndex];
            if (patternImage == null || !patternImage.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!IsPatternCoveredBySurface(patternImage.rectTransform, surface))
            {
                continue;
            }

            float revealedRatio = CalculatePatternRevealedRatio(patternImage.rectTransform, surface);
            if (revealedRatio >= _cellRevealThreshold)
            {
                TryNotifyScratchCellRevealed(cellIndex);
            }
        }
    }

    private void TryNotifyScratchCellRevealed(int cellIndex)
    {
        if (!_revealedCellIndices.Add(cellIndex))
        {
            return;
        }

        OnScratchCellRevealed?.Invoke(cellIndex);
    }

    private bool IsUsingSingleSharedScratchCover()
    {
        return _scratchSurfaces.Count == 1;
    }

    private bool IsUsingSingleSerializedScratchCover()
    {
        return _scratchCoverImages != null && _scratchCoverImages.Length == 1;
    }

    private bool IsPatternCoveredBySurface(RectTransform patternRect, ScratchSurfaceRuntime surface)
    {
        if (patternRect == null || surface?.CoverRect == null)
        {
            return false;
        }

        Rect pixelRect = GetPatternPixelRect(patternRect, surface);
        return pixelRect.width > 0f && pixelRect.height > 0f;
    }

    private float CalculatePatternRevealedRatio(RectTransform patternRect, ScratchSurfaceRuntime surface)
    {
        if (patternRect == null || surface == null || surface.Pixels == null || surface.Pixels.Length == 0)
        {
            return 0f;
        }

        Rect pixelRect = GetPatternPixelRect(patternRect, surface);
        if (pixelRect.width <= 0f || pixelRect.height <= 0f)
        {
            return 0f;
        }

        int minX = Mathf.Clamp(Mathf.FloorToInt(pixelRect.xMin), 0, surface.Width - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(pixelRect.xMax), 0, surface.Width - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt(pixelRect.yMin), 0, surface.Height - 1);
        int maxY = Mathf.Clamp(Mathf.CeilToInt(pixelRect.yMax), 0, surface.Height - 1);

        int total = 0;
        int revealed = 0;
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                total++;
                if (surface.Pixels[y * surface.Width + x].a <= 8)
                {
                    revealed++;
                }
            }
        }

        return total > 0 ? (float)revealed / total : 0f;
    }

    private Rect GetPatternPixelRect(RectTransform patternRect, ScratchSurfaceRuntime surface)
    {
        if (patternRect == null || surface?.CoverRect == null)
        {
            return Rect.zero;
        }

        Vector3[] corners = new Vector3[4];
        patternRect.GetWorldCorners(corners);

        Rect coverRect = surface.CoverRect.rect;
        float minX = 1f;
        float maxX = 0f;
        float minY = 1f;
        float maxY = 0f;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 localPoint = surface.CoverRect.InverseTransformPoint(corners[i]);
            float normalizedX = Mathf.InverseLerp(coverRect.xMin, coverRect.xMax, localPoint.x);
            float normalizedY = Mathf.InverseLerp(coverRect.yMin, coverRect.yMax, localPoint.y);

            minX = Mathf.Min(minX, normalizedX);
            maxX = Mathf.Max(maxX, normalizedX);
            minY = Mathf.Min(minY, normalizedY);
            maxY = Mathf.Max(maxY, normalizedY);
        }

        minX = Mathf.Clamp01(minX);
        maxX = Mathf.Clamp01(maxX);
        minY = Mathf.Clamp01(minY);
        maxY = Mathf.Clamp01(maxY);

        if (maxX <= minX || maxY <= minY)
        {
            return Rect.zero;
        }

        float pixelMinX = minX * (surface.Width - 1);
        float pixelMaxX = maxX * (surface.Width - 1);
        float pixelMinY = minY * (surface.Height - 1);
        float pixelMaxY = maxY * (surface.Height - 1);
        return Rect.MinMaxRect(pixelMinX, pixelMinY, pixelMaxX, pixelMaxY);
    }

    private void BindPatternImages(IReadOnlyList<ScratchCellModel> cells)
    {
        if (_patternImages == null || _patternImages.Length == 0)
        {
            return;
        }

        for (int i = 0; i < _patternImages.Length; i++)
        {
            Image targetImage = _patternImages[i];
            if (targetImage == null)
            {
                continue;
            }

            ResetPatternImageVisual(targetImage);

            bool hasCell = cells != null && i < cells.Count && cells[i] != null;
            targetImage.gameObject.SetActive(hasCell);
            if (!hasCell)
            {
                continue;
            }

            var patternConfig = ScratchPatternDefaultProvider.GetById(cells[i].PatternId);
            Sprite sprite = patternConfig != null
                ? AssetProvider.LoadPatternSprite(patternConfig)
                : null;
            targetImage.sprite = sprite;
            targetImage.enabled = sprite != null;
            targetImage.color = GetPatternImageDefaultColor(targetImage);
            RestorePatternMaterial(targetImage);
            if (cells[i].IsGiantFruit)
            {
                ApplyGiantFruitPatternVisual(targetImage);
            }
        }

        if (_scratchCoverImages == null)
        {
            return;
        }

        bool usesSharedScratchCover = _scratchCoverImages.Length == 1;
        bool hasScratchableCell = false;
        if (usesSharedScratchCover && cells != null)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] != null && cells[i].IsScratchable)
                {
                    hasScratchableCell = true;
                    break;
                }
            }
        }

        for (int i = 0; i < _scratchCoverImages.Length; i++)
        {
            RawImage coverImage = _scratchCoverImages[i];
            if (coverImage == null)
            {
                continue;
            }

            bool hasCell = usesSharedScratchCover
                ? hasScratchableCell
                : cells != null && i < cells.Count && cells[i] != null && cells[i].IsScratchable;
            coverImage.gameObject.SetActive(hasCell);
        }
    }

    private void PlayFloatingText(RectTransform sourceRect, string value, Color color)
    {
        if (sourceRect == null || _cardTransform == null)
        {
            return;
        }

        GameObject textObject = AssetProvider.InstantiatePrefab(FloatingTextPrefabPath, _cardTransform);
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
        textRect.position = sourceRect.position;

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

        Vector2 endPosition = textRect.anchoredPosition + Vector2.up * _scoreFloatDistance;
        Sequence floatSequence = DOTween.Sequence().SetUpdate(true);
        floatSequence
            .Append(textRect.DOAnchorPos(endPosition, _scoreFloatDuration).SetEase(Ease.OutCubic))
            .Join(canvasGroup.DOFade(0f, _scoreFloatDuration).SetEase(Ease.InCubic))
            .OnComplete(() => Destroy(textObject));
    }

    private ScratchSurfaceRuntime FindHoveredScratchSurface(Vector2 screenPoint)
    {
        Camera eventCamera = null;
        if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = _parentCanvas.worldCamera;
        }

        for (int i = _scratchSurfaces.Count - 1; i >= 0; i--)
        {
            ScratchSurfaceRuntime surface = _scratchSurfaces[i];
            if (surface == null || surface.CoverRect == null || !surface.CoverImage.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(surface.CoverRect, screenPoint, eventCamera))
            {
                return surface;
            }
        }

        return null;
    }

    private void KillTweens()
    {
        _spawnTween?.Kill();
        _scaleTween?.Kill();
        _moveTween?.Kill();
        _rotationTween?.Kill();
        StopClaimRewardButtonBreath();
        KillPatternRevealTweens();
        _spawnTween = null;
        _scaleTween = null;
        _moveTween = null;
        _rotationTween = null;
    }

    private void PlayClaimRewardButtonBreath()
    {
        if (_claimRewardButton == null)
        {
            return;
        }

        _claimRewardButtonBreathTween?.Kill();
        Transform buttonTransform = _claimRewardButton.transform;
        buttonTransform.localScale = _claimRewardButtonDefaultScale;
        Vector3 targetScale = _claimRewardButtonDefaultScale * Mathf.Max(1f, _claimRewardButtonBreathScale);
        float halfDuration = Mathf.Max(0.05f, _claimRewardButtonBreathDuration * 0.5f);

        _claimRewardButtonBreathTween = DOTween.Sequence()
            .SetUpdate(true)
            .Append(buttonTransform.DOScale(targetScale, halfDuration).SetEase(Ease.InOutSine))
            .Append(buttonTransform.DOScale(_claimRewardButtonDefaultScale, halfDuration).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart);
    }

    private void StopClaimRewardButtonBreath()
    {
        _claimRewardButtonBreathTween?.Kill();
        _claimRewardButtonBreathTween = null;
        if (_claimRewardButton != null)
        {
            _claimRewardButton.transform.localScale = _claimRewardButtonDefaultScale;
        }
    }

    private void KillPatternRevealTweens()
    {
        foreach (KeyValuePair<Image, Tween> highlightTween in _patternRevealHighlightTweens)
        {
            highlightTween.Value?.Kill();
            RestorePatternRevealMaterial(highlightTween.Key);
        }

        _patternRevealHighlightTweens.Clear();
    }

    private Color GetPatternImageDefaultColor(Image image)
    {
        if (image == null)
        {
            return Color.white;
        }

        if (_patternImageDefaultColors.TryGetValue(image, out Color defaultColor))
        {
            return defaultColor;
        }

        defaultColor = image.color;
        _patternImageDefaultColors[image] = defaultColor;
        return defaultColor;
    }

    private Vector3 GetPatternImageDefaultScale(Image image)
    {
        if (image == null)
        {
            return Vector3.one;
        }

        if (_patternImageDefaultScales.TryGetValue(image, out Vector3 defaultScale))
        {
            return defaultScale;
        }

        RectTransform rectTransform = image.rectTransform;
        defaultScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        _patternImageDefaultScales[image] = defaultScale;
        return defaultScale;
    }

    private bool TryGetPatternImage(int cellIndex, out Image image)
    {
        image = null;
        if (_patternImages == null || cellIndex < 0 || cellIndex >= _patternImages.Length)
        {
            return false;
        }

        image = _patternImages[cellIndex];
        return image != null && image.gameObject.activeInHierarchy;
    }

    private void ApplyGiantFruitPatternVisual(Image image)
    {
        if (image == null)
        {
            return;
        }

        Material giantFruitMaterial = EnsureGiantFruitMaterial(image);
        if (giantFruitMaterial == null)
        {
            return;
        }

        KillPatternRevealTween(image);
        GetPatternImageDefaultMaterial(image);
        Vector3 defaultScale = GetPatternImageDefaultScale(image);
        RectTransform rectTransform = image.rectTransform;
        if (rectTransform != null)
        {
            rectTransform.localScale = defaultScale * Mathf.Max(0f, _giantFruitScale);
        }

        _giantFruitPatternImages.Add(image);
        image.material = giantFruitMaterial;
    }

    private void ClearGiantFruitPatternVisual(Image image)
    {
        if (image == null)
        {
            return;
        }

        KillPatternRevealTween(image);
        _giantFruitPatternImages.Remove(image);

        RectTransform rectTransform = image.rectTransform;
        if (rectTransform != null)
        {
            rectTransform.localScale = GetPatternImageDefaultScale(image);
        }

        RestorePatternMaterial(image);
        DestroyGiantFruitMaterial(image);
    }

    private void ResetPatternImageVisual(Image image)
    {
        if (image == null)
        {
            return;
        }

        KillPatternRevealTween(image);
        _giantFruitPatternImages.Remove(image);

        RectTransform rectTransform = image.rectTransform;
        if (rectTransform != null)
        {
            rectTransform.localScale = GetPatternImageDefaultScale(image);
        }

        RestorePatternMaterial(image);
        DestroyGiantFruitMaterial(image);
    }

    private Material EnsureGiantFruitMaterial()
    {
        if (_giantFruitMaterial != null)
        {
            return _giantFruitMaterial;
        }

        _giantFruitMaterial = AssetProvider.Load<Material>(_giantFruitMaterialPath);
        return _giantFruitMaterial;
    }

    private Material EnsureGiantFruitMaterial(Image image)
    {
        if (image == null)
        {
            return null;
        }

        if (_giantFruitMaterials.TryGetValue(image, out Material material) && material != null)
        {
            return material;
        }

        Material template = EnsureGiantFruitMaterial();
        if (template == null)
        {
            return null;
        }

        material = new Material(template)
        {
            name = "GiantFruitRainbowOutline_Runtime"
        };
        SetPatternRevealHighlightAmount(material, 0f);
        _giantFruitMaterials[image] = material;
        return material;
    }

    private Material GetPatternImageDefaultMaterial(Image image)
    {
        if (image == null)
        {
            return null;
        }

        if (_patternImageDefaultMaterials.TryGetValue(image, out Material defaultMaterial))
        {
            return defaultMaterial;
        }

        defaultMaterial = image.material;
        _patternImageDefaultMaterials[image] = defaultMaterial;
        return defaultMaterial;
    }

    private Material EnsurePatternRevealHighlightMaterial(Image image)
    {
        if (image == null)
        {
            return null;
        }

        if (_patternRevealHighlightMaterials.TryGetValue(image, out Material material) && material != null)
        {
            return material;
        }

        Shader shader = EnsurePatternRevealHighlightShader();
        if (shader == null)
        {
            return null;
        }

        GetPatternImageDefaultMaterial(image);

        material = new Material(shader)
        {
            name = "PatternRevealHighlight_Runtime"
        };
        SetPatternRevealHighlightAmount(material, 0f);
        _patternRevealHighlightMaterials[image] = material;
        return material;
    }

    private Shader EnsurePatternRevealHighlightShader()
    {
        if (_patternRevealHighlightShader != null)
        {
            return _patternRevealHighlightShader;
        }

        _patternRevealHighlightShader = AssetProvider.Load<Shader>(_patternRevealHighlightShaderPath);
        if (_patternRevealHighlightShader == null)
        {
            _patternRevealHighlightShader = Shader.Find("UI/White Highlight");
        }

        return _patternRevealHighlightShader;
    }

    private void RestorePatternRevealMaterial(Image image)
    {
        RestorePatternMaterial(image);
    }

    private void FinishPatternRevealHighlight(Image image)
    {
        if (image == null)
        {
            return;
        }

        if (_patternRevealHighlightTweens.TryGetValue(image, out Tween activeTween))
        {
            activeTween?.Kill();
            _patternRevealHighlightTweens.Remove(image);
        }

        ClearPatternRevealHighlight(image);
        RestorePatternRevealMaterial(image);
    }

    private void ClearPatternRevealHighlight(Image image)
    {
        if (image == null)
        {
            return;
        }

        if (_giantFruitMaterials.TryGetValue(image, out Material giantFruitMaterial))
        {
            SetPatternRevealHighlightAmount(giantFruitMaterial, 0f);
        }

        if (_patternRevealHighlightMaterials.TryGetValue(image, out Material revealMaterial))
        {
            SetPatternRevealHighlightAmount(revealMaterial, 0f);
        }

        image.SetMaterialDirty();
    }

    private void SetPatternRevealHighlightAmount(Material material, float amount)
    {
        if (material != null && material.HasProperty(PatternRevealHighlightAmountProperty))
        {
            material.SetFloat(PatternRevealHighlightAmountProperty, amount);
        }
    }

    private float GetPatternRevealHighlightAmount(Material material)
    {
        return material != null && material.HasProperty(PatternRevealHighlightAmountProperty)
            ? material.GetFloat(PatternRevealHighlightAmountProperty)
            : 0f;
    }

    private float GetPatternRevealHighlightDuration()
    {
        return Mathf.Clamp(_patternRevealHighlightDuration, 0.01f, MaxPatternRevealHighlightDuration);
    }

    private void RestorePatternMaterial(Image image)
    {
        if (image == null)
        {
            return;
        }

        if (_giantFruitPatternImages.Contains(image))
        {
            Material giantFruitMaterial = EnsureGiantFruitMaterial(image);
            if (giantFruitMaterial != null)
            {
                image.material = giantFruitMaterial;
                return;
            }
        }

        if (_patternImageDefaultMaterials.TryGetValue(image, out Material defaultMaterial))
        {
            image.material = defaultMaterial;
        }
    }

    private void DestroyGiantFruitMaterial(Image image)
    {
        if (image == null || !_giantFruitMaterials.TryGetValue(image, out Material material))
        {
            return;
        }

        _giantFruitMaterials.Remove(image);
        if (material != null)
        {
            Destroy(material);
        }
    }

    private void KillPatternRevealTween(Image image)
    {
        if (image == null)
        {
            return;
        }

        FinishPatternRevealHighlight(image);
    }

    private void DestroyPatternRevealMaterials()
    {
        foreach (KeyValuePair<Image, Material> highlightMaterial in _patternRevealHighlightMaterials)
        {
            if (highlightMaterial.Value != null)
            {
                Destroy(highlightMaterial.Value);
            }
        }

        _patternRevealHighlightMaterials.Clear();

        foreach (KeyValuePair<Image, Material> giantFruitMaterial in _giantFruitMaterials)
        {
            if (giantFruitMaterial.Value != null)
            {
                Destroy(giantFruitMaterial.Value);
            }
        }

        _giantFruitMaterials.Clear();
        _patternImageDefaultMaterials.Clear();
        _patternImageDefaultScales.Clear();
        _giantFruitPatternImages.Clear();
    }

    private void EnsureClaimRewardButton()
    {
        if (_claimRewardButton != null)
        {
            if (!_hasClaimRewardButtonDefaultScale)
            {
                _claimRewardButtonDefaultScale = _claimRewardButton.transform.localScale;
                _hasClaimRewardButtonDefaultScale = true;
            }

            if (_claimRewardText == null)
            {
                _claimRewardText = _claimRewardButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            AssetProvider.ApplyDefaultTmpFont(_claimRewardText);
            EnsureClaimRewardMultiplierText();
            DisableTextRaycastTargets();

            _claimRewardButton.onClick.RemoveListener(HandleClaimRewardButtonClicked);
            _claimRewardButton.onClick.AddListener(HandleClaimRewardButtonClicked);
            return;
        }
    }

    private void DisableTextRaycastTargets()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                texts[i].raycastTarget = false;
            }
        }
    }

    private void EnsureClaimRewardMultiplierText()
    {
        if (_claimRewardMultiplierText == null || _claimRewardMultiplierText.name != "Multiply")
        {
            Transform searchRoot = _focusRoot != null ? _focusRoot.transform : transform;
            Transform multiplyTransform = FindChildRecursive(searchRoot, "Multiply");
            if (multiplyTransform == null && searchRoot != transform)
            {
                multiplyTransform = FindChildRecursive(transform, "Multiply");
            }

            TextMeshProUGUI multiplyText = multiplyTransform != null
                ? multiplyTransform.GetComponent<TextMeshProUGUI>()
                : null;
            if (multiplyText != null)
            {
                _claimRewardMultiplierText = multiplyText;
            }
        }

        AssetProvider.ApplyDefaultTmpFont(_claimRewardMultiplierText);
        if (_claimRewardMultiplierText != null && string.IsNullOrEmpty(_claimRewardMultiplierText.text))
        {
            _claimRewardMultiplierText.text = FormatRewardMultiplier(1d);
        }
    }

    private static string FormatRewardMultiplier(double rewardMultiplier)
    {
        double normalizedMultiplier = rewardMultiplier >= 0d ? rewardMultiplier : 1d;
        return $"×{normalizedMultiplier:0.##}";
    }

    private static bool ShouldShowRewardMultiplier(double rewardMultiplier)
    {
        double normalizedMultiplier = rewardMultiplier >= 0d ? rewardMultiplier : 1d;
        return System.Math.Abs(normalizedMultiplier - 1d) > 0.0001d;
    }

    private static string FormatRewardAmount(int score)
    {
        return NumberFormatter.FormatCompact(score);
    }

    private void HandleClaimRewardButtonClicked()
    {
        OnClaimRewardClicked?.Invoke();
    }
}
