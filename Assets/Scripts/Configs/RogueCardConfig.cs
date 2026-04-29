using System.Collections.Generic;

namespace Configs
{
    public class RogueCardConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RogueCardRarity Rarity { get; set; }
        public List<RogueCardLevelConfig> Levels { get; set; } = new List<RogueCardLevelConfig>();

        public RogueCardLevelConfig GetLevelConfig(int level)
        {
            if (Levels == null || Levels.Count == 0)
            {
                return null;
            }

            RogueCardLevelConfig fallback = null;
            for (int i = 0; i < Levels.Count; i++)
            {
                RogueCardLevelConfig levelConfig = Levels[i];
                if (levelConfig == null)
                {
                    continue;
                }

                if (levelConfig.Level == level)
                {
                    return levelConfig;
                }

                if (levelConfig.Level < level && (fallback == null || levelConfig.Level > fallback.Level))
                {
                    fallback = levelConfig;
                }
            }

            return fallback;
        }

        public int GetMaxLevel()
        {
            int maxLevel = 1;
            if (Levels == null)
            {
                return maxLevel;
            }

            for (int i = 0; i < Levels.Count; i++)
            {
                RogueCardLevelConfig levelConfig = Levels[i];
                if (levelConfig != null && levelConfig.Level > maxLevel)
                {
                    maxLevel = levelConfig.Level;
                }
            }

            return maxLevel;
        }

        public string GetDescriptionForLevel(int level)
        {
            RogueCardLevelConfig levelConfig = GetLevelConfig(level);
            if (levelConfig != null && !string.IsNullOrWhiteSpace(levelConfig.Description))
            {
                return levelConfig.Description;
            }

            return Description;
        }

        public string GetRarityDisplayName()
        {
            switch (Rarity)
            {
                case RogueCardRarity.Common:
                    return "普通";
                case RogueCardRarity.Rare:
                    return "罕见";
                case RogueCardRarity.Epic:
                    return "史诗";
                case RogueCardRarity.Legendary:
                    return "传说";
                default:
                    return Rarity.ToString();
            }
        }
    }
}
