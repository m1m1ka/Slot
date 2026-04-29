using System.Collections.Generic;
using UnityEngine;

public class RogueCardRunModifierModel
{
    private readonly Dictionary<int, int> _patternBaseScoreBonuses = new Dictionary<int, int>();
    private readonly Dictionary<int, double> _patternWeightBonuses = new Dictionary<int, double>();
    private readonly Dictionary<int, double> _patternScratchCardMultiplierBonuses = new Dictionary<int, double>();
    private readonly List<DynamicScratchPatternPoolEntryModel> _addedScratchPatterns = new List<DynamicScratchPatternPoolEntryModel>();

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

    public void AddPatternWeightBonus(int patternId, double bonus)
    {
        if (patternId <= 0 || bonus == 0d)
        {
            return;
        }

        if (!_patternWeightBonuses.ContainsKey(patternId))
        {
            _patternWeightBonuses[patternId] = 0d;
        }

        _patternWeightBonuses[patternId] += bonus;
    }

    public double GetPatternWeightBonus(int patternId)
    {
        return _patternWeightBonuses.TryGetValue(patternId, out double bonus) ? bonus : 0d;
    }

    public float GetEffectivePatternWeight(int patternId, int baseWeight)
    {
        return GetEffectivePatternWeight(patternId, (float)baseWeight);
    }

    public float GetEffectivePatternWeight(int patternId, float baseWeight)
    {
        return Mathf.Max(0f, baseWeight + (float)GetPatternWeightBonus(patternId));
    }

    public void AddPatternScratchCardMultiplierBonus(int patternId, double bonus)
    {
        if (patternId <= 0 || bonus <= 0d)
        {
            return;
        }

        if (!_patternScratchCardMultiplierBonuses.ContainsKey(patternId))
        {
            _patternScratchCardMultiplierBonuses[patternId] = 0d;
        }

        _patternScratchCardMultiplierBonuses[patternId] += bonus;
    }

    public double GetPatternScratchCardMultiplierBonus(int patternId)
    {
        return _patternScratchCardMultiplierBonuses.TryGetValue(patternId, out double bonus) ? bonus : 0d;
    }

    public void AddScratchPatternToPool(int patternId, float weight, IReadOnlyList<int> cardTypeIds = null)
    {
        if (patternId <= 0 || weight <= 0f)
        {
            return;
        }

        _addedScratchPatterns.Add(new DynamicScratchPatternPoolEntryModel(patternId, weight, cardTypeIds));
    }

    public List<DynamicScratchPatternPoolEntryModel> GetAddedScratchPatternsForCardType(int cardTypeId)
    {
        var results = new List<DynamicScratchPatternPoolEntryModel>();
        for (int i = 0; i < _addedScratchPatterns.Count; i++)
        {
            DynamicScratchPatternPoolEntryModel entry = _addedScratchPatterns[i];
            if (entry != null && entry.AppliesToCardType(cardTypeId))
            {
                results.Add(entry);
            }
        }

        return results;
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
        _patternWeightBonuses.Clear();
        _patternScratchCardMultiplierBonuses.Clear();
        _addedScratchPatterns.Clear();
        ScratchCardMultiplierBonus = 0d;
    }
}
