namespace Core
{
    /// <summary>
    /// 直接累加基础分的结算策略。
    /// </summary>
    public class SumScoreSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            return new ScratchSettlementResult
            {
                FinalScore = model != null ? model.TotalBaseScore : 0,
                Summary = "Sum all scratchable symbol scores."
            };
        }
    }
}
