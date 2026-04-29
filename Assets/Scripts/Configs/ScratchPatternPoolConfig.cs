using System.Collections.Generic;

namespace Configs
{
    /// <summary>
    /// 图案池配置，定义某种卡可出现哪些图案，以及这些图案在当前卡内的相对权重。
    /// </summary>
    public class ScratchPatternPoolConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ScratchPatternPoolEntryConfig> Entries { get; set; } = new List<ScratchPatternPoolEntryConfig>();
    }

    public class ScratchPatternPoolEntryConfig
    {
        public int PatternId { get; set; }
        public int Weight { get; set; }
    }
}
