namespace Core
{
    public class ScratchPatternWeightEntry
    {
        public int PatternId { get; }
        public float Weight { get; }
        public bool IsDynamicAdded { get; }
        public bool IsCardExtraEffectApplied { get; }

        public ScratchPatternWeightEntry(int patternId, float weight, bool isDynamicAdded, bool isCardExtraEffectApplied = false)
        {
            PatternId = patternId;
            Weight = weight;
            IsDynamicAdded = isDynamicAdded;
            IsCardExtraEffectApplied = isCardExtraEffectApplied;
        }
    }
}
