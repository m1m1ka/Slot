using System.Collections.Generic;

public class DynamicScratchPatternPoolEntryModel
{
    private readonly List<int> _cardTypeIds;

    public int PatternId { get; }
    public float Weight { get; }
    public IReadOnlyList<int> CardTypeIds => _cardTypeIds;

    public DynamicScratchPatternPoolEntryModel(int patternId, float weight, IReadOnlyList<int> cardTypeIds = null)
    {
        PatternId = patternId;
        Weight = weight > 0f ? weight : 0f;
        _cardTypeIds = cardTypeIds != null ? new List<int>(cardTypeIds) : new List<int>();
    }

    public bool AppliesToCardType(int cardTypeId)
    {
        return _cardTypeIds.Count == 0 || _cardTypeIds.Contains(cardTypeId);
    }
}
