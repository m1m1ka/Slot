using Configs;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScratchToolView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tool")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;

    [Header("Description")]
    [SerializeField] private GameObject _descriptionPanel;
    [SerializeField] private TextMeshProUGUI _descriptionTitleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _rewardDescriptionText;

    [Header("Settlement Feedback")]
    [SerializeField] private float _settlementPulseScale = 1.18f;
    [SerializeField] private float _settlementPulseDuration = 0.26f;

    private Tween _settlementPulseTween;
    private Vector3 _initialScale = Vector3.one;
    private bool _useRewardDescription;

    public int ToolId { get; private set; } = -1;

    private void Awake()
    {
        _initialScale = transform.localScale;
        EnsureReferences();
        EnsureHoverRaycastTarget();
        HideDescription();
    }

    public void Bind(ScratchToolConfig config)
    {
        EnsureReferences();
        EnsureHoverRaycastTarget();

        if (config == null)
        {
            ToolId = -1;
            SetText(_nameText, "-");
            SetText(_descriptionTitleText, "-");
            SetText(_descriptionText, string.Empty);
            SetText(_rewardDescriptionText, string.Empty);
            HideDescription();
            return;
        }

        ToolId = config.Id;
        SetText(_nameText, config.Name);
        SetText(_descriptionTitleText, config.Name);
        SetText(_descriptionText, config.Description);
        SetText(_rewardDescriptionText, config.Description);
        SetIcon(config.IconAtlasPath, config.IconSpriteName);
        HideDescription();
    }

    public void UseRewardDescription()
    {
        _useRewardDescription = true;
        EnsureReferences();
        HideDescription();
    }

    public void PlaySettlementPulse()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        _settlementPulseTween?.Kill();
        transform.localScale = _initialScale;

        float resolvedPulseScale = Mathf.Max(1f, _settlementPulseScale);
        float halfDuration = Mathf.Max(0.01f, _settlementPulseDuration * 0.5f);
        _settlementPulseTween = DOTween.Sequence()
            .SetUpdate(true)
            .Append(transform.DOScale(_initialScale * resolvedPulseScale, halfDuration).SetEase(Ease.OutCubic))
            .Append(transform.DOScale(_initialScale, halfDuration).SetEase(Ease.OutCubic))
            .OnComplete(() => _settlementPulseTween = null);
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
        EnsureReferences();

        bool showRewardDescription = _useRewardDescription && _rewardDescriptionText != null;
        SetTextVisible(_descriptionText, !showRewardDescription);
        SetTextVisible(_rewardDescriptionText, showRewardDescription);

        if (_descriptionPanel != null)
        {
            _descriptionPanel.SetActive(true);
            _descriptionPanel.transform.SetAsLastSibling();
        }
        else
        {
            SetTextVisible(_descriptionText, !showRewardDescription);
            SetTextVisible(_rewardDescriptionText, showRewardDescription);
        }
    }

    public void HideDescription()
    {
        SetTextVisible(_descriptionText, false);
        SetTextVisible(_rewardDescriptionText, false);

        if (_descriptionPanel != null)
        {
            _descriptionPanel.SetActive(false);
        }
    }

    private void EnsureReferences()
    {
        EnsureIconImage();
        EnsureTextReferences();

        if (_descriptionPanel == null)
        {
            Transform panel = transform.Find("DescriptionPanel") ?? transform.Find("Tooltip");
            _descriptionPanel = panel != null ? panel.gameObject : null;
        }
    }

    private void EnsureHoverRaycastTarget()
    {
        Image raycastImage = GetComponent<Image>();
        if (raycastImage == null)
        {
            raycastImage = gameObject.AddComponent<Image>();
            raycastImage.color = new Color(0f, 0f, 0f, 0f);
        }

        raycastImage.raycastTarget = true;
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
            else if (_rewardDescriptionText == null && text.name == "RewardDescription")
            {
                _rewardDescriptionText = text;
            }
        }
    }

    private void SetIcon(string folderPath, string spriteName)
    {
        EnsureIconImage();
        if (_iconImage == null)
        {
            return;
        }

        bool hasConfiguredIcon = !string.IsNullOrWhiteSpace(folderPath) && !string.IsNullOrWhiteSpace(spriteName);
        if (!hasConfiguredIcon)
        {
            _iconImage.enabled = false;
            return;
        }

        Sprite icon = AssetProvider.Load<Sprite>($"{folderPath}/{spriteName}");
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

    private static void SetTextVisible(TextMeshProUGUI text, bool visible)
    {
        if (text != null)
        {
            text.gameObject.SetActive(visible);
        }
    }

    private void OnDisable()
    {
        _settlementPulseTween?.Kill();
        _settlementPulseTween = null;
        transform.localScale = _initialScale;
        HideDescription();
    }

    private void OnDestroy()
    {
        _settlementPulseTween?.Kill();
        _settlementPulseTween = null;
    }
}
