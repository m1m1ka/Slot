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

            return ScratchPatternEffectRegistry
                .Resolve(cell.PatternEffectType)
                .GetScore(new ScratchPatternEffectContext(model, cell));
        }

        public static double GetRewardMultiplierBonusOnReveal(ScratchCellModel cell)
        {
            if (cell == null)
            {
                return 0d;
            }

            return ScratchPatternEffectRegistry
                .Resolve(cell.PatternEffectType)
                .GetRewardMultiplierBonusOnReveal(new ScratchPatternEffectContext(null, cell));
        }

        public static double GetRewardMultiplierBonusOnScore(ScratchCellModel cell)
        {
            if (cell == null)
            {
                return 0d;
            }

            double patternBonus = ScratchPatternEffectRegistry
                .Resolve(cell.PatternEffectType)
                .GetRewardMultiplierBonusOnScore(new ScratchPatternEffectContext(null, cell));
            return cell.RewardMultiplierBonusOnScore + patternBonus;
        }

        public static double GetScoreMultiplierOnScore(ScratchCellModel cell)
        {
            if (cell == null || cell.ScoreMultiplierOnScore <= 0d)
            {
                return 1d;
            }

            return cell.ScoreMultiplierOnScore;
        }

        public static int GetCellScoreWithScoreMultiplier(ScratchCardModel model, ScratchCellModel cell, double scoreMultiplier = 1d)
        {
            return ScratchSettlementResult.ApplyMultiplier(
                GetCellScore(model, cell),
                scoreMultiplier * GetScoreMultiplierOnScore(cell));
        }

        public static bool ScoresDirectly(ScratchCardModel model, ScratchCellModel cell)
        {
            return cell != null &&
                ScratchPatternEffectRegistry
                    .Resolve(cell.PatternEffectType)
                    .ScoresDirectly(new ScratchPatternEffectContext(model, cell));
        }

        public static bool ExcludeFromScratchToolScoring(ScratchCardModel model, ScratchCellModel cell)
        {
            return IsFinalRewardMultiplierPattern(cell);
        }

        public static bool AffectsFinalRewardMultiplier(ScratchCellModel cell)
        {
            return IsFinalRewardMultiplierPattern(cell);
        }

        public static double GetFinalRewardMultiplier(ScratchCardModel model)
        {
            double rewardMultiplier = GetRewardMultiplierBeforeFinalMultiplierFactors(model);
            return rewardMultiplier * GetFinalRewardMultiplierFactor(model);
        }

        public static double GetRewardMultiplierBeforeFinalMultiplierFactors(ScratchCardModel model)
        {
            if (model == null)
            {
                return 1d;
            }

            return model.RewardMultiplier + model.SettlementMultiplierBonus;
        }

        public static int GetScoreBeforeFinalMultiplier(ScratchCardModel model, int scoreBeforeRewardMultiplier)
        {
            return scoreBeforeRewardMultiplier + (model != null ? model.SettlementScoreBonus : 0);
        }

        public static double GetFinalRewardMultiplierFactor(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return 1d;
            }

            double multiplierFactor = 1d;
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (IsFinalRewardMultiplierPattern(cell) &&
                    cell.IsScratchable &&
                    cell.IsScratched)
                {
                    multiplierFactor *= System.Math.Max(0d, cell.PatternEffectValue);
                }
            }

            return multiplierFactor;
        }

        private static bool IsFinalRewardMultiplierPattern(ScratchCellModel cell)
        {
            return cell != null &&
                cell.PatternEffectType == Configs.ScratchPatternEffectType.MultiplyRewardMultiplierOnSettlement;
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
                if (cell != null &&
                    ScratchPatternEffectRegistry
                        .Resolve(cell.PatternEffectType)
                        .ForcesFinalRewardZero(new ScratchPatternEffectContext(model, cell)))
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
                GetScoreBeforeFinalMultiplier(model, scoreBeforeRewardMultiplier),
                GetFinalRewardMultiplier(model));
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
                if (cell != null &&
                    cell.IsScratchable &&
                    cell.IsScratched &&
                    !ScoresDirectly(model, cell) &&
                    !ExcludeFromScratchToolScoring(model, cell))
                {
                    score += GetCellScoreWithScoreMultiplier(model, cell);
                }
            }

            return score;
        }

        public static int GetHighestBaseScore(ScratchCardModel model)
        {
            if (model?.Cells == null)
            {
                return 0;
            }

            int highestScore = 0;
            for (int i = 0; i < model.Cells.Count; i++)
            {
                ScratchCellModel cell = model.Cells[i];
                if (cell != null &&
                    cell.IsScratchable &&
                    !ScratchPatternEffectRegistry
                        .Resolve(cell.PatternEffectType)
                        .ExcludeFromHighestBaseScore(new ScratchPatternEffectContext(model, cell)))
                {
                    highestScore = UnityEngine.Mathf.Max(highestScore, cell.BaseScore);
                }
            }

            return highestScore;
        }
    }
}
