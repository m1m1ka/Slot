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
            var candidates = new List<RogueCardConfig>(pool);
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
    }
}
