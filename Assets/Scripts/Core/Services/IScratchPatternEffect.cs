using Configs;

namespace Core
{
    public interface IScratchPatternEffect
    {
        ScratchPatternEffectType EffectType { get; }
        int GetScore(ScratchPatternEffectContext context);
        double GetRewardMultiplierBonusOnReveal(ScratchPatternEffectContext context);
        double GetRewardMultiplierBonusOnScore(ScratchPatternEffectContext context);
        bool ForcesFinalRewardZero(ScratchPatternEffectContext context);
        bool ExcludeFromHighestBaseScore(ScratchPatternEffectContext context);
        bool ScoresDirectly(ScratchPatternEffectContext context);
    }
}
