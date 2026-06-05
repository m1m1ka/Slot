using System.Collections.Generic;

namespace Configs
{
    public class RogueCardEffectConfig
    {
        public RogueCardEffectType EffectType { get; set; }
        public RogueCardTriggerTime TriggerTime { get; set; } = RogueCardTriggerTime.Settlement;
        public List<int> TargetIds { get; set; } = new List<int>();
        public string TargetType { get; set; }
        public List<int> CardTypeIds { get; set; } = new List<int>();
        public double Value { get; set; }
        public string ValueExpression { get; set; }
    }
}
