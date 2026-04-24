using System.Collections.Generic;

/// <summary>
/// Focus panel display data for a scratch card.
/// Pure C# data only; built by Controller and rendered by View.
/// </summary>
public class ScratchCardFocusPanelModel
{
    public string CardName { get; }
    public string PatternPoolName { get; }
    public IReadOnlyList<ScratchCardFocusPatternInfo> Patterns { get; }

    public ScratchCardFocusPanelModel(
        string cardName,
        string patternPoolName,
        IReadOnlyList<ScratchCardFocusPatternInfo> patterns)
    {
        CardName = string.IsNullOrWhiteSpace(cardName) ? "Unknown Card" : cardName;
        PatternPoolName = string.IsNullOrWhiteSpace(patternPoolName) ? "Unknown Pool" : patternPoolName;
        Patterns = patterns ?? new List<ScratchCardFocusPatternInfo>();
    }
}

public class ScratchCardFocusPatternInfo
{
    public int PatternId { get; }
    public string PatternName { get; }
    public int BaseScore { get; }
    public int Weight { get; }
    public float Probability { get; }
    public string AtlasPath { get; }
    public string SpriteName { get; }

    public ScratchCardFocusPatternInfo(
        int patternId,
        string patternName,
        int baseScore,
        int weight,
        float probability,
        string atlasPath,
        string spriteName)
    {
        PatternId = patternId;
        PatternName = string.IsNullOrWhiteSpace(patternName) ? $"Pattern {patternId}" : patternName;
        BaseScore = baseScore;
        Weight = weight;
        Probability = probability;
        AtlasPath = atlasPath;
        SpriteName = spriteName;
    }
}
