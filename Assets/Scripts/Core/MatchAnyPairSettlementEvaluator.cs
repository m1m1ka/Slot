using System.Collections.Generic;

namespace Core
{
    public class MatchAnyPairSettlementEvaluator : IScratchSettlementEvaluator
    {
        private const int PairScoreMultiplier = 2;

        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            if (model == null || model.Cells == null)
            {
                return new ScratchSettlementResult();
            }

            var cellsByPattern = new Dictionary<int, List<ScratchCellModel>>();
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell == null || !cell.IsScratchable || !cell.IsScratched)
                {
                    continue;
                }

                if (!cellsByPattern.TryGetValue(cell.PatternId, out List<ScratchCellModel> cells))
                {
                    cells = new List<ScratchCellModel>();
                    cellsByPattern[cell.PatternId] = cells;
                }

                cells.Add(cell);
            }

            int pairScore = 0;
            var winningPatternIds = new List<int>();
            foreach (KeyValuePair<int, List<ScratchCellModel>> pair in cellsByPattern)
            {
                List<ScratchCellModel> cells = pair.Value;
                int pairCount = cells.Count / 2;
                if (pairCount <= 0)
                {
                    continue;
                }

                winningPatternIds.Add(pair.Key);
                for (int i = 0; i < pairCount * 2; i++)
                {
                    pairScore += cells[i].BaseScore * PairScoreMultiplier;
                }
            }

            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = pairScore,
                FinalScore = ScratchSettlementResult.ApplyMultiplier(pairScore, model.RewardMultiplier),
                Summary = pairScore > 0 ? "配对成功，分数获得×2。" : "没有配对，不获得分数。",
                WinningPatternIds = winningPatternIds
            };
        }
    }
}
