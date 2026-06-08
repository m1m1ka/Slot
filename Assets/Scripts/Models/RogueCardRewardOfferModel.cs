using System.Collections.Generic;
using Configs;

public class RogueCardRewardOfferModel
{
    public IReadOnlyList<RogueCardRewardChoiceModel> Choices { get; }

    public RogueCardRewardOfferModel(IReadOnlyList<RogueCardRewardChoiceModel> choices)
    {
        Choices = choices ?? new List<RogueCardRewardChoiceModel>();
    }
}

public class RogueCardRewardChoiceModel
{
    public RogueCardConfig CardConfig { get; }
    public int Level { get; }

    public int CardId => CardConfig != null ? CardConfig.Id : 0;

    public RogueCardRewardChoiceModel(RogueCardConfig cardConfig, int level)
    {
        CardConfig = cardConfig;
        Level = level < 1 ? 1 : level;
    }
}
