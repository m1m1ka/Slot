using System;
using Configs;

public class LevelProgressModel
{
    public int LevelId { get; }
    public string LevelName { get; }
    public double RequiredCoins { get; }
    public int ScratchCardPurchaseLimit { get; }
    public int ScratchCardPurchasesUsed { get; private set; }
    public bool IsPassed { get; private set; }

    public int RemainingScratchCardPurchases =>
        Math.Max(0, ScratchCardPurchaseLimit - ScratchCardPurchasesUsed);

    public bool CanPurchaseScratchCard =>
        !IsPassed && ScratchCardPurchasesUsed < ScratchCardPurchaseLimit;

    public event Action<int, int> OnScratchCardPurchasesChanged;
    public event Action<bool> OnPassStateChanged;

    public LevelProgressModel(LevelConfig config)
    {
        LevelId = config != null ? config.Id : 0;
        LevelName = config != null ? config.Name : "Unknown Level";
        RequiredCoins = config != null ? config.RequiredCoins : 0;
        ScratchCardPurchaseLimit = Math.Max(0, config != null ? config.ScratchCardPurchaseLimit : 0);
    }

    public bool TryRecordScratchCardPurchase()
    {
        if (!CanPurchaseScratchCard)
        {
            return false;
        }

        ScratchCardPurchasesUsed++;
        OnScratchCardPurchasesChanged?.Invoke(ScratchCardPurchasesUsed, ScratchCardPurchaseLimit);
        return true;
    }

    public bool EvaluatePass(double currentCoins)
    {
        bool passed = currentCoins >= RequiredCoins;
        if (IsPassed == passed)
        {
            return IsPassed;
        }

        IsPassed = passed;
        OnPassStateChanged?.Invoke(IsPassed);
        return IsPassed;
    }
}
