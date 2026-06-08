using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class LevelDefaultsProvider
    {
        private static readonly Dictionary<int, LevelConfig> Levels = new Dictionary<int, LevelConfig>
        {
            { 1, new LevelConfig { Id = 1, Name = "\u7b2c1\u5173", RequiredCoins = 100, ScratchCardPurchaseLimit = 5 } },
            { 2, new LevelConfig { Id = 2, Name = "\u7b2c2\u5173", RequiredCoins = 300, ScratchCardPurchaseLimit = 6 } },
            { 3, new LevelConfig { Id = 3, Name = "\u7b2c3\u5173", RequiredCoins = 1000, ScratchCardPurchaseLimit = 7 } },
            { 4, new LevelConfig { Id = 4, Name = "\u7b2c4\u5173", RequiredCoins = 2100, ScratchCardPurchaseLimit = 7 } },
            { 5, new LevelConfig { Id = 5, Name = "\u7b2c5\u5173", RequiredCoins = 7500, ScratchCardPurchaseLimit = 8 } },
            { 6, new LevelConfig { Id = 6, Name = "\u7b2c6\u5173", RequiredCoins = 15000, ScratchCardPurchaseLimit = 8 } },
            { 7, new LevelConfig { Id = 7, Name = "\u7b2c7\u5173", RequiredCoins = 60000, ScratchCardPurchaseLimit = 9 } },
            { 8, new LevelConfig { Id = 8, Name = "\u7b2c8\u5173", RequiredCoins = 180000, ScratchCardPurchaseLimit = 9 } },
            { 9, new LevelConfig { Id = 9, Name = "\u7b2c9\u5173", RequiredCoins = 1000000, ScratchCardPurchaseLimit = 10 } },
            { 10, new LevelConfig { Id = 10, Name = "\u7b2c10\u5173", RequiredCoins = 5000000, ScratchCardPurchaseLimit = 10 } },
            { 11, new LevelConfig { Id = 11, Name = "\u7b2c11\u5173", RequiredCoins = 15000000, ScratchCardPurchaseLimit = 11 } }
        };

        private static readonly Dictionary<int, LevelRewardConfig> Rewards = new Dictionary<int, LevelRewardConfig>
        {
            { 1, new LevelRewardConfig(false, 2, 0) },
            { 2, new LevelRewardConfig(true, 0, 0) },
            { 3, new LevelRewardConfig(true, 0, 2) },
            { 4, new LevelRewardConfig(true, 5, 0) },
            { 5, new LevelRewardConfig(true, 3, 0) },
            { 6, new LevelRewardConfig(true, 6, 0) },
            { 7, new LevelRewardConfig(true, 8, 0) },
            { 8, new LevelRewardConfig(true, 4, 3) },
            { 9, new LevelRewardConfig(true, 7, 0) },
            { 10, new LevelRewardConfig(true, 10, 0) },
            { 11, LevelRewardConfig.None }
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
            return GetScratchCardRewardId(levelId) > 0;
        }

        public static bool HasScratchToolReward(int levelId)
        {
            return GetScratchToolRewardId(levelId) > 0;
        }

        public static int GetScratchCardRewardId(int levelId)
        {
            return GetRewardConfig(levelId).ScratchCardRewardId;
        }

        public static int GetScratchToolRewardId(int levelId)
        {
            return GetRewardConfig(levelId).ScratchToolRewardId;
        }

        private static LevelRewardConfig GetRewardConfig(int levelId)
        {
            return Rewards.TryGetValue(levelId, out LevelRewardConfig config)
                ? config
                : LevelRewardConfig.None;
        }

        private struct LevelRewardConfig
        {
            public static readonly LevelRewardConfig None = new LevelRewardConfig(false, 0, 0);

            public readonly bool HasRogueCardReward;
            public readonly int ScratchCardRewardId;
            public readonly int ScratchToolRewardId;

            public LevelRewardConfig(bool hasRogueCardReward, int scratchCardRewardId, int scratchToolRewardId)
            {
                HasRogueCardReward = hasRogueCardReward;
                ScratchCardRewardId = scratchCardRewardId > 0 ? scratchCardRewardId : 0;
                ScratchToolRewardId = scratchToolRewardId > 0 ? scratchToolRewardId : 0;
            }
        }
    }
}
