using System.Collections.Generic;

namespace Core
{
    public class MatchAnyThreeSettlementEvaluator : IScratchSettlementEvaluator
    {
        private const double MatchScoreMultiplier = 1d;

        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            if (model == null || model.Cells == null)
            {
                return new ScratchSettlementResult();
            }

            Dictionary<int, List<ScratchCellModel>> cellsByPattern = BuildCellsByPattern(model);
            var winningPatternIds = new List<int>();
            var scoredCellIndices = new List<int>();
            var scoredCellScoreMultipliers = new List<double>();
            int score = 0;

            foreach (KeyValuePair<int, List<ScratchCellModel>> pair in cellsByPattern)
            {
                List<ScratchCellModel> patternCells = pair.Value;
                if (patternCells == null || patternCells.Count < 3)
                {
                    continue;
                }

                winningPatternIds.Add(pair.Key);
                patternCells.Sort((left, right) => left.ScratchOrder.CompareTo(right.ScratchOrder));
                for (int i = 0; i < patternCells.Count; i++)
                {
                    ScratchCellModel cell = patternCells[i];
                    double scoreMultiplier = MatchScoreMultiplier * ScratchPatternScoreService.GetScoreMultiplierOnScore(cell);
                    score += ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, cell, MatchScoreMultiplier);
                    scoredCellIndices.Add(cell.CellIndex);
                    scoredCellScoreMultipliers.Add(scoreMultiplier);
                }
            }

            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = score > 0 ? "出现三个或更多相同图案，命中图案计分。" : "没有三个或更多相同图案，不获得分数。",
                WinningPatternIds = winningPatternIds,
                ScoredCellIndices = scoredCellIndices,
                ScoredCellScoreMultipliers = scoredCellScoreMultipliers
            };
        }

        private static Dictionary<int, List<ScratchCellModel>> BuildCellsByPattern(ScratchCardModel model)
        {
            var cellsByPattern = new Dictionary<int, List<ScratchCellModel>>();
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

                if (!cellsByPattern.TryGetValue(cell.PatternId, out List<ScratchCellModel> patternCells))
                {
                    patternCells = new List<ScratchCellModel>();
                    cellsByPattern[cell.PatternId] = patternCells;
                }

                patternCells.Add(cell);
            }

            return cellsByPattern;
        }
    }
}
