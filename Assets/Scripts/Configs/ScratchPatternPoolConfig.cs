using System.Collections.Generic;

namespace Configs
{
    /// <summary>
    /// 图案池配置，定义某种卡可出现哪些图案。
    /// Runtime probability is resolved from the global pattern weight table, then normalized inside this pool.
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

        // Legacy field kept for config compatibility. Runtime probability uses global pattern weights.
        public int Weight { get; set; }
    }
}
