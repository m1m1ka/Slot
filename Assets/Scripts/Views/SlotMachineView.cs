using UnityEngine;
using System;
using DG.Tweening; // 引入 DOTween

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
    [SerializeField] private Transform _leverTransform; // 存放【拉杆】的子物体，用于播放下拉动画

    [Tooltip("将场景中代表 每一列转轴 的 SlotReelView 拖入此数组")]
    [SerializeField] private SlotReelView[] _reels; // 新增：3个甚至更多垂直的卷轴模块

    [Header("子层级渲染组件引用")]
    [Tooltip("将子物体中带有 SpriteRenderer 或 Canvas 的视觉主体拖入此处（如老虎机壳子、拉杆等）")]
    [SerializeField] private SpriteRenderer[] _childRenderers; // 【引用】用于修改层级的子物体渲染器数组

    // 记录进入焦点特写前自身的原本位置，用于退出焦点时回退
    private Vector3 _originalPosition;

    // --- 【供子物体调用的点击分发中心】 ---
    
    /// <summary>
    /// 被子物体 (SlotPartClicker - Body) 点击触发
    /// </summary>
    public void TriggerBodyClick()
    {
        OnMachineClicked?.Invoke(); // 汇报给 Controller: 主体被点了 (用于进退特写)
    }

    /// <summary>
    /// 被子物体 (SlotPartClicker - Lever) 点击触发
    /// </summary>
    public void TriggerLeverClick()
    {
        OnLeverPulled?.Invoke(); // 汇报给 Controller: 拉杆被点了 (准备扣钱摇奖)
    }

    // --- 【供 Controller 调用的表现方法】 ---

    /// <summary>
    /// 从对象池中取出后进行状态重置，防止带有上一任的残余动画或缩放大小
    /// </summary>
    public void ResetViewState()
    {
        // 杀掉所有残留在自己身上的 DOTween 动画
        DOTween.Kill(transform);
        // 强制恢复默认 1 倍大小，位置和旋转在 Spawn 时已经被覆盖了
        transform.localScale = Vector3.one;

        // 回归默认的场景中间层
        SetSortingLayer(SlotSortingLayer.Mid);
    }

    /// <summary>
    /// 初始化：根据控制器要求，在自己身上动态组装转轮格子
    /// </summary>
    public void SetupReels(int columns, int rows)
    {
        // Controller 告诉我这是 3x3。在此处用对象的池子生成 3根转轴或 9个格子模型。
        Debug.Log($"[View] 老虎机外壳收到了拼接图纸，开始拼接 {columns}x{rows} 的机器...");
    }

    /// <summary>
    /// 设置老虎机所有被引用的身体组件的 SortingLayer
    /// </summary>
    public void SetSortingLayer(SlotSortingLayer layer)
    {
        if (_childRenderers == null || _childRenderers.Length == 0) return;

        string layerName = layer.ToString(); // 枚举名正好对应 Unity 里的 "Background", "Mid", "Top"

        foreach (var renderer in _childRenderers)
        {
            if (renderer != null)
            {
                renderer.sortingLayerName = layerName;
            }
        }
        Debug.Log($"[View] 老虎机层级已被修改为：{layerName}");
    }

    /// <summary>
    /// 当进入 Focus 状态时调用，执行带有缩放和位移的老虎机特写动画
    /// </summary>
    public void MoveToCameraFront(Action onAnimationComplete = null)
    {
        Debug.Log("[View] 播放老虎机飞向屏幕视线的 DOTween 动画...");
        
        // 记录此时的位置，方便后续退回
        _originalPosition = transform.position;

        // 特写模式下，老虎机提升到最前层，遮挡住别的老虎机
        SetSortingLayer(SlotSortingLayer.Top);

        // 杀掉此物体上所有正在跑的DOTween动画，防连点冲突
        DOTween.Kill(transform);

        // 1. 移动到 (0, -1, 0)
        transform.DOMove(new Vector3(0, -1f, 0), 0.5f).SetEase(Ease.OutBack);
        
        // 2. 等比例放大 5 倍
        transform.DOScale(Vector3.one * 5f, 0.5f).SetEase(Ease.OutBack)
            .OnComplete(() => 
            {
                onAnimationComplete?.Invoke();
            });
    }

    /// <summary>
    /// 当退出 Focus 状态时调用，将老虎机还原回原本的阵列位置与尺寸
    /// </summary>
    public void MoveBackToOriginal(Action onAnimationComplete = null)
    {
        Debug.Log("[View] 播放老虎机退回阵列的 DOTween 动画...");

        // 恢复到中间层
        SetSortingLayer(SlotSortingLayer.Mid);

        DOTween.Kill(transform);

        // 1. 移动回刚才记录的原始位置
        transform.DOMove(_originalPosition, 0.5f).SetEase(Ease.OutCubic);
        
        // 2. 等比例缩放回 1 倍
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic)
            .OnComplete(() => 
            {
                onAnimationComplete?.Invoke();
            });
    }

    /// <summary>
    /// 开始转滚轮，并在时间到后将显示画面锁死在 targetGrid 给定的图案ID上
    /// </summary>
    public void PlaySpinAnimation(int[,] targetGrid, Action onSpinFinished)
    {
        Debug.Log("[View] 播放拉杆下拉动画与转轴狂转...");
        
        Sequence seq = DOTween.Sequence();

        // 1. 如果绑定了拉杆 (Lever)节点，让拉杆绕X轴旋转80度，然后弹性回弹
        if (_leverTransform != null)
        {
            // 记录拉杆原本的局部旋转角
            Vector3 originalRot = _leverTransform.localEulerAngles;
            Vector3 targetRot = new Vector3(originalRot.x + 80f, originalRot.y, originalRot.z);

            // 往下压(绕x轴旋转80度)，再利用弹性(OutBack)恢复原本角度
            seq.Append(_leverTransform.DOLocalRotate(targetRot, 0.4f, RotateMode.Fast).SetEase(Ease.InOutSine))
               .Append(_leverTransform.DOLocalRotate(originalRot, 0.3f, RotateMode.Fast).SetEase(Ease.OutBack));
        }

        // 2. 然后，启动所有内嵌卷轴，产生跑马灯无限滚动的错落视觉
        seq.AppendCallback(() => 
        {
            if (_reels == null || _reels.Length == 0)
            {
                // 如果外壳只是个架子没有拖入子物体 _reels 数组，直接延时回调保障逻辑不卡死
                DOVirtual.DelayedCall(2.0f, () => onSpinFinished?.Invoke());
                return;
            }

            int columns = targetGrid.GetLength(0);
            int rows = targetGrid.GetLength(1);

            int completedReelsCount = 0;

            // 逐个激活每一根(列)老虎机转轴
            for (int i = 0; i < _reels.Length; i++)
            {
                if (i >= columns) break; // 如果配置中列数不足，防越界

                // 从 2D 目标矩阵提取出这一纵列要最终落脚的 ID
                int[] targetCol = new int[rows];
                for (int r = 0; r < rows; r++)
                {
                    targetCol[r] = targetGrid[i, r];
                }

                // 给柱子制造“左到右依次停稳”的错落音效与节奏感
                float spinDuration = 1.5f + (i * 0.5f);

                // 命令那根具体的 Reel 自己去跑它的无限滚动动画，并在时间到后强行“咔哒”一声吸准
                _reels[i].SpinReel(targetCol, spinDuration, () => 
                {
                    completedReelsCount++;
                    // 大满贯！如果连最右边的那最后一根卷轴也停靠完毕了，向总控结算回调！
                    if (completedReelsCount == _reels.Length)
                    {
                        onSpinFinished?.Invoke();
                    }
                });
            }
        });
    }

    /// <summary>
    /// 当中奖时调用，在指定行或全屏爆金币特效
    /// </summary>
    public void PlayWinEffect(double winAmount)
    {
        Debug.Log($"[View] 播放撒钱粒子特效！中奖数额展示：{winAmount}");
    }
}
