using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchCardIntrinsicWinSettlementService
    {
        public const int IntrinsicWinSourceId = -2;

        public static List<ScratchSettlementResult> EvaluateByRuleOrder(ScratchCardModel model)
        {
            var results = new List<ScratchSettlementResult>();
            IReadOnlyList<ScratchCardWinRuleConfig> winRules = model?.WinRules;
            if (winRules == null || winRules.Count == 0)
            {
                return results;
            }

            for (int i = 0; i < winRules.Count; i++)
            {
                ScratchCardWinRuleConfig ruleConfig = winRules[i];
                ScratchSettlementResult result = EvaluateRule(model, ruleConfig);
                if (result == null)
                {
                    continue;
                }

                result.SourceScratchToolId = GetIntrinsicWinSourceId(ruleConfig);
                result.SourceScratchToolName = string.IsNullOrWhiteSpace(ruleConfig.Description)
                    ? "Scratch Card Win"
                    : ruleConfig.Description;
                results.Add(result);
            }

            return results;
        }

        private static int GetIntrinsicWinSourceId(ScratchCardWinRuleConfig ruleConfig)
        {
            int ruleId = ruleConfig != null && ruleConfig.Id > 0 ? ruleConfig.Id : 0;
            return IntrinsicWinSourceId - ruleId;
        }

        private static ScratchSettlementResult EvaluateRule(ScratchCardModel model, ScratchCardWinRuleConfig ruleConfig)
        {
            if (model == null || ruleConfig == null)
            {
                return null;
            }

            switch (ruleConfig.RuleType)
            {
                case ScratchCardWinRuleType.SamePatternCount:
                    return EvaluateSamePatternCount(model, ruleConfig);
                case ScratchCardWinRuleType.SpecificPatternCount:
                    return EvaluateSpecificPatternCount(model, ruleConfig);
                case ScratchCardWinRuleType.None:
                default:
                    return null;
            }
        }

        private static ScratchSettlementResult EvaluateSpecificPatternCount(ScratchCardModel model, ScratchCardWinRuleConfig ruleConfig)
        {
            if (ruleConfig.TargetPatternId <= 0)
            {
                return null;
            }

            int requiredCount = ruleConfig.RequiredCount > 0 ? ruleConfig.RequiredCount : 1;
            double ruleMultiplier = ruleConfig.ScoreMultiplier > 0d ? ruleConfig.ScoreMultiplier : 1d;
            Dictionary<int, List<ScratchCellModel>> cellsByPattern = BuildCellsByPattern(model);
            if (!cellsByPattern.TryGetValue(ruleConfig.TargetPatternId, out List<ScratchCellModel> patternCells) ||
                !IsRequiredCountMatched(patternCells, requiredCount, ruleConfig.RequireExactCount))
            {
                return null;
            }

            var winningPatternIds = new List<int> { ruleConfig.TargetPatternId };
            var scoredCellIndices = new List<int>();
            var scoredCellScoreMultipliers = new List<double>();
            var scoredCellFloatTexts = new List<string>();
            int score = 0;

            patternCells.Sort((left, right) => left.ScratchOrder.CompareTo(right.ScratchOrder));
            for (int i = 0; i < patternCells.Count; i++)
            {
                ScratchCellModel cell = patternCells[i];
                double scoreMultiplier = GetRuleCellScoreMultiplier(cell, ruleMultiplier);
                int cellScore = GetRuleCellScore(model, cell, ruleConfig, scoreMultiplier);
                score += cellScore;
                scoredCellIndices.Add(cell.CellIndex);
                scoredCellScoreMultipliers.Add(scoreMultiplier);
                scoredCellFloatTexts.Add(GetRuleCellFloatText(ruleConfig, cellScore));
            }

            string description = string.IsNullOrWhiteSpace(ruleConfig.Description)
                ? $"Scratch card win: pattern {ruleConfig.TargetPatternId} appears {requiredCount} times."
                : ruleConfig.Description;
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = description,
                WinningPatternIds = winningPatternIds,
                ScoredCellIndices = scoredCellIndices,
                ScoredCellScoreMultipliers = scoredCellScoreMultipliers,
                ScoredCellFloatTexts = scoredCellFloatTexts
            };
        }

        private static ScratchSettlementResult EvaluateSamePatternCount(ScratchCardModel model, ScratchCardWinRuleConfig ruleConfig)
        {
            int requiredCount = ruleConfig.RequiredCount > 0 ? ruleConfig.RequiredCount : 1;
            double ruleMultiplier = ruleConfig.ScoreMultiplier > 0d ? ruleConfig.ScoreMultiplier : 1d;
            Dictionary<int, List<ScratchCellModel>> cellsByPattern = BuildCellsByPattern(model);

            var winningPatternIds = new List<int>();
            var scoredCellIndices = new List<int>();
            var scoredCellScoreMultipliers = new List<double>();
            var scoredCellFloatTexts = new List<string>();
            int score = 0;

            foreach (KeyValuePair<int, List<ScratchCellModel>> pair in cellsByPattern)
            {
                List<ScratchCellModel> patternCells = pair.Value;
                if (!IsRequiredCountMatched(patternCells, requiredCount, ruleConfig.RequireExactCount))
                {
                    continue;
                }

                winningPatternIds.Add(pair.Key);
                patternCells.Sort((left, right) => left.ScratchOrder.CompareTo(right.ScratchOrder));
                for (int i = 0; i < patternCells.Count; i++)
                {
                    ScratchCellModel cell = patternCells[i];
                    double scoreMultiplier = GetRuleCellScoreMultiplier(cell, ruleMultiplier);
                    int cellScore = GetRuleCellScore(model, cell, ruleConfig, scoreMultiplier);
                    score += cellScore;
                    scoredCellIndices.Add(cell.CellIndex);
                    scoredCellScoreMultipliers.Add(scoreMultiplier);
                    scoredCellFloatTexts.Add(GetRuleCellFloatText(ruleConfig, cellScore));
                }
            }

            string description = string.IsNullOrWhiteSpace(ruleConfig.Description)
                ? $"刮刮卡自身中奖：{requiredCount} 个相同图案计分。"
                : ruleConfig.Description;
            if (score <= 0)
            {
                return null;
            }

            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = description,
                WinningPatternIds = winningPatternIds,
                ScoredCellIndices = scoredCellIndices,
                ScoredCellScoreMultipliers = scoredCellScoreMultipliers,
                ScoredCellFloatTexts = scoredCellFloatTexts
            };
        }

        private static bool IsRequiredCountMatched(List<ScratchCellModel> patternCells, int requiredCount, bool requireExactCount)
        {
            if (patternCells == null)
            {
                return false;
            }

            return requireExactCount
                ? patternCells.Count == requiredCount
                : patternCells.Count >= requiredCount;
        }

        private static double GetRuleCellScoreMultiplier(ScratchCellModel cell, double ruleMultiplier)
        {
            return ruleMultiplier * ScratchPatternScoreService.GetScoreMultiplierOnScore(cell);
        }

        private static int GetRuleCellScore(
            ScratchCardModel model,
            ScratchCellModel cell,
            ScratchCardWinRuleConfig ruleConfig,
            double scoreMultiplier)
        {
            if (ruleConfig != null && ruleConfig.ScorePerMatchedCell > 0)
            {
                return ScratchSettlementResult.ApplyMultiplier(ruleConfig.ScorePerMatchedCell, scoreMultiplier);
            }

            double ruleMultiplier = ruleConfig != null && ruleConfig.ScoreMultiplier > 0d
                ? ruleConfig.ScoreMultiplier
                : 1d;
            return ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, cell, ruleMultiplier);
        }

        private static string GetRuleCellFloatText(ScratchCardWinRuleConfig ruleConfig, int cellScore)
        {
            return ruleConfig != null && ruleConfig.ScorePerMatchedCell > 0
                ? NumberFormatter.FormatCompact(cellScore)
                : null;
        }

        private static Dictionary<int, List<ScratchCellModel>> BuildCellsByPattern(
            ScratchCardModel model,
            bool includeDirectScoringPatterns = false)
        {
            var cellsByPattern = new Dictionary<int, List<ScratchCellModel>>();
            if (model?.Cells == null)
            {
                return cellsByPattern;
            }

            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell == null ||
                    !cell.IsScratchable ||
                    !cell.IsScratched ||
                    (!includeDirectScoringPatterns && ScratchPatternScoreService.ScoresDirectly(model, cell)) ||
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
