using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 彩票内容生成器。
    /// 当前根据卡种配置、图案池、可刮模板生成单张彩票实例数据。
    /// </summary>
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

            ScratchPatternPoolConfig patternPool = ScratchCardDefaultsProvider.GetPatternPool(cardTypeConfig.PatternPoolId);
            List<ScratchPatternWeightEntry> patternWeights = BuildEffectivePatternWeights(patternPool, cardTypeConfig.Id, runModifiers);
            if (patternWeights.Count == 0)
            {
                return results;
            }

            int cellCount = areaTemplateConfig.Width * areaTemplateConfig.Height;
            for (int i = 0; i < cellCount; i++)
            {
                bool isScratchable = areaTemplateConfig.ScratchableCellIndices.Contains(i);
                ScratchPatternConfig patternConfig = PickRandomPattern(patternWeights);

                int row = areaTemplateConfig.Width > 0 ? i / areaTemplateConfig.Width : 0;
                int column = areaTemplateConfig.Width > 0 ? i % areaTemplateConfig.Width : 0;

                int baseScore = patternConfig != null ? patternConfig.BaseScore : 0;
                bool isBaseScoreEnhanced = false;
                double rewardMultiplierBonusOnScore = 0d;
                if (patternConfig != null && runModifiers != null)
                {
                    int baseScoreBonus = runModifiers.GetPatternBaseScoreBonus(patternConfig.Id);
                    baseScore += baseScoreBonus;
                    isBaseScoreEnhanced = baseScoreBonus != 0;
                    rewardMultiplierBonusOnScore = runModifiers.GetPatternScratchCardMultiplierBonus(patternConfig.Id);
                }

                results.Add(new ScratchCellModel(
                    i,
                    row,
                    column,
                    patternConfig != null ? patternConfig.Id : 0,
                    patternConfig != null ? patternConfig.Name : "空",
                    baseScore,
                    isScratchable,
                    isBaseScoreEnhanced,
                    rewardMultiplierBonusOnScore,
                    patternConfig != null ? patternConfig.EffectType : ScratchPatternEffectType.None,
                    patternConfig != null ? patternConfig.EffectValue : 0d));
            }

            return results;
        }

        public static List<ScratchPatternWeightEntry> BuildEffectivePatternWeights(
            ScratchPatternPoolConfig patternPool,
            int cardTypeId,
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

                    AddWeight(baseWeightsByPattern, entry.PatternId, entry.Weight);
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

                    AddWeight(baseWeightsByPattern, entry.PatternId, entry.Weight);
                    dynamicPatternIds.Add(entry.PatternId);
                }
            }

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

                results.Add(new ScratchPatternWeightEntry(pair.Key, effectiveWeight, dynamicPatternIds.Contains(pair.Key)));
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
    }
}
