namespace Configs
{
    public enum RogueCardEffectType
    {
        None = 0,
        IncreasePatternBaseScore = 1,
        IncreaseScratchCardMultiplier = 2,
        IncreasePatternProbability = 3,
        AddScratchPatternToPool = 4,
        IncreasePatternBaseScoreOnScore = 5,
        ConvertPatternToGiantOnGenerate = 6,
        AddJokerPatternToPool = 7,
        AddRiskMultiplierPatternToPool = 8,
        AddSettlementScoreBonus = 9,
        AddSettlementMultiplierBonus = 10,
        AddSettlementScorePerScratchedPattern = 11,
        AddSettlementMultiplierPerScratchedPattern = 12,
        ConvertPatternToPatternOnReveal = 13,
        ConvertAdjacentPatternsToMetalOnReveal = 14,
        AddSettlementMultiplierBonusWhenAllPatternsScratched = 15,
        IncreaseJackpotAppearanceChance = 16,
        IncreaseJackpotAppearanceChanceAndScratchCardPrice = 17
    }
}
