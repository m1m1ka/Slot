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
    public bool IsScratchable { get; }
    public bool IsScratched { get; private set; }

    public ScratchCellModel(int cellIndex, int row, int column, int patternId, string patternName, int baseScore, bool isScratchable)
    {
        CellIndex = cellIndex;
        Row = row;
        Column = column;
        PatternId = patternId;
        PatternName = patternName;
        BaseScore = baseScore;
        IsScratchable = isScratchable;
    }

    public void MarkScratched()
    {
        if (!IsScratchable)
        {
            return;
        }

        IsScratched = true;
    }
}
