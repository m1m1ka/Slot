using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class RogueCardHoverView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float _hoverOffsetY = 80f;
    [SerializeField] private float _hoverDuration = 0.16f;
    [SerializeField] private GameObject _effectOutline;
    [SerializeField] private float _effectScale = 1.2f;
    [SerializeField] private float _effectDuration = 0.2f;

    private RectTransform _rectTransform;
    private Vector2 _restingAnchoredPosition;
    private Tween _hoverTween;
    private Tween _effectTween;
    private Quaternion _initialRotation = Quaternion.identity;
    private Vector3 _initialScale = Vector3.one;
    private bool _isHovered;
    private bool _hasRestingPosition;

    public int CardId { get; private set; }

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
        _initialRotation = transform.localRotation;
        _initialScale = transform.localScale;
        EnsureEffectOutline();
    }

    private void OnEnable()
    {
        _isHovered = false;
    }

    public void BindCardId(int cardId)
    {
        CardId = cardId;
    }

    public void PlayEffectTriggeredAnimation()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        _effectTween?.Kill();
        transform.localRotation = _initialRotation;
        transform.localScale = _initialScale;
        SetEffectOutlineVisible(true);

        float halfDuration = Mathf.Max(0.01f, _effectDuration * 0.5f);
        _effectTween = DOTween.Sequence()
            .SetUpdate(true)
            .Append(transform.DOScale(_initialScale * Mathf.Max(1f, _effectScale), halfDuration).SetEase(Ease.OutCubic))
            .Append(transform.DOScale(_initialScale, halfDuration).SetEase(Ease.OutCubic))
            .OnComplete(() =>
            {
                transform.localScale = _initialScale;
                SetEffectOutlineVisible(false);
                _effectTween = null;
            });
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
        _effectTween?.Kill();
        _effectTween = null;
        transform.localRotation = _initialRotation;
        transform.localScale = _initialScale;
        SetEffectOutlineVisible(false);

        _isHovered = false;
        if (_rectTransform != null && _hasRestingPosition)
        {
            _rectTransform.anchoredPosition = _restingAnchoredPosition;
        }
    }

    private void SetEffectOutlineVisible(bool visible)
    {
        EnsureEffectOutline();
        if (_effectOutline != null)
        {
            _effectOutline.SetActive(visible);
        }
    }

    private void EnsureEffectOutline()
    {
        if (_effectOutline != null)
        {
            return;
        }

        Transform outline = FindChildRecursive(transform, "Outline");
        if (outline == null)
        {
            outline = FindChildRecursive(transform, "outline");
        }

        _effectOutline = outline != null ? outline.gameObject : null;
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
