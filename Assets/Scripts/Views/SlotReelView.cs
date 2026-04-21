using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// 负责控制单列转轴（Reel）的 2D 无限滚动与最终对齐。
/// </summary>
public class SlotReelView : MonoBehaviour
{
    [Header("格子节点设定")]
    [Tooltip("这根柱子里所有的格子节点（比如3行的老虎机，你至少需要4个节点来做循环替补。")]
    public Transform[] symbolNodes; 
    
    [Tooltip("每个格子在Y轴的标准间距")]
    public float symbolHeight = 2.0f; 

    [Tooltip("模拟速度：每秒转过多少个格子")]
    public float spinSpeed = 20f;     

    /// <summary>
    /// 开始单列自身的跑马灯狂转动画
    /// </summary>
    /// <param name="targetCol">Controller计算好的这一列的目标图案数组(如 {3,5,7})</param>
    /// <param name="duration">该列转动总时长</param>
    /// <param name="onComplete">该列完全停稳之后的回调</param>
    public void SpinReel(int[] targetCol, float duration, Action onComplete)
    {
        // 防手抖：杀掉自己身上现有的 DOTween 任务
        DOTween.Kill(this);
        
        float totalMoveNodes = spinSpeed * duration; // 计算在这段时间里总共会滚过多少个格子
        float currentMove = 0f;

        // 利用 DOVirtual.Float 做一个平滑的纯数值计算过度，代替 MonoBehaviour 的 Update
        DOVirtual.Float(0, totalMoveNodes, duration, value =>
        {
            float delta = value - currentMove;
            currentMove = value;

            foreach (var node in symbolNodes)
            {
                // 1. 将所有格子往下移
                node.localPosition += Vector3.down * delta * symbolHeight;

                // 2. 核心障眼法（越界循环）：当格子掉出了机器底部的遮罩 (假设 -1.5h 是下边缘位置)
                if (node.localPosition.y <= -symbolHeight * 1.5f)
                {
                    // 瞬间把它拽回最顶部排队
                    node.localPosition += new Vector3(0, symbolHeight * symbolNodes.Length, 0);
                    
                    // TODO: 【视觉虚相】这里能获取 node 上的 SpriteRenderer，把它的图片随机换成其它图标，做到"跑马灯"乱转的效果
                    // var renderer = node.GetComponent<SpriteRenderer>(); 
                    // renderer.sprite = GetRandomSprite(); 
                }
            }
        })
        .SetEase(Ease.InOutSine) // 让速度从慢启动，到极快，再到慢慢降速
        .OnComplete(() =>
        {
            // 3. 【决胜时刻】：当虚拟滚动终于停下后，准备对落脚点进行校准。
            // 因为被频繁拽上拖下，数组的顺序已经乱了。我们要根据它们此时 Y 轴的高度从上到下排个序。
            Array.Sort(symbolNodes, (a, b) => b.localPosition.y.CompareTo(a.localPosition.y));

            // TODO: 【真正换图】把排好序的头三个节点（即此时屏幕上展示的3行），强制更换为你从 Controller 传来的必定中奖图！
            // for(int i=0; i < targetCol.Length; i++)
            // { symbolNodes[i].GetComponent<SpriteRenderer>().sprite = GetSpriteById(targetCol[i]); }

            // 4. 机械阻尼/果冻回弹：对所有的格子做一次严丝合缝的吸附补间，消除小数误差，并加上“咔哒”一声的惯性。
            Sequence snapSeq = DOTween.Sequence();
            for (int i = 0; i < symbolNodes.Length; i++)
            {
                // 用数学公式对每个格子算出他应该占据的标准高度中心点 (例如 1.0, 0, -1.0)
                float targetY = (1 - i) * symbolHeight;
                snapSeq.Join(symbolNodes[i].DOLocalMoveY(targetY, 0.3f).SetEase(Ease.OutBack));
            }

            // 对位彻底完毕，通知外层。
            snapSeq.OnComplete(() => onComplete?.Invoke());
            
        }).SetId(this); // 给动画贴个自己的标签，方便安全销毁
    }
}
