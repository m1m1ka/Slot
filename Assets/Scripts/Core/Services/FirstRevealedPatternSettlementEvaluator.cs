using System.Collections.Generic;

namespace Core
{
    public class FirstRevealedPatternSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            ScratchCellModel firstCell = FindFirstScratchedCell(model);
            if (firstCell == null)
            {
                return new ScratchSettlementResult
                {
                    Summary = "尚未刮开可计分图案。"
                };
            }

            double scoreMultiplier = ScratchPatternScoreService.GetScoreMultiplierOnScore(firstCell);
            int score = ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, firstCell);
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = "第一个刮开的图案计分。",
                WinningPatternIds = new List<int> { firstCell.PatternId },
                ScoredCellIndices = new List<int> { firstCell.CellIndex },
                ScoredCellScoreMultipliers = new List<double> { scoreMultiplier }
            };
        }

        private static ScratchCellModel FindFirstScratchedCell(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return null;
            }

            ScratchCellModel firstCell = null;
            int firstOrder = int.MaxValue;
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell == null ||
                    !cell.IsScratchable ||
                    !cell.IsScratched ||
                    cell.ScratchOrder < 0 ||
                    ScratchPatternScoreService.ScoresDirectly(model, cell) ||
                    ScratchPatternScoreService.ExcludeFromScratchToolScoring(model, cell))
                {
                    continue;
                }

                if (cell.ScratchOrder < firstOrder)
                {
                    firstCell = cell;
                    firstOrder = cell.ScratchOrder;
                }
            }

            return firstCell;
        }
    }
}
