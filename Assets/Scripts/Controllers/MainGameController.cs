using UnityEngine;
using UI; // 引入 UIManager 所在的空间
using Core; // 引入 PoolManager
using System.Collections.Generic;

/// <summary>
/// 控制层：挂载在 MainGamePanel 预制体上，获取同级 View 引用。
/// 包含核心的购买验证、升级验证逻辑。
/// </summary>
[RequireComponent(typeof(MainGamePanel))]
public class MainGameController : MonoBehaviour
{
    // View 引用：与 Controller 同属一个 GameObject
    private MainGamePanel _mainGamePanel;

    // Model 引用
    private PlayerModel _playerModel;

    // 存储当前动态加载的商店项引用
    private readonly List<ShopItemView> _shopItems = new List<ShopItemView>();

    private void Awake()
    {
        // 1. 获取同级挂载的 View 组件（严格禁止在此处直接操作 UI 渲染组件）
        _mainGamePanel = GetComponent<MainGamePanel>();
        
        // 2. 初始化 Model (可以给个初始资金，方便测试)
        // 真实项目中，PlayerModel 通常作为全局单例或是被外界依赖注入
        _playerModel = new PlayerModel(initialCoins: 1000);
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 1. 批量生成购物面板的购买按钮
        LoadShopItems();

        // 2. 将数据层的事件绑定到当前 Controller 的响应方法中
        _playerModel.OnCoinsChanged += HandleCoinsChanged;

        // 3. 初始刷新一次视图
        HandleCoinsChanged(_playerModel.Coins);
    }

    /// <summary>
    /// 从对象池动态批量实例化商店左侧的购买项
    /// </summary>
    private void LoadShopItems()
    {
        if (_mainGamePanel == null || _mainGamePanel.SlotListRoot == null) return;

        // 根据最新规范，从 Resources/UI 目录读取该预制体
        GameObject shopItemPrefab = Resources.Load<GameObject>("UI/ShopItemView");
        if (shopItemPrefab == null)
        {
            Debug.LogError("没有找到 UI/ShopItemView 预制体，无法生成购买列表！");
            return;
        }

        // 假设这里我们要初始化加载5个彩票购买选项
        for (int i = 1; i <= 5; i++)
        {
            // 通过架构自带的核心对象池 (PoolManager) 生成 View
            GameObject itemObj = PoolManager.Instance.Spawn(shopItemPrefab, _mainGamePanel.SlotListRoot);
            ShopItemView itemView = itemObj.GetComponent<ShopItemView>();

            if (itemView != null)
            {
                // 初始化配置数据（后期应从真正的 ConfigManager 拿配置）
                // 目前先造一些假数据：价格分别是 100, 500, 2500, 12500, 62500
                double mockCost = 100 * Mathf.Pow(5, i - 1);
                itemView.SetData(i, $"Scratch Card Lv.{i}", mockCost);

                // 核心：由统一的主 Controller 监听所有个体的购买点击意图
                itemView.OnBuyClicked += HandleBuyRequest;
                
                _shopItems.Add(itemView);
            }
        }
    }

    /// <summary>
    /// 响应任何一个 ShopItemView 提交上来的购买请求
    /// </summary>
    private void HandleBuyRequest(int slotId)
    {
        Debug.Log($"收到请求：尝试购买解锁编号为 {slotId} 的肉鸽彩票。");
        
        // 此处应根据 ID 获取配置中的真实价格，暂且打个 Log
        // RequestBuySlot(slotId, realCost);
    }

    /// <summary>
    /// 纯粹的事件响应方法：当玩家金币数值发生改变时，通知对应的 View 进行渲染更新
    /// </summary>
    private void HandleCoinsChanged(double newCoins)
    {
        if (_mainGamePanel != null)
        {
            _mainGamePanel.UpdateCoinDisplay(newCoins);
        }

        // TODO: 通知左侧 SlotShopItemView 和右侧 UpgradeItemView
        // 刷新它们各自按钮的置灰/高亮状态（通过比较 newCoins 与 价格）
    }

    // -----------------------------------------------------
    // 以下为未来预留的业务逻辑桥梁
    // -----------------------------------------------------

    /// <summary>
    /// 当监听到玩家点击了“购买彩票”按钮时触发
    /// </summary>
    public void RequestBuySlot(int slotId, double cost)
    {
        // 核心判断均在 Controller 处理
        if (_playerModel.ConsumeCoins(cost))
        {
            Debug.Log($"成功花费 {cost} 购买彩票 {slotId}");
            // TODO: 生成一张新的肉鸽彩票实例 (Instantiate View & Setup Model)
        }
        else
        {
            Debug.LogWarning("金币不足，无法购买！");
            // TODO: 通知 View 播放“余额不足”的飘字或震动动画
        }
    }

    private void OnDestroy()
    {
        // 架构规范：务必在销毁时注销所有的事件委托，防止内存泄漏
        if (_playerModel != null)
        {
            _playerModel.OnCoinsChanged -= HandleCoinsChanged;
        }

        // 清理由于事件带来的绑定关系及所有的子 View 对象池回收
        foreach (var item in _shopItems)
        {
            if (item != null)
            {
                item.OnBuyClicked -= HandleBuyRequest;
                if (PoolManager.Instance != null && item.gameObject != null)
                {
                    PoolManager.Instance.Despawn(item.gameObject);
                }
            }
        }
        _shopItems.Clear();
        
        // （由于现在 Controller 挂载在面上，它随面板一起被销毁，不再需要调用 UIManager.ClosePanel，直接释放本级与下级资源即可）
    }
}
