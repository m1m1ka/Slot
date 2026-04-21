using System;

/// <summary>
/// 纯数据层：老虎机排布容器的模型，负责存储所有与阵列排列相关的数据
/// </summary>
public class SlotContainerModel
{
    // --- 排列参数 ---
    public int ItemsPerRow { get; private set; } // 每行最多几个
    public int MaxRows { get; private set; }     // 最多几行
    
    // --- 间距参数 ---
    public float SpacingX { get; private set; }  // 横向间距
    public float SpacingY { get; private set; }  // 纵向间距
    
    // --- 起始锚点位置（左上角第一台老虎机的位置） ---
    public float StartX { get; private set; }
    public float StartY { get; private set; }

    // --- 状态数据 ---
    public int CurrentCount { get; private set; } // 当前已有的老虎机数量

    public event Action<int> OnCountChanged;

    public SlotContainerModel(int itemsPerRow = 3, int maxRows = 3, float spacingX = 3f, float spacingY = 3f, float startX = -3f, float startY = 3f)
    {
        ItemsPerRow = itemsPerRow;
        MaxRows = maxRows;
        SpacingX = spacingX;
        SpacingY = spacingY;
        StartX = startX;
        StartY = startY;
        CurrentCount = 0;
    }

    public bool IsFull => CurrentCount >= ItemsPerRow * MaxRows;

    public void AddMachineCount()
    {
        if (IsFull) return;
        CurrentCount++;
        OnCountChanged?.Invoke(CurrentCount);
    }

    public void RemoveMachineCount()
    {
        if (CurrentCount <= 0) return;
        CurrentCount--;
        OnCountChanged?.Invoke(CurrentCount);
    }
}
