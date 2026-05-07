using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchToolDefaultsProvider
    {
        private const string IconAtlasPath = "Icons/ScratchTools";

        private static readonly Dictionary<int, ScratchToolConfig> Tools = new Dictionary<int, ScratchToolConfig>
        {
            {
                1,
                new ScratchToolConfig
                {
                    Id = 1,
                    Name = "默认刮具",
                    Description = "第一个刮开的图案计分。",
                    SettlementType = ScratchSettlementType.FirstRevealedPattern,
                    IconAtlasPath = IconAtlasPath,
                    IconSpriteName = "ScratchTools_0"
                }
            },
            {
                2,
                new ScratchToolConfig
                {
                    Id = 2,
                    Name = "配对刮具",
                    Description = "每凑出一对相同图案就计分；已配对图案不再参与后续配对，成对图案分数获得x2。",
                    SettlementType = ScratchSettlementType.MatchAnyPair,
                    IconAtlasPath = IconAtlasPath,
                    IconSpriteName = "ScratchTools_1"
                }
            }
        };

        public static ScratchToolConfig GetTool(int toolId)
        {
            Tools.TryGetValue(toolId, out ScratchToolConfig config);
            return config;
        }

        public static IReadOnlyList<ScratchToolConfig> GetStarterTools()
        {
            return new List<ScratchToolConfig>
            {
                GetTool(1),
                GetTool(2)
            };
        }
    }
}
