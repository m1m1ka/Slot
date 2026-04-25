using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class RogueCardDefaultsProvider
    {
        private static readonly List<RogueCardConfig> Cards = new List<RogueCardConfig>
        {
            new RogueCardConfig
            {
                Id = 1,
                Name = "Cherry Polish",
                Rarity = "Common",
                Description = "Cherry patterns gain more base score.",
                Effects = new List<RogueCardEffectConfig>
                {
                    new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetId = 1, Value = 5 }
                }
            },
            new RogueCardConfig
            {
                Id = 2,
                Name = "Lucky Scraper",
                Rarity = "Common",
                Description = "Scratch card rewards gain a small multiplier.",
                Effects = new List<RogueCardEffectConfig>
                {
                    new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreaseScratchCardMultiplier, Value = 0.1 }
                }
            },
            new RogueCardConfig
            {
                Id = 3,
                Name = "Bell Echo",
                Rarity = "Common",
                Description = "Bell patterns gain more base score.",
                Effects = new List<RogueCardEffectConfig>
                {
                    new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetId = 5, Value = 8 }
                }
            },
            new RogueCardConfig
            {
                Id = 4,
                Name = "Golden Edge",
                Rarity = "Uncommon",
                Description = "Future rewards can hook into this card for extra payout rules.",
                Effects = new List<RogueCardEffectConfig>
                {
                    new RogueCardEffectConfig { EffectType = RogueCardEffectType.None }
                }
            }
        };

        public static IReadOnlyList<RogueCardConfig> GetAll()
        {
            return Cards;
        }

        public static RogueCardConfig GetById(int id)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].Id == id)
                {
                    return Cards[i];
                }
            }

            return null;
        }
    }
}
