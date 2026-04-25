using System;
using System.Collections.Generic;
using Configs;

public class RogueCardInventoryModel
{
    private readonly List<RogueCardConfig> _ownedCards = new List<RogueCardConfig>();

    public IReadOnlyList<RogueCardConfig> OwnedCards => _ownedCards;

    public event Action<RogueCardConfig> OnCardAdded;

    public void AddCard(RogueCardConfig cardConfig)
    {
        if (cardConfig == null)
        {
            return;
        }

        _ownedCards.Add(cardConfig);
        OnCardAdded?.Invoke(cardConfig);
    }
}
