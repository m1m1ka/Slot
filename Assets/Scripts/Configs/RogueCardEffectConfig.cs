using System.Collections.Generic;

namespace Configs
{
    public class RogueCardEffectConfig
    {
        public RogueCardEffectType EffectType { get; set; }
        public List<int> TargetIds { get; set; } = new List<int>();
        public List<int> CardTypeIds { get; set; } = new List<int>();
        public double Value { get; set; }
    }
}
