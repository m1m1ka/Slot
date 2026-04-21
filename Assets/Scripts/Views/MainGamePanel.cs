using UnityEngine;
using TMPro; // 推荐使用 TextMeshPro 来显示文本
using System;
using UI; // 引入你已有的 UI 命名空间，包含 UIPanel 基类
/// <summary>
/// 视图层：只负责呈现UI并收集用户点击输入，不包含任何业务逻辑判断。
/// 注意：这里继承自你已有的 UIPanel 基类。
/// </summary>
public class MainGamePanel : UIPanel
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _coinText;

    [Header("Shop & Upgrade Lists Root")]
    [SerializeField] private Transform _slotListRoot;       // 挂载左侧购买老虎机按钮的父节点
    [SerializeField] private Transform _upgradeListRoot;    // 挂载右侧升级按钮的父节点

    // 对外暴露列表的父节点供 Controller 挂载子物体引用
    public Transform SlotListRoot => _slotListRoot;
    public Transform UpgradeListRoot => _upgradeListRoot;

    // 如果面板上有通用的点击按钮，可以通过事件向Controller抛出
    // public event Action OnSomeGlobalButtonClicked;

    /// <summary>
    /// 供 Controller 调用，用于刷新金币显示的表现
    /// </summary>
    /// <param name="currentCoins">玩家当前金币数量</param>
    public void UpdateCoinDisplay(double currentCoins)
    {
        if (_coinText != null)
        {
            // 增量游戏通常需要格式化庞大数字 (如 1.2K, 3.5M)
            // 这里暂且使用字符串格式化展示
            _coinText.text = currentCoins.ToString("N0");
        }
    }

    // 后续可以增加动态实例化子项的方法
    // public void AddSlotShopItem(SlotShopItemView itemPrefab) { ... }
}
