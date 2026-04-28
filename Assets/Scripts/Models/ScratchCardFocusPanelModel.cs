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
    public IReadOnlyList<ScratchCardFocusPatternInfo> Patterns { get; }

    public ScratchCardFocusPanelModel(
        string cardName,
        string patternPoolName,
        IReadOnlyList<ScratchCardFocusPatternInfo> patterns,
        string winDescription = null)
    {
        CardName = string.IsNullOrWhiteSpace(cardName) ? "未知刮刮卡" : cardName;
        PatternPoolName = string.IsNullOrWhiteSpace(patternPoolName) ? "未知图案池" : patternPoolName;
        WinDescription = string.IsNullOrWhiteSpace(winDescription) ? "暂无获奖说明。" : winDescription;
        Patterns = patterns ?? new List<ScratchCardFocusPatternInfo>();
    }
}

public class ScratchCardFocusPatternInfo
{
    public int PatternId { get; }
    public string PatternName { get; }
    public int BaseScore { get; }
    public bool IsBaseScoreEnhanced { get; }
    public int Weight { get; }
    public float Probability { get; }
    public string AtlasPath { get; }
    public string SpriteName { get; }

    public ScratchCardFocusPatternInfo(
        int patternId,
        string patternName,
        int baseScore,
        bool isBaseScoreEnhanced,
        int weight,
        float probability,
        string atlasPath,
        string spriteName)
    {
        PatternId = patternId;
        PatternName = string.IsNullOrWhiteSpace(patternName) ? $"图案 {patternId}" : patternName;
        BaseScore = baseScore;
        IsBaseScoreEnhanced = isBaseScoreEnhanced;
        Weight = weight;
        Probability = probability;
        AtlasPath = atlasPath;
        SpriteName = spriteName;
    }
}
