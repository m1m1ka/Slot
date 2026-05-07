namespace Configs
{
    /// <summary>
    /// 刮具配置。玩家拥有的刮具决定当前启用的结算规则。
    /// </summary>
    public class ScratchToolConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ScratchSettlementType SettlementType { get; set; }
        public string IconAtlasPath { get; set; }
        public string IconSpriteName { get; set; }
    }
}
