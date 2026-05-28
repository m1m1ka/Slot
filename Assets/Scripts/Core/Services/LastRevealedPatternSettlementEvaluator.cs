using System.Collections.Generic;

namespace Core
{
    public class LastRevealedPatternSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            ScratchCellModel lastCell = FindLastScratchedCell(model);
            if (lastCell == null)
            {
                return new ScratchSettlementResult
                {
                    Summary = "尚未刮开可计分图案。"
                };
            }

            double scoreMultiplier = ScratchPatternScoreService.GetScoreMultiplierOnScore(lastCell);
            int score = ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, lastCell);
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = "最后一个刮开的图案计分。",
                WinningPatternIds = new List<int> { lastCell.PatternId },
                ScoredCellIndices = new List<int> { lastCell.CellIndex },
                ScoredCellScoreMultipliers = new List<double> { scoreMultiplier }
            };
        }

        private static ScratchCellModel FindLastScratchedCell(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return null;
            }

            ScratchCellModel lastCell = null;
            int lastOrder = int.MinValue;
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

                if (cell.ScratchOrder > lastOrder)
                {
                    lastCell = cell;
                    lastOrder = cell.ScratchOrder;
                }
            }

            return lastCell;
        }
    }
}
