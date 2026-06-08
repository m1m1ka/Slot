using Configs;

public class RogueCardInventoryEntryModel
{
    public RogueCardConfig Config { get; }
    public int Level { get; private set; }

    public int CardId => Config != null ? Config.Id : 0;
    public string Name => Config != null ? Config.Name : string.Empty;
    public string Description => Config != null ? Config.Description : string.Empty;
    public RogueCardRarity Rarity => Config != null ? Config.Rarity : RogueCardRarity.Common;
    public string RarityDisplayName => Config != null ? Config.GetRarityDisplayName() : string.Empty;

    public RogueCardInventoryEntryModel(RogueCardConfig config, int level = 1)
    {
        Config = config;
        Level = level < 1 ? 1 : level;
    }

    public void Upgrade(int maxLevel)
    {
        if (maxLevel < 1)
        {
            maxLevel = 1;
        }

        if (Level < maxLevel)
        {
            Level++;
        }
    }

    public void SetLevel(int level, int maxLevel)
    {
        if (maxLevel < 1)
        {
            maxLevel = 1;
        }

        if (level < 1)
        {
            level = 1;
        }
        else if (level > maxLevel)
        {
            level = maxLevel;
        }

        if (level > Level)
        {
            Level = level;
        }
    }
}
