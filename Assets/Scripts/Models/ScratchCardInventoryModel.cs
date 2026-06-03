using System;
using System.Collections.Generic;
using Configs;
using Core;

public class ScratchCardInventoryModel
{
    private readonly List<ScratchCardTypeConfig> _ownedCardTypes = new List<ScratchCardTypeConfig>();

    public IReadOnlyList<ScratchCardTypeConfig> OwnedCardTypes => _ownedCardTypes;

    public event Action<ScratchCardTypeConfig> OnCardTypeAdded;

    public ScratchCardInventoryModel(IEnumerable<ScratchCardTypeConfig> starterCardTypes = null)
    {
        if (starterCardTypes == null)
        {
            return;
        }

        foreach (ScratchCardTypeConfig cardType in starterCardTypes)
        {
            AddCardType(cardType);
        }
    }

    public bool AddCardType(ScratchCardTypeConfig cardType)
    {
        if (cardType == null || cardType.Id <= 0 || HasCardType(cardType.Id))
        {
            return false;
        }

        _ownedCardTypes.Add(cardType);
        OnCardTypeAdded?.Invoke(cardType);
        return true;
    }

    public bool HasCardType(int cardTypeId)
    {
        for (int i = 0; i < _ownedCardTypes.Count; i++)
        {
            if (_ownedCardTypes[i] != null && _ownedCardTypes[i].Id == cardTypeId)
            {
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<ScratchCardTypeConfig> GetStarterCardTypes()
    {
        return new List<ScratchCardTypeConfig>
        {
            ScratchCardDefaultsProvider.GetCardType(1)
        };
    }
}
