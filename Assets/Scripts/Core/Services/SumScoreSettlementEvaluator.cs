namespace Core
{
    /// <summary>
    /// 直接累加基础分的结算策略。
    /// </summary>
    public class SumScoreSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            int score = ScratchPatternScoreService.SumScratchableScores(model);
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = "累加所有可刮图案分数。"
            };
        }
    }
}
