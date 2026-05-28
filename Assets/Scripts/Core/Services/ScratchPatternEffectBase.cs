using Configs;

namespace Core
{
    public abstract class ScratchPatternEffectBase : IScratchPatternEffect
    {
        public abstract ScratchPatternEffectType EffectType { get; }

        public virtual int GetScore(ScratchPatternEffectContext context)
        {
            return context.Cell != null ? context.Cell.BaseScore : 0;
        }

        public virtual double GetRewardMultiplierBonusOnReveal(ScratchPatternEffectContext context)
        {
            return 0d;
        }

        public virtual double GetRewardMultiplierBonusOnScore(ScratchPatternEffectContext context)
        {
            return 0d;
        }

        public virtual bool ForcesFinalRewardZero(ScratchPatternEffectContext context)
        {
            return false;
        }

        public virtual bool ExcludeFromHighestBaseScore(ScratchPatternEffectContext context)
        {
            return false;
        }

        public virtual bool ScoresDirectly(ScratchPatternEffectContext context)
        {
            return false;
        }
    }
}
