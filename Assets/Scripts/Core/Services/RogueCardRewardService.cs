using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Core
{
    public class RogueCardRewardService
    {
        public RogueCardRewardOfferModel CreateRewardOffer(int choiceCount = 3)
        {
            IReadOnlyList<RogueCardConfig> pool = RogueCardDefaultsProvider.GetAll();
            return CreateRewardOfferFromPool(pool, choiceCount);
        }

        public RogueCardRewardOfferModel CreateRewardOffer(
            int levelId,
            IReadOnlyList<RogueCardInventoryEntryModel> ownedCards,
            int choiceCount = 3)
        {
            IReadOnlyList<RogueCardConfig> pool = RogueCardDefaultsProvider.GetAvailableForLevel(levelId);
            var candidates = new List<RogueCardConfig>();
            int poolCount = pool != null ? pool.Count : 0;
            for (int i = 0; i < poolCount; i++)
            {
                RogueCardConfig card = pool[i];
                if (card != null && !IsOwnedCardMaxLevel(card, ownedCards))
                {
                    candidates.Add(card);
                }
            }

            return CreateRewardOfferFromPool(candidates, choiceCount);
        }

        private static RogueCardRewardOfferModel CreateRewardOfferFromPool(IReadOnlyList<RogueCardConfig> pool, int choiceCount)
        {
            var candidates = pool != null ? new List<RogueCardConfig>(pool) : new List<RogueCardConfig>();
            var choices = new List<RogueCardConfig>();

            choiceCount = Mathf.Max(1, choiceCount);
            while (choices.Count < choiceCount && candidates.Count > 0)
            {
                int index = Random.Range(0, candidates.Count);
                choices.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            return new RogueCardRewardOfferModel(choices);
        }

        private static bool IsOwnedCardMaxLevel(RogueCardConfig card, IReadOnlyList<RogueCardInventoryEntryModel> ownedCards)
        {
            int count = ownedCards != null ? ownedCards.Count : 0;
            for (int i = 0; i < count; i++)
            {
                RogueCardInventoryEntryModel ownedCard = ownedCards[i];
                if (ownedCard != null && ownedCard.CardId == card.Id)
                {
                    return ownedCard.Level >= card.GetMaxLevel();
                }
            }

            return false;
        }
    }
}
