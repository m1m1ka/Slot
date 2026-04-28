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
                Name = "樱桃抛光",
                Rarity = "普通",
                Description = "樱桃图案基础分增加。",
                Effects = new List<RogueCardEffectConfig>
                {
                    new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetId = 1, Value = 5 }
                }
            },
            new RogueCardConfig
            {
                Id = 2,
                Name = "幸运刮刀",
                Rarity = "普通",
                Description = "刮刮卡奖励获得少量倍率。",
                Effects = new List<RogueCardEffectConfig>
                {
                    new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreaseScratchCardMultiplier, Value = 0.1 }
                }
            },
            new RogueCardConfig
            {
                Id = 3,
                Name = "铃铛回响",
                Rarity = "普通",
                Description = "铃铛图案基础分增加。",
                Effects = new List<RogueCardEffectConfig>
                {
                    new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetId = 5, Value = 8 }
                }
            },
            new RogueCardConfig
            {
                Id = 4,
                Name = "黄金边缘",
                Rarity = "罕见",
                Description = "后续奖励规则可接入这张卡。",
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
