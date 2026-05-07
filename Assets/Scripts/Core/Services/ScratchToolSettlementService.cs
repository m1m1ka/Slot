using System.Collections.Generic;
using Configs;

namespace Core
{
    public static class ScratchToolSettlementService
    {
        public static ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            var result = new ScratchSettlementResult();
            IReadOnlyList<ScratchToolConfig> scratchTools = model?.ScratchTools;
            if (scratchTools == null || scratchTools.Count == 0)
            {
                result.FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, 0);
                result.Summary = "没有可用刮具。";
                return result;
            }

            var summaries = new List<string>();
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

                result.ScoreBeforeRewardMultiplier += toolResult.ScoreBeforeRewardMultiplier;
                AddRange(result.WinningPatternIds, toolResult.WinningPatternIds);
                AddRange(result.ScoredCellIndices, toolResult.ScoredCellIndices);
                AddRange(result.ScoredCellScoreMultipliers, toolResult.ScoredCellScoreMultipliers);

                if (!string.IsNullOrWhiteSpace(toolResult.Summary))
                {
                    summaries.Add($"{tool.Name}: {toolResult.Summary}");
                }
            }

            result.FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, result.ScoreBeforeRewardMultiplier);
            result.Summary = summaries.Count > 0 ? string.Join(" ", summaries) : "没有触发刮具计分。";
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
    }
}
