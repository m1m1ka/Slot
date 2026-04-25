public class RogueCardEffectContext
{
    public PlayerContext PlayerContext { get; }
    public GameSession GameSession { get; }

    public RogueCardEffectContext(PlayerContext playerContext, GameSession gameSession)
    {
        PlayerContext = playerContext;
        GameSession = gameSession;
    }
}
