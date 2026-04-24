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
            new ScratchPatternConfig { Id = 1, Name = "Cherry", BaseScore = 10, AtlasPath = "Icons/Patterns", SpriteName = "Cherry" },
            new ScratchPatternConfig { Id = 2, Name = "Lemon", BaseScore = 15, AtlasPath = "Icons/Patterns", SpriteName = "Lemon" },
            new ScratchPatternConfig { Id = 3, Name = "Orange", BaseScore = 20, AtlasPath = "Icons/Patterns", SpriteName = "Orange" },
            new ScratchPatternConfig { Id = 4, Name = "Grape", BaseScore = 28, AtlasPath = "Icons/Patterns", SpriteName = "Grape" },
            new ScratchPatternConfig { Id = 5, Name = "Bell", BaseScore = 40, AtlasPath = "Icons/Patterns", SpriteName = "Bell" },
            new ScratchPatternConfig { Id = 6, Name = "Bar", BaseScore = 55, AtlasPath = "Icons/Patterns", SpriteName = "Bar" },
            new ScratchPatternConfig { Id = 7, Name = "Star", BaseScore = 75, AtlasPath = "Icons/Patterns", SpriteName = "Star" },
            new ScratchPatternConfig { Id = 8, Name = "Diamond", BaseScore = 100, AtlasPath = "Icons/Patterns2", SpriteName = "Diamond" },
            new ScratchPatternConfig { Id = 9, Name = "Crown", BaseScore = 150, AtlasPath = "Icons/Patterns2", SpriteName = "Crown" },
            new ScratchPatternConfig { Id = 10, Name = "Seven", BaseScore = 250, AtlasPath = "Icons/Patterns", SpriteName = "Seven" }
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
