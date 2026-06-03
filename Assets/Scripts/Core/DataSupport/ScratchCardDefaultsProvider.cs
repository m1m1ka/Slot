using System.Collections.Generic;
using Configs;
using UnityEngine;

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
                        new ScratchPatternPoolEntryConfig { PatternId = 1, Weight = 24 },
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 20 },
                        new ScratchPatternPoolEntryConfig { PatternId = 3, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 8 },
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 8 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 6 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 4 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 2 },
                        new ScratchPatternPoolEntryConfig { PatternId = 10, Weight = 1 }
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
                        new ScratchPatternPoolEntryConfig { PatternId = 1, Weight = 24 },
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 20 },
                        new ScratchPatternPoolEntryConfig { PatternId = 3, Weight = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 8 }
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
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 8 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 4 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 2 }
                    }
                }
            }
        };

        private static readonly Dictionary<int, ScratchAreaTemplateConfig> AreaTemplates = new Dictionary<int, ScratchAreaTemplateConfig>();
        private static readonly Dictionary<int, ScratchCardTypeConfig> CardTypes = new Dictionary<int, ScratchCardTypeConfig>();
        private static readonly Dictionary<int, Vector2Int> LevelDistributions = new Dictionary<int, Vector2Int>
        {
            { 1, new Vector2Int(1, 15) },
            { 2, new Vector2Int(1, 15) },
            { 3, new Vector2Int(3, 15) },
            { 4, new Vector2Int(8, 15) },
            { 5, new Vector2Int(1, 15) },
            { 6, new Vector2Int(3, 15) },
            { 7, new Vector2Int(8, 15) },
            { 8, new Vector2Int(3, 15) },
            { 9, new Vector2Int(6, 15) },
            { 10, new Vector2Int(8, 15) },
            { 11, new Vector2Int(4, 15) },
            { 12, new Vector2Int(8, 15) },
            { 13, new Vector2Int(4, 15) },
            { 14, new Vector2Int(8, 15) },
            { 15, new Vector2Int(6, 15) },
            { 16, new Vector2Int(8, 15) }
        };
        private static readonly Dictionary<int, Dictionary<int, int>> LevelCardPrices = new Dictionary<int, Dictionary<int, int>>
        {
            { 1, CreateLevelPriceRow(50, 50, 50, 100, 50, 50, 100, 50, 50, 100, 50, 100, 50, 100, 50, 100) },
            { 2, CreateLevelPriceRow(200, 200, 400, 600, 200, 400, 600, 200, 200, 600, 200, 600, 200, 600, 200, 600) },
            { 3, CreateLevelPriceRow(500, 500, 1000, 1500, 500, 1000, 1500, 500, 500, 1500, 500, 1500, 500, 1500, 500, 1500) },
            { 4, CreateLevelPriceRow(1000, 1000, 2000, 3000, 1000, 2000, 3000, 1000, 1000, 3000, 1000, 3000, 1000, 3000, 1000, 3000) },
            { 5, CreateLevelPriceRow(1500, 1500, 3000, 4500, 1500, 3000, 4500, 1500, 1500, 4500, 1500, 4500, 1500, 4500, 1500, 4500) },
            { 6, CreateLevelPriceRow(2000, 2000, 4000, 6000, 2000, 4000, 6000, 2000, 2000, 6000, 2000, 6000, 2000, 6000, 2000, 6000) },
            { 7, CreateLevelPriceRow(2500, 2500, 5000, 7500, 2500, 5000, 7500, 2500, 2500, 7500, 2500, 7500, 2500, 7500, 2500, 7500) },
            { 8, CreateLevelPriceRow(3000, 3000, 6000, 9000, 3000, 6000, 9000, 3000, 3000, 9000, 3000, 9000, 3000, 9000, 3000, 9000) },
            { 9, CreateLevelPriceRow(3750, 3750, 7500, 11250, 3750, 7500, 11250, 3750, 3750, 11250, 3750, 11250, 3750, 11250, 3750, 11250) },
            { 10, CreateLevelPriceRow(5000, 5000, 10000, 15000, 5000, 10000, 15000, 5000, 5000, 15000, 5000, 15000, 5000, 15000, 5000, 15000) },
            { 11, CreateLevelPriceRow(6250, 6250, 12500, 18750, 6250, 12500, 18750, 6250, 6250, 18750, 6250, 18750, 6250, 18750, 6250, 18750) },
            { 12, CreateLevelPriceRow(7500, 7500, 15000, 22500, 7500, 15000, 22500, 7500, 7500, 22500, 7500, 22500, 7500, 22500, 7500, 22500) },
            { 13, CreateLevelPriceRow(8750, 8750, 17500, 26250, 8750, 17500, 26250, 8750, 8750, 26250, 8750, 26250, 8750, 26250, 8750, 26250) },
            { 14, CreateLevelPriceRow(10000, 10000, 20000, 30000, 10000, 20000, 30000, 10000, 10000, 30000, 10000, 30000, 10000, 30000, 10000, 30000) },
            { 15, CreateLevelPriceRow(12500, 12500, 25000, 37500, 12500, 25000, 37500, 12500, 12500, 37500, 12500, 37500, 12500, 37500, 12500, 37500) }
        };

        static ScratchCardDefaultsProvider()
        {
            AreaTemplates[1] = CreateRectangleTemplate(1, "Single Row 1x3", 3, 1);
            AreaTemplates[2] = CreateRectangleTemplate(2, "Classic 2x3", 3, 2);
            AreaTemplates[3] = CreateAreaTemplate(3, "Cross 2x3", 3, 2, new List<int> { 0, 1, 2, 4 });
            AreaTemplates[4] = CreateRectangleTemplate(4, "Square 2x2", 2, 2);
            AreaTemplates[5] = CreateRectangleTemplate(5, "Single Row 1x5", 5, 1);
            AreaTemplates[6] = CreateRectangleTemplate(6, "Classic 2x5", 5, 2);
            AreaTemplates[7] = CreateRectangleTemplate(7, "Classic 3x5", 5, 3);
            AreaTemplates[8] = CreateRectangleTemplate(8, "Single Cell 1x1", 1, 1);

            CardTypes[1] = CreateCardType(1, "刮刮卡", "刮开5个相同图案获奖", 1, GlobalPatternPoolId, 5, 5, "UI/ScratchCards/ScratchCardView_1", "Icons/Clover");
            CardTypes[2] = CreateCardType(2, "水果超市", "刮开5个相同图案获奖；初始只有水果图案", 1, FruitPatternPoolId, 5, 5, "UI/ScratchCards/ScratchCardView_2", "Icons/FruitMarket");
            CardTypes[3] = CreateCardType(3, "水果超市+", "刮开10个相同图案获奖；初始只有水果图案", 2, FruitPatternPoolId, 10, 6, "UI/ScratchCards/ScratchCardView_3", "Icons/FruitMarket");
            CardTypes[4] = CreateCardType(4, "水果超市++", "刮开15个相同图案获奖；初始只有水果图案", 2, FruitPatternPoolId, 15, 7, "UI/ScratchCards/ScratchCardView_4", "Icons/FruitMarket");
            CardTypes[5] = CreateCardType(5, "金属探测器", "刮开5个相同图案获奖；金属图案概率翻倍", 1, GlobalPatternPoolId, 5, 5, "UI/ScratchCards/ScratchCardView_5", "Icons/Clover", CreateMetalWeightDoubleEffect());
            CardTypes[6] = CreateCardType(6, "金属探测器+", "刮开10个相同图案获奖；金属图案概率翻倍", 1, GlobalPatternPoolId, 10, 6, "UI/ScratchCards/ScratchCardView_6", "Icons/Clover", CreateMetalWeightDoubleEffect());
            CardTypes[7] = CreateCardType(7, "金属探测器++", "刮开15个相同图案获奖；金属图案概率翻倍", 1, GlobalPatternPoolId, 15, 7, "UI/ScratchCards/ScratchCardView_7", "Icons/Clover", CreateMetalWeightDoubleEffect());
            CardTypes[8] = CreateCardType(8, "保险箱", "刮开即中奖；并获得10倍率", 1, GlobalPatternPoolId, 1, 8, "UI/ScratchCards/ScratchCardView_8", "Icons/Clover", null, 10d);
            CardTypes[9] = CreateCardType(9, "马戏团", "刮出5个小丑获奖；小丑图案概率翻倍", 1, GlobalPatternPoolId, 5, 6, "UI/ScratchCards/ScratchCardView_9", "Icons/Clover", CreatePatternWeightMultiplierEffect(14, 2d), 1d, ScratchCardWinRuleType.SpecificPatternCount, 14);
            CardTypes[10] = CreateCardType(10, "马戏团+", "刮出10个小丑获奖；小丑图案概率翻倍", 1, GlobalPatternPoolId, 10, 7, "UI/ScratchCards/ScratchCardView_10", "Icons/Clover", CreatePatternWeightMultiplierEffect(14, 2d), 1d, ScratchCardWinRuleType.SpecificPatternCount, 14);
            CardTypes[11] = CreateCardType(11, "幸运四叶草", "刮出10个相同图案获奖；特定位置图案获得额外效果", 1, GlobalPatternPoolId, 10, 6, "UI/ScratchCards/ScratchCardView_11", "Icons/Clover", CreateCellScoreMultiplierEffect(3d, 0.08d));
            CardTypes[12] = CreateCardType(12, "幸运四叶草+", "刮开15个相同图案获奖；特定位置图案获得额外效果", 1, GlobalPatternPoolId, 15, 7, "UI/ScratchCards/ScratchCardView_12", "Icons/Clover", CreateCellScoreMultiplierEffect(3d, 0.08d));
            CardTypes[13] = CreateCardType(13, "幸运数字", "刮出7个相同图案获奖；刮出幸运7图案概率翻倍", 1, GlobalPatternPoolId, 7, 6, "UI/ScratchCards/ScratchCardView_13", "Icons/Clover", CreatePatternWeightMultiplierEffect(10, 2d));
            CardTypes[14] = CreateCardType(14, "幸运数字+", "刮出14个相同图案获奖；刮出幸运7图案概率翻倍", 1, GlobalPatternPoolId, 14, 7, "UI/ScratchCards/ScratchCardView_14", "Icons/Clover", CreatePatternWeightMultiplierEffect(10, 2d));
            CardTypes[15] = CreateCardType(15, "幸运倍率", "刮出10个相同图案获奖；倍率图案概率翻倍", 1, GlobalPatternPoolId, 10, 7, "UI/ScratchCards/ScratchCardView_15", "Icons/Clover", CreatePatternWeightMultiplierEffect(15, 2d));
            CardTypes[16] = CreateCardType(16, "幸运倍率+", "刮出10个相同图案获奖；倍率图案概率翻倍", 1, GlobalPatternPoolId, 15, 7, "UI/ScratchCards/ScratchCardView_16", "Icons/Clover", CreatePatternWeightMultiplierEffect(15, 2d));
        }

        public static int GetCardTypePrice(int cardTypeId, int levelId)
        {
            if (LevelCardPrices.TryGetValue(levelId, out Dictionary<int, int> levelPrices) &&
                levelPrices.TryGetValue(cardTypeId, out int price))
            {
                return price;
            }

            ScratchCardTypeConfig config = GetCardType(cardTypeId);
            return config != null ? config.Price : 0;
        }

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

        public static IReadOnlyList<ScratchCardTypeConfig> GetAvailableCardTypesForLevel(int levelId)
        {
            var configs = new List<ScratchCardTypeConfig>();
            foreach (ScratchCardTypeConfig config in CardTypes.Values)
            {
                if (config != null && IsCardTypeAvailableForLevel(config.Id, levelId))
                {
                    configs.Add(config);
                }
            }

            configs.Sort((left, right) => left.Id.CompareTo(right.Id));
            return configs;
        }

        public static bool IsCardTypeAvailableForLevel(int cardTypeId, int levelId)
        {
            if (!LevelDistributions.TryGetValue(cardTypeId, out Vector2Int levelRange))
            {
                return true;
            }

            return levelId >= levelRange.x && levelId <= levelRange.y;
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

        private static ScratchAreaTemplateConfig CreateRectangleTemplate(int id, string name, int width, int height)
        {
            var scratchableCellIndices = new List<int>();
            int cellCount = width * height;
            for (int i = 0; i < cellCount; i++)
            {
                scratchableCellIndices.Add(i);
            }

            return CreateAreaTemplate(id, name, width, height, scratchableCellIndices);
        }

        private static ScratchAreaTemplateConfig CreateAreaTemplate(int id, string name, int width, int height, List<int> scratchableCellIndices)
        {
            return new ScratchAreaTemplateConfig
            {
                Id = id,
                Name = name,
                Width = width,
                Height = height,
                ScratchableCellIndices = scratchableCellIndices ?? new List<int>()
            };
        }

        private static Dictionary<int, int> CreateLevelPriceRow(params int[] prices)
        {
            var row = new Dictionary<int, int>();
            if (prices == null)
            {
                return row;
            }

            for (int i = 0; i < prices.Length; i++)
            {
                row[i + 1] = prices[i];
            }

            return row;
        }

        private static ScratchCardTypeConfig CreateCardType(
            int id,
            string name,
            string description,
            int price,
            int patternPoolId,
            int requiredCount,
            int areaTemplateId,
            string prefabPath,
            string shopIconPath,
            List<ScratchCardExtraEffectConfig> extraEffects = null,
            double scoreMultiplier = 1d,
            ScratchCardWinRuleType winRuleType = ScratchCardWinRuleType.SamePatternCount,
            int targetPatternId = 0,
            int scorePerMatchedCell = 0,
            bool requireExactCount = true)
        {
            return new ScratchCardTypeConfig
            {
                Id = id,
                Name = name,
                WinDescription = description,
                Price = price,
                PatternPoolId = patternPoolId,
                AllowedPatternIds = new List<int>(),
                ExtraEffects = extraEffects ?? new List<ScratchCardExtraEffectConfig>(),
                WinRules = new List<ScratchCardWinRuleConfig>
                {
                    new ScratchCardWinRuleConfig
                    {
                        Id = id,
                        RuleType = winRuleType,
                        TargetPatternId = targetPatternId,
                        RequiredCount = requiredCount,
                        RequireExactCount = requireExactCount,
                        ScorePerMatchedCell = scorePerMatchedCell,
                        ScoreMultiplier = scoreMultiplier > 0d ? scoreMultiplier : 1d,
                        Description = description
                    }
                },
                AreaTemplateId = areaTemplateId,
                PrefabPath = prefabPath,
                ShopIconPath = shopIconPath
            };
        }

        private static List<ScratchCardExtraEffectConfig> CreateMetalWeightDoubleEffect()
        {
            return new List<ScratchCardExtraEffectConfig>
            {
                new ScratchCardExtraEffectConfig
                {
                    EffectType = ScratchCardExtraEffectType.MultiplyPatternWeight,
                    TargetPatternIds = new List<int> { 5, 6, 8, 9 },
                    Value = 2d,
                    Description = "Metal pattern weights x2."
                }
            };
        }

        private static List<ScratchCardExtraEffectConfig> CreatePatternWeightMultiplierEffect(int targetPatternId, double multiplier)
        {
            return new List<ScratchCardExtraEffectConfig>
            {
                new ScratchCardExtraEffectConfig
                {
                    EffectType = ScratchCardExtraEffectType.MultiplyPatternWeight,
                    TargetPatternIds = new List<int> { targetPatternId },
                    Value = multiplier,
                    Description = $"Pattern {targetPatternId} weight x{multiplier:0.##}."
                }
            };
        }

        private static List<ScratchCardExtraEffectConfig> CreateCellScoreMultiplierEffect(double multiplier, double probability)
        {
            return new List<ScratchCardExtraEffectConfig>
            {
                new ScratchCardExtraEffectConfig
                {
                    EffectType = ScratchCardExtraEffectType.MultiplyCellScoreMultiplier,
                    Value = multiplier,
                    Probability = probability,
                    Description = $"Each scratchable cell has {probability:P0} chance to score x{multiplier:0.##}."
                }
            };
        }

        private static List<ScratchCardExtraEffectConfig> CreateRewardMultiplierBonusOnSettlementEffect(double bonus = 0.1d)
        {
            return new List<ScratchCardExtraEffectConfig>
            {
                new ScratchCardExtraEffectConfig
                {
                    EffectType = ScratchCardExtraEffectType.AddRewardMultiplierOnSettlement,
                    Value = bonus > 0d ? bonus : 0.1d,
                    Description = $"Reward multiplier +{(bonus > 0d ? bonus : 0.1d):0.##} on settlement."
                }
            };
        }
    }
}
