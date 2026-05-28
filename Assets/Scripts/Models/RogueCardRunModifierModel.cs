using System.Collections.Generic;
using UnityEngine;

public class RogueCardRunModifierModel
{
    private readonly Dictionary<int, int> _patternBaseScoreBonuses = new Dictionary<int, int>();
    private readonly Dictionary<int, int> _permanentPatternBaseScoreBonuses = new Dictionary<int, int>();
    private readonly Dictionary<int, double> _patternWeightBonuses = new Dictionary<int, double>();
    private readonly Dictionary<int, double> _patternScratchCardMultiplierBonuses = new Dictionary<int, double>();
    private readonly Dictionary<int, List<PatternBaseScoreGrowthRule>> _patternBaseScoreGrowthRulesByScoredPattern = new Dictionary<int, List<PatternBaseScoreGrowthRule>>();
    private readonly Dictionary<int, List<GiantPatternRule>> _giantPatternRulesByPattern = new Dictionary<int, List<GiantPatternRule>>();
    private readonly Dictionary<int, List<JokerPatternRule>> _jokerPatternRulesByPattern = new Dictionary<int, List<JokerPatternRule>>();
    private readonly Dictionary<int, List<RiskMultiplierPatternRule>> _riskMultiplierRulesByPattern = new Dictionary<int, List<RiskMultiplierPatternRule>>();
    private readonly Dictionary<int, int> _jokerGoodFaceScoreOverrides = new Dictionary<int, int>();
    private readonly Dictionary<int, HashSet<int>> _patternBaseScoreSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> _permanentPatternBaseScoreSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> _patternBaseScoreGrowthSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> _patternWeightSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> _patternScratchCardMultiplierSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> _giantPatternSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> _jokerPatternSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> _riskMultiplierPatternSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly Dictionary<int, HashSet<int>> _addedScratchPatternSourceCardIds = new Dictionary<int, HashSet<int>>();
    private readonly HashSet<int> _scratchCardMultiplierSourceCardIds = new HashSet<int>();
    private readonly HashSet<int> _settlementScoreBonusSourceCardIds = new HashSet<int>();
    private readonly HashSet<int> _settlementMultiplierBonusSourceCardIds = new HashSet<int>();
    private readonly List<DynamicScratchPatternPoolEntryModel> _addedScratchPatterns = new List<DynamicScratchPatternPoolEntryModel>();

    public double ScratchCardMultiplierBonus { get; private set; }
    public double ScratchCardMultiplier => 1d + ScratchCardMultiplierBonus;
    public int SettlementScoreBonus { get; private set; }
    public double SettlementMultiplierBonus { get; private set; }

