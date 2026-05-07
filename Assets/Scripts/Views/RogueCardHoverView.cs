using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class RogueCardHoverView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float _hoverOffsetY = 80f;
    [SerializeField] private float _hoverDuration = 0.16f;

    private RectTransform _rectTransform;
    private Vector2 _restingAnchoredPosition;
    private Tween _hoverTween;
    private bool _isHovered;
    private bool _hasRestingPosition;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
    }

    private void OnEnable()
    {
        _isHovered = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_rectTransform == null)
        {
            return;
        }

        if (!_isHovered)
        {
            _restingAnchoredPosition = _rectTransform.anchoredPosition;
            _hasRestingPosition = true;
        }

        _isHovered = true;
        _hoverTween?.Kill();
        _hoverTween = _rectTransform
            .DOAnchorPosY(_restingAnchoredPosition.y + _hoverOffsetY, _hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => _hoverTween = null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_rectTransform == null)
        {
            return;
        }

        _isHovered = false;
        _hoverTween?.Kill();
        _hoverTween = _rectTransform
            .DOAnchorPos(_restingAnchoredPosition, _hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => _hoverTween = null);
    }

    private void OnDisable()
    {
        _hoverTween?.Kill();
        _hoverTween = null;

        _isHovered = false;
        if (_rectTransform != null && _hasRestingPosition)
        {
            _rectTransform.anchoredPosition = _restingAnchoredPosition;
        }
    }
}
