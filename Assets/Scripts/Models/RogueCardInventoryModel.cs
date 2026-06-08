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
        return AddCard(cardConfig, 1);
    }

    public RogueCardInventoryEntryModel AddCard(RogueCardConfig cardConfig, int level)
    {
        if (cardConfig == null)
        {
            return null;
        }

        RogueCardInventoryEntryModel ownedCard = FindByCardId(cardConfig.Id);
        if (ownedCard != null)
        {
            ownedCard.SetLevel(level, cardConfig.GetMaxLevel());
            OnCardChanged?.Invoke(ownedCard);
            return ownedCard;
        }

        ownedCard = new RogueCardInventoryEntryModel(cardConfig, level);
        _ownedCards.Add(ownedCard);
        OnCardChanged?.Invoke(ownedCard);
        return ownedCard;
    }

    public RogueCardInventoryEntryModel ReplaceCard(int oldCardId, RogueCardConfig newCardConfig)
    {
        return ReplaceCard(oldCardId, newCardConfig, 1);
    }

    public RogueCardInventoryEntryModel ReplaceCard(int oldCardId, RogueCardConfig newCardConfig, int level)
    {
        if (oldCardId <= 0 || newCardConfig == null)
        {
            return null;
        }

        RogueCardInventoryEntryModel existingNewCard = FindByCardId(newCardConfig.Id);
        if (existingNewCard != null)
        {
            existingNewCard.SetLevel(level, newCardConfig.GetMaxLevel());
            if (oldCardId != newCardConfig.Id)
            {
                RemoveCard(oldCardId);
            }

            OnCardChanged?.Invoke(existingNewCard);
            return existingNewCard;
        }

        for (int i = 0; i < _ownedCards.Count; i++)
        {
            RogueCardInventoryEntryModel ownedCard = _ownedCards[i];
            if (ownedCard != null && ownedCard.CardId == oldCardId)
            {
                RogueCardInventoryEntryModel replacement = new RogueCardInventoryEntryModel(newCardConfig, level);
                _ownedCards[i] = replacement;
                OnCardChanged?.Invoke(replacement);
                return replacement;
            }
        }

        return AddCard(newCardConfig, level);
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

    private void RemoveCard(int cardId)
    {
        for (int i = _ownedCards.Count - 1; i >= 0; i--)
        {
            RogueCardInventoryEntryModel ownedCard = _ownedCards[i];
            if (ownedCard != null && ownedCard.CardId == cardId)
            {
                _ownedCards.RemoveAt(i);
                return;
            }
        }
    }
}
