namespace Core
{
    public class ScratchPatternWeightEntry
    {
        public int PatternId { get; }
        public float Weight { get; }
        public int BaseScore { get; }
        public bool IsDynamicAdded { get; }
        public bool IsCardExtraEffectApplied { get; }

        public ScratchPatternWeightEntry(
            int patternId,
            float weight,
            int baseScore,
            bool isDynamicAdded,
            bool isCardExtraEffectApplied = false)
        {
            PatternId = patternId;
            Weight = weight;
            BaseScore = baseScore;
            IsDynamicAdded = isDynamicAdded;
            IsCardExtraEffectApplied = isCardExtraEffectApplied;
        }
    }
}
