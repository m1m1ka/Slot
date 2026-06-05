namespace Configs
{
    /// <summary>
    /// 刮刮卡图案的静态配置定义。
    /// 当前先使用默认内置数据，后续可切换为正式配表。
    /// </summary>
    public class ScratchPatternConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BaseScore { get; set; }
        public string Type { get; set; }
        public string AtlasPath { get; set; }
        public string SpriteName { get; set; }
        public string SpritePath { get; set; }
        public ScratchPatternEffectType EffectType { get; set; }
        public double EffectValue { get; set; }
    }
}
