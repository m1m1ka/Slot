using System.Collections.Generic;

namespace Core
{
    public class ConsecutiveLineMatchSettlementEvaluator : IScratchSettlementEvaluator
    {
        private const int MinMatchLength = 3;
        private const double MatchScoreMultiplier = 2d;

        private static readonly LineDirection[] Directions =
        {
            new LineDirection(0, 1),
            new LineDirection(1, 0),
            new LineDirection(1, 1),
            new LineDirection(1, -1)
        };

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
            var scoredCellIndexSet = new HashSet<int>();
            int score = 0;
            int matchedLineCount = 0;

            for (int directionIndex = 0; directionIndex < Directions.Length; directionIndex++)
            {
                LineDirection direction = Directions[directionIndex];
                for (int row = 0; row < model.GridHeight; row++)
                {
                    for (int column = 0; column < model.GridWidth; column++)
                    {
                        if (!IsLineStart(cellsByPosition, row, column, direction, model))
                        {
                            continue;
                        }

                        List<ScratchCellModel> matchedCells = GetMatchingLine(cellsByPosition, row, column, direction, model);
                        if (matchedCells.Count < MinMatchLength)
                        {
                            continue;
                        }

                        matchedLineCount++;
                        for (int i = 0; i < matchedCells.Count; i++)
                        {
                            AddMatchedCell(
                                model,
                                matchedCells[i],
                                winningPatternIds,
                                scoredCellIndices,
                                scoredCellScoreMultipliers,
                                scoredCellIndexSet,
                                ref score);
                        }
                    }
                }
            }

            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = matchedLineCount > 0
                    ? "\u7ed3\u7b97\u8fde\u7eed\u4e09\u4e2a\u6216\u4ee5\u4e0a\u76f8\u540c\u56fe\u6848\uff0c\u6bcf\u4e2a\u56fe\u6848\u5206\u6570\u00d72\u3002"
                    : "\u6ca1\u6709\u6a2a\u5411\u3001\u7ad6\u5411\u6216\u659c\u5411\u4e09\u8fde\u76f8\u540c\u56fe\u6848\u3002",
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

        private static bool IsLineStart(
            Dictionary<int, ScratchCellModel> cellsByPosition,
            int row,
            int column,
            LineDirection direction,
            ScratchCardModel model)
        {
            ScratchCellModel current = GetCell(cellsByPosition, row, column, model.GridWidth, model.GridHeight);
            if (current == null)
            {
                return false;
            }

            ScratchCellModel previous = GetCell(
                cellsByPosition,
                row - direction.RowStep,
                column - direction.ColumnStep,
                model.GridWidth,
                model.GridHeight);
            return previous == null || previous.PatternId != current.PatternId;
        }

        private static List<ScratchCellModel> GetMatchingLine(
            Dictionary<int, ScratchCellModel> cellsByPosition,
            int startRow,
            int startColumn,
            LineDirection direction,
            ScratchCardModel model)
        {
            var matchedCells = new List<ScratchCellModel>();
            ScratchCellModel first = GetCell(cellsByPosition, startRow, startColumn, model.GridWidth, model.GridHeight);
            if (first == null)
            {
                return matchedCells;
            }

            int row = startRow;
            int column = startColumn;
            while (row >= 0 && row < model.GridHeight && column >= 0 && column < model.GridWidth)
            {
                ScratchCellModel cell = GetCell(cellsByPosition, row, column, model.GridWidth, model.GridHeight);
                if (cell == null || cell.PatternId != first.PatternId)
                {
                    break;
                }

                matchedCells.Add(cell);
                row += direction.RowStep;
                column += direction.ColumnStep;
            }

            return matchedCells;
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

        private static ScratchCellModel GetCell(
            Dictionary<int, ScratchCellModel> cellsByPosition,
            int row,
            int column,
            int gridWidth,
            int gridHeight)
        {
            if (row < 0 || row >= gridHeight || column < 0 || column >= gridWidth)
            {
                return null;
            }

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
            HashSet<int> scoredCellIndexSet,
            ref int score)
        {
            if (cell == null || scoredCellIndexSet == null || !scoredCellIndexSet.Add(cell.CellIndex))
            {
                return;
            }

            score += ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, cell, MatchScoreMultiplier);
            winningPatternIds.Add(cell.PatternId);
            scoredCellIndices.Add(cell.CellIndex);
            scoredCellScoreMultipliers.Add(MatchScoreMultiplier * ScratchPatternScoreService.GetScoreMultiplierOnScore(cell));
        }

        private readonly struct LineDirection
        {
            public readonly int RowStep;
            public readonly int ColumnStep;

            public LineDirection(int rowStep, int columnStep)
            {
                RowStep = rowStep;
                ColumnStep = columnStep;
            }
        }
    }
}
