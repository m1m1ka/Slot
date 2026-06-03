using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchPatternDefaultProvider
    {
        private static readonly List<ScratchPatternConfig> Patterns = new List<ScratchPatternConfig>
        {
            new ScratchPatternConfig { Id = 1, Name = "樱桃", BaseScore = 10, SpritePath = "Icons/PatternIcons/Cherry" },
            new ScratchPatternConfig { Id = 2, Name = "柠檬", BaseScore = 30, SpritePath = "Icons/PatternIcons/Lemon" },
            new ScratchPatternConfig { Id = 3, Name = "橙子", BaseScore = 60, SpritePath = "Icons/PatternIcons/Orange" },
            new ScratchPatternConfig { Id = 4, Name = "葡萄", BaseScore = 180, SpritePath = "Icons/PatternIcons/Grape" },
            new ScratchPatternConfig { Id = 5, Name = "银条", BaseScore = 100, SpritePath = "Icons/PatternIcons/Silver" },
            new ScratchPatternConfig { Id = 6, Name = "金条", BaseScore = 200, SpritePath = "Icons/PatternIcons/Gold" },
            new ScratchPatternConfig { Id = 7, Name = "星星", BaseScore = 250, SpritePath = "Icons/PatternIcons/Star" },
            new ScratchPatternConfig { Id = 8, Name = "绿宝石", BaseScore = 500, SpritePath = "Icons/PatternIcons/Emerald" },
            new ScratchPatternConfig { Id = 9, Name = "钻石", BaseScore = 1000, SpritePath = "Icons/PatternIcons/Diamond" },
            new ScratchPatternConfig { Id = 10, Name = "幸运7", BaseScore = 3000, SpritePath = "Icons/PatternIcons/Seven" },
            new ScratchPatternConfig { Id = 11, Name = "Multiplier", BaseScore = 0, AtlasPath = "Icons/Patterns", SpriteName = "Multiplier", EffectType = ScratchPatternEffectType.AddRewardMultiplierOnRevealed, EffectValue = 0.5d },
            new ScratchPatternConfig { Id = 12, Name = "Good Joker", BaseScore = 3000, AtlasPath = "Icons/Clown_Good", SpriteName = "Clown_Good", EffectType = ScratchPatternEffectType.FixedScore },
            new ScratchPatternConfig { Id = 13, Name = "Bad Joker", BaseScore = 0, AtlasPath = "Icons/Clown_Bad", SpriteName = "Clown_Bad", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 0d },
            new ScratchPatternConfig { Id = 14, Name = "Joker", BaseScore = 0, AtlasPath = "Icons/Clown_Good", SpriteName = "Clown_Good" },
            new ScratchPatternConfig { Id = 15, Name = "Risk Multiplier", BaseScore = 0, AtlasPath = "Icons/×？", SpriteName = "×？" },
            new ScratchPatternConfig { Id = 16, Name = "Multiplier x0", BaseScore = 0, AtlasPath = "Icons/×0", SpriteName = "×0", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 0d },
            new ScratchPatternConfig { Id = 17, Name = "Multiplier x1", BaseScore = 0, AtlasPath = "Icons/×1", SpriteName = "×1", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 1d },
            new ScratchPatternConfig { Id = 18, Name = "Multiplier x2", BaseScore = 0, AtlasPath = "Icons/×2", SpriteName = "×2", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 2d },
            new ScratchPatternConfig { Id = 19, Name = "Multiplier x3", BaseScore = 0, AtlasPath = "Icons/×3", SpriteName = "×3", EffectType = ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement, EffectValue = 3d }
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
