using System.Collections.Generic;

namespace Core
{
    public class HorizontalTripleMatchSettlementEvaluator : IScratchSettlementEvaluator
    {
        private const double TripleScoreMultiplier = 2d;

        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            if (model == null || model.Cells == null || model.GridWidth <= 0 || model.GridHeight <= 0)
            {
                return new ScratchSettlementResult();
            }

            Dictionary<int, ScratchCellModel> cellsByPosition = BuildCellsByPosition(model);
            var winningPatternIds = new List<int>();
            var scoredCellIndices = new List<int>();
            var scoredCellScoreMultipliers = new List<double>();
            int score = 0;
            int matchedTripleCount = 0;

            for (int row = 0; row < model.GridHeight; row++)
            {
                int column = 0;
                while (column <= model.GridWidth - 3)
                {
                    ScratchCellModel first = GetCell(cellsByPosition, row, column, model.GridWidth);
                    ScratchCellModel second = GetCell(cellsByPosition, row, column + 1, model.GridWidth);
                    ScratchCellModel third = GetCell(cellsByPosition, row, column + 2, model.GridWidth);

                    if (IsMatchingTriple(first, second, third))
                    {
                        AddMatchedCell(model, first, winningPatternIds, scoredCellIndices, scoredCellScoreMultipliers, ref score);
                        AddMatchedCell(model, second, winningPatternIds, scoredCellIndices, scoredCellScoreMultipliers, ref score);
                        AddMatchedCell(model, third, winningPatternIds, scoredCellIndices, scoredCellScoreMultipliers, ref score);
                        matchedTripleCount++;
                        column += 3;
                        continue;
                    }

                    column++;
                }
            }

            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = matchedTripleCount > 0 ? "横向连续三个相同图案计分，每个图案分数x2。" : "没有横向三连相同图案。",
                WinningPatternIds = winningPatternIds,
                ScoredCellIndices = scoredCellIndices,
                ScoredCellScoreMultipliers = scoredCellScoreMultipliers
            };
        }

        private static Dictionary<int, ScratchCellModel> BuildCellsByPosition(ScratchCardModel model)
        {
            var cellsByPosition = new Dictionary<int, ScratchCellModel>();
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (!IsEligibleCell(model, cell))
                {
                    continue;
                }

                int key = GetPositionKey(cell.Row, cell.Column, model.GridWidth);
                if (!cellsByPosition.ContainsKey(key))
                {
                    cellsByPosition.Add(key, cell);
                }
            }

            return cellsByPosition;
        }

        private static bool IsMatchingTriple(ScratchCellModel first, ScratchCellModel second, ScratchCellModel third)
        {
            return first != null &&
                second != null &&
                third != null &&
                first.PatternId == second.PatternId &&
                first.PatternId == third.PatternId;
        }

        private static bool IsEligibleCell(ScratchCardModel model, ScratchCellModel cell)
        {
            return cell != null &&
                cell.IsScratchable &&
                cell.IsScratched &&
                cell.Row >= 0 &&
                cell.Column >= 0 &&
                !ScratchPatternScoreService.ScoresDirectly(model, cell) &&
                !ScratchPatternScoreService.ExcludeFromScratchToolScoring(model, cell);
        }

        private static ScratchCellModel GetCell(Dictionary<int, ScratchCellModel> cellsByPosition, int row, int column, int gridWidth)
        {
            cellsByPosition.TryGetValue(GetPositionKey(row, column, gridWidth), out ScratchCellModel cell);
            return cell;
        }

        private static int GetPositionKey(int row, int column, int gridWidth)
        {
            return row * gridWidth + column;
        }

        private static void AddMatchedCell(
            ScratchCardModel model,
            ScratchCellModel cell,
            List<int> winningPatternIds,
            List<int> scoredCellIndices,
            List<double> scoredCellScoreMultipliers,
            ref int score)
        {
            score += ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, cell, TripleScoreMultiplier);
            winningPatternIds.Add(cell.PatternId);
            scoredCellIndices.Add(cell.CellIndex);
            scoredCellScoreMultipliers.Add(TripleScoreMultiplier * ScratchPatternScoreService.GetScoreMultiplierOnScore(cell));
        }
    }
}
