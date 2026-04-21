using System;

/// <summary>
/// 老虎机当前所处的状态
/// </summary>
public enum SlotState
{
    Idle,       // 空闲待机（等待玩家拉杆）

    Focus,      // 被玩家选中、特写中（可以拉杆了）
    Spinning,   // 正在播放滚轮动画中
    Cooldown,   // 冷却或展示中奖动画中
    Auto        // 全自动挂机状态
}

/// <summary>
/// 纯数据层：记录单台老虎机的数据与状态信息
/// </summary>
public class SlotMachineModel
{
    public int SlotId { get; private set; }
    public int Level { get; private set; }
    public SlotState State { get; private set; }
    
    // 抽象的老虎机结果面盘（无论是1x3还是3x3或者蜂窝，都可以映射为多维数组或一维数组）
    public int[,] CurrentGrid { get; private set; }

    public event Action<SlotState> OnStateChanged;
    public event Action<int[,]> OnGridUpdated;

    public SlotMachineModel(int slotId, int columns, int rows, int initialLevel = 1)
    {
        SlotId = slotId;
        Level = initialLevel;
        State = SlotState.Idle;
        CurrentGrid = new int[columns, rows];
    }

    /// <summary>
    /// 变更状态并广播
    /// </summary>
    public void SetState(SlotState newState)
    {
        if (State == newState) return;
        State = newState;
        OnStateChanged?.Invoke(State);
    }

    /// <summary>
    /// 摇奖结束后记录结果矩阵
    /// </summary>
    public void UpdateGrid(int[,] newGrid)
    {
        CurrentGrid = newGrid;
        OnGridUpdated?.Invoke(CurrentGrid);
    }
}
