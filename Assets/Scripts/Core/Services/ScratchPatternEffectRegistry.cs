using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Core
{
    public static class ScratchPatternEffectRegistry
    {
        private static readonly Dictionary<ScratchPatternEffectType, IScratchPatternEffect> Effects =
            new Dictionary<ScratchPatternEffectType, IScratchPatternEffect>();

        static ScratchPatternEffectRegistry()
        {
            Register(new DefaultScratchPatternEffect());
            Register(new AddRewardMultiplierOnRevealedEffect());
            Register(new ScoreHighestPatternBaseScoreMultiplierEffect());
            Register(new ForceFinalRewardZeroEffect());
            Register(new FixedScoreEffect());
            Register(new MultiplyRewardMultiplierOnSettlementEffect());
        }

        public static void Register(IScratchPatternEffect effect)
        {
            if (effect == null)
            {
                return;
            }

            Effects[effect.EffectType] = effect;
        }

        public static IScratchPatternEffect Resolve(ScratchPatternEffectType effectType)
        {
            return Effects.TryGetValue(effectType, out IScratchPatternEffect effect)
                ? effect
                : Effects[ScratchPatternEffectType.None];
        }

        private class DefaultScratchPatternEffect : ScratchPatternEffectBase
        {
            public override ScratchPatternEffectType EffectType => ScratchPatternEffectType.None;
        }

        private class AddRewardMultiplierOnRevealedEffect : ScratchPatternEffectBase
        {
            public override ScratchPatternEffectType EffectType => ScratchPatternEffectType.AddRewardMultiplierOnRevealed;

            public override double GetRewardMultiplierBonusOnReveal(ScratchPatternEffectContext context)
            {
                return context.Cell != null && context.Cell.PatternEffectValue > 0d
                    ? context.Cell.PatternEffectValue
                    : 0d;
            }
        }

        private class ScoreHighestPatternBaseScoreMultiplierEffect : ScratchPatternEffectBase
        {
            public override ScratchPatternEffectType EffectType => ScratchPatternEffectType.ScoreHighestPatternBaseScoreMultiplier;

            public override int GetScore(ScratchPatternEffectContext context)
            {
                double multiplier = context.Cell != null && context.Cell.PatternEffectValue > 0d
                    ? context.Cell.PatternEffectValue
                    : 2d;
                return Mathf.RoundToInt(ScratchPatternScoreService.GetHighestBaseScore(context.Model) * (float)multiplier);
            }
        }

        private class ForceFinalRewardZeroEffect : ScratchPatternEffectBase
        {
            public override ScratchPatternEffectType EffectType => ScratchPatternEffectType.ForceFinalRewardZero;

            public override bool ForcesFinalRewardZero(ScratchPatternEffectContext context)
            {
                return context.Cell != null && context.Cell.IsScratchable && context.Cell.IsScratched;
            }

            public override bool ExcludeFromHighestBaseScore(ScratchPatternEffectContext context)
            {
                return true;
            }
        }

        private class FixedScoreEffect : ScratchPatternEffectBase
        {
            public override ScratchPatternEffectType EffectType => ScratchPatternEffectType.FixedScore;

            public override int GetScore(ScratchPatternEffectContext context)
            {
                if (context.Cell == null)
                {
                    return 0;
                }

                return context.Cell.PatternEffectValue > 0d
                    ? Mathf.RoundToInt((float)context.Cell.PatternEffectValue)
                    : context.Cell.BaseScore;
            }

            public override bool ScoresDirectly(ScratchPatternEffectContext context)
            {
                return true;
            }
        }

        private class MultiplyRewardMultiplierOnSettlementEffect : ScratchPatternEffectBase
        {
            public override ScratchPatternEffectType EffectType => ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement;

            public override bool ExcludeFromHighestBaseScore(ScratchPatternEffectContext context)
            {
                return true;
            }
        }
    }
}
