using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchToolSettlementService
    {
        public static ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            var result = new ScratchSettlementResult();
            List<ScratchSettlementResult> toolResults = EvaluateByToolOrder(model);
            if (toolResults.Count == 0)
            {
                result.FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, 0);
                result.Summary = "\u6ca1\u6709\u53ef\u7528\u522e\u5177\u3002";
                return result;
            }

            var summaries = new List<string>();
            for (int i = 0; i < toolResults.Count; i++)
            {
                ScratchSettlementResult toolResult = toolResults[i];
                if (toolResult == null)
                {
                    continue;
                }

                result.ScoreBeforeRewardMultiplier += toolResult.ScoreBeforeRewardMultiplier;
                AddRange(result.WinningPatternIds, toolResult.WinningPatternIds);
                AddRange(result.ScoredCellIndices, toolResult.ScoredCellIndices);
                AddRange(result.ScoredCellScoreMultipliers, toolResult.ScoredCellScoreMultipliers);
                AddRange(result.ScoredCellFloatTexts, toolResult.ScoredCellFloatTexts);

                if (!string.IsNullOrWhiteSpace(toolResult.Summary))
                {
                    summaries.Add(toolResult.Summary);
                }
            }

            result.FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, result.ScoreBeforeRewardMultiplier);
            result.Summary = summaries.Count > 0 ? string.Join(" ", summaries) : "\u6ca1\u6709\u89e6\u53d1\u522e\u5177\u8ba1\u5206\u3002";
            return result;
        }

        public static List<ScratchSettlementResult> EvaluateByToolOrder(ScratchCardModel model)
        {
            var results = new List<ScratchSettlementResult>();
            ScratchSettlementResult directPatternResult = EvaluateDirectPatternScores(model);
            if (directPatternResult != null &&
                directPatternResult.ScoredCellIndices != null &&
                directPatternResult.ScoredCellIndices.Count > 0)
            {
                results.Add(directPatternResult);
            }

            IReadOnlyList<ScratchToolConfig> scratchTools = model?.ScratchTools;
            if (scratchTools == null || scratchTools.Count == 0)
            {
                return results;
            }

            for (int i = 0; i < scratchTools.Count; i++)
            {
                ScratchToolConfig tool = scratchTools[i];
                if (tool == null)
                {
                    continue;
                }

                IScratchSettlementEvaluator evaluator = ScratchSettlementEvaluatorFactory.Create(tool.SettlementType);
                ScratchSettlementResult toolResult = evaluator.Evaluate(model);
                if (toolResult == null)
                {
                    continue;
                }

                toolResult.SourceScratchToolId = tool.Id;
                toolResult.SourceScratchToolName = tool.Name;

                if (!string.IsNullOrWhiteSpace(toolResult.Summary))
                {
                    toolResult.Summary = $"{tool.Name}: {toolResult.Summary}";
                }

                results.Add(toolResult);
            }

            return results;
        }

        private static ScratchSettlementResult EvaluateDirectPatternScores(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return null;
            }

            var result = new ScratchSettlementResult
            {
                SourceScratchToolId = -1,
                SourceScratchToolName = "Direct Pattern",
                Summary = "特殊图案直接计分。"
            };

            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell == null || !cell.IsScratchable || !cell.IsScratched)
                {
                    continue;
                }

                bool scoresDirectly = ScratchPatternScoreService.ScoresDirectly(model, cell);
                bool affectsFinalRewardMultiplier = ScratchPatternScoreService.AffectsFinalRewardMultiplier(cell);
                if (!scoresDirectly && !affectsFinalRewardMultiplier)
                {
                    continue;
                }

                double scoreMultiplier = scoresDirectly
                    ? ScratchPatternScoreService.GetScoreMultiplierOnScore(cell)
                    : 1d;
                if (scoresDirectly)
                {
                    result.ScoreBeforeRewardMultiplier += ScratchPatternScoreService.GetCellScoreWithScoreMultiplier(model, cell);
                }

                result.WinningPatternIds.Add(cell.PatternId);
                result.ScoredCellIndices.Add(cell.CellIndex);
                result.ScoredCellScoreMultipliers.Add(scoreMultiplier);
                result.ScoredCellFloatTexts.Add(affectsFinalRewardMultiplier ? FormatFinalRewardMultiplierText(cell.PatternEffectValue) : null);
            }

            result.FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, result.ScoreBeforeRewardMultiplier);
            return result;
        }

        private static void AddRange<T>(List<T> target, List<T> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                target.Add(source[i]);
            }
        }

        private static string FormatFinalRewardMultiplierText(double multiplier)
        {
            double normalizedMultiplier = multiplier >= 0d ? multiplier : 0d;
            return $"×{normalizedMultiplier:0.##}";
        }
    }
}
