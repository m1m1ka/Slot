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

    public void StartNewRun()
    {
        IsRunActive = true;
    }

    public void Reset()
    {
        IsRunActive = false;
    }
}
