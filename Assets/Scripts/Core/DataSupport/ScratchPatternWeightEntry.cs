namespace Core
{
    public class ScratchPatternWeightEntry
    {
        public int PatternId { get; }
        public float Weight { get; }
        public bool IsDynamicAdded { get; }

        public ScratchPatternWeightEntry(int patternId, float weight, bool isDynamicAdded)
        {
            PatternId = patternId;
            Weight = weight;
            IsDynamicAdded = isDynamicAdded;
        }
    }
}
