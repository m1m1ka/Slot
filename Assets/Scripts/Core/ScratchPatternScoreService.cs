using Configs;
using UnityEngine;

namespace Core
{
    public static class ScratchPatternScoreService
    {
        public static int GetCellScore(ScratchCardModel model, ScratchCellModel cell)
        {
            if (cell == null)
            {
                return 0;
            }

            switch (cell.PatternEffectType)
            {
                case ScratchPatternEffectType.ScoreHighestPatternBaseScoreMultiplier:
                    double multiplier = cell.PatternEffectValue > 0d ? cell.PatternEffectValue : 2d;
                    return Mathf.RoundToInt(GetHighestBaseScore(model) * (float)multiplier);
                default:
                    return cell.BaseScore;
            }
        }

        public static double GetRewardMultiplierBonusOnReveal(ScratchCellModel cell)
        {
            if (cell == null)
            {
                return 0d;
            }

            return cell.PatternEffectType == ScratchPatternEffectType.AddRewardMultiplierOnRevealed && cell.PatternEffectValue > 0d
                ? cell.PatternEffectValue
                : 0d;
        }

        public static double GetRewardMultiplierBonusOnScore(ScratchCellModel cell)
        {
            if (cell == null)
            {
                return 0d;
            }

            return cell.RewardMultiplierBonusOnScore;
        }

        public static bool ForcesFinalRewardZero(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return false;
            }

            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell != null && cell.IsScratchable && cell.PatternEffectType == ScratchPatternEffectType.ForceFinalRewardZero)
                {
                    return true;
                }
            }

            return false;
        }

        public static int ApplyFinalScoreRules(ScratchCardModel model, int scoreBeforeRewardMultiplier)
        {
            if (ForcesFinalRewardZero(model))
            {
                return 0;
            }

            return ScratchSettlementResult.ApplyMultiplier(
                scoreBeforeRewardMultiplier,
                model != null ? model.RewardMultiplier : 1d);
        }

        public static int SumScratchableScores(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return 0;
            }

            int score = 0;
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell != null && cell.IsScratchable)
                {
                    score += GetCellScore(model, cell);
                }
            }

            return score;
        }

        private static int GetHighestBaseScore(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return 0;
            }

            int highestScore = 0;
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell != null && cell.IsScratchable && cell.PatternEffectType != ScratchPatternEffectType.ForceFinalRewardZero)
                {
                    highestScore = Mathf.Max(highestScore, cell.BaseScore);
                }
            }

            return highestScore;
        }
    }
}
