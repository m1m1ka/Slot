namespace Configs
{
    /// <summary>
    /// 刮刮卡种类配置。
    /// 定义某种卡的价格、图案池、区域模板与结算方式。
    /// </summary>
    public class ScratchCardTypeConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string WinDescription { get; set; }
        public int Price { get; set; }
        public int PatternPoolId { get; set; }
        public int AreaTemplateId { get; set; }
        public ScratchSettlementType SettlementType { get; set; }
        public string PrefabPath { get; set; }
        public string ShopIconAtlasPath { get; set; }
        public string ShopIconSpriteName { get; set; }
    }
}
