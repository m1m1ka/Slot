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
    public string WinDescription { get; }
    public int GridWidth { get; }
    public int GridHeight { get; }
    public int AreaTemplateId { get; }
    public IReadOnlyList<ScratchCellModel> Cells { get; }
    public IReadOnlyList<ScratchToolConfig> ScratchTools { get; }
    public IReadOnlyList<ScratchCardExtraEffectConfig> ExtraEffects { get; }
    public IReadOnlyList<ScratchCardWinRuleConfig> WinRules { get; }
    public int TotalBaseScore { get; }
    public int SettlementScoreBonus { get; }
    public IReadOnlyList<int> SettlementScoreBonusSourceCardIds { get; }
    public double SettlementMultiplierBonus { get; }
    public IReadOnlyList<int> SettlementMultiplierBonusSourceCardIds { get; }
    public IReadOnlyList<PatternRevealConversionRuleModel> PatternRevealConversionRules { get; }
    public IReadOnlyList<AdjacentPatternMetalConversionRuleModel> AdjacentPatternMetalConversionRules { get; }
    public IReadOnlyList<PatternSettlementScoreBonusRuleModel> PatternSettlementScoreBonusRules { get; }
    public IReadOnlyList<PatternSettlementMultiplierBonusRuleModel> PatternSettlementMultiplierBonusRules { get; }
    private double _rewardMultiplier = 1d;

    public double RewardMultiplier => _rewardMultiplier;
    public float ScratchProgress { get; private set; }
    public ScratchCardState State { get; private set; }

    public event Action<float> OnScratchProgressChanged;
    public event Action<ScratchCardState> OnStateChanged;
    public event Action OnScratchCompleted;
    public event Action<double> OnRewardMultiplierChanged;

    public ScratchCardModel(
        int cardId,
        int sourceSlotId,
        ScratchCardTypeConfig cardTypeConfig,
        ScratchAreaTemplateConfig areaTemplateConfig,
        IReadOnlyList<ScratchCellModel> cells,
        IReadOnlyList<ScratchToolConfig> scratchTools,
        double rewardMultiplier = 1d,
        int settlementScoreBonus = 0,
        IReadOnlyList<int> settlementScoreBonusSourceCardIds = null,
        double settlementMultiplierBonus = 0d,
        IReadOnlyList<int> settlementMultiplierBonusSourceCardIds = null,
        IReadOnlyList<PatternRevealConversionRuleModel> patternRevealConversionRules = null,
        IReadOnlyList<AdjacentPatternMetalConversionRuleModel> adjacentPatternMetalConversionRules = null,
        IReadOnlyList<PatternSettlementScoreBonusRuleModel> patternSettlementScoreBonusRules = null,
        IReadOnlyList<PatternSettlementMultiplierBonusRuleModel> patternSettlementMultiplierBonusRules = null)
    {
        CardId = cardId;
        SourceSlotId = sourceSlotId;
        CardTypeId = cardTypeConfig != null ? cardTypeConfig.Id : 0;
        CardTypeName = cardTypeConfig != null ? cardTypeConfig.Name : "未知刮刮卡";
        WinDescription = cardTypeConfig != null ? cardTypeConfig.WinDescription : string.Empty;
        GridWidth = areaTemplateConfig != null ? areaTemplateConfig.Width : 0;
        GridHeight = areaTemplateConfig != null ? areaTemplateConfig.Height : 0;
        AreaTemplateId = areaTemplateConfig != null ? areaTemplateConfig.Id : 0;
        Cells = cells ?? Array.Empty<ScratchCellModel>();
        ScratchTools = scratchTools ?? Array.Empty<ScratchToolConfig>();
        ExtraEffects = cardTypeConfig != null && cardTypeConfig.ExtraEffects != null
            ? (IReadOnlyList<ScratchCardExtraEffectConfig>)new List<ScratchCardExtraEffectConfig>(cardTypeConfig.ExtraEffects)
            : Array.Empty<ScratchCardExtraEffectConfig>();
        WinRules = cardTypeConfig != null && cardTypeConfig.WinRules != null
            ? (IReadOnlyList<ScratchCardWinRuleConfig>)new List<ScratchCardWinRuleConfig>(cardTypeConfig.WinRules)
            : Array.Empty<ScratchCardWinRuleConfig>();
        TotalBaseScore = CalculateTotalBaseScore(Cells);
        SettlementScoreBonus = settlementScoreBonus;
        SettlementScoreBonusSourceCardIds = settlementScoreBonusSourceCardIds != null
            ? (IReadOnlyList<int>)new List<int>(settlementScoreBonusSourceCardIds)
            : Array.Empty<int>();
        SettlementMultiplierBonus = settlementMultiplierBonus > 0d ? settlementMultiplierBonus : 0d;
        SettlementMultiplierBonusSourceCardIds = settlementMultiplierBonusSourceCardIds != null
            ? (IReadOnlyList<int>)new List<int>(settlementMultiplierBonusSourceCardIds)
            : Array.Empty<int>();
        PatternRevealConversionRules = patternRevealConversionRules != null
            ? (IReadOnlyList<PatternRevealConversionRuleModel>)new List<PatternRevealConversionRuleModel>(patternRevealConversionRules)
            : Array.Empty<PatternRevealConversionRuleModel>();
        AdjacentPatternMetalConversionRules = adjacentPatternMetalConversionRules != null
            ? (IReadOnlyList<AdjacentPatternMetalConversionRuleModel>)new List<AdjacentPatternMetalConversionRuleModel>(adjacentPatternMetalConversionRules)
            : Array.Empty<AdjacentPatternMetalConversionRuleModel>();
        PatternSettlementScoreBonusRules = patternSettlementScoreBonusRules != null
            ? (IReadOnlyList<PatternSettlementScoreBonusRuleModel>)new List<PatternSettlementScoreBonusRuleModel>(patternSettlementScoreBonusRules)
            : Array.Empty<PatternSettlementScoreBonusRuleModel>();
        PatternSettlementMultiplierBonusRules = patternSettlementMultiplierBonusRules != null
            ? (IReadOnlyList<PatternSettlementMultiplierBonusRuleModel>)new List<PatternSettlementMultiplierBonusRuleModel>(patternSettlementMultiplierBonusRules)
            : Array.Empty<PatternSettlementMultiplierBonusRuleModel>();
        _rewardMultiplier = NormalizeRewardMultiplier(rewardMultiplier);
        ScratchProgress = 0f;
        State = ScratchCardState.Falling;
    }

    public void SetRewardMultiplier(double rewardMultiplier)
    {
        double normalizedMultiplier = NormalizeRewardMultiplier(rewardMultiplier);
        if (Math.Abs(_rewardMultiplier - normalizedMultiplier) < 0.0001d)
        {
            return;
        }

        _rewardMultiplier = normalizedMultiplier;
        OnRewardMultiplierChanged?.Invoke(_rewardMultiplier);
    }

    public void AddRewardMultiplierBonus(double bonus)
    {
        if (bonus <= 0d)
        {
            return;
        }

        SetRewardMultiplier(_rewardMultiplier + bonus);
    }

    public void MultiplyRewardMultiplier(double multiplier)
    {
        if (multiplier < 0d)
        {
            multiplier = 0d;
        }

        SetRewardMultiplier(_rewardMultiplier * multiplier);
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

    private static double NormalizeRewardMultiplier(double rewardMultiplier)
    {
        return rewardMultiplier >= 0d ? rewardMultiplier : 1d;
    }
}
