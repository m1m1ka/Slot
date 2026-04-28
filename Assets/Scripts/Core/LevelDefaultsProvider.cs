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
            { 3, new LevelConfig { Id = 3, Name = "第3关", RequiredCoins = 1500, ScratchCardPurchaseLimit = 7 } }
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
