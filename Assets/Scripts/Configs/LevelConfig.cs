namespace Configs
{
    public class LevelConfig : IConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double RequiredCoins { get; set; }
        public int ScratchCardPurchaseLimit { get; set; }
    }
}
