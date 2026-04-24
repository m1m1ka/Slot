using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// 任意图案达到三个或以上时获得额外奖励。
    /// 当前先做一个简单示例实现。
    /// </summary>
    public class MatchAnyThreeSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            if (model == null)
            {
                return new ScratchSettlementResult();
            }

            var counts = new Dictionary<int, int>();
            int bonus = 0;
            var winningPatternIds = new List<int>();

            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell == null || !cell.IsScratchable)
                {
                    continue;
                }

                if (!counts.ContainsKey(cell.PatternId))
                {
                    counts[cell.PatternId] = 0;
                }

                counts[cell.PatternId]++;
            }

            foreach (KeyValuePair<int, int> pair in counts)
            {
                if (pair.Value >= 3)
                {
                    winningPatternIds.Add(pair.Key);
                    bonus += 100 * pair.Value;
                }
            }

            return new ScratchSettlementResult
            {
                FinalScore = model.TotalBaseScore + bonus,
                Summary = bonus > 0 ? "Matched three or more identical symbols." : "No triple match bonus.",
                WinningPatternIds = winningPatternIds
            };
        }
    }
}
