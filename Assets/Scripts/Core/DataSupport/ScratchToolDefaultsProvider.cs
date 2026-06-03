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
                    Name = "硬币",
                    Description = "结算最高分图案",
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
                    Name = "银钥匙",
                    Description = "结算连续横向的3个相同图案，每个图案分数×2",
                    SettlementType = ScratchSettlementType.HorizontalTripleMatch,
                    IconAtlasPath = IconFolderPath,
                    IconSpriteName = "SilverKey"
                }
            },
            {
                3,
                new ScratchToolConfig
                {
                    Id = 3,
                    Name = "金钥匙",
                    Description = "结算连续横向的5个相同图案，每个图案分数×3",
                    SettlementType = ScratchSettlementType.HorizontalFiveMatch,
                    IconAtlasPath = IconFolderPath,
                    IconSpriteName = "GoldKey"
                }
            },
            {
                4,
                new ScratchToolConfig
                {
                    Id = 4,
                    Name = "猫爪",
                    Description = "结算3个相同或以上的图案",
                    SettlementType = ScratchSettlementType.MatchAnyThree,
                    IconAtlasPath = IconFolderPath,
                    IconSpriteName = "CatPaw"
                }
            },
            {
                5,
                new ScratchToolConfig
                {
                    Id = 5,
                    Name = "尺子",
                    Description = "结算连续竖向的3个相同图案，每个图案分数×2",
                    SettlementType = ScratchSettlementType.VerticalTripleMatch,
                    IconAtlasPath = IconFolderPath,
                    IconSpriteName = "Ruler"
                }
            },
            {
                6,
                new ScratchToolConfig
                {
                    Id = 6,
                    Name = "磁铁",
                    Description = "结算2个相同或以上的金属图案",
                    SettlementType = ScratchSettlementType.MetalMatchAnyPair,
                    IconAtlasPath = IconFolderPath,
                    IconSpriteName = "Magnet"
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
