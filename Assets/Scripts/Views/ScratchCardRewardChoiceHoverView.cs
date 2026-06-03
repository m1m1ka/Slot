using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScratchCardRewardChoiceHoverView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float _hoverScale = 1.6f;
    [SerializeField] private float _hoverOffsetY = 60f;
    [SerializeField] private float _hoverDuration = 0.16f;
    [SerializeField] private GameObject _outline;
    [SerializeField] private GameObject _descriptionPanel;

    private RectTransform _rectTransform;
    private Vector2 _restingAnchoredPosition;
    private Vector3 _restingScale = Vector3.one;
    private Tween _hoverTween;
    private bool _isHovered;
    private bool _interactionEnabled = true;
    private bool _hasRestingState;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
        CaptureCurrentTransformAsRestingState();
        EnsureOutline();
        SetHoverTargetsVisible(false);
    }

    private void OnEnable()
    {
        _isHovered = false;
        SetHoverTargetsVisible(false);
    }

    public void Configure(float hoverScale, float hoverOffsetY, float hoverDuration, GameObject outline, GameObject descriptionPanel)
    {
        _hoverScale = Mathf.Max(0.01f, hoverScale);
        _hoverOffsetY = hoverOffsetY;
        _hoverDuration = Mathf.Max(0.01f, hoverDuration);
        _outline = outline;
        _descriptionPanel = descriptionPanel;
        EnsureOutline();
        SetHoverTargetsVisible(false);
    }

    public void SetInteractionEnabled(bool enabled)
    {
        _interactionEnabled = enabled;
        if (enabled)
        {
            return;
        }

        EndHover(true);
    }

    public void CaptureCurrentTransformAsRestingState()
    {
        if (_rectTransform == null)
        {
            _rectTransform = transform as RectTransform;
        }

        _restingScale = transform.localScale;
        if (_rectTransform != null)
        {
            _restingAnchoredPosition = _rectTransform.anchoredPosition;
        }

        _hasRestingState = true;
    }

    public void RefreshHoverState(Vector2 screenPosition, Camera eventCamera)
    {
        if (!_interactionEnabled || _rectTransform == null)
        {
            return;
        }

        bool containsPointer = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPosition, eventCamera);
        if (containsPointer && !_isHovered)
        {
            BeginHover();
        }
        else if (!containsPointer && _isHovered)
        {
            EndHover(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_interactionEnabled)
        {
            return;
        }

        if (_rectTransform == null)
        {
            return;
        }

        BeginHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_interactionEnabled)
        {
            return;
        }

        EndHover(false);
    }

    private void BeginHover()
    {
        if (!_hasRestingState)
        {
            CaptureCurrentTransformAsRestingState();
        }

        if (_rectTransform == null)
        {
            return;
        }

        _isHovered = true;
        SetHoverTargetsVisible(true);
        _hoverTween?.Kill();

        Vector3 targetScale = Vector3.one * Mathf.Max(0.01f, _hoverScale);
        Vector2 targetPosition = _restingAnchoredPosition + new Vector2(0f, _hoverOffsetY);
        _hoverTween = DOTween.Sequence()
            .SetUpdate(true)
            .Join(_rectTransform.DOAnchorPos(targetPosition, _hoverDuration).SetEase(Ease.OutCubic))
            .Join(transform.DOScale(targetScale, _hoverDuration).SetEase(Ease.OutCubic))
            .OnComplete(() => _hoverTween = null);
    }

    private void EndHover(bool instant)
    {
        if (!_isHovered && !instant)
        {
            return;
        }

        _isHovered = false;
        SetHoverTargetsVisible(false);
        _hoverTween?.Kill();

        if (instant || _rectTransform == null)
        {
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _restingAnchoredPosition;
            }

            transform.localScale = _restingScale;
            _hoverTween = null;
            return;
        }

        _hoverTween = DOTween.Sequence()
            .SetUpdate(true)
            .Join(_rectTransform.DOAnchorPos(_restingAnchoredPosition, _hoverDuration).SetEase(Ease.OutCubic))
            .Join(transform.DOScale(_restingScale, _hoverDuration).SetEase(Ease.OutCubic))
            .OnComplete(() => _hoverTween = null);
    }

    private void OnDisable()
    {
        _hoverTween?.Kill();
        _hoverTween = null;
        if (_rectTransform != null && _hasRestingState)
        {
            _rectTransform.anchoredPosition = _restingAnchoredPosition;
        }

        transform.localScale = _restingScale;
        SetHoverTargetsVisible(false);
        _isHovered = false;
    }

    private void EnsureOutline()
    {
        if (_outline != null)
        {
            return;
        }

        Transform outline = FindChildRecursive(transform, "Outline");
        _outline = outline != null ? outline.gameObject : null;
    }

    private void SetOutlineVisible(bool visible)
    {
        EnsureOutline();
        if (_outline != null)
        {
            _outline.SetActive(visible);
        }
    }

    private void SetHoverTargetsVisible(bool visible)
    {
        SetOutlineVisible(visible);
        if (_descriptionPanel != null)
        {
            _descriptionPanel.SetActive(visible);
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
}
