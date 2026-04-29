using System.Collections.Generic;

namespace Configs
{
    public class RogueCardLevelConfig
    {
        public int Level { get; set; }
        public string Description { get; set; }
        public List<RogueCardEffectConfig> Effects { get; set; } = new List<RogueCardEffectConfig>();
    }
}
