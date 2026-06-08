namespace Core
{
    public enum TutorialEventType
    {
        None = 0,
        ScratchCardBought = 1,
        ScratchCardClicked = 2,
        ScratchCardCompleted = 3,
        SettlementButtonClicked = 4,
        RewardClaimed = 5,
        CannotAffordAnyScratchCardAfterSettlement = 6,
        TutorialCompleted = 7,
        FirstLevelPassed = 8,
        ScratchCardRewardButtonClicked = 9,
        SecondLevelPassed = 10,
        RogueCardRewardButtonClicked = 11
    }

    public struct TutorialEvent : IEvent
    {
        public TutorialEventType Type { get; }
        public int IntValue { get; }

        public TutorialEvent(TutorialEventType type, int intValue = 0)
        {
            Type = type;
            IntValue = intValue;
        }
    }
}
