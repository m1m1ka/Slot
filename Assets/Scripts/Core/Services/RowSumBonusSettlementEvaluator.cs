using UnityEngine;

namespace Core
{
    /// <summary>
    /// 按行计算并给予额外加成的结算策略示例。
    /// </summary>
    public class RowSumBonusSettlementEvaluator : IScratchSettlementEvaluator
    {
        public ScratchSettlementResult Evaluate(ScratchCardModel model)
        {
            if (model == null)
            {
                return new ScratchSettlementResult();
            }

            bool[] scratchedRows = new bool[Mathf.Max(0, model.GridHeight)];
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell == null || !cell.IsScratchable || !cell.IsScratched || cell.Row < 0 || cell.Row >= scratchedRows.Length)
                {
                    continue;
                }

                scratchedRows[cell.Row] = true;
            }

            int scratchedRowCount = 0;
            for (int i = 0; i < scratchedRows.Length; i++)
            {
                if (scratchedRows[i])
                {
                    scratchedRowCount++;
                }
            }

            int rowBonus = scratchedRowCount * 50;
            int score = ScratchPatternScoreService.SumScratchableScores(model) + rowBonus;
            return new ScratchSettlementResult
            {
                ScoreBeforeRewardMultiplier = score,
                FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(model, score),
                Summary = "已应用行加成。",
            };
        }
    }
}
