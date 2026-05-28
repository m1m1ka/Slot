using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchPatternDefaultProvider
    {
        private static readonly List<ScratchPatternConfig> Patterns = new List<ScratchPatternConfig>
        {
            new ScratchPatternConfig { Id = 1, Name = "Cherry", BaseScore = 10, AtlasPath = "Icons/Patterns", SpriteName = "Cherry" },
            new ScratchPatternConfig { Id = 2, Name = "Lemon", BaseScore = 15, AtlasPath = "Icons/Patterns", SpriteName = "Lemon" },
            new ScratchPatternConfig { Id = 3, Name = "Orange", BaseScore = 20, AtlasPath = "Icons/Patterns", SpriteName = "Orange" },
            new ScratchPatternConfig { Id = 4, Name = "Grape", BaseScore = 28, AtlasPath = "Icons/Patterns", SpriteName = "Grape" },
            new ScratchPatternConfig { Id = 5, Name = "Bell", BaseScore = 40, AtlasPath = "Icons/Patterns", SpriteName = "Bell" },
            new ScratchPatternConfig { Id = 6, Name = "Bar", BaseScore = 55, AtlasPath = "Icons/Patterns", SpriteName = "Bar" },
            new ScratchPatternConfig { Id = 7, Name = "Star", BaseScore = 75, AtlasPath = "Icons/Patterns", SpriteName = "Star" },
            new ScratchPatternConfig { Id = 8, Name = "Diamond", BaseScore = 100, AtlasPath = "Icons/Patterns", SpriteName = "Diamond" },
            new ScratchPatternConfig { Id = 9, Name = "Crown", BaseScore = 150, AtlasPath = "Icons/Patterns", SpriteName = "Crown" },
            new ScratchPatternConfig { Id = 10, Name = "Seven", BaseScore = 250, AtlasPath = "Icons/Patterns", SpriteName = "Seven" },
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
