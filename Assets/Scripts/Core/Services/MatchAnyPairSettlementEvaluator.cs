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

            List<ScratchCellModel> scratchedCells = GetScratchedCellsByRevealOrder(model);
            var unpairedCellsByPattern = new Dictionary<int, ScratchCellModel>();
            var winningPatternIds = new List<int>();
            var scoredCellIndices = new List<int>();
            var scoredCellScoreMultipliers = new List<double>();

            int pairScore = 0;
            for (int i = 0; i < scratchedCells.Count; i++)
            {
                ScratchCellModel cell = scratchedCells[i];
                if (!unpairedCellsByPattern.TryGetValue(cell.PatternId, out ScratchCellModel pairedCell))
                {
                    unpairedCellsByPattern[cell.PatternId] = cell;
                    continue;
                }

                double pairedCellMultiplier = PairScoreMultiplier * ScratchPatternScoreService.GetScoreMultiplierOnScore(pairedCell);
                double cellMultiplier = PairScoreMultiplier * ScratchPatternScoreService.GetScoreMultiplierOnScore(cell);
                pairScore += ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, pairedCell, PairScoreMultiplier);
                pairScore += ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, cell, PairScoreMultiplier);

                winningPatternIds.Add(cell.PatternId);
                scoredCellIndices.Add(pairedCell.CellIndex);
                scoredCellIndices.Add(cell.CellIndex);
                scoredCellScoreMultipliers.Add(pairedCellMultiplier);
                scoredCellScoreMultipliers.Add(cellMultiplier);

                unpairedCellsByPattern.Remove(cell.PatternId);
            }

            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = pairScore,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, pairScore),
                Summary = pairScore > 0 ? "配对成功，成对图案分数获得x2。" : "没有配对，不获得分数。",
                WinningPatternIds = winningPatternIds,
                ScoredCellIndices = scoredCellIndices,
                ScoredCellScoreMultipliers = scoredCellScoreMultipliers
            };
        }

        private static List<ScratchCellModel> GetScratchedCellsByRevealOrder(ScratchCardModel model)
        {
            var scratchedCells = new List<ScratchCellModel>();
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell != null &&
                    cell.IsScratchable &&
                    cell.IsScratched &&
                    !ScratchPatternScoreService.ScoresDirectly(model, cell) &&
                    !ScratchPatternScoreService.ExcludeFromScratchToolScoring(model, cell))
                {
                    scratchedCells.Add(cell);
                }
            }

            scratchedCells.Sort((left, right) => left.ScratchOrder.CompareTo(right.ScratchOrder));
            return scratchedCells;
        }
    }
}
