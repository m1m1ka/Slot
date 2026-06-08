using System.Collections.Generic;

namespace Configs
{
    public class ScratchCardExtraEffectConfig
    {
        public ScratchCardExtraEffectType EffectType { get; set; } = ScratchCardExtraEffectType.None;
        public List<int> TargetPatternIds { get; set; } = new List<int>();
        public List<int> TargetCellIndices { get; set; } = new List<int>();
        public string TargetPatternType { get; set; }
        public double Probability { get; set; } = 1d;
        public double Value { get; set; }
        public string Description { get; set; }
    }
}
