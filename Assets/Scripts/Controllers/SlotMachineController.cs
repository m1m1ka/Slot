using UnityEngine;
using Core; // 引入 PoolManager
using System;

/// <summary>
/// 单台老虎机的微型“大脑”。每造一台老虎机就实例化一个此类保管状态。
/// 并非挂载在 GameObject 里，而是纯 C# 管理者，可以在此实现离线计时器、调用测算器等。
/// </summary>
public class SlotMachineController
{
    // MVC的骨架依赖
    private readonly SlotMachineModel _myModel;
    private SlotMachineView _myView;

    // --- 【策略模式与配置：负责各种千奇百怪的老虎机数值】 ---
    private readonly SlotMachineConfig _myConfig; 
    private readonly ISlotEvaluator _myEvaluator;   

    // 持有一个全局经济对象引用，方便扣钱和加钱
    private readonly PlayerModel _playerModel;     

    // 暴露老虎机唯一ID，供外部查找与管理使用
    public string InstanceId => _myModel.InstanceId;
    public int SlotId => _myModel.SlotId;

    /// <summary>
    /// 当一被实例化时，这台老虎机会在内存中形成状态闭环
    /// </summary>
    public SlotMachineController(int slotId, PlayerModel playerModel)
    {
        _playerModel = playerModel;
        
        // 1. 根据 ID 提取策划表格里的说明书（长宽，是不是消除玩法，出金率等）
        // _myConfig = ConfigManager.GetSlotConfig(slotId);
        _myConfig = new SlotMachineConfig { Columns = 3, Rows = 3, Cost = 10, Rule = "LineMatch" }; 

        // 2. 根据说明书上的“算分规则”，从兵器库挑一把专门配对该老虎机用算分器
        // _myEvaluator = EvaluatorFactory.Create(_myConfig.Rule);
        _myEvaluator = new MockLineMatchEvaluator();

        // 3. 构建只有这台机器才独享的记忆：模型数据
        _myModel = new SlotMachineModel(slotId, _myConfig.Columns, _myConfig.Rows);
        _myModel.OnStateChanged += HandleStateChanged;
    }

    /// <summary>
    /// 让控制器给这台逻辑“套上皮套”。这层皮套可能是购买时临时放到场景里的，也可能中途被回收了。
    /// （当这台老虎机被玩家滑出屏幕时，可以把 View 收掉。后台 Controller 还活着继续给 _playerModel 塞钱）
    /// </summary>
    public void BindAndSpawnView(Vector3 spawnPosition)
    {
        // 从对象池拿个 3D 模型过来做 View 表现层
        GameObject prefab = Resources.Load<GameObject>("Models/SlotMachineViewPrefab"); // 假设这是老虎机的样子
        if (prefab == null) return;
        
        GameObject viewObj = PoolManager.Instance.Spawn(prefab, spawnPosition, Quaternion.identity);
        _myView = viewObj.GetComponent<SlotMachineView>();

        if (_myView != null)
        {
            // 从对象池拿出来的二手壳子，可能有上一个哥们放大(x5)的残留。由大脑命令它立即擦干净内存，重置默认外形
            _myView.ResetViewState();

            // 给这个壳子安装它应有的长宽尺寸和拼接
            _myView.SetupReels(_myConfig.Columns, _myConfig.Rows);

            // 监听玩家在这台老虎机上的操作
            _myView.OnMachineClicked += HandleMachineClicked;
            _myView.OnLeverPulled += RequestSpin;
        }
    }

    /// <summary>
    /// 销毁逻辑，断开连接，拆卸皮套
    /// </summary>
    public void Despawn()
    {
        if (_myModel != null) _myModel.OnStateChanged -= HandleStateChanged;
        if (_myView != null)
        {
            _myView.OnMachineClicked -= HandleMachineClicked;
            _myView.OnLeverPulled -= RequestSpin;
            PoolManager.Instance.Despawn(_myView.gameObject);
        }
    }

    // --- 【核心业务：响应玩家在这台机器上的点击与摇奖】 ---

    private void HandleMachineClicked()
    {
        // 玩家点击了老虎机触发该事件。根据 MVC，我们在这里判断状态并修改 Model
        // 只有当前处于 Idle 空闲状态时，才可以被聚焦特写
        if (_myModel.State == SlotState.Idle && _myView != null)
        {
            _myModel.SetState(SlotState.Focus);
        }
        else if (_myModel.State == SlotState.Focus)
        {
            // 已经是特写状态，再点一次老虎机自身（而不是拉杆），则退回为Idle状态归位
            _myModel.SetState(SlotState.Idle);
        }
    }

