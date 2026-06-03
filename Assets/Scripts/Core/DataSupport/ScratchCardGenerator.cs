using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Core
{
    public static class ScratchCardGenerator
    {
        public static List<ScratchCellModel> GenerateCells(
            ScratchCardTypeConfig cardTypeConfig,
            ScratchAreaTemplateConfig areaTemplateConfig,
            RogueCardRunModifierModel runModifiers = null)
        {
            var results = new List<ScratchCellModel>();

            if (cardTypeConfig == null || areaTemplateConfig == null)
            {
                return results;
            }

            List<ScratchPatternWeightEntry> patternWeights = BuildEffectivePatternWeights(cardTypeConfig, runModifiers);
            if (patternWeights.Count == 0)
            {
                return results;
            }

            int cellCount = areaTemplateConfig.Width * areaTemplateConfig.Height;
            bool hasGeneratedRiskMultiplierPattern = false;
            for (int i = 0; i < cellCount; i++)
            {
                bool isScratchable = areaTemplateConfig.ScratchableCellIndices.Contains(i);
                ScratchPatternConfig patternConfig = PickRandomPattern(patternWeights);

                int row = areaTemplateConfig.Width > 0 ? i / areaTemplateConfig.Width : 0;
                int column = areaTemplateConfig.Width > 0 ? i % areaTemplateConfig.Width : 0;

                int baseScore = patternConfig != null ? patternConfig.BaseScore : 0;
                bool isBaseScoreEnhanced = false;
                bool isGiantFruit = false;
                double scoreMultiplierOnScore = 1d;
                double rewardMultiplierBonusOnScore = 0d;
                var rogueCardEffectSourceIds = new HashSet<int>();
                if (patternConfig != null && runModifiers != null)
                {
                    int originalPatternId = patternConfig.Id;
                    if (runModifiers.TryRollJokerPattern(
                        originalPatternId,
                        out int resolvedJokerPatternId,
                        out IReadOnlyCollection<int> jokerSourceCardIds))
                    {
                        AddRange(rogueCardEffectSourceIds, jokerSourceCardIds);
                        patternConfig = ScratchPatternDefaultProvider.GetById(resolvedJokerPatternId) ?? patternConfig;
                    }

                    if (runModifiers.TryRollRiskMultiplierPattern(
                        originalPatternId,
                        out int resolvedRiskMultiplierPatternId,
                        out IReadOnlyCollection<int> riskMultiplierSourceCardIds))
                    {
                        if (hasGeneratedRiskMultiplierPattern)
                        {
                            patternConfig = PickRandomPatternWithoutPattern(patternWeights, originalPatternId) ?? patternConfig;
                            originalPatternId = patternConfig.Id;
                        }
                        else
                        {
                            hasGeneratedRiskMultiplierPattern = true;
                            AddRange(rogueCardEffectSourceIds, riskMultiplierSourceCardIds);
                            patternConfig = ScratchPatternDefaultProvider.GetById(resolvedRiskMultiplierPatternId) ?? patternConfig;
                        }
                    }

                    baseScore = patternConfig.BaseScore;
                    int baseScoreBonus = runModifiers.GetPatternBaseScoreBonus(patternConfig.Id);
                    int jokerGoodFaceScore = runModifiers.GetJokerGoodFaceScoreOverride(patternConfig.Id);
                    if (jokerGoodFaceScore > 0)
                    {
                        baseScore = jokerGoodFaceScore;
                    }

                    baseScore += baseScoreBonus;
                    isBaseScoreEnhanced = baseScoreBonus != 0;
                    rewardMultiplierBonusOnScore = runModifiers.GetPatternScratchCardMultiplierBonus(patternConfig.Id);
                    AddRange(rogueCardEffectSourceIds, runModifiers.GetAddedScratchPatternSourceCardIds(originalPatternId));
                    AddRange(rogueCardEffectSourceIds, runModifiers.GetPatternBaseScoreSourceCardIds(patternConfig.Id));
                    AddRange(rogueCardEffectSourceIds, runModifiers.GetPatternWeightSourceCardIds(patternConfig.Id));
                    AddRange(rogueCardEffectSourceIds, runModifiers.GetAddedScratchPatternSourceCardIds(patternConfig.Id));
                    AddRange(rogueCardEffectSourceIds, runModifiers.GetPatternScratchCardMultiplierSourceCardIds(patternConfig.Id));
                    AddRange(rogueCardEffectSourceIds, runModifiers.GetPatternBaseScoreGrowthSourceCardIds(patternConfig.Id));
                    AddRange(rogueCardEffectSourceIds, runModifiers.GetScratchCardMultiplierSourceCardIds());

                    if (runModifiers.TryRollGiantPattern(
                        patternConfig.Id,
                        out double giantScoreMultiplier,
                        out IReadOnlyCollection<int> giantSourceCardIds))
                    {
                        isGiantFruit = true;
                        scoreMultiplierOnScore = giantScoreMultiplier;
                        AddRange(rogueCardEffectSourceIds, giantSourceCardIds);
                    }
                }

                ApplyCellExtraEffects(
                    cardTypeConfig.ExtraEffects,
                    i,
                    isScratchable,
                    ref scoreMultiplierOnScore);

                results.Add(new ScratchCellModel(
                    i,
                    row,
                    column,
                    patternConfig != null ? patternConfig.Id : 0,
                    patternConfig != null ? patternConfig.Name : "Empty",
                    baseScore,
                    isScratchable,
                    isBaseScoreEnhanced,
                    isGiantFruit,
                    scoreMultiplierOnScore,
                    rewardMultiplierBonusOnScore,
                    new List<int>(rogueCardEffectSourceIds),
                    patternConfig != null ? patternConfig.EffectType : ScratchPatternEffectType.None,
                    patternConfig != null ? patternConfig.EffectValue : 0d));
            }

            return results;
        }

        public static List<ScratchPatternWeightEntry> BuildEffectivePatternWeights(
            ScratchCardTypeConfig cardTypeConfig,
            RogueCardRunModifierModel runModifiers)
        {
            ScratchPatternPoolConfig patternPool = cardTypeConfig != null
                ? ScratchCardDefaultsProvider.GetPatternPool(cardTypeConfig.PatternPoolId)
                : null;
            if (patternPool == null)
            {
                patternPool = ScratchCardDefaultsProvider.GetGlobalPatternPool();
            }

            return BuildEffectivePatternWeights(patternPool, cardTypeConfig, runModifiers);
        }

        public static List<ScratchPatternWeightEntry> BuildEffectivePatternWeights(
            ScratchPatternPoolConfig globalPatternPool,
            ScratchCardTypeConfig cardTypeConfig,
            RogueCardRunModifierModel runModifiers)
        {
            int cardTypeId = cardTypeConfig != null ? cardTypeConfig.Id : 0;
            HashSet<int> allowedPatternIds = BuildAllowedPatternIdSet(cardTypeConfig);
            return BuildEffectivePatternWeights(globalPatternPool, cardTypeId, allowedPatternIds, cardTypeConfig?.ExtraEffects, runModifiers);
        }

        public static List<ScratchPatternWeightEntry> BuildEffectivePatternWeights(
            ScratchPatternPoolConfig patternPool,
            int cardTypeId,
            RogueCardRunModifierModel runModifiers)
        {
            return BuildEffectivePatternWeights(patternPool, cardTypeId, null, null, runModifiers);
        }

        private static List<ScratchPatternWeightEntry> BuildEffectivePatternWeights(
            ScratchPatternPoolConfig patternPool,
            int cardTypeId,
            HashSet<int> allowedPatternIds,
            IReadOnlyList<ScratchCardExtraEffectConfig> extraEffects,
            RogueCardRunModifierModel runModifiers)
        {
            var baseWeightsByPattern = new Dictionary<int, float>();
            var dynamicPatternIds = new HashSet<int>();

            if (patternPool != null && patternPool.Entries != null)
            {
                for (int i = 0; i < patternPool.Entries.Count; i++)
                {
                    ScratchPatternPoolEntryConfig entry = patternPool.Entries[i];
                    if (entry == null || entry.PatternId <= 0 || entry.Weight <= 0)
                    {
                        continue;
                    }

                    if (IsPatternAllowed(entry.PatternId, allowedPatternIds))
                    {
                        AddWeight(baseWeightsByPattern, entry.PatternId, entry.Weight);
                    }
                }
            }

            if (runModifiers != null)
            {
                List<DynamicScratchPatternPoolEntryModel> addedPatterns = runModifiers.GetAddedScratchPatternsForCardType(cardTypeId);
                for (int i = 0; i < addedPatterns.Count; i++)
                {
                    DynamicScratchPatternPoolEntryModel entry = addedPatterns[i];
                    if (entry == null || entry.PatternId <= 0 || entry.Weight <= 0f)
                    {
                        continue;
                    }

                    float addedWeight = entry.IsProbability
                        ? ConvertProbabilityToWeight(baseWeightsByPattern, entry.Weight)
                        : entry.Weight;
                    AddWeight(baseWeightsByPattern, entry.PatternId, addedWeight);
                    dynamicPatternIds.Add(entry.PatternId);
                }
            }

            HashSet<int> cardExtraEffectPatternIds = ApplyCardExtraEffects(baseWeightsByPattern, extraEffects);

            var results = new List<ScratchPatternWeightEntry>();
            foreach (KeyValuePair<int, float> pair in baseWeightsByPattern)
            {
                if (ScratchPatternDefaultProvider.GetById(pair.Key) == null)
                {
                    continue;
                }

                float effectiveWeight = runModifiers != null
                    ? runModifiers.GetEffectivePatternWeight(pair.Key, pair.Value)
                    : Mathf.Max(0f, pair.Value);
                if (effectiveWeight <= 0f)
                {
                    continue;
                }

                results.Add(new ScratchPatternWeightEntry(
                    pair.Key,
                    effectiveWeight,
                    dynamicPatternIds.Contains(pair.Key),
                    cardExtraEffectPatternIds.Contains(pair.Key)));
            }

            results.Sort((left, right) => left.PatternId.CompareTo(right.PatternId));
            return results;
        }

        private static ScratchPatternConfig PickRandomPattern(IReadOnlyList<ScratchPatternWeightEntry> patternWeights)
        {
            float totalWeight = 0f;
            for (int i = 0; i < patternWeights.Count; i++)
            {
                ScratchPatternWeightEntry entry = patternWeights[i];
                if (entry != null)
                {
                    totalWeight += Mathf.Max(0f, entry.Weight);
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float accumulatedWeight = 0f;

            for (int i = 0; i < patternWeights.Count; i++)
            {
                ScratchPatternWeightEntry entry = patternWeights[i];
                if (entry == null)
                {
                    continue;
                }

                accumulatedWeight += Mathf.Max(0f, entry.Weight);
                if (randomValue < accumulatedWeight)
                {
                    return ScratchPatternDefaultProvider.GetById(entry.PatternId);
                }
            }

            return null;
        }

        private static ScratchPatternConfig PickRandomPatternWithoutPattern(
            IReadOnlyList<ScratchPatternWeightEntry> patternWeights,
            int excludedPatternId)
        {
            float totalWeight = 0f;
            for (int i = 0; i < patternWeights.Count; i++)
            {
                ScratchPatternWeightEntry entry = patternWeights[i];
                if (entry != null && entry.PatternId != excludedPatternId)
                {
                    totalWeight += Mathf.Max(0f, entry.Weight);
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float accumulatedWeight = 0f;
            for (int i = 0; i < patternWeights.Count; i++)
            {
                ScratchPatternWeightEntry entry = patternWeights[i];
                if (entry == null || entry.PatternId == excludedPatternId)
                {
                    continue;
                }

                accumulatedWeight += Mathf.Max(0f, entry.Weight);
                if (randomValue < accumulatedWeight)
                {
                    return ScratchPatternDefaultProvider.GetById(entry.PatternId);
                }
            }

            return null;
        }

        private static void AddWeight(Dictionary<int, float> weightsByPattern, int patternId, float weight)
        {
            if (patternId <= 0 || weight <= 0f)
            {
                return;
            }

            if (!weightsByPattern.ContainsKey(patternId))
            {
                weightsByPattern[patternId] = 0f;
            }

            weightsByPattern[patternId] += weight;
        }

        private static float ConvertProbabilityToWeight(Dictionary<int, float> weightsByPattern, float probability)
        {
            probability = Mathf.Clamp01(probability);
            if (probability <= 0f)
            {
                return 0f;
            }

            if (probability >= 1f)
            {
                return 1000000f;
            }

            float baseTotalWeight = 0f;
            foreach (float weight in weightsByPattern.Values)
            {
                baseTotalWeight += Mathf.Max(0f, weight);
            }

            return baseTotalWeight > 0f
                ? baseTotalWeight * probability / (1f - probability)
                : probability;
        }

        private static HashSet<int> BuildAllowedPatternIdSet(ScratchCardTypeConfig cardTypeConfig)
        {
            if (cardTypeConfig?.AllowedPatternIds == null || cardTypeConfig.AllowedPatternIds.Count == 0)
            {
                return null;
            }

            var allowedPatternIds = new HashSet<int>();
            for (int i = 0; i < cardTypeConfig.AllowedPatternIds.Count; i++)
            {
                int patternId = cardTypeConfig.AllowedPatternIds[i];
                if (patternId > 0)
                {
                    allowedPatternIds.Add(patternId);
                }
            }

            return allowedPatternIds.Count > 0 ? allowedPatternIds : null;
        }

        private static bool IsPatternAllowed(int patternId, HashSet<int> allowedPatternIds)
        {
            return allowedPatternIds == null || allowedPatternIds.Contains(patternId);
        }

        private static HashSet<int> ApplyCardExtraEffects(
            Dictionary<int, float> weightsByPattern,
            IReadOnlyList<ScratchCardExtraEffectConfig> extraEffects)
        {
            var affectedPatternIds = new HashSet<int>();
            if (weightsByPattern == null ||
                weightsByPattern.Count == 0 ||
                extraEffects == null ||
                extraEffects.Count == 0)
            {
                return affectedPatternIds;
            }

            for (int i = 0; i < extraEffects.Count; i++)
            {
                ScratchCardExtraEffectConfig effect = extraEffects[i];
                if (effect == null)
                {
                    continue;
                }

                switch (effect.EffectType)
                {
                    case ScratchCardExtraEffectType.MultiplyPatternWeight:
                        ApplyPatternWeightMultiplier(weightsByPattern, effect, affectedPatternIds);
                        break;
                }
            }

            return affectedPatternIds;
        }

        private static void ApplyPatternWeightMultiplier(
            Dictionary<int, float> weightsByPattern,
            ScratchCardExtraEffectConfig effect,
            HashSet<int> affectedPatternIds)
        {
            if (weightsByPattern == null || effect == null)
            {
                return;
            }

            float multiplier = Mathf.Max(0f, (float)effect.Value);
            if (Mathf.Approximately(multiplier, 1f))
            {
                return;
            }

            if (effect.TargetPatternIds == null || effect.TargetPatternIds.Count == 0)
            {
                var allPatternIds = new List<int>(weightsByPattern.Keys);
                for (int i = 0; i < allPatternIds.Count; i++)
                {
                    MultiplyPatternWeight(weightsByPattern, allPatternIds[i], multiplier, affectedPatternIds);
                }

                return;
            }

            var targetPatternIds = new HashSet<int>();
            for (int i = 0; i < effect.TargetPatternIds.Count; i++)
            {
                int patternId = effect.TargetPatternIds[i];
                if (targetPatternIds.Add(patternId))
                {
                    MultiplyPatternWeight(weightsByPattern, patternId, multiplier, affectedPatternIds);
                }
            }
        }

        private static void MultiplyPatternWeight(
            Dictionary<int, float> weightsByPattern,
            int patternId,
            float multiplier,
            HashSet<int> affectedPatternIds)
        {
            if (patternId <= 0 || !weightsByPattern.TryGetValue(patternId, out float weight))
            {
                return;
            }

            weightsByPattern[patternId] = Mathf.Max(0f, weight * multiplier);
            affectedPatternIds?.Add(patternId);
        }

        private static void ApplyCellExtraEffects(
            IReadOnlyList<ScratchCardExtraEffectConfig> extraEffects,
            int cellIndex,
            bool isScratchable,
            ref double scoreMultiplierOnScore)
        {
            if (!isScratchable || extraEffects == null || extraEffects.Count == 0)
            {
                return;
            }

            for (int i = 0; i < extraEffects.Count; i++)
            {
                ScratchCardExtraEffectConfig effect = extraEffects[i];
                if (effect == null)
                {
                    continue;
                }

                switch (effect.EffectType)
                {
                    case ScratchCardExtraEffectType.MultiplyCellScoreMultiplier:
                        ApplyCellScoreMultiplierEffect(effect, cellIndex, ref scoreMultiplierOnScore);
                        break;
                }
            }
        }

        private static void ApplyCellScoreMultiplierEffect(
            ScratchCardExtraEffectConfig effect,
            int cellIndex,
            ref double scoreMultiplierOnScore)
        {
            if (effect == null || !IsTargetCell(effect, cellIndex) || !RollProbability(effect.Probability))
            {
                return;
            }

            double multiplier = effect.Value > 0d ? effect.Value : 1d;
            scoreMultiplierOnScore *= multiplier;
        }

        private static bool IsTargetCell(ScratchCardExtraEffectConfig effect, int cellIndex)
        {
            if (effect?.TargetCellIndices == null || effect.TargetCellIndices.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < effect.TargetCellIndices.Count; i++)
            {
                if (effect.TargetCellIndices[i] == cellIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RollProbability(double probability)
        {
            if (probability <= 0d)
            {
                return false;
            }

            if (probability >= 1d)
            {
                return true;
            }

            return Random.value < (float)probability;
        }

        private static void AddRange(HashSet<int> target, IEnumerable<int> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (int value in source)
            {
                if (value > 0)
                {
                    target.Add(value);
                }
            }
        }
    }
}
