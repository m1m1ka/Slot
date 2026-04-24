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
            return new ScratchSettlementResult
            {
                FinalScore = model.TotalBaseScore + rowBonus,
                Summary = "Applied row-based bonus.",
            };
        }
    }
}
