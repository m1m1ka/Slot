namespace Core
{
    /// <summary>
    /// 直接累加基础分的结算策略。
    /// </summary>
    public class SumScoreSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            int score = model != null ? model.TotalBaseScore : 0;
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchSettlementResult.ApplyMultiplier(score, model != null ? model.RewardMultiplier : 1d),
                Summary = "累加所有可刮图案分数。"
            };
        }
    }
}
