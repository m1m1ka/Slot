namespace Configs
{
    using System.Collections.Generic;

    /// <summary>
    /// 刮刮卡种类配置，只描述卡本身的价格、图案池、区域模板和资源路径。
    /// 结算方式由玩家拥有的刮具配置决定。
    /// </summary>
    public class ScratchCardTypeConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string WinDescription { get; set; }
        public string SpecialDescription { get; set; }
        public double Price { get; set; }
        public int PatternPoolId { get; set; }
        public List<int> AllowedPatternIds { get; set; } = new List<int>();
        public List<ScratchCardExtraEffectConfig> ExtraEffects { get; set; } = new List<ScratchCardExtraEffectConfig>();
        public List<ScratchCardWinRuleConfig> WinRules { get; set; } = new List<ScratchCardWinRuleConfig>();
        public int AreaTemplateId { get; set; }
        public string PrefabPath { get; set; }
        public string ShopIconPath { get; set; }
    }
}
