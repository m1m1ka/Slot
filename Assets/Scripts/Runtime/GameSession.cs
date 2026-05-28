/// <summary>
/// 单局或单轮运行时会话数据。
/// 当前先保留为轻量骨架，后续可逐步接入关卡、构筑、战斗或流程状态。
/// </summary>
public class GameSession
{
    /// <summary>
    /// 当前是否已经开始一局正式流程。
    /// </summary>
    public bool IsRunActive { get; private set; }
    public LevelProgressModel CurrentLevel { get; private set; }
    public RogueCardRunModifierModel RunModifiers { get; } = new RogueCardRunModifierModel();

    public void StartNewRun()
    {
        RunModifiers.ClearAll();
        IsRunActive = true;
    }

    public void StartLevel(Configs.LevelConfig levelConfig)
    {
        CurrentLevel = new LevelProgressModel(levelConfig);
        IsRunActive = true;
    }

    public bool StartNextLevel()
    {
        if (CurrentLevel == null)
        {
            StartLevel(Core.LevelDefaultsProvider.GetFirstLevel());
            return CurrentLevel != null;
        }

        Configs.LevelConfig nextLevelConfig = Core.LevelDefaultsProvider.GetNextLevel(CurrentLevel.LevelId);
        if (nextLevelConfig == null)
        {
            return false;
        }

        StartLevel(nextLevelConfig);
        return true;
    }

    public void Reset()
    {
        IsRunActive = false;
        CurrentLevel = null;
        RunModifiers.ClearAll();
    }
}
