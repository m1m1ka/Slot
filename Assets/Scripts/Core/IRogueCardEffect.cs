using Configs;

namespace Core
{
    public interface IRogueCardEffect
    {
        RogueCardEffectType EffectType { get; }
        void Apply(RogueCardEffectConfig effectConfig, RogueCardEffectContext context);
    }
}
