using Configs;

namespace Core
{
    /// <summary>
    /// 根据卡种配置选择对应的结算策略。
    /// </summary>
    public static class ScratchSettlementEvaluatorFactory
    {
        public static IScratchSettlementEvaluator Create(ScratchSettlementType settlementType)
        {
            return settlementType switch
            {
                ScratchSettlementType.MatchAnyThree => new MatchAnyThreeSettlementEvaluator(),
                ScratchSettlementType.RowSumBonus => new RowSumBonusSettlementEvaluator(),
                _ => new SumScoreSettlementEvaluator()
            };
        }
    }
}
