namespace Configs
{
    /// <summary>
    /// 刮具结算策略类型。
    /// </summary>
    public enum ScratchSettlementType
    {
        SumScore = 0,
        MatchAnyThree = 1,
        RowSumBonus = 2,
        MatchAnyPair = 3,
        FirstRevealedPattern = 4,
        HighestScorePattern = 5,
        LastRevealedPattern = 6,
        HorizontalTripleMatch = 7,
        HorizontalFiveMatch = 8,
        VerticalTripleMatch = 9,
        MetalMatchAnyPair = 10,
        FruitMatchAnyPair = 11,
        ConsecutiveLineMatch = 12
    }
}
