using System.Collections.Generic;

namespace Core
{
    public class HighestScorePatternSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            ScratchCellModel highestCell = FindHighestScoreCell(model);
            if (highestCell == null)
            {
                return new ScratchSettlementResult
                {
                    Summary = "尚未刮开可计分图案。"
                };
            }

            double scoreMultiplier = ScratchPatternScoreService.GetScoreMultiplierOnScore(highestCell);
            int score = ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, highestCell);
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = "最高分图案计分。",
                WinningPatternIds = new List<int> { highestCell.PatternId },
                ScoredCellIndices = new List<int> { highestCell.CellIndex },
                ScoredCellScoreMultipliers = new List<double> { scoreMultiplier }
            };
        }

        private static ScratchCellModel FindHighestScoreCell(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return null;
            }

            ScratchCellModel highestCell = null;
            int highestScore = int.MinValue;
            int highestScratchOrder = int.MaxValue;
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell == null ||
                    !cell.IsScratchable ||
                    !cell.IsScratched ||
                    ScratchPatternScoreService.ScoresDirectly(model, cell) ||
                    ScratchPatternScoreService.ExcludeFromScratchToolScoring(model, cell))
                {
                    continue;
                }

                int score = ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, cell);
                int scratchOrder = cell.ScratchOrder >= 0 ? cell.ScratchOrder : int.MaxValue;
                if (score > highestScore || score == highestScore && scratchOrder < highestScratchOrder)
                {
                    highestCell = cell;
                    highestScore = score;
                    highestScratchOrder = scratchOrder;
                }
            }

            return highestCell;
        }
    }
}
