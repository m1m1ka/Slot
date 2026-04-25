using System;
using System.Collections.Generic;
using Configs;

/// <summary>
/// 单张彩票的运行时数据。
/// 当前先提供基础状态、刮开进度和奖励占位，后续可扩展为真实奖励矩阵。
/// </summary>
public class ScratchCardModel
{
    public enum ScratchCardState
    {
        Falling,
        Idle,
        Focused,
        Scratching,
        Completed
    }

    public int CardId { get; }
    public int SourceSlotId { get; }
    public int CardTypeId { get; }
    public string CardTypeName { get; }
    public int GridWidth { get; }
    public int GridHeight { get; }
    public int AreaTemplateId { get; }
    public ScratchSettlementType SettlementType { get; }
    public IReadOnlyList<ScratchCellModel> Cells { get; }
    public int TotalBaseScore { get; }
    public float ScratchProgress { get; private set; }
    public ScratchCardState State { get; private set; }

    public event Action<float> OnScratchProgressChanged;
    public event Action<ScratchCardState> OnStateChanged;
    public event Action OnScratchCompleted;

    public ScratchCardModel(
        int cardId,
        int sourceSlotId,
        ScratchCardTypeConfig cardTypeConfig,
        ScratchAreaTemplateConfig areaTemplateConfig,
        IReadOnlyList<ScratchCellModel> cells)
    {
        CardId = cardId;
        SourceSlotId = sourceSlotId;
        CardTypeId = cardTypeConfig != null ? cardTypeConfig.Id : 0;
        CardTypeName = cardTypeConfig != null ? cardTypeConfig.Name : "Unknown Card";
        GridWidth = areaTemplateConfig != null ? areaTemplateConfig.Width : 0;
        GridHeight = areaTemplateConfig != null ? areaTemplateConfig.Height : 0;
        AreaTemplateId = areaTemplateConfig != null ? areaTemplateConfig.Id : 0;
        SettlementType = cardTypeConfig != null ? cardTypeConfig.SettlementType : ScratchSettlementType.SumScore;
        Cells = cells ?? Array.Empty<ScratchCellModel>();
        TotalBaseScore = CalculateTotalBaseScore(Cells);
        ScratchProgress = 0f;
        State = ScratchCardState.Falling;
    }

    public void SetState(ScratchCardState newState)
    {
        if (State == newState)
        {
            return;
        }

        State = newState;
        OnStateChanged?.Invoke(State);
    }

    public void AddScratchProgress(float amount)
    {
        if (State == ScratchCardState.Completed || amount <= 0f)
        {
            return;
        }

        SetScratchProgress(ScratchProgress + amount);
    }

    public void SetScratchProgress(float progress)
    {
        if (State == ScratchCardState.Completed)
        {
            return;
        }

        if (progress < 0f)
        {
            progress = 0f;
        }
        else if (progress > 1f)
        {
            progress = 1f;
        }

        ScratchProgress = progress;
        OnScratchProgressChanged?.Invoke(ScratchProgress);

        if (ScratchProgress >= 1f)
        {
            SetState(ScratchCardState.Completed);
            OnScratchCompleted?.Invoke();
        }
        else if (State != ScratchCardState.Scratching)
        {
            SetState(ScratchCardState.Scratching);
        }
    }

    private static int CalculateTotalBaseScore(IReadOnlyList<ScratchCellModel> cells)
    {
        if (cells == null || cells.Count == 0)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i] != null && cells[i].IsScratchable)
            {
                total += cells[i].BaseScore;
            }
        }

        return total;
    }
}
