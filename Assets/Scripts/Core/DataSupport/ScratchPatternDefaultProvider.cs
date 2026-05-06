using System.Collections.Generic;
using Configs;

namespace Core
{
    /// <summary>
    /// 临时默认图案数据提供者。
    /// 后续接入正式配表后，可由 ConfigManager 替换此来源。
    /// </summary>
    public static class ScratchPatternDefaultProvider
    {
        private static readonly List<ScratchPatternConfig> Patterns = new List<ScratchPatternConfig>
        {
            new ScratchPatternConfig { Id = 1, Name = "樱桃", BaseScore = 10, AtlasPath = "Icons/Patterns", SpriteName = "Cherry" },
            new ScratchPatternConfig { Id = 2, Name = "柠檬", BaseScore = 15, AtlasPath = "Icons/Patterns", SpriteName = "Lemon" },
            new ScratchPatternConfig { Id = 3, Name = "橙子", BaseScore = 20, AtlasPath = "Icons/Patterns", SpriteName = "Orange" },
            new ScratchPatternConfig { Id = 4, Name = "葡萄", BaseScore = 28, AtlasPath = "Icons/Patterns", SpriteName = "Grape" },
            new ScratchPatternConfig { Id = 5, Name = "铃铛", BaseScore = 40, AtlasPath = "Icons/Patterns", SpriteName = "Bell" },
            new ScratchPatternConfig { Id = 6, Name = "金条", BaseScore = 55, AtlasPath = "Icons/Patterns", SpriteName = "Bar" },
            new ScratchPatternConfig { Id = 7, Name = "星星", BaseScore = 75, AtlasPath = "Icons/Patterns", SpriteName = "Star" },
            new ScratchPatternConfig { Id = 8, Name = "钻石", BaseScore = 100, AtlasPath = "Icons/Patterns2", SpriteName = "Diamond" },
            new ScratchPatternConfig { Id = 9, Name = "皇冠", BaseScore = 150, AtlasPath = "Icons/Patterns2", SpriteName = "Crown" },
            new ScratchPatternConfig { Id = 10, Name = "幸运7", BaseScore = 250, AtlasPath = "Icons/Patterns", SpriteName = "Seven" },
            new ScratchPatternConfig { Id = 11, Name = "倍率图案", BaseScore = 0, AtlasPath = "Icons/Patterns2", SpriteName = "Multiplier", EffectType = ScratchPatternEffectType.AddRewardMultiplierOnRevealed, EffectValue = 0.5d },
            new ScratchPatternConfig { Id = 12, Name = "好脸", BaseScore = 0, AtlasPath = "Icons/Patterns2", SpriteName = "GoodFace", EffectType = ScratchPatternEffectType.ScoreHighestPatternBaseScoreMultiplier, EffectValue = 2d },
            new ScratchPatternConfig { Id = 13, Name = "坏脸", BaseScore = 0, AtlasPath = "Icons/Patterns2", SpriteName = "BadFace", EffectType = ScratchPatternEffectType.ForceFinalRewardZero }
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
