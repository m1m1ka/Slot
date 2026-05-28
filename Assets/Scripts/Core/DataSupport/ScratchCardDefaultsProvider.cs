using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchCardDefaultsProvider
    {
        public const int GlobalPatternPoolId = 1;
        public const int FruitPatternPoolId = 2;
        public const int MetalPatternPoolId = 3;

        private static readonly Dictionary<int, ScratchPatternPoolConfig> PatternPools = new Dictionary<int, ScratchPatternPoolConfig>
        {
            {
                GlobalPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = GlobalPatternPoolId,
                    Name = "Global Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 1, Weight = 35 },
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 28 },
                        new ScratchPatternPoolEntryConfig { PatternId = 3, Weight = 22 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 18 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 14 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 5 },
                        new ScratchPatternPoolEntryConfig { PatternId = 10, Weight = 8 }
                    }
                }
            },
            {
                FruitPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = FruitPatternPoolId,
                    Name = "Fruit Pattern Pool",
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
                MetalPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = MetalPatternPoolId,
                    Name = "Metal Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 18 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 5 }
                    }
                }
            }
        };

        private static readonly Dictionary<int, ScratchAreaTemplateConfig> AreaTemplates = new Dictionary<int, ScratchAreaTemplateConfig>
        {
            { 1, new ScratchAreaTemplateConfig { Id = 1, Name = "Single Row 1x3", Width = 3, Height = 1, ScratchableCellIndices = new List<int> { 0, 1, 2 } } },
            { 2, new ScratchAreaTemplateConfig { Id = 2, Name = "Classic 2x3", Width = 3, Height = 2, ScratchableCellIndices = new List<int> { 0, 1, 2, 3, 4, 5 } } },
            { 3, new ScratchAreaTemplateConfig { Id = 3, Name = "Cross 2x3", Width = 3, Height = 2, ScratchableCellIndices = new List<int> { 0, 1, 2, 4 } } },
            { 4, new ScratchAreaTemplateConfig { Id = 4, Name = "Square 2x2", Width = 2, Height = 2, ScratchableCellIndices = new List<int> { 0, 1, 2, 3 } } },
            { 5, new ScratchAreaTemplateConfig { Id = 5, Name = "Single Row 1x5", Width = 5, Height = 1, ScratchableCellIndices = new List<int> { 0, 1, 2, 3, 4 } } }
        };

        private static readonly Dictionary<int, ScratchCardTypeConfig> CardTypes = new Dictionary<int, ScratchCardTypeConfig>
        {
            { 1, new ScratchCardTypeConfig { Id = 1, Name = "刮刮卡", WinDescription = "刮开5个相同图案获奖", Price = 10, PatternPoolId = GlobalPatternPoolId, AllowedPatternIds = new List<int>(), WinRules = new List<ScratchCardWinRuleConfig> { new ScratchCardWinRuleConfig { Id = 1, RuleType = ScratchCardWinRuleType.SamePatternCount, RequiredCount = 5, RequireExactCount = true, ScoreMultiplier = 1d, Description = "刮开5个相同图案获奖" } }, AreaTemplateId = 5, PrefabPath = "UI/ScratchCardView_1", ShopIconPath = "Icons/Clover" } },
            { 2, new ScratchCardTypeConfig { Id = 2, Name = "水果超市", WinDescription = "刮开5个相同图案获奖；初始只有水果图案", Price = 30, PatternPoolId = FruitPatternPoolId, AllowedPatternIds = new List<int>(), WinRules = new List<ScratchCardWinRuleConfig> { new ScratchCardWinRuleConfig { Id = 1, RuleType = ScratchCardWinRuleType.SamePatternCount, RequiredCount = 5, RequireExactCount = true, ScoreMultiplier = 1d, Description = "刮开5个相同图案获奖；初始只有水果图案" } }, AreaTemplateId = 5, PrefabPath = "UI/ScratchCardView_2", ShopIconPath = "Icons/FruitMarket" } }
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

            return null;
        }

        public static IReadOnlyList<ScratchCardTypeConfig> GetAllCardTypes()
        {
            var configs = new List<ScratchCardTypeConfig>(CardTypes.Values);
            configs.Sort((left, right) => left.Id.CompareTo(right.Id));
            return configs;
        }

        public static ScratchPatternPoolConfig GetPatternPool(int poolId)
        {
            PatternPools.TryGetValue(poolId, out ScratchPatternPoolConfig config);
            return config;
        }

        public static ScratchPatternPoolConfig GetGlobalPatternPool()
        {
            return GetPatternPool(GlobalPatternPoolId);
        }

        public static ScratchAreaTemplateConfig GetAreaTemplate(int templateId)
        {
            AreaTemplates.TryGetValue(templateId, out ScratchAreaTemplateConfig config);
            return config;
        }
    }
}
