using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchToolDefaultsProvider
    {
        private const string IconFolderPath = "Icons/ScratchTools";

        private static readonly Dictionary<int, ScratchToolConfig> Tools = new Dictionary<int, ScratchToolConfig>
        {
            {
                1,
                new ScratchToolConfig
                {
                    Id = 1,
                    Name = "\u786c\u5e01",
                    Description = "\u7ed3\u7b97\u6700\u9ad8\u5206\u56fe\u6848",
                    SettlementType = ScratchSettlementType.HighestScorePattern,
                    IconAtlasPath = IconFolderPath,
                    IconSpriteName = "Coin"
                }
            },
            {
                2,
                new ScratchToolConfig
                {
                    Id = 2,
                    Name = "\u8d1d\u58f3",
                    Description = "\u7ed3\u7b973\u4e2a\u76f8\u540c\u6216\u4ee5\u4e0a\u7684\u56fe\u6848",
                    SettlementType = ScratchSettlementType.MatchAnyThree,
                    IconAtlasPath = IconFolderPath,
                    IconSpriteName = "Shell"
                }
            },
            {
                3,
                new ScratchToolConfig
                {
                    Id = 3,
                    Name = "\u732b\u722a",
                    Description = "\u7ed3\u7b97\u8fde\u7eed3\u4e2a\u76f8\u540c\u6216\u4ee5\u4e0a\u7684\u56fe\u6848",
                    SettlementType = ScratchSettlementType.ConsecutiveLineMatch,
                    IconAtlasPath = IconFolderPath,
                    IconSpriteName = "CatPaw"
                }
            }
        };

        public static ScratchToolConfig GetTool(int toolId)
        {
            Tools.TryGetValue(toolId, out ScratchToolConfig config);
            return config;
        }

        public static IReadOnlyList<ScratchToolConfig> GetAll()
        {
            return new List<ScratchToolConfig>(Tools.Values);
        }

        public static IReadOnlyList<ScratchToolConfig> GetStarterTools()
        {
            return new List<ScratchToolConfig>
            {
                GetTool(1)
            };
        }
    }
}
