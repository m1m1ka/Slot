namespace Core
{
    /// <summary>
    /// 按行计算并给予额外加成的结算策略示例。
    /// </summary>
    public class RowSumBonusSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            if (model == null)
            {
                return new ScratchSettlementResult();
            }

            int rowBonus = model.GridHeight * 50;
            int score = model.TotalBaseScore + rowBonus;
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchSettlementResult.ApplyMultiplier(score, model.RewardMultiplier),
                Summary = "已应用行加成。",
            };
        }
    }
}
