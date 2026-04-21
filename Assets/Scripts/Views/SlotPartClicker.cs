using UnityEngine;

/// <summary>
/// 区分老虎机各个可交互部件的类型
/// </summary>
public enum SlotPartType
{
    Body,   // 老虎机主体
    Lever   // 摇杆
}

/// <summary>
/// 挂载在老虎机的子物体（分别挂在“主体”和“拉杆”图像节点上）
/// 利用子物体的 Collider 拦截点击事件，并统一上报给父物体
/// </summary>
public class SlotPartClicker : MonoBehaviour
{
    [Tooltip("选择这个子物体是老虎机的哪个部位")]
    public SlotPartType PartType;

    private SlotMachineView _parentView;

    private void Start()
    {
        // 自动向父级节点寻找 SlotMachineView 核心皮套脚本
        _parentView = GetComponentInParent<SlotMachineView>();
    }

    private void OnMouseDown()
    {
        if (_parentView == null) return;

        // 根据自己的部位身份，告诉父物体是谁被点了
        if (PartType == SlotPartType.Body)
        {
            _parentView.TriggerBodyClick();
        }
        else if (PartType == SlotPartType.Lever)
        {
            _parentView.TriggerLeverClick();
        }
    }
}
