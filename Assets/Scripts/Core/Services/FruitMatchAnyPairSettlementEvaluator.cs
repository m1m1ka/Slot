using System.Collections.Generic;
using Configs;

namespace Core
{
    public class FruitMatchAnyPairSettlementEvaluator : IScratchSettlementEvaluator
    {
        private const double MatchScoreMultiplier = 1d;
        private const int RequiredMatchCount = 2;

        private static readonly HashSet<int> FruitPatternIds = BuildFruitPatternIds();

        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            if (model == null || model.Cells == null)
            {
                return new ScratchSettlementResult();
            }

            Dictionary<int, List<ScratchCellModel>> cellsByPattern = BuildFruitCellsByPattern(model);
            var winningPatternIds = new List<int>();
            var scoredCellIndices = new List<int>();
            var scoredCellScoreMultipliers = new List<double>();
            int score = 0;

            foreach (KeyValuePair<int, List<ScratchCellModel>> pair in cellsByPattern)
            {
                List<ScratchCellModel> patternCells = pair.Value;
                if (patternCells == null || patternCells.Count < RequiredMatchCount)
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
                Summary = score > 0 ? "Two or more matching fruit patterns scored." : "No matching fruit pattern pair.",
                WinningPatternIds = winningPatternIds,
                ScoredCellIndices = scoredCellIndices,
                ScoredCellScoreMultipliers = scoredCellScoreMultipliers
            };
        }

        private static Dictionary<int, List<ScratchCellModel>> BuildFruitCellsByPattern(ScratchCardModel model)
        {
            var cellsByPattern = new Dictionary<int, List<ScratchCellModel>>();
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell == null ||
                    !cell.IsScratchable ||
                    !cell.IsScratched ||
                    !FruitPatternIds.Contains(cell.PatternId) ||
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

        private static HashSet<int> BuildFruitPatternIds()
        {
            var patternIds = new HashSet<int>();
            ScratchPatternPoolConfig fruitPool = ScratchCardDefaultsProvider.GetPatternPool(ScratchCardDefaultsProvider.FruitPatternPoolId);
            if (fruitPool?.Entries == null)
            {
                return patternIds;
            }

            for (int i = 0; i < fruitPool.Entries.Count; i++)
            {
                ScratchPatternPoolEntryConfig entry = fruitPool.Entries[i];
                if (entry != null && entry.PatternId > 0)
                {
                    patternIds.Add(entry.PatternId);
                }
            }

            return patternIds;
        }
    }
}
