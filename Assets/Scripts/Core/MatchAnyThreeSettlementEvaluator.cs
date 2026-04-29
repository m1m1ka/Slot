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

            int score = ScratchPatternScoreService.SumScratchableScores(model) + bonus;
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = bonus > 0 ? "出现三个或更多相同图案。" : "没有三连图案加成。",
                WinningPatternIds = winningPatternIds
            };
        }
    }
}
