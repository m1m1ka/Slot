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
        private const string ShopIconAtlasPath = "Icons/ScratchCardsListView";

        private static readonly Dictionary<int, ScratchPatternPoolConfig> PatternPools = new Dictionary<int, ScratchPatternPoolConfig>
        {
            {
                1,
                new ScratchPatternPoolConfig
                {
                    Id = 1,
                    Name = "入门水果池",
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
                    Name = "经典配对池",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 28 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 15 },
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
                    Name = "奖励网格池",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 28 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 18 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 14 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 5 }
                    }
                }
            },
            {
                4,
                new ScratchPatternPoolConfig
                {
                    Id = 4,
                    Name = "十字幸运池",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 28 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 18 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 14 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 5 }
                    }
                }
            },
            {
                5,
                new ScratchPatternPoolConfig
                {
                    Id = 5,
                    Name = "高额奖励池",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 18 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 14 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 5 },
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
                    Name = "单排 1×3",
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
                    Name = "经典 2×3",
                    Width = 3,
                    Height = 2,
                    ScratchableCellIndices = new List<int> { 0, 1, 2, 3, 4, 5 }
                }
            },
            {
                4,
                new ScratchAreaTemplateConfig
                {
                    Id = 4,
                    Name = "方形 2×2",
                    Width = 2,
                    Height = 2,
                    ScratchableCellIndices = new List<int> { 0, 1, 2, 3 }
                }
            },
            {
                3,
                new ScratchAreaTemplateConfig
                {
                    Id = 3,
                    Name = "十字 2×3",
                    Width = 3,
                    Height = 2,
                    ScratchableCellIndices = new List<int> { 0, 1, 2, 4 }
                }
            }
        };

        private static readonly Dictionary<int, ScratchCardTypeConfig> CardTypes = new Dictionary<int, ScratchCardTypeConfig>
        {
            { 1, new ScratchCardTypeConfig { Id = 1, Name = "入门单排卡", WinDescription = "刮开的图案可被玩家当前拥有的刮具规则结算。", Price = 10, PatternPoolId = 1, AreaTemplateId = 4, PrefabPath = "UI/ScratchCardView_1", ShopIconAtlasPath = ShopIconAtlasPath, ShopIconSpriteName = "ScratchCards_1" } },
            { 2, new ScratchCardTypeConfig { Id = 2, Name = "经典配对卡", WinDescription = "刮开的图案可被玩家当前拥有的刮具规则结算。", Price = 30, PatternPoolId = 2, AreaTemplateId = 2, PrefabPath = "UI/ScratchCardView_2", ShopIconAtlasPath = ShopIconAtlasPath, ShopIconSpriteName = "ScratchCards_2" } },
            { 3, new ScratchCardTypeConfig { Id = 3, Name = "奖励网格卡", WinDescription = "刮开的图案可被玩家当前拥有的刮具规则结算。", Price = 70, PatternPoolId = 3, AreaTemplateId = 2, PrefabPath = "UI/ScratchCardView_1", ShopIconAtlasPath = ShopIconAtlasPath, ShopIconSpriteName = "ScratchCards_3" } },
            { 4, new ScratchCardTypeConfig { Id = 4, Name = "十字幸运卡", WinDescription = "刮开的图案可被玩家当前拥有的刮具规则结算。", Price = 120, PatternPoolId = 4, AreaTemplateId = 3, PrefabPath = "UI/ScratchCardView_1", ShopIconAtlasPath = ShopIconAtlasPath, ShopIconSpriteName = "ScratchCards_4" } },
            { 5, new ScratchCardTypeConfig { Id = 5, Name = "高额玩家卡", WinDescription = "刮开的图案可被玩家当前拥有的刮具规则结算。", Price = 250, PatternPoolId = 5, AreaTemplateId = 2, PrefabPath = "UI/ScratchCardView_1", ShopIconAtlasPath = ShopIconAtlasPath, ShopIconSpriteName = "ScratchCards_5" } }
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

        public static ScratchAreaTemplateConfig GetAreaTemplate(int templateId)
        {
            AreaTemplates.TryGetValue(templateId, out ScratchAreaTemplateConfig config);
            return config;
        }
    }
}
