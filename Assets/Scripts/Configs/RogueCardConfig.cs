using System.Collections.Generic;

namespace Configs
{
    public class RogueCardConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rarity { get; set; }
        public List<RogueCardEffectConfig> Effects { get; set; } = new List<RogueCardEffectConfig>();
    }
}
