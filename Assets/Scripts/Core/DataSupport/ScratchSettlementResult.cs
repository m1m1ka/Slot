using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// 刮刮卡结算结果。
    /// </summary>
    public class ScratchSettlementResult
    {
        public int ScoreBeforeRewardMultiplier { get; set; }
        public int FinalScore { get; set; }
        public string Summary { get; set; }
        public int SourceScratchToolId { get; set; } = -1;
        public string SourceScratchToolName { get; set; }
        public List<int> WinningPatternIds { get; set; } = new List<int>();
        public List<int> ScoredCellIndices { get; set; } = new List<int>();
        public List<double> ScoredCellScoreMultipliers { get; set; } = new List<double>();
        public List<string> ScoredCellFloatTexts { get; set; } = new List<string>();

        public static int ApplyMultiplier(int score, double multiplier)
        {
            if (multiplier < 0d)
            {
                multiplier = 1d;
            }

            return (int)System.Math.Round(score * multiplier);
        }
    }
}
