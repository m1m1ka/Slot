using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class LevelDefaultsProvider
    {
        private static readonly Dictionary<int, LevelConfig> Levels = new Dictionary<int, LevelConfig>
        {
            { 1, new LevelConfig { Id = 1, Name = "第1关", RequiredCoins = 1100, ScratchCardPurchaseLimit = 5 } },
            { 2, new LevelConfig { Id = 2, Name = "第2关", RequiredCoins = 1300, ScratchCardPurchaseLimit = 6 } },
            { 3, new LevelConfig { Id = 3, Name = "第3关", RequiredCoins = 1500, ScratchCardPurchaseLimit = 7 } },
            { 4, new LevelConfig { Id = 4, Name = "第4关", RequiredCoins = 1800, ScratchCardPurchaseLimit = 7 } },
            { 5, new LevelConfig { Id = 5, Name = "第5关", RequiredCoins = 2200, ScratchCardPurchaseLimit = 8 } },
            { 6, new LevelConfig { Id = 6, Name = "第6关", RequiredCoins = 2700, ScratchCardPurchaseLimit = 8 } },
            { 7, new LevelConfig { Id = 7, Name = "第7关", RequiredCoins = 3300, ScratchCardPurchaseLimit = 9 } },
            { 8, new LevelConfig { Id = 8, Name = "第8关", RequiredCoins = 4100, ScratchCardPurchaseLimit = 9 } },
            { 9, new LevelConfig { Id = 9, Name = "第9关", RequiredCoins = 5000, ScratchCardPurchaseLimit = 10 } },
            { 10, new LevelConfig { Id = 10, Name = "第10关", RequiredCoins = 6200, ScratchCardPurchaseLimit = 10 } },
            { 11, new LevelConfig { Id = 11, Name = "第11关", RequiredCoins = 7600, ScratchCardPurchaseLimit = 11 } },
            { 12, new LevelConfig { Id = 12, Name = "第12关", RequiredCoins = 9300, ScratchCardPurchaseLimit = 11 } },
            { 13, new LevelConfig { Id = 13, Name = "第13关", RequiredCoins = 11300, ScratchCardPurchaseLimit = 12 } },
            { 14, new LevelConfig { Id = 14, Name = "第14关", RequiredCoins = 13700, ScratchCardPurchaseLimit = 12 } },
            { 15, new LevelConfig { Id = 15, Name = "第15关", RequiredCoins = 16500, ScratchCardPurchaseLimit = 13 } }
        };

        public static LevelConfig GetLevel(int levelId)
        {
            Levels.TryGetValue(levelId, out LevelConfig config);
            return config;
        }

        public static LevelConfig GetFirstLevel()
        {
            return GetLevel(1);
        }

        public static LevelConfig GetNextLevel(int currentLevelId)
        {
            return GetLevel(currentLevelId + 1);
        }
    }
}
