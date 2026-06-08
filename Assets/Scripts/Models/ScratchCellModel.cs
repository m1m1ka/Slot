using System;
using System.Collections.Generic;
using Configs;

/// <summary>
/// 刮刮卡单元格实例数据。
/// </summary>
public class ScratchCellModel
{
    public int CellIndex { get; }
    public int Row { get; }
    public int Column { get; }
    public int PatternId { get; private set; }
    public string PatternName { get; private set; }
    public int BaseScore { get; private set; }
    public bool IsBaseScoreEnhanced { get; private set; }
    public bool IsGiantFruit { get; private set; }
    public double ScoreMultiplierOnScore { get; private set; }
    public double RewardMultiplierBonusOnScore { get; private set; }
    public IReadOnlyList<int> RogueCardEffectSourceIds { get; private set; }
    public ScratchPatternEffectType PatternEffectType { get; private set; }
    public double PatternEffectValue { get; private set; }
    public bool IsScratchable { get; }
    public bool IsScratched { get; private set; }
    public int ScratchOrder { get; private set; } = -1;

    public ScratchCellModel(
        int cellIndex,
        int row,
        int column,
        int patternId,
        string patternName,
        int baseScore,
        bool isScratchable,
        bool isBaseScoreEnhanced = false,
        bool isGiantFruit = false,
        double scoreMultiplierOnScore = 1d,
        double rewardMultiplierBonusOnScore = 0d,
        IReadOnlyList<int> rogueCardEffectSourceIds = null,
        ScratchPatternEffectType patternEffectType = ScratchPatternEffectType.None,
        double patternEffectValue = 0d)
    {
        CellIndex = cellIndex;
        Row = row;
        Column = column;
        PatternId = patternId;
        PatternName = patternName;
        BaseScore = baseScore;
        IsScratchable = isScratchable;
        IsBaseScoreEnhanced = isBaseScoreEnhanced;
        IsGiantFruit = isGiantFruit;
        ScoreMultiplierOnScore = scoreMultiplierOnScore > 0d ? scoreMultiplierOnScore : 1d;
        RewardMultiplierBonusOnScore = rewardMultiplierBonusOnScore > 0d ? rewardMultiplierBonusOnScore : 0d;
        RogueCardEffectSourceIds = rogueCardEffectSourceIds != null
            ? (IReadOnlyList<int>)new List<int>(rogueCardEffectSourceIds)
            : Array.Empty<int>();
        PatternEffectType = patternEffectType;
        PatternEffectValue = patternEffectValue;
    }

    public void MarkScratched(int scratchOrder = -1)
    {
        if (!IsScratchable)
        {
            return;
        }

        IsScratched = true;
        ScratchOrder = scratchOrder;
    }

    public void TransformPattern(
        ScratchPatternConfig patternConfig,
        int baseScore,
        bool isBaseScoreEnhanced,
        IReadOnlyList<int> rogueCardEffectSourceIds = null)
    {
        if (patternConfig == null)
        {
            return;
        }

        PatternId = patternConfig.Id;
        PatternName = patternConfig.Name;
        BaseScore = baseScore;
        IsBaseScoreEnhanced = isBaseScoreEnhanced;
        IsGiantFruit = false;
        ScoreMultiplierOnScore = 1d;
        RewardMultiplierBonusOnScore = 0d;
        PatternEffectType = patternConfig.EffectType;
        PatternEffectValue = patternConfig.EffectValue;
        RogueCardEffectSourceIds = rogueCardEffectSourceIds != null
            ? (IReadOnlyList<int>)new List<int>(rogueCardEffectSourceIds)
            : Array.Empty<int>();
    }
}
