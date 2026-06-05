using System;
using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Core
{
    public static class RogueCardDefaultsProvider
    {
        private const string TablePath = "Configs/RogueCards/RogueCards";

        private static readonly Dictionary<int, Vector2Int> LevelDistributions = new Dictionary<int, Vector2Int>();
        private static readonly List<RogueCardConfig> DefaultCards = new List<RogueCardConfig>();

        private static List<RogueCardConfig> _cachedCards;

        public static IReadOnlyList<RogueCardConfig> GetAll()
        {
            return GetCards();
        }

        public static IReadOnlyList<RogueCardConfig> GetAvailableForLevel(int levelId)
        {
            IReadOnlyList<RogueCardConfig> cards = GetCards();
            var availableCards = new List<RogueCardConfig>();
            int count = cards != null ? cards.Count : 0;
            for (int i = 0; i < count; i++)
            {
                RogueCardConfig card = cards[i];
                if (card != null && IsAvailableForLevel(card.Id, levelId))
                {
                    availableCards.Add(card);
                }
            }

            return availableCards;
        }

        public static bool IsAvailableForLevel(int cardId, int levelId)
        {
            if (!LevelDistributions.TryGetValue(cardId, out Vector2Int levelRange))
            {
                return true;
            }

            return levelId >= levelRange.x && levelId <= levelRange.y;
        }

        public static RogueCardConfig GetById(int id)
        {
            IReadOnlyList<RogueCardConfig> cards = GetCards();
            int count = cards != null ? cards.Count : 0;
            for (int i = 0; i < count; i++)
            {
                if (cards[i] != null && cards[i].Id == id)
                {
                    return cards[i];
                }
            }

            return null;
        }

        public static void Reload()
        {
            _cachedCards = null;
        }

        private static IReadOnlyList<RogueCardConfig> GetCards()
        {
            if (_cachedCards != null)
            {
                return _cachedCards;
            }

            _cachedCards = LoadCardsFromJson();
            if (_cachedCards == null)
            {
                _cachedCards = DefaultCards;
            }

            return _cachedCards;
        }

        private static List<RogueCardConfig> LoadCardsFromJson()
        {
            TextAsset configAsset = AssetProvider.LoadTextAsset(TablePath);
            if (configAsset == null || string.IsNullOrWhiteSpace(configAsset.text))
            {
                return null;
            }

            RogueCardTableData tableData;
            try
            {
                tableData = JsonUtility.FromJson<RogueCardTableData>(configAsset.text);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[RogueCardDefaultsProvider] Failed to parse Resources/{TablePath}.json: {exception.Message}");
                return null;
            }

            if (tableData == null || tableData.cards == null)
            {
                return new List<RogueCardConfig>();
            }

            var cards = new List<RogueCardConfig>();
            for (int cardIndex = 0; cardIndex < tableData.cards.Count; cardIndex++)
            {
                RogueCardData cardData = tableData.cards[cardIndex];
                if (cardData == null || cardData.id <= 0)
                {
                    Debug.LogWarning($"[RogueCardDefaultsProvider] Skip card entry {cardIndex + 1}: invalid id.");
                    continue;
                }

                if (!TryParseRarity(cardData.rarity, out RogueCardRarity rarity))
                {
                    Debug.LogWarning($"[RogueCardDefaultsProvider] Skip card id={cardData.id}: invalid rarity '{cardData.rarity}'.");
                    continue;
                }

                var cardConfig = new RogueCardConfig
                {
                    Id = cardData.id,
                    Name = cardData.name,
                    Rarity = rarity,
                    Description = cardData.description,
                    Levels = new List<RogueCardLevelConfig>()
                };

                int levelCount = cardData.levels != null ? cardData.levels.Count : 0;
                for (int levelIndex = 0; levelIndex < levelCount; levelIndex++)
                {
                    RogueCardLevelConfig levelConfig = BuildLevelConfig(cardData.id, rarity, cardData.levels[levelIndex]);
                    if (levelConfig != null)
                    {
                        cardConfig.Levels.Add(levelConfig);
                    }
                }

                cardConfig.Levels.Sort((left, right) => left.Level.CompareTo(right.Level));
                if (cardConfig.Levels.Count == 0)
                {
                    Debug.LogWarning($"[RogueCardDefaultsProvider] Skip card id={cardData.id}: no valid levels.");
                    continue;
                }

                cards.Add(cardConfig);
            }

            cards.Sort((left, right) => left.Id.CompareTo(right.Id));
            return cards;
        }

        private static RogueCardLevelConfig BuildLevelConfig(int cardId, RogueCardRarity cardRarity, RogueCardLevelData levelData)
        {
            if (levelData == null || levelData.level <= 0)
            {
                Debug.LogWarning($"[RogueCardDefaultsProvider] Skip invalid level on card id={cardId}.");
                return null;
            }

            var levelConfig = new RogueCardLevelConfig
            {
                Level = levelData.level,
                Rarity = TryParseRarity(levelData.rarity, out RogueCardRarity levelRarity) ? levelRarity : cardRarity,
                Description = levelData.description,
                Effects = new List<RogueCardEffectConfig>()
            };

            int effectCount = levelData.effects != null ? levelData.effects.Count : 0;
            for (int effectIndex = 0; effectIndex < effectCount; effectIndex++)
            {
                RogueCardEffectData effectData = levelData.effects[effectIndex];
                if (effectData == null)
                {
                    continue;
                }

                if (!TryParseEffectType(effectData.effectType, out RogueCardEffectType effectType))
                {
                    Debug.LogWarning($"[RogueCardDefaultsProvider] Skip invalid effect '{effectData.effectType}' on card id={cardId}, level={levelData.level}.");
                    continue;
                }

                levelConfig.Effects.Add(new RogueCardEffectConfig
                {
                    EffectType = effectType,
                    TriggerTime = TryParseTriggerTime(effectData.triggerTime, out RogueCardTriggerTime triggerTime)
                        ? triggerTime
                        : RogueCardTriggerTime.Settlement,
                    TargetIds = effectData.targetIds ?? new List<int>(),
                    TargetType = effectData.targetType,
                    CardTypeIds = effectData.cardTypeIds ?? new List<int>(),
                    Value = ParseEffectValue(effectData.value),
                    ValueExpression = effectData.value
                });
            }

            return levelConfig;
        }

        private static bool TryParseEffectType(string rawValue, out RogueCardEffectType effectType)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                effectType = RogueCardEffectType.None;
                return false;
            }

            if (int.TryParse(rawValue, out int intValue))
            {
                effectType = (RogueCardEffectType)intValue;
                return Enum.IsDefined(typeof(RogueCardEffectType), effectType);
            }

            return Enum.TryParse(rawValue, true, out effectType);
        }

        private static double ParseEffectValue(string rawValue)
        {
            string[] parts = RogueCardEffectValueParser.Split(rawValue);
            return parts.Length > 0 && RogueCardEffectValueParser.TryParseNumber(parts[0], out double value)
                ? value
                : 0d;
        }

        private static bool TryParseRarity(string rawValue, out RogueCardRarity rarity)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                rarity = RogueCardRarity.Common;
                return false;
            }

            if (int.TryParse(rawValue, out int intValue))
            {
                rarity = (RogueCardRarity)intValue;
                return Enum.IsDefined(typeof(RogueCardRarity), rarity);
            }

            return Enum.TryParse(rawValue, true, out rarity);
        }

        private static bool TryParseTriggerTime(string rawValue, out RogueCardTriggerTime triggerTime)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                triggerTime = RogueCardTriggerTime.Settlement;
                return false;
            }

            if (int.TryParse(rawValue, out int intValue))
            {
                triggerTime = (RogueCardTriggerTime)intValue;
                return Enum.IsDefined(typeof(RogueCardTriggerTime), triggerTime);
            }

            return Enum.TryParse(rawValue, true, out triggerTime);
        }

        [Serializable]
        private class RogueCardTableData
        {
            public List<RogueCardData> cards;
        }

        [Serializable]
        private class RogueCardData
        {
            public int id;
            public string name;
            public string rarity;
            public string description;
            public List<RogueCardLevelData> levels;
        }

        [Serializable]
        private class RogueCardLevelData
        {
            public int level;
            public string rarity;
            public string description;
            public List<RogueCardEffectData> effects;
        }

        [Serializable]
        private class RogueCardEffectData
        {
            public string effectType;
            public string triggerTime;
            public List<int> targetIds;
            public string targetType;
            public List<int> cardTypeIds;
            public string value;
        }
    }
}
