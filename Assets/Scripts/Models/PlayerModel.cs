using System;

/// <summary>
/// 纯数据层：负责存储玩家资产状态，如金币，以及触发数值变更事件。
/// 严格遵守MVC：不继承MonoBehaviour，不引用任何UI组件。
/// </summary>
public class PlayerModel
{
    // 使用 double 是因为挂机游戏的数值在中后期会非常庞大
    public double Coins { get; private set; }

    // 当金币数量发生变化时，抛出此事件，通知 Controller 或 View 刷新
    public event Action<double> OnCoinsChanged;

    public PlayerModel(double initialCoins = 0)
    {
        Coins = initialCoins;
    }

    /// <summary>
    /// 增加金币
    /// </summary>
    public void AddCoins(double amount)
    {
        if (amount <= 0) return;
        
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    /// <summary>
    /// 消费金币
    /// </summary>
    /// <returns>如果金币足够并成功扣除返回 true，否则返回 false</returns>
    public bool ConsumeCoins(double amount)
    {
        if (amount < 0 || Coins < amount)
        {
            return false;
        }

        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }
}
