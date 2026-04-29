using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Core
{
    public class RogueCardEffectService
    {
        private readonly Dictionary<RogueCardEffectType, IRogueCardEffect> _effects = new Dictionary<RogueCardEffectType, IRogueCardEffect>();

        public RogueCardEffectService()
        {
            Register(new NoOpRogueCardEffect());
            Register(new IncreasePatternBaseScoreEffect());
            Register(new IncreaseScratchCardMultiplierEffect());
            Register(new IncreasePatternProbabilityEffect());
            Register(new AddScratchPatternToPoolEffect());
        }

        public void Register(IRogueCardEffect effect)
        {
            if (effect == null)
            {
                return;
            }

            _effects[effect.EffectType] = effect;
        }

        public void ApplyCard(RogueCardConfig cardConfig, RogueCardEffectContext context)
        {
            ApplyCard(cardConfig, 1, context);
        }

        public void ApplyCard(RogueCardConfig cardConfig, int level, RogueCardEffectContext context)
        {
            RogueCardLevelConfig levelConfig = cardConfig != null ? cardConfig.GetLevelConfig(level) : null;
            if (levelConfig == null || levelConfig.Effects == null)
            {
                return;
            }

            for (int i = 0; i < levelConfig.Effects.Count; i++)
            {
                RogueCardEffectConfig effectConfig = levelConfig.Effects[i];
                if (effectConfig == null)
                {
                    continue;
                }

                if (_effects.TryGetValue(effectConfig.EffectType, out IRogueCardEffect effect))
                {
                    effect.Apply(effectConfig, context);
                    continue;
                }

                Debug.Log($"[RogueCardEffectService] Effect '{effectConfig.EffectType}' is registered as data, but no runtime handler exists yet.");
            }
        }

        public void RebuildRunModifiers(IReadOnlyList<RogueCardInventoryEntryModel> ownedCards, RogueCardEffectContext context)
        {
            if (context?.GameSession?.RunModifiers == null)
            {
                return;
            }

            context.GameSession.RunModifiers.Clear();

            int count = ownedCards != null ? ownedCards.Count : 0;
            for (int i = 0; i < count; i++)
            {
                RogueCardInventoryEntryModel ownedCard = ownedCards[i];
                if (ownedCard == null)
                {
                    continue;
                }

                ApplyCard(ownedCard.Config, ownedCard.Level, context);
            }
        }

        private class NoOpRogueCardEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.None;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
            }
        }

        private class IncreasePatternBaseScoreEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.IncreasePatternBaseScore;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                if (effectConfig.TargetIds == null)
                {
                    return;
                }

                int bonus = Mathf.RoundToInt((float)effectConfig.Value);
                for (int i = 0; i < effectConfig.TargetIds.Count; i++)
                {
                    context.GameSession.RunModifiers.AddPatternBaseScoreBonus(
                        effectConfig.TargetIds[i],
                        bonus);
                }
            }
        }

        private class IncreaseScratchCardMultiplierEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.IncreaseScratchCardMultiplier;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                if (effectConfig.TargetIds != null && effectConfig.TargetIds.Count > 0)
                {
                    for (int i = 0; i < effectConfig.TargetIds.Count; i++)
                    {
                        context.GameSession.RunModifiers.AddPatternScratchCardMultiplierBonus(
                            effectConfig.TargetIds[i],
                            effectConfig.Value);
                    }

                    return;
                }

                context.GameSession.RunModifiers.AddScratchCardMultiplierBonus(effectConfig.Value);
            }
        }

        private class IncreasePatternProbabilityEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.IncreasePatternProbability;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || effectConfig.TargetIds == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                for (int i = 0; i < effectConfig.TargetIds.Count; i++)
                {
                    context.GameSession.RunModifiers.AddPatternWeightBonus(
                        effectConfig.TargetIds[i],
                        effectConfig.Value);
                }
            }
        }

        private class AddScratchPatternToPoolEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.AddScratchPatternToPool;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || effectConfig.TargetIds == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                float weight = effectConfig.Value > 0d ? (float)effectConfig.Value : 10f;
                for (int i = 0; i < effectConfig.TargetIds.Count; i++)
                {
                    context.GameSession.RunModifiers.AddScratchPatternToPool(
                        effectConfig.TargetIds[i],
                        weight,
                        effectConfig.CardTypeIds);
                }
            }
        }
    }
}
