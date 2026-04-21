using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 挂载在场景中空物体上的测试脚手架类。
/// 目标：按下 G 键，遵循 MVC 架构生成一台老虎机。
/// </summary>
public class SlotTest : MonoBehaviour
{
    // 模拟玩家的数据模型，保存金币数量等
    private PlayerModel _playerModel;
    
    // 用于管理被测试生出来的阵列位置状态 (也就是老虎机的挂载板)
    private SlotContainerController _containerController;

    // 用于管理和持有所有已被实例化的老虎机 Controller
    private List<SlotMachineController> _slotMachines = new List<SlotMachineController>();

    private void Start()
    {
        // 1. 初始化一个拥有足够金币的纯数据玩家对象
        _playerModel = new PlayerModel(initialCoins: 10000);
        
        // 2. 初始化老虎机容器（控制层与模型数据）。设定从左到右，从上到下。间距为 3f。每行放 3 个。
        SlotContainerModel containerModel = new SlotContainerModel(
            itemsPerRow: 3, 
            maxRows: 3, 
            spacingX: 3f, 
            spacingY: 3f, 
            startX: -3f, 
            startY: 3f);
            
        _containerController = new SlotContainerController(containerModel);

        Debug.Log("[SlotTest] 玩家与老虎机容器排版网格初始化完毕，当前金额: " + _playerModel.Coins);
    }

    private void Update()
    {
        // 2. 监听测试按键 G: 生成型号为 1 的老虎机
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateSlotMachine();
        }

        // 3. 监听测试按键 H: 模拟玩家“出售/销毁”最早生成的那台老虎机
        if (Input.GetKeyDown(KeyCode.H))
        {
            SellOldestSlotMachine();
        }

        // 4. 监听测试按键 J: 销毁刚刚生成的最新那台老虎机实例 (演示针对实例的精确打击)
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (_slotMachines.Count > 0)
            {
                // 获取最后生成的那个实例的唯一 ID（UID）
                string targetInstanceId = _slotMachines[_slotMachines.Count - 1].InstanceId;
                SellSlotMachineByInstance(targetInstanceId);
            }
        }
    }

    /// <summary>
    /// 模拟销毁指定"唯一实例ID (InstanceId)"的老虎机
    /// 无论世界里有多少台型号为 1 的机器，这个 InstanceId 全局唯一！
    /// </summary>
    private void SellSlotMachineByInstance(string targetInstanceId)
    {
        // 根据唯一实例ID查找对应的控制器
        SlotMachineController targetSlot = _slotMachines.Find(slot => slot.InstanceId == targetInstanceId);

        if (targetSlot != null)
        {
            // 1. 调用 Controller 的清理函数
            targetSlot.Despawn();
            
            // 2. 从列表中安全移除引用
            _slotMachines.Remove(targetSlot);
            
            // 3. 告诉容器控制器，腾出了一个空位
            _containerController.MachineRemoved();
            
            Debug.Log($"[SlotTest] 精确销毁实例ID为 {targetInstanceId} 的老虎机！(型号: {targetSlot.SlotId}) 当前场上剩余: {_slotMachines.Count}");
        }
        else
        {
            Debug.LogWarning($"[SlotTest] 找不到实例ID为 {targetInstanceId} 的机器。");
        }
    }

    /// <summary>
    /// 模拟真正的销毁行为（出售老虎机），以释放内存
    /// </summary>
    private void SellOldestSlotMachine()
    {
        if (_slotMachines.Count == 0) return;

        // 取出列表中最老的一台
        SlotMachineController slotToDestroy = _slotMachines[0];

        // 1. 调用 Controller 的清理函数：断开 Model/View 的事件监听，把 View 丢回对象池
        slotToDestroy.Despawn();

        // 2. 把 Controller 从强引用列表中移除，失去所有引用后，GC 就会回收它所在的内存！
        _slotMachines.RemoveAt(0);
        
        // 3. 告诉容器控制器，腾出了一个空位
        _containerController.MachineRemoved();

        Debug.Log($"[SlotTest] 成功出售并销毁一台老虎机！当前场上剩余: {_slotMachines.Count}");
    }

    /// <summary>
    /// 根据 MVC 框架创建一台新的老虎机实体并交由容器排版
    /// </summary>
    private void GenerateSlotMachine()
    {
        // 0. 判断是否满了
        if (!_containerController.HasSpace)
        {
            Debug.LogWarning("[SlotTest] 容器网格已经放满 9 台（3x3）了！无法放入。");
            return;
        }

        // 模拟每次都购买“配置表里 ID/型号为 1 的老虎机”
        int configSlotId = 1;

        // 根据文档约束，Controller 作为桥梁，由我们 (GamePlayManager 或 Test 脚本) 来负责构建并塞入依赖
        SlotMachineController newSlot = new SlotMachineController(configSlotId, _playerModel);

        // -- 【核心：由容器大脑计算出下一个摆放的物理坐标！】 --
        // 新机器也就是排在 _slotMachines 现在的末尾
        Vector3 spawnPos = _containerController.CalculatePositionByIndex(_slotMachines.Count);

        // Controller 内部会根据传入的坐标去生成自己的 View 蒙皮，并让 Model、View 以及其自身形成闭环
        newSlot.BindAndSpawnView(spawnPos);

        // 记录引用，防止被 GC 或者方便后续做离线挂机后台刷新等操作
        _slotMachines.Add(newSlot);
        
        // 告诉容器：场上多增加了一台老虎机，更新它内部数量
        _containerController.MachineAdded();

        Debug.Log($"[SlotTest] 生成了一台“型号 {configSlotId}”的老虎机！放置在了坐标: {spawnPos}。 它的UID是: {newSlot.InstanceId}");
    }
}