    public void AddPatternBaseScoreBonus(int patternId, int bonus, int sourceCardId = 0)
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
        AddSourceCardId(_patternBaseScoreSourceCardIds, patternId, sourceCardId);
    }

    public int GetPatternBaseScoreBonus(int patternId)
    {
        int bonus = _patternBaseScoreBonuses.TryGetValue(patternId, out int staticBonus) ? staticBonus : 0;
        bonus += _permanentPatternBaseScoreBonuses.TryGetValue(patternId, out int permanentBonus) ? permanentBonus : 0;
        return bonus;
    }

    public bool HasPatternBaseScoreBonus(int patternId)
    {
        return GetPatternBaseScoreBonus(patternId) != 0;
    }

    public IReadOnlyCollection<int> GetPatternBaseScoreSourceCardIds(int patternId)
    {
        var sourceCardIds = new HashSet<int>();
        AddSourceCardIds(sourceCardIds, GetSourceCardIds(_patternBaseScoreSourceCardIds, patternId));
        AddSourceCardIds(sourceCardIds, GetSourceCardIds(_permanentPatternBaseScoreSourceCardIds, patternId));
        return sourceCardIds.Count > 0 ? (IReadOnlyCollection<int>)sourceCardIds : System.Array.Empty<int>();
    }

    public void AddPatternBaseScoreGrowthOnScore(int scoredPatternId, IReadOnlyList<int> targetPatternIds, int bonus, int sourceCardId = 0)
    {
        if (scoredPatternId <= 0 || targetPatternIds == null || targetPatternIds.Count == 0 || bonus == 0)
        {
            return;
        }

        if (!_patternBaseScoreGrowthRulesByScoredPattern.TryGetValue(scoredPatternId, out List<PatternBaseScoreGrowthRule> rules))
        {
            rules = new List<PatternBaseScoreGrowthRule>();
            _patternBaseScoreGrowthRulesByScoredPattern[scoredPatternId] = rules;
        }

        rules.Add(new PatternBaseScoreGrowthRule(targetPatternIds, bonus, sourceCardId));
        AddSourceCardId(_patternBaseScoreGrowthSourceCardIds, scoredPatternId, sourceCardId);
    }

    public IReadOnlyCollection<int> ApplyPatternBaseScoreGrowthOnScore(int scoredPatternId)
    {
        if (scoredPatternId <= 0 || !_patternBaseScoreGrowthRulesByScoredPattern.TryGetValue(scoredPatternId, out List<PatternBaseScoreGrowthRule> rules))
        {
            return System.Array.Empty<int>();
        }

        var triggeredSourceCardIds = new HashSet<int>();
        for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
        {
            PatternBaseScoreGrowthRule rule = rules[ruleIndex];
            if (rule == null)
            {
                continue;
            }

            for (int targetIndex = 0; targetIndex < rule.TargetPatternIds.Count; targetIndex++)
            {
                int targetPatternId = rule.TargetPatternIds[targetIndex];
                if (targetPatternId <= 0)
                {
                    continue;
                }

                if (!_permanentPatternBaseScoreBonuses.ContainsKey(targetPatternId))
                {
                    _permanentPatternBaseScoreBonuses[targetPatternId] = 0;
                }

                _permanentPatternBaseScoreBonuses[targetPatternId] += rule.Bonus;
                AddSourceCardId(_permanentPatternBaseScoreSourceCardIds, targetPatternId, rule.SourceCardId);
            }

            if (rule.SourceCardId > 0)
            {
                triggeredSourceCardIds.Add(rule.SourceCardId);
            }
        }

        return triggeredSourceCardIds.Count > 0 ? (IReadOnlyCollection<int>)triggeredSourceCardIds : System.Array.Empty<int>();
    }

    public IReadOnlyCollection<int> GetPatternBaseScoreGrowthSourceCardIds(int scoredPatternId)
    {
        return GetSourceCardIds(_patternBaseScoreGrowthSourceCardIds, scoredPatternId);
    }

    public void AddGiantPatternRule(int patternId, double chance, double scoreMultiplier, int sourceCardId = 0)
    {
        if (patternId <= 0 || chance <= 0d)
        {
            return;
        }

        if (!_giantPatternRulesByPattern.TryGetValue(patternId, out List<GiantPatternRule> rules))
        {
            rules = new List<GiantPatternRule>();
            _giantPatternRulesByPattern[patternId] = rules;
        }

        rules.Add(new GiantPatternRule(NormalizeChance(chance), scoreMultiplier > 0d ? scoreMultiplier : 1d, sourceCardId));
        AddSourceCardId(_giantPatternSourceCardIds, patternId, sourceCardId);
    }

    public bool TryRollGiantPattern(int patternId, out double scoreMultiplier, out IReadOnlyCollection<int> sourceCardIds)
    {
        scoreMultiplier = 1d;
        sourceCardIds = System.Array.Empty<int>();
        if (patternId <= 0 || !_giantPatternRulesByPattern.TryGetValue(patternId, out List<GiantPatternRule> rules))
        {
            return false;
        }

        var triggeredSourceCardIds = new HashSet<int>();
        bool isGiant = false;
        for (int i = 0; i < rules.Count; i++)
        {
            GiantPatternRule rule = rules[i];
            if (rule == null || rule.Chance <= 0d)
            {
                continue;
            }

            if (Random.value > rule.Chance)
            {
                continue;
            }

            isGiant = true;
            scoreMultiplier = System.Math.Max(scoreMultiplier, rule.ScoreMultiplier);
            if (rule.SourceCardId > 0)
            {
                triggeredSourceCardIds.Add(rule.SourceCardId);
            }
        }

        sourceCardIds = triggeredSourceCardIds.Count > 0
            ? (IReadOnlyCollection<int>)triggeredSourceCardIds
            : System.Array.Empty<int>();
        return isGiant;
    }

    public IReadOnlyCollection<int> GetGiantPatternSourceCardIds(int patternId)
    {
        return GetSourceCardIds(_giantPatternSourceCardIds, patternId);
    }

    public void AddJokerPatternRule(
        int jokerPatternId,
        double jokerChance,
        double goodFaceChance,
        int goodFacePatternId,
        int badFacePatternId,
        int goodFaceScore,
        IReadOnlyList<int> cardTypeIds = null,
        int sourceCardId = 0)
    {
        if (jokerPatternId <= 0 || jokerChance <= 0d || goodFacePatternId <= 0 || badFacePatternId <= 0)
        {
            return;
        }

        AddScratchPatternToPoolByProbability(jokerPatternId, jokerChance, cardTypeIds, sourceCardId);
        if (!_jokerPatternRulesByPattern.TryGetValue(jokerPatternId, out List<JokerPatternRule> rules))
        {
            rules = new List<JokerPatternRule>();
            _jokerPatternRulesByPattern[jokerPatternId] = rules;
        }

        rules.Add(new JokerPatternRule(
            NormalizeChance(goodFaceChance),
            goodFacePatternId,
            badFacePatternId,
            goodFaceScore,
            sourceCardId));
        if (goodFaceScore > 0)
        {
            _jokerGoodFaceScoreOverrides[goodFacePatternId] = goodFaceScore;
        }

        AddSourceCardId(_jokerPatternSourceCardIds, jokerPatternId, sourceCardId);
    }

    public bool TryRollJokerPattern(
        int jokerPatternId,
        out int resolvedPatternId,
        out IReadOnlyCollection<int> sourceCardIds)
    {
        resolvedPatternId = jokerPatternId;
        sourceCardIds = System.Array.Empty<int>();
        if (jokerPatternId <= 0 || !_jokerPatternRulesByPattern.TryGetValue(jokerPatternId, out List<JokerPatternRule> rules) || rules.Count == 0)
        {
            return false;
        }

        JokerPatternRule rule = rules[rules.Count - 1];
        if (rule == null)
        {
            return false;
        }

        resolvedPatternId = Random.value <= rule.GoodFaceChance
            ? rule.GoodFacePatternId
            : rule.BadFacePatternId;
        sourceCardIds = GetSourceCardIds(_jokerPatternSourceCardIds, jokerPatternId);
        return true;
    }

    public int GetJokerGoodFaceScoreOverride(int goodFacePatternId)
    {
        return _jokerGoodFaceScoreOverrides.TryGetValue(goodFacePatternId, out int score) ? score : 0;
    }

    public void AddRiskMultiplierPatternRule(
        int riskMultiplierPatternId,
        double chance,
        IReadOnlyList<int> resolvedPatternIds,
        IReadOnlyList<double> resolvedPatternWeights,
        IReadOnlyList<int> cardTypeIds = null,
        int sourceCardId = 0)
    {
        if (riskMultiplierPatternId <= 0 || chance <= 0d || resolvedPatternIds == null || resolvedPatternIds.Count == 0)
        {
            return;
        }

        AddScratchPatternToPoolByProbability(riskMultiplierPatternId, chance, cardTypeIds, sourceCardId);
        if (!_riskMultiplierRulesByPattern.TryGetValue(riskMultiplierPatternId, out List<RiskMultiplierPatternRule> rules))
        {
            rules = new List<RiskMultiplierPatternRule>();
            _riskMultiplierRulesByPattern[riskMultiplierPatternId] = rules;
        }

        rules.Add(new RiskMultiplierPatternRule(resolvedPatternIds, resolvedPatternWeights, sourceCardId));
        AddSourceCardId(_riskMultiplierPatternSourceCardIds, riskMultiplierPatternId, sourceCardId);
    }

    public bool TryRollRiskMultiplierPattern(
        int riskMultiplierPatternId,
        out int resolvedPatternId,
        out IReadOnlyCollection<int> sourceCardIds)
    {
        resolvedPatternId = riskMultiplierPatternId;
        sourceCardIds = System.Array.Empty<int>();
        if (riskMultiplierPatternId <= 0 ||
            !_riskMultiplierRulesByPattern.TryGetValue(riskMultiplierPatternId, out List<RiskMultiplierPatternRule> rules) ||
            rules.Count == 0)
        {
            return false;
        }

        RiskMultiplierPatternRule rule = rules[rules.Count - 1];
        if (rule == null)
        {
            return false;
        }

        resolvedPatternId = rule.RollPatternId();
        sourceCardIds = GetSourceCardIds(_riskMultiplierPatternSourceCardIds, riskMultiplierPatternId);
        return resolvedPatternId > 0;
    }

    public void AddPatternWeightBonus(int patternId, double bonus, int sourceCardId = 0)
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
        AddSourceCardId(_patternWeightSourceCardIds, patternId, sourceCardId);
    }

    public double GetPatternWeightBonus(int patternId)
    {
        return _patternWeightBonuses.TryGetValue(patternId, out double bonus) ? bonus : 0d;
    }

    public IReadOnlyCollection<int> GetPatternWeightSourceCardIds(int patternId)
    {
        return GetSourceCardIds(_patternWeightSourceCardIds, patternId);
    }

    public float GetEffectivePatternWeight(int patternId, int baseWeight)
    {
        return GetEffectivePatternWeight(patternId, (float)baseWeight);
    }

    public float GetEffectivePatternWeight(int patternId, float baseWeight)
    {
        return Mathf.Max(0f, baseWeight + (float)GetPatternWeightBonus(patternId));
    }

    public void AddPatternScratchCardMultiplierBonus(int patternId, double bonus, int sourceCardId = 0)
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
        AddSourceCardId(_patternScratchCardMultiplierSourceCardIds, patternId, sourceCardId);
    }

    public double GetPatternScratchCardMultiplierBonus(int patternId)
    {
        return _patternScratchCardMultiplierBonuses.TryGetValue(patternId, out double bonus) ? bonus : 0d;
    }

    public IReadOnlyCollection<int> GetPatternScratchCardMultiplierSourceCardIds(int patternId)
    {
        return GetSourceCardIds(_patternScratchCardMultiplierSourceCardIds, patternId);
    }

    public void AddScratchPatternToPool(int patternId, float weight, IReadOnlyList<int> cardTypeIds = null, int sourceCardId = 0)
    {
        AddScratchPatternToPool(patternId, weight, cardTypeIds, sourceCardId, false);
    }

    public void AddScratchPatternToPoolByProbability(int patternId, double chance, IReadOnlyList<int> cardTypeIds = null, int sourceCardId = 0)
    {
        AddScratchPatternToPool(patternId, (float)NormalizeChance(chance), cardTypeIds, sourceCardId, true);
    }

    private void AddScratchPatternToPool(int patternId, float weight, IReadOnlyList<int> cardTypeIds, int sourceCardId, bool isProbability)
    {
        if (patternId <= 0 || weight <= 0f)
        {
            return;
        }

        _addedScratchPatterns.Add(new DynamicScratchPatternPoolEntryModel(patternId, weight, cardTypeIds, isProbability));
        AddSourceCardId(_addedScratchPatternSourceCardIds, patternId, sourceCardId);
    }

    public IReadOnlyCollection<int> GetAddedScratchPatternSourceCardIds(int patternId)
    {
        return GetSourceCardIds(_addedScratchPatternSourceCardIds, patternId);
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

    public void AddScratchCardMultiplierBonus(double bonus, int sourceCardId = 0)
    {
        if (bonus <= 0d)
        {
            return;
        }

        ScratchCardMultiplierBonus += bonus;
        if (sourceCardId > 0)
        {
            _scratchCardMultiplierSourceCardIds.Add(sourceCardId);
        }
    }

    public IReadOnlyCollection<int> GetScratchCardMultiplierSourceCardIds()
    {
        return _scratchCardMultiplierSourceCardIds;
    }

    public void AddSettlementScoreBonus(int bonus, int sourceCardId = 0)
    {
        if (bonus == 0)
        {
            return;
        }

        SettlementScoreBonus += bonus;
        if (sourceCardId > 0)
        {
            _settlementScoreBonusSourceCardIds.Add(sourceCardId);
        }
    }

    public IReadOnlyCollection<int> GetSettlementScoreBonusSourceCardIds()
    {
        return _settlementScoreBonusSourceCardIds;
    }

    public void AddSettlementMultiplierBonus(double bonus, int sourceCardId = 0)
    {
        if (bonus <= 0d)
        {
            return;
        }

        SettlementMultiplierBonus += bonus;
        if (sourceCardId > 0)
        {
            _settlementMultiplierBonusSourceCardIds.Add(sourceCardId);
        }
    }

    public IReadOnlyCollection<int> GetSettlementMultiplierBonusSourceCardIds()
    {
        return _settlementMultiplierBonusSourceCardIds;
    }

    public void Clear()
    {
        _patternBaseScoreBonuses.Clear();
        _patternWeightBonuses.Clear();
        _patternScratchCardMultiplierBonuses.Clear();
        _patternBaseScoreGrowthRulesByScoredPattern.Clear();
        _giantPatternRulesByPattern.Clear();
        _jokerPatternRulesByPattern.Clear();
        _riskMultiplierRulesByPattern.Clear();
        _jokerGoodFaceScoreOverrides.Clear();
        _patternBaseScoreSourceCardIds.Clear();
        _patternBaseScoreGrowthSourceCardIds.Clear();
        _patternWeightSourceCardIds.Clear();
        _patternScratchCardMultiplierSourceCardIds.Clear();
        _giantPatternSourceCardIds.Clear();
        _jokerPatternSourceCardIds.Clear();
        _riskMultiplierPatternSourceCardIds.Clear();
        _addedScratchPatternSourceCardIds.Clear();
        _scratchCardMultiplierSourceCardIds.Clear();
        _settlementScoreBonusSourceCardIds.Clear();
        _settlementMultiplierBonusSourceCardIds.Clear();
        _addedScratchPatterns.Clear();
        ScratchCardMultiplierBonus = 0d;
        SettlementScoreBonus = 0;
        SettlementMultiplierBonus = 0d;
    }

    public void ClearAll()
    {
        Clear();
        _permanentPatternBaseScoreBonuses.Clear();
        _permanentPatternBaseScoreSourceCardIds.Clear();
    }

    private static void AddSourceCardId(Dictionary<int, HashSet<int>> sourceCardIdsByPattern, int patternId, int sourceCardId)
    {
        if (sourceCardId <= 0)
        {
            return;
        }

        if (!sourceCardIdsByPattern.TryGetValue(patternId, out HashSet<int> sourceCardIds))
        {
            sourceCardIds = new HashSet<int>();
            sourceCardIdsByPattern[patternId] = sourceCardIds;
        }

        sourceCardIds.Add(sourceCardId);
    }

    private static IReadOnlyCollection<int> GetSourceCardIds(Dictionary<int, HashSet<int>> sourceCardIdsByPattern, int patternId)
    {
        return sourceCardIdsByPattern.TryGetValue(patternId, out HashSet<int> sourceCardIds)
            ? (IReadOnlyCollection<int>)sourceCardIds
            : System.Array.Empty<int>();
    }

    private static void AddSourceCardIds(HashSet<int> target, IEnumerable<int> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (int sourceCardId in source)
        {
            if (sourceCardId > 0)
            {
                target.Add(sourceCardId);
            }
        }
    }

    private static double NormalizeChance(double chance)
    {
        if (chance > 1d)
        {
            chance /= 100d;
        }

        return System.Math.Max(0d, System.Math.Min(1d, chance));
    }

    private class PatternBaseScoreGrowthRule
    {
        public IReadOnlyList<int> TargetPatternIds { get; }
        public int Bonus { get; }
        public int SourceCardId { get; }

        public PatternBaseScoreGrowthRule(IReadOnlyList<int> targetPatternIds, int bonus, int sourceCardId)
        {
            TargetPatternIds = new List<int>(targetPatternIds);
            Bonus = bonus;
            SourceCardId = sourceCardId;
        }
    }

    private class GiantPatternRule
    {
        public double Chance { get; }
        public double ScoreMultiplier { get; }
        public int SourceCardId { get; }

        public GiantPatternRule(double chance, double scoreMultiplier, int sourceCardId)
        {
            Chance = chance;
            ScoreMultiplier = scoreMultiplier;
            SourceCardId = sourceCardId;
        }
    }

    private class JokerPatternRule
    {
        public double GoodFaceChance { get; }
        public int GoodFacePatternId { get; }
        public int BadFacePatternId { get; }
        public int GoodFaceScore { get; }
        public int SourceCardId { get; }

        public JokerPatternRule(double goodFaceChance, int goodFacePatternId, int badFacePatternId, int goodFaceScore, int sourceCardId)
        {
            GoodFaceChance = goodFaceChance;
            GoodFacePatternId = goodFacePatternId;
            BadFacePatternId = badFacePatternId;
            GoodFaceScore = goodFaceScore;
            SourceCardId = sourceCardId;
        }
    }

    private class RiskMultiplierPatternRule
    {
        private readonly List<int> _resolvedPatternIds;
        private readonly List<double> _resolvedPatternWeights;

        public int SourceCardId { get; }

        public RiskMultiplierPatternRule(IReadOnlyList<int> resolvedPatternIds, IReadOnlyList<double> resolvedPatternWeights, int sourceCardId)
        {
            _resolvedPatternIds = new List<int>();
            _resolvedPatternWeights = new List<double>();
            for (int i = 0; i < resolvedPatternIds.Count; i++)
            {
                int patternId = resolvedPatternIds[i];
                if (patternId <= 0)
                {
                    continue;
                }

                double weight = resolvedPatternWeights != null && i < resolvedPatternWeights.Count
                    ? resolvedPatternWeights[i]
                    : 1d;
                if (weight <= 0d)
                {
                    continue;
                }

                _resolvedPatternIds.Add(patternId);
                _resolvedPatternWeights.Add(weight);
            }

            SourceCardId = sourceCardId;
        }

        public int RollPatternId()
        {
            if (_resolvedPatternIds.Count == 0)
            {
                return 0;
            }

            double totalWeight = 0d;
            for (int i = 0; i < _resolvedPatternWeights.Count; i++)
            {
                totalWeight += System.Math.Max(0d, _resolvedPatternWeights[i]);
            }

            if (totalWeight <= 0d)
            {
                return _resolvedPatternIds[0];
            }

            double randomValue = Random.value * totalWeight;
            double accumulatedWeight = 0d;
            for (int i = 0; i < _resolvedPatternIds.Count; i++)
            {
                accumulatedWeight += System.Math.Max(0d, _resolvedPatternWeights[i]);
                if (randomValue < accumulatedWeight)
                {
                    return _resolvedPatternIds[i];
                }
            }

            return _resolvedPatternIds[_resolvedPatternIds.Count - 1];
        }
    }
}
