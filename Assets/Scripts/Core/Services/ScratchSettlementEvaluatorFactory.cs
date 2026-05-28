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
                ScratchSettlementType.FirstRevealedPattern => new FirstRevealedPatternSettlementEvaluator(),
                ScratchSettlementType.MatchAnyPair => new MatchAnyPairSettlementEvaluator(),
                ScratchSettlementType.MatchAnyThree => new MatchAnyThreeSettlementEvaluator(),
                ScratchSettlementType.RowSumBonus => new RowSumBonusSettlementEvaluator(),
                ScratchSettlementType.HighestScorePattern => new HighestScorePatternSettlementEvaluator(),
                ScratchSettlementType.LastRevealedPattern => new LastRevealedPatternSettlementEvaluator(),
                ScratchSettlementType.HorizontalTripleMatch => new HorizontalTripleMatchSettlementEvaluator(),
                ScratchSettlementType.HorizontalFiveMatch => new HorizontalFiveMatchSettlementEvaluator(),
                ScratchSettlementType.VerticalTripleMatch => new VerticalTripleMatchSettlementEvaluator(),
                _ => new SumScoreSettlementEvaluator()
            };
        }
    }
}
