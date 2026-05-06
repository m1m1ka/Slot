namespace Core
{
    /// <summary>
    /// 刮刮卡结算策略接口。
    /// </summary>
    public interface IScratchSettlementEvaluator
    {
        ScratchSettlementResult Evaluate(ScratchCardModel model);
    }
}
