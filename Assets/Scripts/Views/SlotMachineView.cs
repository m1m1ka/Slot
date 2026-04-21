using UnityEngine;
using System;

/// <summary>
/// 视图层：只负责播放老虎机动画、撒钱特效、收集3D碰撞/按键输入。
/// 不具备任何摇奖算法、不具有配置状态。它是纯粹的"皮套"。
/// </summary>
public class SlotMachineView : MonoBehaviour
{
    // --- 【向外抛出的意图事件】 ---
    
    // 当玩家点击到这台机器本体（例如将其拉近特写）
    public event Action OnMachineClicked;
    // 当玩家拉动了摇杆（或者按下了Spin按钮）
    public event Action OnLeverPulled;

    [Header("挂载组件")]
    [SerializeField] private Transform _reelsRoot; // 存放转轴的父节点

    // 如果是3D模型，可以利用射线检测点击或挂载Collider触发 OnMouseDown
    private void OnMouseDown()
    {
        // 只有被聚焦、被特写时才能拉杆，或者如果点击身体就凑近
        // 这里简化为：点它，就是点拉杆
        OnLeverPulled?.Invoke();
    }

    // --- 【供 Controller 调用的表现方法】 ---

    /// <summary>
    /// 初始化：根据控制器要求，在自己身上动态组装转轮格子
    /// </summary>
    public void SetupReels(int columns, int rows)
    {
        // Controller 告诉我这是 3x3。在此处用对象的池子生成 3根转轴或 9个格子模型。
        Debug.Log($"[View] 老虎机外壳收到了拼接图纸，开始拼接 {columns}x{rows} 的机器...");
    }

    /// <summary>
    /// 指定这台机器移动到镜头的前方（供特写使用，或者直接使用 Timeline/DOTween）
    /// </summary>
    public void MoveToCameraFront()
    {
        Debug.Log("[View] 播放老虎机飞向屏幕视线的 DOTween 动画...");
    }

    /// <summary>
    /// 开始转滚轮，并在时间到后将显示画面锁死在 targetGrid 给定的图案ID上
    /// </summary>
    public void PlaySpinAnimation(int[,] targetGrid, Action onSpinFinished)
    {
        Debug.Log("[View] 播放拉杆下拉动画...转轴开始狂转...");
        Debug.Log("[View] 根据 targetGrid 控制每个转轮最后落脚的刻度...");
        
        // 假设这里开了一个协程等了2秒转动，转完了必须要告诉Controller回调：
        // onSpinFinished?.Invoke(); 
    }

    /// <summary>
    /// 当中奖时调用，在指定行或全屏爆金币特效
    /// </summary>
    public void PlayWinEffect(double winAmount)
    {
        Debug.Log($"[View] 播放撒钱粒子特效！中奖数额展示：{winAmount}");
    }
}
