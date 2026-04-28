using System.Collections.Generic;

public class RogueCardRunModifierModel
{
    private readonly Dictionary<int, int> _patternBaseScoreBonuses = new Dictionary<int, int>();

    public double ScratchCardMultiplierBonus { get; private set; }
    public double ScratchCardMultiplier => 1d + ScratchCardMultiplierBonus;

    public void AddPatternBaseScoreBonus(int patternId, int bonus)
    {
        if (patternId <= 0 || bonus == 0)
        {
            return;
        }

        if (!_patternBaseScoreBonuses.ContainsKey(patternId))
        {
            _patternBaseScoreBonuses[patternId] = 0;
        }

        _patternBaseScoreBonuses[patternId] += bonus;
    }

    public int GetPatternBaseScoreBonus(int patternId)
    {
        return _patternBaseScoreBonuses.TryGetValue(patternId, out int bonus) ? bonus : 0;
    }

    public bool HasPatternBaseScoreBonus(int patternId)
    {
        return GetPatternBaseScoreBonus(patternId) != 0;
    }

    public void AddScratchCardMultiplierBonus(double bonus)
    {
        if (bonus <= 0d)
        {
            return;
        }

        ScratchCardMultiplierBonus += bonus;
    }

    public void Clear()
    {
        _patternBaseScoreBonuses.Clear();
        ScratchCardMultiplierBonus = 0d;
    }
}
