using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatternInfoView : MonoBehaviour
{
    [Header("Pattern Info")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _probabilityText;

    [Header("Enhanced Colors")]
    [SerializeField] private Color _normalTextColor = Color.white;
    [SerializeField] private Color _enhancedTextColor = new Color(0.35f, 0.65f, 1f, 1f);

    private void Awake()
    {
        EnsureReferences();
    }

    public void Bind(ScratchCardFocusPatternInfo pattern)
    {
        EnsureReferences();
        if (pattern == null)
        {
            SetIcon(null);
            SetText(_scoreText, "-");
            SetText(_probabilityText, "-");
            return;
        }

        Sprite sprite = AssetProvider.LoadSpriteFromAtlas(pattern.AtlasPath, pattern.SpriteName);
        SetIcon(sprite);

        SetText(_scoreText, $"+{pattern.BaseScore}");
        SetText(_probabilityText, FormatProbability(pattern.Probability));

        if (_scoreText != null)
        {
            _scoreText.color = pattern.IsBaseScoreEnhanced ? _enhancedTextColor : _normalTextColor;
        }

        if (_probabilityText != null)
        {
            _probabilityText.color = pattern.IsProbabilityEnhanced ? _enhancedTextColor : _normalTextColor;
        }
    }

    private void SetIcon(Sprite sprite)
    {
        if (_iconImage == null)
        {
            return;
        }

        _iconImage.sprite = sprite;
        _iconImage.preserveAspect = true;
        _iconImage.enabled = sprite != null;
    }

    private static void SetText(TextMeshProUGUI label, string value)
    {
        if (label == null)
        {
            return;
        }

        AssetProvider.ApplyDefaultTmpFont(label);
        label.text = string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string FormatProbability(float probability)
    {
        float percent = Mathf.Max(0f, probability * 100f);
        if (percent < 1f)
        {
            return $"{percent:0.0}%";
        }

        return $"{Mathf.FloorToInt(percent)}%";
    }

    private void EnsureReferences()
    {
        if (_iconImage == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].name == "Icon")
                {
                    _iconImage = images[i];
                    break;
                }
            }
        }

        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null)
            {
                continue;
            }

            string normalizedName = text.name.Trim();
            if (_scoreText == null && normalizedName == "Score")
            {
                _scoreText = text;
            }
            else if (_probabilityText == null && normalizedName == "Probability")
            {
                _probabilityText = text;
            }
        }
    }
}
