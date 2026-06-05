using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class LevelDefaultsProvider
    {
        private static readonly Dictionary<int, LevelConfig> Levels = new Dictionary<int, LevelConfig>
        {
            { 1, new LevelConfig { Id = 1, Name = "第1关", RequiredCoins = 100, ScratchCardPurchaseLimit = 5 } },
            { 2, new LevelConfig { Id = 2, Name = "第2关", RequiredCoins = 300, ScratchCardPurchaseLimit = 6 } },
            { 3, new LevelConfig { Id = 3, Name = "第3关", RequiredCoins = 600, ScratchCardPurchaseLimit = 7 } },
            { 4, new LevelConfig { Id = 4, Name = "第4关", RequiredCoins = 800, ScratchCardPurchaseLimit = 7 } },
            { 5, new LevelConfig { Id = 5, Name = "第5关", RequiredCoins = 1500, ScratchCardPurchaseLimit = 8 } },
            { 6, new LevelConfig { Id = 6, Name = "第6关", RequiredCoins = 2000, ScratchCardPurchaseLimit = 8 } },
            { 7, new LevelConfig { Id = 7, Name = "第7关", RequiredCoins = 4000, ScratchCardPurchaseLimit = 9 } },
            { 8, new LevelConfig { Id = 8, Name = "第8关", RequiredCoins = 8000, ScratchCardPurchaseLimit = 9 } },
            { 9, new LevelConfig { Id = 9, Name = "第9关", RequiredCoins = 15000, ScratchCardPurchaseLimit = 10 } },
            { 10, new LevelConfig { Id = 10, Name = "第10关", RequiredCoins = 30000, ScratchCardPurchaseLimit = 10 } },
            { 11, new LevelConfig { Id = 11, Name = "第11关", RequiredCoins = 100000, ScratchCardPurchaseLimit = 11 } },
            { 12, new LevelConfig { Id = 12, Name = "第12关", RequiredCoins = 400000, ScratchCardPurchaseLimit = 11 } },
            { 13, new LevelConfig { Id = 13, Name = "第13关", RequiredCoins = 800000, ScratchCardPurchaseLimit = 12 } },
            { 14, new LevelConfig { Id = 14, Name = "第14关", RequiredCoins = 1000000, ScratchCardPurchaseLimit = 12 } },
            { 15, new LevelConfig { Id = 15, Name = "第15关", RequiredCoins = 2000000, ScratchCardPurchaseLimit = 13 } }
        };

        private static readonly Dictionary<int, LevelRewardConfig> Rewards = new Dictionary<int, LevelRewardConfig>
        {
            { 1, new LevelRewardConfig(true, true, true) },
            { 2, new LevelRewardConfig(true, false, false) },
            { 3, new LevelRewardConfig(true, false, false) },
            { 4, new LevelRewardConfig(true, true, true) },
            { 5, new LevelRewardConfig(true, false, false) },
            { 6, new LevelRewardConfig(true, false, false) },
            { 7, new LevelRewardConfig(true, true, false) },
            { 8, new LevelRewardConfig(true, false, true) },
            { 9, new LevelRewardConfig(true, true, false) },
            { 10, new LevelRewardConfig(true, false, false) },
            { 11, new LevelRewardConfig(true, true, false) },
            { 12, new LevelRewardConfig(true, true, true) },
            { 13, new LevelRewardConfig(true, true, false) },
            { 14, new LevelRewardConfig(true, true, false) },
            { 15, new LevelRewardConfig(true, true, false) }
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

        public static bool HasRogueCardReward(int levelId)
        {
            return GetRewardConfig(levelId).HasRogueCardReward;
        }

        public static bool HasScratchCardReward(int levelId)
        {
            return GetRewardConfig(levelId).HasScratchCardReward;
        }

        public static bool HasScratchToolReward(int levelId)
        {
            return GetRewardConfig(levelId).HasScratchToolReward;
        }

        private static LevelRewardConfig GetRewardConfig(int levelId)
        {
            return Rewards.TryGetValue(levelId, out LevelRewardConfig config)
                ? config
                : LevelRewardConfig.None;
        }

        private struct LevelRewardConfig
        {
            public static readonly LevelRewardConfig None = new LevelRewardConfig(false, false, false);

            public readonly bool HasRogueCardReward;
            public readonly bool HasScratchCardReward;
            public readonly bool HasScratchToolReward;

            public LevelRewardConfig(bool hasRogueCardReward, bool hasScratchCardReward, bool hasScratchToolReward)
            {
                HasRogueCardReward = hasRogueCardReward;
                HasScratchCardReward = hasScratchCardReward;
                HasScratchToolReward = hasScratchToolReward;
            }
        }
    }
}