    /// <summary>
    /// 【大脑算账中心】：由于拉动了杆子，开始测算老虎机会否中奖
    /// </summary>
    private void RequestSpin()
    {
        // 1. 判断是否被占用或资金不足 (只有当前位于Focus或者离线挂机状态时，拉杆才有效！)
        if (_myModel.State != SlotState.Focus && _myModel.State != SlotState.Auto) return;
        
        if (!_playerModel.ConsumeCoins(_myConfig.Cost))
        {
            Debug.Log("金币不够拉这台机器的起步价！");
            return;
        }

        // 2. 切状态到摇奖中，防止玩家多拉几次
        _myModel.SetState(SlotState.Spinning);

        // 3. 【测算环节】：假数据随机个 3x3 矩阵，在这一瞬间代码其实已经知道玩家中没中、中了多少钱
        int[,] randomGrid = new int[_myConfig.Columns, _myConfig.Rows];
        for (int i = 0; i < _myConfig.Columns; i++)
            for (int j = 0; j < _myConfig.Rows; j++)
                randomGrid[i, j] = UnityEngine.Random.Range(1, 10);
        
        _myModel.UpdateGrid(randomGrid);
        
        // 算账！把生成的格子丢给这个机器专属的 RuleEvaluator 算出奖金（比如 3个7 就是 500块）
        double winAmount = _myEvaluator.Evaluate(randomGrid, _myConfig);

        // 4. 【如果 View 还在场景里（也就是玩家正在盯着它看），我们就需要让 View 配合表演演戏：】
        if (_myView != null)
        {
            // 给它最终的格子目标，让它狂转，并等它转完（靠回调）再发奖金。
            _myView.PlaySpinAnimation(randomGrid, () => 
            {
                HandleSpinFinished(winAmount);
            });
        }
        else
        {
            // 【神级优化】：如果 View 被关了回收了（说明玩家不在看这页了），且机器是 Auto自动摇奖 模式
            // 那咱们甚至连播动画都不播了，瞬间打款，完成后台全自动挂机刷钱功能！
            HandleSpinFinished(winAmount);
        }
    }

    /// <summary>
    /// 动画结束后的金币分发与冷却处理
    /// </summary>
    private void HandleSpinFinished(double totalWin)
    {
        if (totalWin > 0)
        {
            _playerModel.AddCoins(totalWin);
            
            if (_myView != null)
                _myView.PlayWinEffect(totalWin);
        }

        // 摇奖完毕后临时保留在 Focus 特写状态，等待玩家点击主体退出，或者进行下一次拉杆
        _myModel.SetState(SlotState.Focus);
    }
    
    // --- 【纯数据变化监听】 ---
    private SlotState _previousState = SlotState.Idle; // 记录上一次的状态，防止相同动画在状态流转中被重复触发

    private void HandleStateChanged(SlotState newState)
    {
        // Model的数据变了，Controller 根据新状态同步指挥 View 表演
        Debug.Log($"[Controller] 这台ID:{_myModel.SlotId} 的老虎机状态变为：{newState}");

        if (newState == SlotState.Focus && _myView != null && _previousState == SlotState.Idle)
        {
            // 只有从 【Idle首次进入Focus】 时，才指挥视图执行放大占屏动画
            _myView.MoveToCameraFront(() => 
            {
                Debug.Log("[Controller] 老虎机进入焦点特写完毕，可以开始拉摇杆了。");
            });
        }
        else if (newState == SlotState.Idle && _myView != null && _previousState == SlotState.Focus)
        {
            // 退回 Idle 状态时，命令视图还原排版位置
            _myView.MoveBackToOriginal();
        }

        // 更新历史状态用于下一次判断
        _previousState = newState;
    }
}

// -------------------------------------------------------------
// [策略模式打底接口]
// 这些通常放在长长的 Core/Evaluators 目录下。方便后期增加 N种奇怪的算结算方法
public interface ISlotEvaluator { double Evaluate(int[,] grid, SlotMachineConfig config); }
public class MockLineMatchEvaluator : ISlotEvaluator { public double Evaluate(int[,] grid, SlotMachineConfig config) { return 0; } }
public struct SlotMachineConfig { public int Columns; public int Rows; public double Cost; public string Rule; }
