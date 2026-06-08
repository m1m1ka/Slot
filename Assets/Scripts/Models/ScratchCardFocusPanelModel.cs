using System.Collections.Generic;

/// <summary>
/// Focus panel display data for a scratch card.
/// Pure C# data only; built by Controller and rendered by View.
/// </summary>
public class ScratchCardFocusPanelModel
{
    public string CardName { get; }
    public string PatternPoolName { get; }
    public string WinDescription { get; }
    public string SpecialDescription { get; }
    public ScratchCardFocusPatternInfo JackpotPattern { get; }
    public IReadOnlyList<ScratchCardFocusPatternInfo> Patterns { get; }

    public ScratchCardFocusPanelModel(
        string cardName,
        string patternPoolName,
        IReadOnlyList<ScratchCardFocusPatternInfo> patterns,
        string winDescription = null,
        string specialDescription = null,
        ScratchCardFocusPatternInfo jackpotPattern = null)
    {
        CardName = string.IsNullOrWhiteSpace(cardName) ? "\u672a\u77e5\u522e\u522e\u5361" : cardName;
        PatternPoolName = string.IsNullOrWhiteSpace(patternPoolName) ? "\u672a\u77e5\u56fe\u6848\u6c60" : patternPoolName;
        WinDescription = string.IsNullOrWhiteSpace(winDescription) ? string.Empty : winDescription;
        SpecialDescription = string.IsNullOrWhiteSpace(specialDescription) ? string.Empty : specialDescription;
        JackpotPattern = jackpotPattern;
        Patterns = patterns ?? new List<ScratchCardFocusPatternInfo>();
    }
}

public class ScratchCardFocusPatternInfo
{
    public int PatternId { get; }
    public string PatternName { get; }
    public int BaseScore { get; }
    public bool IsBaseScoreEnhanced { get; }
    public bool IsProbabilityEnhanced { get; }
    public int Weight { get; }
    public float Probability { get; }
    public string AtlasPath { get; }
    public string SpriteName { get; }
    public string SpritePath { get; }

    public ScratchCardFocusPatternInfo(
        int patternId,
        string patternName,
        int baseScore,
        bool isBaseScoreEnhanced,
        bool isProbabilityEnhanced,
        int weight,
        float probability,
        string atlasPath,
        string spriteName,
        string spritePath = null)
    {
        PatternId = patternId;
        PatternName = string.IsNullOrWhiteSpace(patternName) ? $"\u56fe\u6848 {patternId}" : patternName;
        BaseScore = baseScore;
        IsBaseScoreEnhanced = isBaseScoreEnhanced;
        IsProbabilityEnhanced = isProbabilityEnhanced;
        Weight = weight;
        Probability = probability;
        AtlasPath = atlasPath;
        SpriteName = spriteName;
        SpritePath = spritePath;
    }
}
