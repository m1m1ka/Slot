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
            var candidates = new List<RogueCardRewardChoiceModel>();
            int poolCount = pool != null ? pool.Count : 0;
            for (int i = 0; i < poolCount; i++)
            {
                RogueCardConfig card = pool[i];
                if (card != null)
                {
                    candidates.Add(new RogueCardRewardChoiceModel(card, 1));
                }
            }

            return CreateRewardOfferFromPool(candidates, choiceCount);
        }

        public RogueCardRewardOfferModel CreateRewardOffer(
            int levelId,
            IReadOnlyList<RogueCardInventoryEntryModel> ownedCards,
            int choiceCount = 3)
        {
            IReadOnlyList<RogueCardConfig> pool = RogueCardDefaultsProvider.GetAvailableForLevel(levelId);
            var candidates = new List<RogueCardRewardChoiceModel>();
            int poolCount = pool != null ? pool.Count : 0;
            for (int i = 0; i < poolCount; i++)
            {
                RogueCardConfig card = pool[i];
                if (card == null)
                {
                    continue;
                }

                IReadOnlyList<int> availableLevels = RogueCardDefaultsProvider.GetAvailableCardLevelsForLevel(card.Id, levelId);
                int availableLevelCount = availableLevels != null ? availableLevels.Count : 0;
                for (int levelIndex = 0; levelIndex < availableLevelCount; levelIndex++)
                {
                    int targetLevel = availableLevels[levelIndex];
                    if (HasLevelConfig(card, targetLevel) && IsTargetLevelHigherThanOwned(card.Id, targetLevel, ownedCards))
                    {
                        candidates.Add(new RogueCardRewardChoiceModel(card, targetLevel));
                    }
                }
            }

            return CreateRewardOfferFromPool(candidates, choiceCount);
        }

        private static RogueCardRewardOfferModel CreateRewardOfferFromPool(IReadOnlyList<RogueCardRewardChoiceModel> pool, int choiceCount)
        {
            var candidates = pool != null ? new List<RogueCardRewardChoiceModel>(pool) : new List<RogueCardRewardChoiceModel>();
            var choices = new List<RogueCardRewardChoiceModel>();

            choiceCount = Mathf.Max(1, choiceCount);
            while (choices.Count < choiceCount && candidates.Count > 0)
            {
                int index = Random.Range(0, candidates.Count);
                choices.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            return new RogueCardRewardOfferModel(choices);
        }

        private static bool HasLevelConfig(RogueCardConfig card, int targetLevel)
        {
            return card != null && card.GetLevelConfig(targetLevel) != null;
        }

        private static bool IsTargetLevelHigherThanOwned(int cardId, int targetLevel, IReadOnlyList<RogueCardInventoryEntryModel> ownedCards)
        {
            int count = ownedCards != null ? ownedCards.Count : 0;
            for (int i = 0; i < count; i++)
            {
                RogueCardInventoryEntryModel ownedCard = ownedCards[i];
                if (ownedCard != null && ownedCard.CardId == cardId)
                {
                    return targetLevel > ownedCard.Level;
                }
            }

            return true;
        }
    }
}
