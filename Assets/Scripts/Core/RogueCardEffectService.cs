using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Core
{
    public class RogueCardEffectService
    {
        private readonly Dictionary<RogueCardEffectType, IRogueCardEffect> _effects = new Dictionary<RogueCardEffectType, IRogueCardEffect>();

        public RogueCardEffectService()
        {
            Register(new NoOpRogueCardEffect());
        }

        public void Register(IRogueCardEffect effect)
        {
            if (effect == null)
            {
                return;
            }

            _effects[effect.EffectType] = effect;
        }

        public void ApplyCard(RogueCardConfig cardConfig, RogueCardEffectContext context)
        {
            if (cardConfig == null || cardConfig.Effects == null)
            {
                return;
            }

            for (int i = 0; i < cardConfig.Effects.Count; i++)
            {
                RogueCardEffectConfig effectConfig = cardConfig.Effects[i];
                if (effectConfig == null)
                {
                    continue;
                }

                if (_effects.TryGetValue(effectConfig.EffectType, out IRogueCardEffect effect))
                {
                    effect.Apply(effectConfig, context);
                    continue;
                }

                Debug.Log($"[RogueCardEffectService] Effect '{effectConfig.EffectType}' is registered as data, but no runtime handler exists yet.");
            }
        }

        private class NoOpRogueCardEffect : IRogueCardEffect
        {
            public RogueCardEffectType EffectType => RogueCardEffectType.None;

            public void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context)
            {
            }
        }
    }
}
