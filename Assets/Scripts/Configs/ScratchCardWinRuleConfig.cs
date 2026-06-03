namespace Configs
{
    public class ScratchCardWinRuleConfig
    {
        public int Id { get; set; }
        public ScratchCardWinRuleType RuleType { get; set; }
        public int TargetPatternId { get; set; }
        public int RequiredCount { get; set; }
        public bool RequireExactCount { get; set; }
        public int ScorePerMatchedCell { get; set; }
        public double ScoreMultiplier { get; set; } = 1d;
        public string Description { get; set; }
    }
}
