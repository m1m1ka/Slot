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

                if (!string.IsNullOrWhiteSpace(toolResult.Summary))
                {
                    toolResult.Summary = $"{tool.Name}: {toolResult.Summary}";
                }

                results.Add(toolResult);
            }

            return results;
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
    }
}
