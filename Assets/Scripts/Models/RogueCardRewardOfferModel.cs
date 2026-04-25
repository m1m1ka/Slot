using System.Collections.Generic;
using Configs;

public class RogueCardRewardOfferModel
{
    public IReadOnlyList<RogueCardConfig> Choices { get; }

    public RogueCardRewardOfferModel(IReadOnlyList<RogueCardConfig> choices)
    {
        Choices = choices ?? new List<RogueCardConfig>();
    }
}
