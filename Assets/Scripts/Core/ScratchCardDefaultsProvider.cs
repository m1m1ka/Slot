using System.Collections.Generic;
using Configs;

namespace Core
{
    /// <summary>
    /// 刮刮卡默认配置提供者。
    /// 后续可替换为正式配表读取。
    /// </summary>
    public static class ScratchCardDefaultsProvider
    {
        private static readonly Dictionary<int, ScratchPatternPoolConfig> PatternPools = new Dictionary<int, ScratchPatternPoolConfig>
        {
            {
                1,
                new ScratchPatternPoolConfig
                {
                    Id = 1,
                    Name = "Starter Fruit Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 1, Weight = 35 },
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 28 },
                        new ScratchPatternPoolEntryConfig { PatternId = 3, Weight = 22 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 15 }
                    }
                }
            },
            {
                2,
                new ScratchPatternPoolConfig
                {
                    Id = 2,
                    Name = "Mixed Bonus Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 18 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 20 },
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 18 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 14 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 5 }
                    }
                }
            },
            {
                3,
                new ScratchPatternPoolConfig
                {
                    Id = 3,
                    Name = "High Roller Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 24 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 22 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 18 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 16 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 12 },
                        new ScratchPatternPoolEntryConfig { PatternId = 10, Weight = 8 }
                    }
                }
            }
        };

        private static readonly Dictionary<int, ScratchAreaTemplateConfig> AreaTemplates = new Dictionary<int, ScratchAreaTemplateConfig>
        {
            {
                1,
                new ScratchAreaTemplateConfig
                {
                    Id = 1,
                    Name = "Single Row 1x3",
                    Width = 3,
                    Height = 1,
                    ScratchableCellIndices = new List<int> { 0, 1, 2 }
                }
            },
            {
                2,
                new ScratchAreaTemplateConfig
                {
                    Id = 2,
                    Name = "Classic 2x3",
                    Width = 3,
                    Height = 2,
                    ScratchableCellIndices = new List<int> { 0, 1, 2, 3, 4, 5 }
                }
            },
            {
                3,
                new ScratchAreaTemplateConfig
                {
                    Id = 3,
                    Name = "Cross 2x3",
                    Width = 3,
                    Height = 2,
                    ScratchableCellIndices = new List<int> { 0, 1, 2, 4 }
                }
            }
        };

        private static readonly Dictionary<int, ScratchCardTypeConfig> CardTypes = new Dictionary<int, ScratchCardTypeConfig>
        {
            { 1, new ScratchCardTypeConfig { Id = 1, Name = "Starter Row Card", Price = 100, PatternPoolId = 1, AreaTemplateId = 1, SettlementType = ScratchSettlementType.SumScore, PrefabPath = "UI/ScratchCardView" } },
            { 2, new ScratchCardTypeConfig { Id = 2, Name = "Classic Grid Card", Price = 300, PatternPoolId = 1, AreaTemplateId = 2, SettlementType = ScratchSettlementType.MatchAnyThree, PrefabPath = "UI/ScratchCardView" } },
            { 3, new ScratchCardTypeConfig { Id = 3, Name = "Bonus Grid Card", Price = 700, PatternPoolId = 2, AreaTemplateId = 2, SettlementType = ScratchSettlementType.SumScore, PrefabPath = "UI/ScratchCardView" } },
            { 4, new ScratchCardTypeConfig { Id = 4, Name = "Cross Fortune Card", Price = 1200, PatternPoolId = 2, AreaTemplateId = 3, SettlementType = ScratchSettlementType.RowSumBonus, PrefabPath = "UI/ScratchCardView" } },
            { 5, new ScratchCardTypeConfig { Id = 5, Name = "High Roller Card", Price = 2500, PatternPoolId = 3, AreaTemplateId = 2, SettlementType = ScratchSettlementType.MatchAnyThree, PrefabPath = "UI/ScratchCardView" } }
        };

        public static ScratchCardTypeConfig GetCardType(int cardTypeId)
        {
            CardTypes.TryGetValue(cardTypeId, out ScratchCardTypeConfig config);
            return config;
        }

        public static ScratchCardTypeConfig GetCardTypeForShopSlot(int slotId)
        {
            if (CardTypes.TryGetValue(slotId, out ScratchCardTypeConfig config))
            {
                return config;
            }

            return GetCardType(1);
        }

        public static ScratchPatternPoolConfig GetPatternPool(int poolId)
        {
            PatternPools.TryGetValue(poolId, out ScratchPatternPoolConfig config);
            return config;
        }

        public static ScratchAreaTemplateConfig GetAreaTemplate(int templateId)
        {
            AreaTemplates.TryGetValue(templateId, out ScratchAreaTemplateConfig config);
            return config;
        }
    }
}
