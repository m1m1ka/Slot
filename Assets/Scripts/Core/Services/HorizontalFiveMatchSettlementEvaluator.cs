using System.Collections.Generic;

namespace Core
{
    public class HorizontalFiveMatchSettlementEvaluator : IScratchSettlementEvaluator
    {
        private const int MatchLength = 5;
        private const double MatchScoreMultiplier = 3d;

        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            if (model == null || model.Cells == null || model.GridWidth < MatchLength || model.GridHeight <= 0)
            {
                return new ScratchSettlementResult();
            }

            Dictionary<int, ScratchCellModel> cellsByPosition = BuildCellsByPosition(model);
            var winningPatternIds = new List<int>();
            var scoredCellIndices = new List<int>();
            var scoredCellScoreMultipliers = new List<double>();
            int score = 0;
            int matchedLineCount = 0;

            for (int row = 0; row < model.GridHeight; row++)
            {
                int column = 0;
                while (column <= model.GridWidth - MatchLength)
                {
                    if (TryGetHorizontalMatch(cellsByPosition, row, column, model.GridWidth, out List<ScratchCellModel> matchedCells))
                    {
                        for (int i = 0; i < matchedCells.Count; i++)
                        {
                            AddMatchedCell(model, matchedCells[i], winningPatternIds, scoredCellIndices, scoredCellScoreMultipliers, ref score);
                        }

                        matchedLineCount++;
                        column += MatchLength;
                        continue;
                    }

                    column++;
                }
            }

            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = matchedLineCount > 0 ? "横向连续五个相同图案计分，每个图案分数x3。" : "没有横向五连相同图案。",
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

        private static bool TryGetHorizontalMatch(
            Dictionary<int, ScratchCellModel> cellsByPosition,
            int row,
            int startColumn,
            int gridWidth,
            out List<ScratchCellModel> matchedCells)
        {
            matchedCells = new List<ScratchCellModel>(MatchLength);
            ScratchCellModel first = GetCell(cellsByPosition, row, startColumn, gridWidth);
            if (first == null)
            {
                return false;
            }

            matchedCells.Add(first);
            for (int offset = 1; offset < MatchLength; offset++)
            {
                ScratchCellModel next = GetCell(cellsByPosition, row, startColumn + offset, gridWidth);
                if (next == null || next.PatternId != first.PatternId)
                {
                    matchedCells.Clear();
                    return false;
                }

                matchedCells.Add(next);
            }

            return true;
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
            score += ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, cell, MatchScoreMultiplier);
            winningPatternIds.Add(cell.PatternId);
            scoredCellIndices.Add(cell.CellIndex);
            scoredCellScoreMultipliers.Add(MatchScoreMultiplier * ScratchPatternScoreService.GetScoreMultiplierOnScore(cell));
        }
    }
}
