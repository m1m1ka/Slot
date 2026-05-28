public class RogueCardEffectContext
{
    public PlayerContext PlayerContext { get; }
    public GameSession GameSession { get; }
    public int SourceRogueCardId { get; }

    public RogueCardEffectContext(PlayerContext playerContext, GameSession gameSession, int sourceRogueCardId = 0)
    {
        PlayerContext = playerContext;
        GameSession = gameSession;
        SourceRogueCardId = sourceRogueCardId;
    }

    public RogueCardEffectContext WithSourceRogueCard(int sourceRogueCardId)
    {
        return new RogueCardEffectContext(PlayerContext, GameSession, sourceRogueCardId);
    }
}
