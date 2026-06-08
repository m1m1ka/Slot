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
        public const int PlanetPatternPoolId = 4;
        public const int BasicScratchCardPatternPoolId = 5;
        public const int FruitBushPatternPoolId = 6;
        public const int StonePatternPoolId = 7;
        public const int OrchardPatternPoolId = 8;
        public const int MinePatternPoolId = 9;
        public const int BankVaultPatternPoolId = 10;
        public const int GoldenTreePatternPoolId = 11;
        public const int EndlessPatternPoolId = 12;

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
                        new ScratchPatternPoolEntryConfig { PatternId = 1, Weight = 34, BaseScore = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 20, BaseScore = 20 },
                        new ScratchPatternPoolEntryConfig { PatternId = 3, Weight = 16, BaseScore = 40 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 12, BaseScore = 80 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 8, BaseScore = 100 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 2, BaseScore = 400 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 30, BaseScore = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 10, Weight = 24, BaseScore = 30 },
                        new ScratchPatternPoolEntryConfig { PatternId = 11, Weight = 16, BaseScore = 100 },
                        new ScratchPatternPoolEntryConfig { PatternId = 12, Weight = 6, BaseScore = 400 },
                        new ScratchPatternPoolEntryConfig { PatternId = 13, Weight = 2, BaseScore = 800 },
                        new ScratchPatternPoolEntryConfig { PatternId = 17, Weight = 18, BaseScore = 80 },
                        new ScratchPatternPoolEntryConfig { PatternId = 18, Weight = 12, BaseScore = 120 },
                        new ScratchPatternPoolEntryConfig { PatternId = 19, Weight = 10, BaseScore = 160 },
                        new ScratchPatternPoolEntryConfig { PatternId = 20, Weight = 8, BaseScore = 200 },
                        new ScratchPatternPoolEntryConfig { PatternId = 21, Weight = 5, BaseScore = 300 },
                        new ScratchPatternPoolEntryConfig { PatternId = 22, Weight = 3, BaseScore = 500 },
                        new ScratchPatternPoolEntryConfig { PatternId = 25, Weight = 3, BaseScore = 300 },
                        new ScratchPatternPoolEntryConfig { PatternId = 26, Weight = 18, BaseScore = 50 },
                        new ScratchPatternPoolEntryConfig { PatternId = 27, Weight = 4, BaseScore = 3000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 28, Weight = 4, BaseScore = 0 },
                        new ScratchPatternPoolEntryConfig { PatternId = 29, Weight = 3, BaseScore = 0 }
                    }
                }
            },
            {
                BasicScratchCardPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = BasicScratchCardPatternPoolId,
                    Name = "Basic Scratch Card Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 1, Weight = 34, BaseScore = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 25, BaseScore = 20 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 35, BaseScore = 15 },
                        new ScratchPatternPoolEntryConfig { PatternId = 10, Weight = 5, BaseScore = 60 },
                        new ScratchPatternPoolEntryConfig { PatternId = 25, Weight = 1, BaseScore = 200 }
                    }
                }
            },
            {
                BankVaultPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = BankVaultPatternPoolId,
                    Name = "Bank Vault Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 16, Weight = 10, BaseScore = 150000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 34, Weight = 90, BaseScore = -30000 }
                    }
                }
            },
            {
                GoldenTreePatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = GoldenTreePatternPoolId,
                    Name = "Golden Tree Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 10, BaseScore = 1000000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 35, Weight = 90, BaseScore = -1000000 }
                    }
                }
            },
            {
                FruitBushPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = FruitBushPatternPoolId,
                    Name = "Fruit Bush Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 2, Weight = 15, BaseScore = 20 },
                        new ScratchPatternPoolEntryConfig { PatternId = 3, Weight = 30, BaseScore = 40 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 15, BaseScore = 80 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 5, BaseScore = 100 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 1, BaseScore = 400 },
                        new ScratchPatternPoolEntryConfig { PatternId = 10, Weight = 14, BaseScore = 60 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 14, BaseScore = 10 },
                        new ScratchPatternPoolEntryConfig { PatternId = 17, Weight = 4, BaseScore = 150 },
                        new ScratchPatternPoolEntryConfig { PatternId = 25, Weight = 1, BaseScore = 300 }
                    }
                }
            },
            {
                OrchardPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = OrchardPatternPoolId,
                    Name = "Orchard Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 3, Weight = 10, BaseScore = 300 },
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 20, BaseScore = 400 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 1, BaseScore = 4000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 20, BaseScore = 600 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 10, BaseScore = 1000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 11, Weight = 20, BaseScore = 500 },
                        new ScratchPatternPoolEntryConfig { PatternId = 12, Weight = 10, BaseScore = 600 },
                        new ScratchPatternPoolEntryConfig { PatternId = 18, Weight = 4, BaseScore = 1200 },
                        new ScratchPatternPoolEntryConfig { PatternId = 25, Weight = 1, BaseScore = 1500 }
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
                        new ScratchPatternPoolEntryConfig { PatternId = 4, Weight = 30, BaseScore = 1000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 7, Weight = 14, BaseScore = 10000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 10, BaseScore = 5000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 8, Weight = 25, BaseScore = 4000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 5, Weight = 1, BaseScore = 100000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 13, Weight = 10, BaseScore = 4000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 14, Weight = 4, BaseScore = 6000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 18, Weight = 5, BaseScore = 20000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 25, Weight = 1, BaseScore = 40000 }
                    }
                }
            },
            {
                StonePatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = StonePatternPoolId,
                    Name = "Stone Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 3, Weight = 10, BaseScore = 100 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 20, BaseScore = 200 },
                        new ScratchPatternPoolEntryConfig { PatternId = 10, Weight = 40, BaseScore = 300 },
                        new ScratchPatternPoolEntryConfig { PatternId = 11, Weight = 20, BaseScore = 600 },
                        new ScratchPatternPoolEntryConfig { PatternId = 12, Weight = 4, BaseScore = 1000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 13, Weight = 1, BaseScore = 4000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 17, Weight = 4, BaseScore = 500 },
                        new ScratchPatternPoolEntryConfig { PatternId = 25, Weight = 1, BaseScore = 800 }
                    }
                }
            },
            {
                MinePatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = MinePatternPoolId,
                    Name = "Mine Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 10, BaseScore = 200 },
                        new ScratchPatternPoolEntryConfig { PatternId = 9, Weight = 40, BaseScore = 300 },
                        new ScratchPatternPoolEntryConfig { PatternId = 12, Weight = 20, BaseScore = 1000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 13, Weight = 15, BaseScore = 4000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 14, Weight = 5, BaseScore = 6000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 15, Weight = 1, BaseScore = 30000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 18, Weight = 4, BaseScore = 7000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 25, Weight = 5, BaseScore = 5000 }
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
                        new ScratchPatternPoolEntryConfig { PatternId = 6, Weight = 10, BaseScore = 500 },
                        new ScratchPatternPoolEntryConfig { PatternId = 11, Weight = 15, BaseScore = 3000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 12, Weight = 30, BaseScore = 5000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 13, Weight = 25, BaseScore = 80000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 14, Weight = 9, BaseScore = 100000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 15, Weight = 5, BaseScore = 150000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 16, Weight = 1, BaseScore = 800000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 19, Weight = 4, BaseScore = 300000 },
                        new ScratchPatternPoolEntryConfig { PatternId = 25, Weight = 1, BaseScore = 200000 }
                    }
                }
            },
            {
                PlanetPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = PlanetPatternPoolId,
                    Name = "Planet Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 17, Weight = 24, BaseScore = 80 },
                        new ScratchPatternPoolEntryConfig { PatternId = 18, Weight = 18, BaseScore = 120 },
                        new ScratchPatternPoolEntryConfig { PatternId = 19, Weight = 14, BaseScore = 160 },
                        new ScratchPatternPoolEntryConfig { PatternId = 20, Weight = 10, BaseScore = 200 },
                        new ScratchPatternPoolEntryConfig { PatternId = 21, Weight = 6, BaseScore = 300 },
                        new ScratchPatternPoolEntryConfig { PatternId = 22, Weight = 4, BaseScore = 500 },
                        new ScratchPatternPoolEntryConfig { PatternId = 23, Weight = 2, BaseScore = 700 },
                        new ScratchPatternPoolEntryConfig { PatternId = 24, Weight = 1, BaseScore = 1000 }
                    }
                }
            },
            {
                EndlessPatternPoolId,
                new ScratchPatternPoolConfig
                {
                    Id = EndlessPatternPoolId,
                    Name = "Endless Pattern Pool",
                    Entries = new List<ScratchPatternPoolEntryConfig>
                    {
                        new ScratchPatternPoolEntryConfig { PatternId = 36, Weight = 50, BaseScore = 0 },
                        new ScratchPatternPoolEntryConfig { PatternId = 37, Weight = 50, BaseScore = 0 }
                    }
                }
            }
        };

        private static readonly Dictionary<int, ScratchAreaTemplateConfig> AreaTemplates = new Dictionary<int, ScratchAreaTemplateConfig>();
        private static readonly Dictionary<int, ScratchCardTypeConfig> CardTypes = new Dictionary<int, ScratchCardTypeConfig>();
        private static readonly Dictionary<int, Vector2Int> LevelDistributions = new Dictionary<int, Vector2Int>
        {
            { 1, new Vector2Int(2, 6) },
            { 2, new Vector2Int(2, 6) },
            { 3, new Vector2Int(8, 11) },
            { 4, new Vector2Int(8, 11) },
            { 5, new Vector2Int(5, 11) },
            { 6, new Vector2Int(9, 11) },
            { 7, new Vector2Int(7, 11) },
            { 8, new Vector2Int(7, 11) },
            { 9, new Vector2Int(6, 11) },
            { 10, new Vector2Int(6, 11) },
            { 11, new Vector2Int(2, 6) },
            { 12, new Vector2Int(6, 11) },
            { 13, new Vector2Int(11, 11) }
        };
        private static readonly Dictionary<int, Dictionary<int, double>> LevelCardPrices = new Dictionary<int, Dictionary<int, double>>
        {
            { 1, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 2, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 3, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 4, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 5, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 6, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 7, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 8, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 9, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 10, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) },
            { 11, CreateLevelPriceRowWithCardPrices(15d, 50d, 1000d, 15000d, 1500d, 10000d) }
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
            AreaTemplates[9] = CreateRectangleTemplate(9, "Single Row 1x6", 6, 1);

            CardTypes[1] = CreateCardType(1, "\u522e\u522e\u5361", "\u522e\u5f005\u4e2a\u76f8\u540c\u56fe\u6848\u83b7\u5956\uff1b\u666e\u901a\u7684\u522e\u522e\u5361", 15, BasicScratchCardPatternPoolId, 5, 5, "UI/ScratchCards/ScratchCardView_1", "Icons/Clover");
            CardTypes[2] = CreateCardType(2, "\u679c\u4e1b", "\u5168\u90e8\u56fe\u6848\u4e3a\u6c34\u679c\u5373\u4e2d\u5956\uff1b\u66f4\u591a\u66f4\u7a00\u6709\u7684\u6c34\u679c\u56fe\u6848", 50, FruitBushPatternPoolId, 1, 5, "UI/ScratchCards/ScratchCardView_2", "Icons/\u679c\u4e1b", null, 1d, ScratchCardWinRuleType.AllFruitPatterns);
            CardTypes[3] = CreateCardType(3, "\u679c\u6797", "\u5168\u90e8\u56fe\u6848\u4e3a\u6c34\u679c\u5373\u4e2d\u5956\uff1b\u66f4\u591a\u66f4\u7a00\u6709\u7684\u6c34\u679c\u56fe\u6848", 1000, OrchardPatternPoolId, 1, 6, "UI/ScratchCards/ScratchCardView_3", "Icons/\u679c\u56ed", null, 1d, ScratchCardWinRuleType.AllFruitPatterns);
            CardTypes[4] = CreateCardType(4, "\u4f0a\u7538\u56ed", "\u5168\u90e8\u56fe\u6848\u4e3a\u6c34\u679c\u5373\u4e2d\u5956\uff1b\u66f4\u591a\u66f4\u7a00\u6709\u7684\u6c34\u679c\u56fe\u6848", 1, FruitPatternPoolId, 1, 7, "UI/ScratchCards/ScratchCardView_4", "Icons/\u679c\u56ed", null, 1d, ScratchCardWinRuleType.AllFruitPatterns);
            CardTypes[5] = CreateCardType(5, "\u77f3\u5757", "\u5168\u90e8\u56fe\u6848\u4e3a\u77ff\u7269\u5373\u4e2d\u5956\uff1b\u66f4\u591a\u66f4\u7a00\u6709\u7684\u77ff\u7269\u56fe\u6848", 2000, StonePatternPoolId, 1, 5, "UI/ScratchCards/ScratchCardView_5", "Icons/\u77f3\u5757", null, 1d, ScratchCardWinRuleType.AllMineralPatterns);
            CardTypes[6] = CreateCardType(6, "\u77ff\u533a", "\u5168\u90e8\u56fe\u6848\u4e3a\u77ff\u7269\u5373\u4e2d\u5956\uff1b\u66f4\u591a\u66f4\u7a00\u6709\u7684\u77ff\u7269\u56fe\u6848", 10000, MinePatternPoolId, 1, 6, "UI/ScratchCards/ScratchCardView_6", "Icons/\u77ff\u533a", null, 1d, ScratchCardWinRuleType.AllMineralPatterns);
            CardTypes[7] = CreateCardType(7, "\u6df1\u77ff\u533a", "\u5168\u90e8\u56fe\u6848\u4e3a\u77ff\u7269\u5373\u4e2d\u5956\uff1b\u66f4\u591a\u66f4\u7a00\u6709\u7684\u77ff\u7269\u56fe\u6848", 1, MetalPatternPoolId, 1, 7, "UI/ScratchCards/ScratchCardView_7", "Icons/\u6df1\u77ff\u533a", null, 1d, ScratchCardWinRuleType.AllMineralPatterns);
            CardTypes[8] = CreateCardType(8, "\u94f6\u884c\u4fdd\u9669\u67dc", "\u5168\u90e8\u5e26\u8d70\u8fd8\u662f\u9512\u94db\u5165\u72f1", 1, BankVaultPatternPoolId, 1, 8, "UI/ScratchCards/ScratchCardView_8", "Icons/\u4fdd\u9669\u67dc", null, 1d, ScratchCardWinRuleType.NoWinPatternEffectsOnly, 0, 0, true, new List<int> { 16, 34 });
            CardTypes[9] = CreateCardType(9, "\u9a6c\u620f\u56e2", "\u5168\u90e8\u56fe\u6848\u4e3a\u597d\u8138\u5c0f\u4e11\u5373\u4e2d\u5956\uff1b\u53ea\u80fd\u522e\u51fa\u5c0f\u4e11\u56fe\u6848", 1, GlobalPatternPoolId, 1, 5, "UI/ScratchCards/ScratchCardView_9", "Icons/\u9a6c\u620f\u56e2", CreatePatternTypeRestrictionEffect("Joker"), 1d, ScratchCardWinRuleType.AllGoodFaceJokerPatterns);
            CardTypes[10] = CreateCardType(10, "\u9ec4\u91d1\u6811", "\u522e\u5f00\u5373\u4e2d\u5956\uff1b\u522e\u5f00\u5373\u4e2d\u5956...\u4f46\u8981\u5c0f\u5fc3\u86c7\uff01", 1, GoldenTreePatternPoolId, 1, 9, "UI/ScratchCards/ScratchCardView_10", "Icons/\u9ec4\u91d1\u6811", null, 1d, ScratchCardWinRuleType.ScoreEveryRevealedPattern, 0, 0, true, new List<int> { 5, 35 });
            CardTypes[11] = CreateCardType(11, "\u671b\u8fdc\u955c", "\u5168\u90e8\u56fe\u6848\u4e3a\u661f\u7403\u5373\u4e2d\u5956\uff1b\u66f4\u591a\u66f4\u7a00\u6709\u7684\u661f\u7403\u56fe\u6848", 1, PlanetPatternPoolId, 1, 6, "UI/ScratchCards/ScratchCardView_11", "Icons/\u671b\u8fdc\u955c", null, 1d, ScratchCardWinRuleType.AllPlanetPatterns);
            CardTypes[12] = CreateCardType(12, "\u5929\u6587\u53f0", "\u5168\u90e8\u56fe\u6848\u4e3a\u661f\u7403\u5373\u4e2d\u5956\uff1b\u66f4\u591a\u66f4\u7a00\u6709\u7684\u661f\u7403\u56fe\u6848", 1, PlanetPatternPoolId, 1, 7, "UI/ScratchCards/ScratchCardView_12", "Icons/\u5929\u6587\u53f0", null, 1d, ScratchCardWinRuleType.AllPlanetPatterns);
            CardTypes[13] = CreateCardType(13, "\u4e71\u65e0\u6b62\u5883", "\u4e0d\u4f1a\u83b7\u5f97\u91d1\u5e01\uff1b????", 1, EndlessPatternPoolId, 1, 8, "UI/ScratchCards/ScratchCardView_13", "Icons/\u4e71\u65e0\u6b62\u5883", null, 1d, ScratchCardWinRuleType.GameOver);
        }

        public static double GetCardTypePrice(int cardTypeId, int levelId)
        {
            if (LevelCardPrices.TryGetValue(levelId, out Dictionary<int, double> levelPrices) &&
                levelPrices.TryGetValue(cardTypeId, out double price))
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

        public static bool IsHighestBaseScorePatternInCardPool(int cardTypeId, int patternId)
        {
            if (patternId <= 0)
            {
                return false;
            }

            if (cardTypeId == 13)
            {
                return patternId == 37;
            }

            ScratchCardTypeConfig cardTypeConfig = GetCardType(cardTypeId);
            ScratchPatternPoolConfig patternPool = cardTypeConfig != null
                ? GetPatternPool(cardTypeConfig.PatternPoolId)
                : GetGlobalPatternPool();

            if (patternPool?.Entries == null || patternPool.Entries.Count == 0)
            {
                return false;
            }

            int highestBaseScore = int.MinValue;
            for (int i = 0; i < patternPool.Entries.Count; i++)
            {
                ScratchPatternPoolEntryConfig entry = patternPool.Entries[i];
                if (entry != null && IsPatternAllowedForCardType(cardTypeConfig, entry.PatternId))
                {
                    highestBaseScore = Mathf.Max(highestBaseScore, entry.BaseScore);
                }
            }

            if (highestBaseScore == int.MinValue)
            {
                return false;
            }

            for (int i = 0; i < patternPool.Entries.Count; i++)
            {
                ScratchPatternPoolEntryConfig entry = patternPool.Entries[i];
                if (entry != null &&
                    entry.PatternId == patternId &&
                    entry.BaseScore == highestBaseScore &&
                    IsPatternAllowedForCardType(cardTypeConfig, entry.PatternId))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFruitPattern(int patternId)
        {
            return IsPatternType(patternId, "Fruit");
        }

        public static bool IsMineralPattern(int patternId)
        {
            return IsPatternType(patternId, "Mineral");
        }

        public static bool IsPlanetPattern(int patternId)
        {
            return IsPatternType(patternId, "Planet");
        }

        private static bool IsPatternType(int patternId, string patternType)
        {
            ScratchPatternConfig patternConfig = ScratchPatternDefaultProvider.GetById(patternId);
            return patternConfig != null && patternConfig.Type == patternType;
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

        private static Dictionary<int, double> CreateLevelPriceRow(params double[] prices)
        {
            var row = new Dictionary<int, double>();
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

        private static Dictionary<int, double> CreateUniformLevelPriceRow(double price)
        {
            return CreateLevelPriceRow(
                price, price, price, price,
                price, price, price, price,
                price, price, price, price);
        }

        private static Dictionary<int, double> CreateLevelPriceRowWithCardPrices(double card1Price, double card2Price, double card3Price, double card4Price, double card5Price, double card6Price)
        {
            Dictionary<int, double> row = CreateUniformLevelPriceRow(0d);
            row[1] = card1Price;
            row[2] = card2Price;
            row[3] = card3Price;
            row[4] = card4Price;
            row[5] = card5Price;
            row[6] = card6Price;
            row[7] = 500000d;
            row[8] = 0d;
            row[10] = 1000000d;
            return row;
        }

        private static ScratchCardTypeConfig CreateCardType(
            int id,
            string name,
            string description,
            double price,
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
            bool requireExactCount = true,
            List<int> allowedPatternIds = null)
        {
            return new ScratchCardTypeConfig
            {
                Id = id,
                Name = name,
                WinDescription = GetDescriptionPart(description, 0),
                SpecialDescription = GetDescriptionPart(description, 1),
                Price = price,
                PatternPoolId = patternPoolId,
                AllowedPatternIds = allowedPatternIds ?? new List<int>(),
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
                    TargetPatternIds = new List<int> { 9, 10, 11, 12, 13, 14, 15, 16 },
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

        private static string GetDescriptionPart(string description, int index)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return string.Empty;
            }

            string[] parts = description.Split(new[] { '\uff1b', ';' });
            if (index < 0 || index >= parts.Length)
            {
                return index == 0 ? description.Trim() : string.Empty;
            }

            return parts[index].Trim();
        }

        private static List<ScratchCardExtraEffectConfig> CreatePatternTypeRestrictionEffect(string patternType)
        {
            return new List<ScratchCardExtraEffectConfig>
            {
                new ScratchCardExtraEffectConfig
                {
                    EffectType = ScratchCardExtraEffectType.RestrictPatternType,
                    TargetPatternType = patternType,
                    Description = $"Only {patternType} patterns can appear."
                }
            };
        }

        private static bool IsPatternAllowedForCardType(ScratchCardTypeConfig cardTypeConfig, int patternId)
        {
            if (cardTypeConfig?.AllowedPatternIds == null || cardTypeConfig.AllowedPatternIds.Count == 0)
            {
                return true;
            }

            return cardTypeConfig.AllowedPatternIds.Contains(patternId);
        }
    }
}
