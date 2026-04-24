using System.Collections.Generic;

namespace Configs
{
    /// <summary>
    /// 可刮区域模板配置。
    /// 通过宽高和可刮单元索引定义一张卡的基础布局。
    /// </summary>
    public class ScratchAreaTemplateConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<int> ScratchableCellIndices { get; set; } = new List<int>();
    }
}
