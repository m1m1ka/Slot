using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchPatternDefaultProvider
    {
        private static readonly List<ScratchPatternConfig> Patterns = new List<ScratchPatternConfig>
        {
            new ScratchPatternConfig { Id = 1, Name = "樱桃", SpritePath = "Icons/PatternIcons/Cherry", Type = "Fruit" },
            new ScratchPatternConfig { Id = 2, Name = "柠檬", SpritePath = "Icons/PatternIcons/Lemon", Type = "Fruit" },
            new ScratchPatternConfig { Id = 3, Name = "橙子", SpritePath = "Icons/PatternIcons/Orange", Type = "Fruit" },
            new ScratchPatternConfig { Id = 4, Name = "葡萄", SpritePath = "Icons/PatternIcons/Grape", Type = "Fruit" },
            new ScratchPatternConfig { Id = 5, Name = "金苹果", SpritePath = "Icons/PatternIcons/GoldApple", Type = "Fruit" },
            new ScratchPatternConfig { Id = 6, Name = "西瓜", SpritePath = "Icons/PatternIcons/Watermelon", Type = "Fruit" },
            new ScratchPatternConfig { Id = 7, Name = "菠萝", SpritePath = "Icons/PatternIcons/Pineapple", Type = "Fruit" },
            new ScratchPatternConfig { Id = 8, Name = "蓝莓", SpritePath = "Icons/PatternIcons/Blueberry", Type = "Fruit" },
            new ScratchPatternConfig { Id = 9, Name = "煤炭", SpritePath = "Icons/PatternIcons/Coal", Type = "Mineral" },
            new ScratchPatternConfig { Id = 10, Name = "铜块", SpritePath = "Icons/PatternIcons/Copper", Type = "Mineral" },
            new ScratchPatternConfig { Id = 11, Name = "银块", SpritePath = "Icons/PatternIcons/Sliver", Type = "Mineral" },
            new ScratchPatternConfig { Id = 12, Name = "金块", SpritePath = "Icons/PatternIcons/Gold", Type = "Mineral" },
            new ScratchPatternConfig { Id = 13, Name = "蓝宝石", SpritePath = "Icons/PatternIcons/Sapphire", Type = "Mineral" },
            new ScratchPatternConfig { Id = 14, Name = "红宝石", SpritePath = "Icons/PatternIcons/Ruby", Type = "Mineral" },
            new ScratchPatternConfig { Id = 15, Name = "绿宝石", SpritePath = "Icons/PatternIcons/Emerald", Type = "Mineral" },
            new ScratchPatternConfig { Id = 16, Name = "钻石", SpritePath = "Icons/PatternIcons/Diamond", Type = "Mineral" },
            new ScratchPatternConfig { Id = 17, Name = "水星", SpritePath = "Icons/PatternIcons/Mercury", Type = "Planet" },
            new ScratchPatternConfig { Id = 18, Name = "金星", SpritePath = "Icons/PatternIcons/Venus", Type = "Planet" },
            new ScratchPatternConfig { Id = 19, Name = "地球", SpritePath = "Icons/PatternIcons/Earth", Type = "Planet" },
            new ScratchPatternConfig { Id = 20, Name = "火星", SpritePath = "Icons/PatternIcons/Mars", Type = "Planet" },
            new ScratchPatternConfig { Id = 21, Name = "木星", SpritePath = "Icons/PatternIcons/Jupiter", Type = "Planet" },
            new ScratchPatternConfig { Id = 22, Name = "土星", SpritePath = "Icons/PatternIcons/Saturn", Type = "Planet" },
            new ScratchPatternConfig { Id = 23, Name = "天王星", SpritePath = "Icons/PatternIcons/Uranus", Type = "Planet" },
            new ScratchPatternConfig { Id = 24, Name = "海王星", SpritePath = "Icons/PatternIcons/Neptune", Type = "Planet" },
            new ScratchPatternConfig { Id = 25, Name = "幸运7", SpritePath = "Icons/PatternIcons/Seven", Type = "Number" },
            new ScratchPatternConfig { Id = 26, Name = "小丑", SpritePath = "Icons/PatternIcons/Joker", Type = "Joker" },
            new ScratchPatternConfig { Id = 27, Name = "好脸", SpritePath = "Icons/PatternIcons/GoodFace", Type = "Joker", EffectType = ScratchPatternEffectType.FixedScore },
            new ScratchPatternConfig { Id = 28, Name = "坏脸", SpritePath = "Icons/PatternIcons/BadFace", Type = "Joker", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 0d },
            new ScratchPatternConfig { Id = 29, Name = "倍率", SpritePath = "Icons/PatternIcons/Multiplier", Type = "Multiplier" },
            new ScratchPatternConfig { Id = 30, Name = "×0倍率", SpritePath = "Icons/PatternIcons/×0", Type = "Multiplier", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 0d },
            new ScratchPatternConfig { Id = 31, Name = "×1倍率", SpritePath = "Icons/PatternIcons/×1", Type = "Multiplier", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 1d },
            new ScratchPatternConfig { Id = 32, Name = "×2倍率", SpritePath = "Icons/PatternIcons/×2", Type = "Multiplier", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 2d },
            new ScratchPatternConfig { Id = 33, Name = "×3倍率", SpritePath = "Icons/PatternIcons/×3", Type = "Multiplier", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 3d },
            new ScratchPatternConfig { Id = 34, Name = "手铐", SpritePath = "Icons/PatternIcons/HandCuffs", Type = "Handcuffs" },
            new ScratchPatternConfig { Id = 35, Name = "坏苹果", SpritePath = "Icons/PatternIcons/BadApple", Type = "BadApple" },
            new ScratchPatternConfig { Id = 36, Name = "骷髅", SpritePath = "Icons/PatternIcons/Skull", Type = "Skull" },
            new ScratchPatternConfig { Id = 37, Name = "金币堆", SpritePath = "Icons/PatternIcons/CoinPile", Type = "CoinPile" }
        };

        public static IReadOnlyList<ScratchPatternConfig> GetAll()
        {
            return Patterns;
        }

        public static ScratchPatternConfig GetById(int patternId)
        {
            for (int i = 0; i < Patterns.Count; i++)
            {
                if (Patterns[i] != null && Patterns[i].Id == patternId)
                {
                    return Patterns[i];
                }
            }

            return null;
        }
    }
}
