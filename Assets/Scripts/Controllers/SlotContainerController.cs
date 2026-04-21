using UnityEngine;

/// <summary>
/// 容器控制层：纯 C# 类，负责读取自己 Model 里的排列参数，并计算出每一台老虎机应该被摆放的地方。
/// 符合 MVC 守则：本身不挂载 GameObject，只是负责几何测算与计数管理。
/// </summary>
public class SlotContainerController
{
    private readonly SlotContainerModel _model;

    /// <summary>
    /// 初始化容器，将它的数据核心传入
    /// </summary>
    public SlotContainerController(SlotContainerModel model)
    {
        _model = model;
    }

    /// <summary>
    /// 判断当前场上的容器是否满了（已经放不下更多老虎机了）
    /// </summary>
    public bool HasSpace => !_model.IsFull;

    /// <summary>
    /// 添加一台机器，更新容器内的存放数量数据。
    /// </summary>
    public void MachineAdded()
    {
        _model.AddMachineCount();
    }

    /// <summary>
    /// 移除一台机器，腾出空位。
    /// </summary>
    public void MachineRemoved()
    {
        _model.RemoveMachineCount();
    }

    /// <summary>
    /// 核心算法：以序列(Index)为参数，精准算出位于阵列中的第N台机器的三维坐标应处于哪个位置。
    /// 从左到右，从上到下。
    /// </summary>
    /// <param name="index">老虎机当前处于阵列中的序列号 (0代表第一台, 3代表第二行第一台 等)</param>
    /// <returns>最终世界摆放坐标</returns>
    public Vector3 CalculatePositionByIndex(int index)
    {
        // 计算其位于第几行 (Row) 和 第几列 (Col)
        int row = index / _model.ItemsPerRow;
        int col = index % _model.ItemsPerRow;

        // 根据起始点锚点向右、向下推算间距
        float posX = _model.StartX + (col * _model.SpacingX);
        float posY = _model.StartY - (row * _model.SpacingY); // 向下排列，因此是减法

        return new Vector3(posX, posY, 0f);
    }
}
