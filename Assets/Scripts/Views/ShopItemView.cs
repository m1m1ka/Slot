using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Core;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// 视图层：单个商店购买/升级按钮项。
/// 仅负责展示名称、价格，并上报点击事件。不包含购买判断逻辑。
/// </summary>
public class ShopItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 引用")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _buyBtn;
    [SerializeField] private GameObject _outline;
    [SerializeField] private Vector2 _purchaseFeedbackOffset = new Vector2(8f, -8f);
    [SerializeField] private float _purchaseFeedbackMoveDuration = 0.08f;
    [SerializeField] private float _purchaseFeedbackReturnDuration = 0.16f;

    // 向外抛出购买点击意图，附带商品 ID
    public event Action<int> OnBuyClicked; 
    
    private int _mySlotId;
    private RectTransform _purchaseFeedbackRect;
    private Vector2 _purchaseFeedbackRestingPosition;
    private Tween _purchaseFeedbackTween;

    public int SlotId => _mySlotId;

    private void Awake()
    {
        EnsureIconImage();
        EnsureOutline();
        EnsurePurchaseFeedbackRect();
        SetOutlineVisible(false);
        AssetProvider.ApplyDefaultTmpFont(_nameText);
        AssetProvider.ApplyDefaultTmpFont(_costText);

        if (_buyBtn != null)
        {
            AssetProvider.ApplyDefaultTmpFont(_buyBtn.GetComponentInChildren<TextMeshProUGUI>(true));
            _buyBtn.onClick.AddListener(() => 
            {
                OnBuyClicked?.Invoke(_mySlotId);
            });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetOutlineVisible(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetOutlineVisible(false);
    }

    private void OnDisable()
    {
        ResetPurchaseFeedbackPosition();
        SetOutlineVisible(false);
    }

    private void OnDestroy()
    {
        _purchaseFeedbackTween?.Kill();
        _purchaseFeedbackTween = null;
    }

    /// <summary>
    /// 供给 Controller 初始化和局部刷新数据时使用
    /// </summary>
    /// <param name="slotId">唯一标识ID</param>
    /// <param name="slotName">老虎机名称</param>
    /// <param name="cost">购买需要的价格</param>
    public void SetData(int slotId, string slotName, double cost, string iconPath = null)
    {
        _mySlotId = slotId;
        
        if (_nameText != null) 
            _nameText.text = slotName;
            
        if (_costText != null) 
            _costText.text = NumberFormatter.FormatCompact(cost);

        SetIcon(iconPath);
    }

    /// <summary>
    /// 当玩家资产变化时，可以调用此方法将按钮置灰（如果买不起）
    /// </summary>
    public void UpdateAffordability(bool canAfford)
    {
        if (_buyBtn != null)
        {
            _buyBtn.interactable = canAfford;
        }
    }

    public void PlayPurchaseFeedback()
    {
        EnsurePurchaseFeedbackRect();
        if (_purchaseFeedbackRect == null)
        {
            return;
        }

        if (_purchaseFeedbackTween != null && _purchaseFeedbackTween.IsActive())
        {
            _purchaseFeedbackTween.Kill();
            _purchaseFeedbackRect.anchoredPosition = _purchaseFeedbackRestingPosition;
        }
        else
        {
            _purchaseFeedbackRestingPosition = _purchaseFeedbackRect.anchoredPosition;
        }

        _purchaseFeedbackTween = DOTween.Sequence()
            .SetUpdate(true)
            .Append(_purchaseFeedbackRect.DOAnchorPos(
                _purchaseFeedbackRestingPosition + _purchaseFeedbackOffset,
                _purchaseFeedbackMoveDuration).SetEase(Ease.OutQuad))
            .Append(_purchaseFeedbackRect.DOAnchorPos(
                _purchaseFeedbackRestingPosition,
                _purchaseFeedbackReturnDuration).SetEase(Ease.OutBack))
            .OnComplete(() => _purchaseFeedbackTween = null);
    }

    private void SetIcon(string iconPath)
    {
        EnsureIconImage();
        if (_iconImage == null)
        {
            return;
        }

        Sprite icon = !string.IsNullOrWhiteSpace(iconPath)
            ? AssetProvider.Load<Sprite>(iconPath)
            : null;
        _iconImage.sprite = icon;
        _iconImage.preserveAspect = true;
        _iconImage.enabled = icon != null;
    }

    private void EnsureIconImage()
    {
        if (_iconImage != null)
        {
            return;
        }

        Image[] images = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == "Icon")
            {
                _iconImage = images[i];
                return;
            }
        }
    }

    private void EnsureOutline()
    {
        if (_outline != null)
        {
            return;
        }

        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && transforms[i].name == "Outline")
            {
                _outline = transforms[i].gameObject;
                return;
            }
        }
    }

    private void SetOutlineVisible(bool visible)
    {
        EnsureOutline();
        if (_outline != null)
        {
            _outline.SetActive(visible);
        }
    }

    private void EnsurePurchaseFeedbackRect()
    {
        if (_purchaseFeedbackRect != null)
        {
            return;
        }

        _purchaseFeedbackRect = _buyBtn != null
            ? _buyBtn.transform as RectTransform
            : transform as RectTransform;

        if (_purchaseFeedbackRect != null)
        {
            _purchaseFeedbackRestingPosition = _purchaseFeedbackRect.anchoredPosition;
        }
    }

    private void ResetPurchaseFeedbackPosition()
    {
        _purchaseFeedbackTween?.Kill();
        _purchaseFeedbackTween = null;

        if (_purchaseFeedbackRect != null)
        {
            _purchaseFeedbackRect.anchoredPosition = _purchaseFeedbackRestingPosition;
        }
    }
}
