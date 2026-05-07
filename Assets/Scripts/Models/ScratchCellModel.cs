using Configs;

/// <summary>
/// 刮刮卡单元格实例数据。
/// </summary>
public class ScratchCellModel
{
    public int CellIndex { get; }
    public int Row { get; }
    public int Column { get; }
    public int PatternId { get; }
    public string PatternName { get; }
    public int BaseScore { get; }
    public bool IsBaseScoreEnhanced { get; }
    public double RewardMultiplierBonusOnScore { get; }
    public ScratchPatternEffectType PatternEffectType { get; }
    public double PatternEffectValue { get; }
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
        double rewardMultiplierBonusOnScore = 0d,
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
        RewardMultiplierBonusOnScore = rewardMultiplierBonusOnScore > 0d ? rewardMultiplierBonusOnScore : 0d;
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
}
