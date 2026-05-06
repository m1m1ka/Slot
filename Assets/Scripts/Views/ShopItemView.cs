using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Core;

/// <summary>
/// 视图层：单个商店购买/升级按钮项。
/// 仅负责展示名称、价格，并上报点击事件。不包含购买判断逻辑。
/// </summary>
public class ShopItemView : MonoBehaviour 
{
    [Header("UI 引用")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _buyBtn;

    // 向外抛出购买点击意图，附带商品 ID
    public event Action<int> OnBuyClicked; 
    
    private int _mySlotId;

    private void Awake()
    {
        EnsureIconImage();
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

    /// <summary>
    /// 供给 Controller 初始化和局部刷新数据时使用
    /// </summary>
    /// <param name="slotId">唯一标识ID</param>
    /// <param name="slotName">老虎机名称</param>
    /// <param name="cost">购买需要的价格</param>
    public void SetData(int slotId, string slotName, double cost, string iconAtlasPath = null, string iconSpriteName = null)
    {
        _mySlotId = slotId;
        
        if (_nameText != null) 
            _nameText.text = slotName;
            
        if (_costText != null) 
            _costText.text = $"{cost:N0}";

        SetIcon(iconAtlasPath, iconSpriteName);
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

    private void SetIcon(string atlasPath, string spriteName)
    {
        EnsureIconImage();
        if (_iconImage == null)
        {
            return;
        }

        Sprite icon = !string.IsNullOrWhiteSpace(atlasPath) && !string.IsNullOrWhiteSpace(spriteName)
            ? AssetProvider.LoadSpriteFromAtlas(atlasPath, spriteName)
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
}
