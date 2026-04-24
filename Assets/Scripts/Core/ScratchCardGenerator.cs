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
        public static List<ScratchCellModel> GenerateCells(ScratchCardTypeConfig cardTypeConfig, ScratchAreaTemplateConfig areaTemplateConfig)
        {
            var results = new List<ScratchCellModel>();

            if (cardTypeConfig == null || areaTemplateConfig == null)
            {
                return results;
            }

            ScratchPatternPoolConfig patternPool = ScratchCardDefaultsProvider.GetPatternPool(cardTypeConfig.PatternPoolId);
            if (patternPool == null || patternPool.Entries == null || patternPool.Entries.Count == 0)
            {
                return results;
            }

            int cellCount = areaTemplateConfig.Width * areaTemplateConfig.Height;
            for (int i = 0; i < cellCount; i++)
            {
                bool isScratchable = areaTemplateConfig.ScratchableCellIndices.Contains(i);
                ScratchPatternConfig patternConfig = PickRandomPattern(patternPool);

                int row = areaTemplateConfig.Width > 0 ? i / areaTemplateConfig.Width : 0;
                int column = areaTemplateConfig.Width > 0 ? i % areaTemplateConfig.Width : 0;

                results.Add(new ScratchCellModel(
                    i,
                    row,
                    column,
                    patternConfig != null ? patternConfig.Id : 0,
                    patternConfig != null ? patternConfig.Name : "Empty",
                    patternConfig != null ? patternConfig.BaseScore : 0,
                    isScratchable));
            }

            return results;
        }

        private static ScratchPatternConfig PickRandomPattern(ScratchPatternPoolConfig patternPool)
        {
            int totalWeight = 0;
            for (int i = 0; i < patternPool.Entries.Count; i++)
            {
                totalWeight += Mathf.Max(0, patternPool.Entries[i].Weight);
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int randomValue = Random.Range(0, totalWeight);
            int accumulatedWeight = 0;

            for (int i = 0; i < patternPool.Entries.Count; i++)
            {
                ScratchPatternPoolEntryConfig entry = patternPool.Entries[i];
                accumulatedWeight += Mathf.Max(0, entry.Weight);

                if (randomValue < accumulatedWeight)
                {
                    return FindPatternById(entry.PatternId);
                }
            }

            return null;
        }

        private static ScratchPatternConfig FindPatternById(int patternId)
        {
            IReadOnlyList<ScratchPatternConfig> patterns = ScratchPatternDefaultProvider.GetAll();
            for (int i = 0; i < patterns.Count; i++)
            {
                if (patterns[i] != null && patterns[i].Id == patternId)
                {
                    return patterns[i];
                }
            }

            return null;
        }
    }
}
