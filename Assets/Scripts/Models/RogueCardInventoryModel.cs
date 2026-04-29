using System;
using System.Collections.Generic;
using Configs;

public class RogueCardInventoryModel
{
    private readonly List<RogueCardInventoryEntryModel> _ownedCards = new List<RogueCardInventoryEntryModel>();

    public IReadOnlyList<RogueCardInventoryEntryModel> OwnedCards => _ownedCards;

    public event Action<RogueCardInventoryEntryModel> OnCardChanged;

    public RogueCardInventoryEntryModel AddCard(RogueCardConfig cardConfig)
    {
        if (cardConfig == null)
        {
            return null;
        }

        RogueCardInventoryEntryModel ownedCard = FindByCardId(cardConfig.Id);
        if (ownedCard != null)
        {
            ownedCard.Upgrade(cardConfig.GetMaxLevel());
            OnCardChanged?.Invoke(ownedCard);
            return ownedCard;
        }

        ownedCard = new RogueCardInventoryEntryModel(cardConfig);
        _ownedCards.Add(ownedCard);
        OnCardChanged?.Invoke(ownedCard);
        return ownedCard;
    }

    public int GetCardLevel(int cardId)
    {
        RogueCardInventoryEntryModel ownedCard = FindByCardId(cardId);
        return ownedCard != null ? ownedCard.Level : 0;
    }

    private RogueCardInventoryEntryModel FindByCardId(int cardId)
    {
        for (int i = 0; i < _ownedCards.Count; i++)
        {
            RogueCardInventoryEntryModel ownedCard = _ownedCards[i];
            if (ownedCard != null && ownedCard.CardId == cardId)
            {
                return ownedCard;
            }
        }

        return null;
    }
}
