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
public class ScratchCardView : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler
{
    [Header("Card Root")]
    [SerializeField] private RectTransform _cardTransform;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Scratch Visual")]
    [SerializeField] private Image[] _patternImages;
    [SerializeField] private RawImage[] _scratchCoverImages;
    [SerializeField] private Image _scratchCoverFill;
    [SerializeField] private Slider _scratchProgressSlider;
    [SerializeField] private Button _claimRewardButton;
    [SerializeField] private TextMeshProUGUI _claimRewardText;

    [Header("Scratch Settings")]
    [SerializeField] private int _scratchTextureWidth = 256;
    [SerializeField] private int _scratchTextureHeight = 256;
    [SerializeField] private int _brushRadius = 126;
    [SerializeField] [Range(0.1f, 1f)] private float _brushSoftness = 0.85f;
    [SerializeField] private float _autoClearThreshold = 0.5f;
    [SerializeField] private Color _scratchCoverColor = new Color(0.78f, 0.78f, 0.78f, 1f);

    [Header("Animation")]
    [SerializeField] private float _spawnDuration = 0.7f;
    [SerializeField] private float _focusScale = 4f;
    [SerializeField] private float _focusDuration = 0.28f;

    public event Action OnCardClicked;
    public event Action<float> OnScratchDragged;
    public event Action OnScratchLayerCleared;
    public event Action<int> OnScratchCellRevealed;
    public event Action OnSpawnAnimationFinished;
    public event Action OnClaimRewardClicked;

    private Tween _spawnTween;
    private Tween _scaleTween;
    private Tween _moveTween;
    private Vector3 _defaultScale = Vector3.one;
    private bool _isFocused;
    private Vector2 _restingAnchoredPosition;
    private Canvas _parentCanvas;
    private Vector2 _lastHoverScreenPosition = new Vector2(float.MinValue, float.MinValue);
    private readonly List<ScratchSurfaceRuntime> _scratchSurfaces = new List<ScratchSurfaceRuntime>();

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
        EnsureClaimRewardButton();
    }

    public void BindCardData(IReadOnlyList<ScratchCellModel> cells)
    {
        BindPatternImages(cells);
    }

    public void SetupInitialVisual()
    {
        InitializeScratchSurface();
        SetScratchProgress(0f);
        SetCurrentRewardText(0, false);
        SetFocused(false, instant: true);
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

        if (_cardTransform == null)
        {
            return;
        }

        _scaleTween?.Kill();

        if (instant)
        {
            _cardTransform.localScale = focused ? _defaultScale * _focusScale : _defaultScale;
            _cardTransform.anchoredPosition = focused ? Vector2.zero : _restingAnchoredPosition;
            return;
        }

        _moveTween?.Kill();
        _scaleTween = _cardTransform
            .DOScale(focused ? _defaultScale * _focusScale : _defaultScale, _focusDuration)
            .SetEase(focused ? Ease.OutBack : Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => _scaleTween = null);

        _moveTween = _cardTransform
            .DOAnchorPos(focused ? Vector2.zero : _restingAnchoredPosition, _focusDuration)
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

    public void ShowClaimRewardButton(int finalScore)
    {
        EnsureClaimRewardButton();
        if (_claimRewardButton == null)
        {
            return;
        }

        _claimRewardButton.gameObject.SetActive(true);
        _claimRewardButton.interactable = true;
    }

    public void HideClaimRewardButton()
    {
        if (_claimRewardButton == null)
        {
            return;
        }

        _claimRewardButton.interactable = false;
        _claimRewardButton.gameObject.SetActive(false);
    }

    public void SetCurrentRewardText(int reward, bool visible)
    {
        if (_claimRewardText == null)
        {
            return;
        }

        _claimRewardText.text = $"+{reward}";
        _claimRewardText.gameObject.SetActive(visible);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isFocused)
        {
            return;
        }

        TryScratchAt(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isFocused)
        {
            return;
        }

        TryScratchAt(eventData);
    }

    private void Update()
    {
        if (!_isFocused || _scratchSurfaces.Count == 0)
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

        _lastHoverScreenPosition = screenPoint;
        TryScratchAt(screenPoint);
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

            surface.Texture = new Texture2D(surface.Width, surface.Height, TextureFormat.RGBA32, false);
            surface.Texture.wrapMode = TextureWrapMode.Clamp;
            surface.Texture.filterMode = FilterMode.Bilinear;
            surface.MaxAlphaAmount = surface.TotalPixelCount * 255f;
            surface.Pixels = new Color32[surface.TotalPixelCount];

            Color32 coverColor = _scratchCoverColor;
            for (int pixelIndex = 0; pixelIndex < surface.Pixels.Length; pixelIndex++)
            {
                surface.Pixels[pixelIndex] = coverColor;
            }

            surface.Texture.SetPixels32(surface.Pixels);
            surface.Texture.Apply();
            surface.CoverImage.texture = surface.Texture;
            surface.CoverImage.color = Color.white;

            _scratchSurfaces.Add(surface);
        }
    }

    private void TryScratchAt(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        TryScratchAt(eventData.position);
    }

    private void TryScratchAt(Vector2 screenPoint)
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

        float currentProgress = GetCurrentScratchProgress();
        OnScratchDragged?.Invoke(currentProgress);

        float deltaProgress = EraseCircle(hoveredSurface, pixelX, pixelY);
        if (deltaProgress > 0f)
        {
            TryAutoClearScratchSurface(hoveredSurface);
            TryNotifyScratchLayerCleared(hoveredSurface);
            TryFinalizeScratchCompletion();
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

    private float TryAutoClearScratchSurface(ScratchSurfaceRuntime surface)
    {
        if (surface == null || surface.Pixels == null || surface.MaxAlphaAmount <= 0f)
        {
            return 0f;
        }

        float clearedRatio = surface.ClearedAlphaAmount / surface.MaxAlphaAmount;
        if (clearedRatio < _autoClearThreshold)
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
        OnScratchCellRevealed?.Invoke(surface.CellIndex);
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

            bool hasCell = cells != null && i < cells.Count && cells[i] != null;
            targetImage.gameObject.SetActive(hasCell);
            if (!hasCell)
            {
                continue;
            }

            var patternConfig = ScratchPatternDefaultProvider.GetById(cells[i].PatternId);
            Sprite sprite = patternConfig != null
                ? AssetProvider.LoadSpriteFromAtlas(patternConfig.AtlasPath, patternConfig.SpriteName)
                : null;
            targetImage.sprite = sprite;
            targetImage.enabled = sprite != null;
        }

        if (_scratchCoverImages == null)
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

            bool hasCell = cells != null && i < cells.Count && cells[i] != null && cells[i].IsScratchable;
            coverImage.gameObject.SetActive(hasCell);
        }
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
        _spawnTween = null;
        _scaleTween = null;
        _moveTween = null;
    }

    private void EnsureClaimRewardButton()
    {
        if (_claimRewardButton != null)
        {
            if (_claimRewardText == null)
            {
                _claimRewardText = _claimRewardButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            _claimRewardButton.onClick.RemoveListener(HandleClaimRewardButtonClicked);
            _claimRewardButton.onClick.AddListener(HandleClaimRewardButtonClicked);
            return;
        }
    }

    private void HandleClaimRewardButtonClicked()
    {
        OnClaimRewardClicked?.Invoke();
    }
}
