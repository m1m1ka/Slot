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
            Register(new IncreasePatternBaseScoreOnScoreEffect());
            Register(new ConvertPatternToGiantOnGenerateEffect());
            Register(new AddJokerPatternToPoolEffect());
            Register(new AddRiskMultiplierPatternToPoolEffect());
            Register(new AddSettlementScoreBonusEffect());
            Register(new AddSettlementMultiplierBonusEffect());
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

                ApplyCard(ownedCard.Config, ownedCard.Level, context.WithSourceRogueCard(ownedCard.CardId));
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
                        bonus,
                        context.SourceRogueCardId);
                }
            }
        }

        private class IncreasePatternBaseScoreOnScoreEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.IncreasePatternBaseScoreOnScore;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || effectConfig.TargetIds == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                int bonus = Mathf.RoundToInt((float)effectConfig.Value);
                for (int i = 0; i < effectConfig.TargetIds.Count; i++)
                {
                    int scoredPatternId = effectConfig.TargetIds[i];
                    context.GameSession.RunModifiers.AddPatternBaseScoreGrowthOnScore(
                        scoredPatternId,
                        effectConfig.TargetIds,
                        bonus,
                        context.SourceRogueCardId);
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
                            effectConfig.Value,
                            context.SourceRogueCardId);
                    }

                    return;
                }

                context.GameSession.RunModifiers.AddScratchCardMultiplierBonus(effectConfig.Value, context.SourceRogueCardId);
            }
        }

        private class ConvertPatternToGiantOnGenerateEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.ConvertPatternToGiantOnGenerate;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || effectConfig.TargetIds == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                ParseChanceAndMultiplier(effectConfig, out double chance, out double multiplier);
                for (int i = 0; i < effectConfig.TargetIds.Count; i++)
                {
                    context.GameSession.RunModifiers.AddGiantPatternRule(
                        effectConfig.TargetIds[i],
                        chance,
                        multiplier,
                        context.SourceRogueCardId);
                }
            }

            private static void ParseChanceAndMultiplier(RogueCardEffectConfig effectConfig, out double chance, out double multiplier)
            {
                chance = effectConfig != null ? effectConfig.Value : 0d;
                multiplier = 1d;
                string expression = effectConfig != null ? effectConfig.ValueExpression : null;
                if (string.IsNullOrWhiteSpace(expression))
                {
                    return;
                }

                string[] parts = expression.Split('|', '/', ',', '，', ';', '；');
                if (parts.Length > 0 && double.TryParse(parts[0], out double parsedChance))
                {
                    chance = parsedChance;
                }

                if (parts.Length > 1 && double.TryParse(parts[1], out double parsedMultiplier))
                {
                    multiplier = parsedMultiplier;
                }
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
                        effectConfig.Value,
                        context.SourceRogueCardId);
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
                        effectConfig.CardTypeIds,
                        context.SourceRogueCardId);
                }
            }
        }

        private class AddJokerPatternToPoolEffect : IRogueCardEffect
        {
            private const int DefaultJokerPatternId = 14;
            private const int DefaultGoodFacePatternId = 12;
            private const int DefaultBadFacePatternId = 13;
            private const int DefaultGoodFaceScore = 3000;
            private const float DefaultWeight = 10f;
            private const double DefaultGoodFaceChance = 0.5d;

            public RogueCardEffectType EffectType => RogueCardEffectType.AddJokerPatternToPool;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                ParseJokerValue(effectConfig, out double jokerChance, out double goodFaceChance, out int goodFaceScore);

                IReadOnlyList<int> jokerPatternIds = effectConfig.TargetIds != null && effectConfig.TargetIds.Count > 0
                    ? effectConfig.TargetIds
                    : new List<int> { DefaultJokerPatternId };
                for (int i = 0; i < jokerPatternIds.Count; i++)
                {
                    context.GameSession.RunModifiers.AddJokerPatternRule(
                        jokerPatternIds[i],
                        jokerChance,
                        goodFaceChance,
                        DefaultGoodFacePatternId,
                        DefaultBadFacePatternId,
                        goodFaceScore,
                        effectConfig.CardTypeIds,
                        context.SourceRogueCardId);
                }
            }

            private static void ParseJokerValue(
                RogueCardEffectConfig effectConfig,
                out double jokerChance,
                out double goodFaceChance,
                out int goodFaceScore)
            {
                jokerChance = effectConfig != null && effectConfig.Value > 0d ? effectConfig.Value : DefaultWeight;
                goodFaceChance = DefaultGoodFaceChance;
                goodFaceScore = DefaultGoodFaceScore;

                string expression = effectConfig != null ? effectConfig.ValueExpression : null;
                if (string.IsNullOrWhiteSpace(expression))
                {
                    return;
                }

                string[] parts = RogueCardEffectValueParser.Split(expression);
                if (parts.Length > 0 && RogueCardEffectValueParser.TryParseNumber(parts[0], out double parsedJokerChance) && parsedJokerChance > 0d)
                {
                    jokerChance = parsedJokerChance;
                }

                if (parts.Length > 1 && RogueCardEffectValueParser.TryParseNumber(parts[1], out double parsedGoodFaceChance))
                {
                    goodFaceChance = parsedGoodFaceChance;
                }

                if (parts.Length > 2 && int.TryParse(parts[2], out int parsedGoodFaceScore) && parsedGoodFaceScore > 0)
                {
                    goodFaceScore = parsedGoodFaceScore;
                }
            }
        }

        private class AddRiskMultiplierPatternToPoolEffect : IRogueCardEffect
        {
            private const int DefaultRiskMultiplierPatternId = 15;
            private const double DefaultChance = 0.05d;

            private static readonly int[] ResolvedPatternIds = { 16, 17, 18, 19 };
            private static readonly double[] DefaultResolvedWeights = { 1d, 1d, 1d, 1d };

            public RogueCardEffectType EffectType => RogueCardEffectType.AddRiskMultiplierPatternToPool;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                ParseRiskMultiplierValue(effectConfig, out double chance, out List<double> resolvedWeights);
                IReadOnlyList<int> riskMultiplierPatternIds = effectConfig.TargetIds != null && effectConfig.TargetIds.Count > 0
                    ? effectConfig.TargetIds
                    : new List<int> { DefaultRiskMultiplierPatternId };

                for (int i = 0; i < riskMultiplierPatternIds.Count; i++)
                {
                    context.GameSession.RunModifiers.AddRiskMultiplierPatternRule(
                        riskMultiplierPatternIds[i],
                        chance,
                        ResolvedPatternIds,
                        resolvedWeights,
                        effectConfig.CardTypeIds,
                        context.SourceRogueCardId);
                }
            }

            private static void ParseRiskMultiplierValue(
                RogueCardEffectConfig effectConfig,
                out double chance,
                out List<double> resolvedWeights)
            {
                chance = effectConfig != null && effectConfig.Value > 0d ? effectConfig.Value : DefaultChance;
                resolvedWeights = new List<double>(DefaultResolvedWeights);

                string expression = effectConfig != null ? effectConfig.ValueExpression : null;
                if (string.IsNullOrWhiteSpace(expression))
                {
                    return;
                }

                string[] parts = RogueCardEffectValueParser.Split(expression);
                if (parts.Length > 0 && RogueCardEffectValueParser.TryParseNumber(parts[0], out double parsedChance) && parsedChance > 0d)
                {
                    chance = parsedChance;
                }

                for (int i = 0; i < resolvedWeights.Count; i++)
                {
                    int partIndex = i + 1;
                    if (partIndex >= parts.Length)
                    {
                        break;
                    }

                    if (RogueCardEffectValueParser.TryParseNumber(parts[partIndex], out double parsedWeight) && parsedWeight > 0d)
                    {
                        resolvedWeights[i] = parsedWeight;
                    }
                }
            }
        }

        private class AddSettlementScoreBonusEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.AddSettlementScoreBonus;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                int bonus = Mathf.RoundToInt((float)effectConfig.Value);
                if (!string.IsNullOrWhiteSpace(effectConfig.ValueExpression) &&
                    RogueCardEffectValueParser.TryParseNumber(effectConfig.ValueExpression, out double parsedBonus))
                {
                    bonus = Mathf.RoundToInt((float)parsedBonus);
                }

                context.GameSession.RunModifiers.AddSettlementScoreBonus(bonus, context.SourceRogueCardId);
            }
        }

        private class AddSettlementMultiplierBonusEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.AddSettlementMultiplierBonus;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
                if (effectConfig == null || context?.GameSession?.RunModifiers == null)
                {
                    return;
                }

                double bonus = effectConfig.Value;
                if (!string.IsNullOrWhiteSpace(effectConfig.ValueExpression) &&
                    RogueCardEffectValueParser.TryParseNumber(effectConfig.ValueExpression, out double parsedBonus))
                {
                    bonus = parsedBonus;
                }

                context.GameSession.RunModifiers.AddSettlementMultiplierBonus(bonus, context.SourceRogueCardId);
            }
        }
    }
}
