using Configs;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScratchToolView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tool")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;

    [Header("Description Panel")]
    [SerializeField] private GameObject _descriptionPanel;
    [SerializeField] private TextMeshProUGUI _descriptionTitleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    private void Awake()
    {
        EnsureReferences();
        HideDescription();
    }

    public void Bind(ScratchToolConfig config)
    {
        EnsureReferences();
        if (config == null)
        {
            SetText(_nameText, "-");
            SetText(_descriptionTitleText, "-");
            SetText(_descriptionText, string.Empty);
            return;
        }

        SetText(_nameText, config.Name);
        SetText(_descriptionTitleText, config.Name);
        SetText(_descriptionText, config.Description);
        SetIcon(config.IconAtlasPath, config.IconSpriteName);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowDescription();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideDescription();
    }

    public void ShowDescription()
    {
        if (_descriptionPanel != null)
        {
            _descriptionPanel.SetActive(true);
        }
    }

    public void HideDescription()
    {
        if (_descriptionPanel != null)
        {
            _descriptionPanel.SetActive(false);
        }
    }

    private void SetIcon(string atlasPath, string spriteName)
    {
        EnsureIconImage();
        if (_iconImage == null)
        {
            return;
        }

        bool hasConfiguredIcon = !string.IsNullOrWhiteSpace(atlasPath) && !string.IsNullOrWhiteSpace(spriteName);
        Sprite icon = hasConfiguredIcon ? AssetProvider.LoadSpriteFromAtlas(atlasPath, spriteName) : null;
        if (!hasConfiguredIcon)
        {
            return;
        }

        if (icon == null)
        {
            _iconImage.enabled = false;
            return;
        }

        _iconImage.sprite = icon;
        _iconImage.preserveAspect = true;
        _iconImage.enabled = true;
    }

    private void SetText(TextMeshProUGUI label, string text)
    {
        if (label == null)
        {
            return;
        }

        AssetProvider.ApplyDefaultTmpFont(label);
        label.text = string.IsNullOrWhiteSpace(text) ? "-" : text;
    }

    private void EnsureReferences()
    {
        EnsureIconImage();
        EnsureTextReferences();

        if (_descriptionPanel == null)
        {
            Transform panel = transform.Find("DescriptionPanel");
            if (panel == null)
            {
                panel = transform.Find("Tooltip");
            }

            _descriptionPanel = panel != null ? panel.gameObject : null;
        }
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

    private void EnsureTextReferences()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null)
            {
                continue;
            }

            if (_nameText == null && text.name == "Name")
            {
                _nameText = text;
            }
            else if (_descriptionTitleText == null && (text.name == "DescriptionTitle" || text.name == "Title"))
            {
                _descriptionTitleText = text;
            }
            else if (_descriptionText == null && (text.name == "Description" || text.name == "Desc"))
            {
                _descriptionText = text;
            }
        }
    }
}
