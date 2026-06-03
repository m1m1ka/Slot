using Core;

/// <summary>
/// 玩家长期运行时上下文。
/// 用于聚合玩家相关的多个 Model，并在运行时统一对外提供访问入口。
/// </summary>
public class PlayerContext
{
    public PlayerModel Player { get; }
    public RogueCardInventoryModel RogueCards { get; }
    public ScratchToolInventoryModel ScratchTools { get; }
    public ScratchCardInventoryModel ScratchCards { get; }

    public PlayerContext(double initialCoins = 0)
    {
        Player = new PlayerModel(initialCoins);
        RogueCards = new RogueCardInventoryModel();
        ScratchTools = new ScratchToolInventoryModel(ScratchToolDefaultsProvider.GetStarterTools());
        ScratchCards = new ScratchCardInventoryModel(ScratchCardInventoryModel.GetStarterCardTypes());
    }
}
