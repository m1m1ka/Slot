using System;
using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Core
{
    public static class RogueCardDefaultsProvider
    {
        private const string TablePath = "Configs/RogueCards/RogueCards";

        private static List<RogueCardConfig> _cachedCards;

        private static readonly List<RogueCardConfig> DefaultCards = new List<RogueCardConfig>
        {
            new RogueCardConfig
            {
                Id = 1,
                Name = "樱桃抛光",
                Rarity = RogueCardRarity.Common,
                Description = "樱桃图案基础分增加。",
                Levels = new List<RogueCardLevelConfig>
                {
                    new RogueCardLevelConfig
                    {
                        Level = 1,
                        Description = "樱桃图案基础分增加 5。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetIds = new List<int> { 1 }, Value = 5 }
                        }
                    },
                    new RogueCardLevelConfig
                    {
                        Level = 2,
                        Description = "樱桃与柠檬图案基础分增加 12。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetIds = new List<int> { 1, 2 }, Value = 12 }
                        }
                    },
                    new RogueCardLevelConfig
                    {
                        Level = 3,
                        Description = "樱桃、柠檬与橙子图案基础分增加 22。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetIds = new List<int> { 1, 2, 3 }, Value = 22 }
                        }
                    }
                }
            },
            new RogueCardConfig
            {
                Id = 2,
                Name = "幸运刮刀",
                Rarity = RogueCardRarity.Common,
                Description = "刮刮卡奖励获得少量倍率。",
                Levels = new List<RogueCardLevelConfig>
                {
                    new RogueCardLevelConfig
                    {
                        Level = 1,
                        Description = "刮刮卡奖励倍率 +0.1。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreaseScratchCardMultiplier, Value = 0.1 }
                        }
                    },
                    new RogueCardLevelConfig
                    {
                        Level = 2,
                        Description = "刮刮卡奖励倍率 +0.25。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreaseScratchCardMultiplier, Value = 0.25 }
                        }
                    },
                    new RogueCardLevelConfig
                    {
                        Level = 3,
                        Description = "刮刮卡奖励倍率 +0.45。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreaseScratchCardMultiplier, Value = 0.45 }
                        }
                    }
                }
            },
            new RogueCardConfig
            {
                Id = 3,
                Name = "铃铛回响",
                Rarity = RogueCardRarity.Common,
                Description = "铃铛图案基础分增加。",
                Levels = new List<RogueCardLevelConfig>
                {
                    new RogueCardLevelConfig
                    {
                        Level = 1,
                        Description = "铃铛图案基础分增加 8。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetIds = new List<int> { 5 }, Value = 8 }
                        }
                    },
                    new RogueCardLevelConfig
                    {
                        Level = 2,
                        Description = "铃铛与横条图案基础分增加 18。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetIds = new List<int> { 5, 6 }, Value = 18 }
                        }
                    },
                    new RogueCardLevelConfig
                    {
                        Level = 3,
                        Description = "铃铛、横条与星星图案基础分增加 32。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.IncreasePatternBaseScore, TargetIds = new List<int> { 5, 6, 7 }, Value = 32 }
                        }
                    }
                }
            },
            new RogueCardConfig
            {
                Id = 4,
                Name = "黄金边缘",
                Rarity = RogueCardRarity.Rare,
                Description = "后续奖励规则可接入这张卡。",
                Levels = new List<RogueCardLevelConfig>
                {
                    new RogueCardLevelConfig
                    {
                        Level = 1,
                        Description = "后续奖励规则可接入这张卡。",
                        Effects = new List<RogueCardEffectConfig>
                        {
                            new RogueCardEffectConfig { EffectType = RogueCardEffectType.None }
                        }
                    }
                }
            }
        };

        public static IReadOnlyList<RogueCardConfig> GetAll()
        {
            return GetCards();
        }

        public static RogueCardConfig GetById(int id)
        {
            IReadOnlyList<RogueCardConfig> cards = GetCards();
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Id == id)
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
            if (_cachedCards == null || _cachedCards.Count == 0)
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

            if (tableData == null || tableData.cards == null || tableData.cards.Count == 0)
            {
                return null;
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
                    RogueCardLevelData levelData = cardData.levels[levelIndex];
                    RogueCardLevelConfig levelConfig = BuildLevelConfig(cardData.id, levelData);
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

        private static RogueCardLevelConfig BuildLevelConfig(int cardId, RogueCardLevelData levelData)
        {
            if (levelData == null || levelData.level <= 0)
            {
                Debug.LogWarning($"[RogueCardDefaultsProvider] Skip invalid level on card id={cardId}.");
                return null;
            }

            var levelConfig = new RogueCardLevelConfig
            {
                Level = levelData.level,
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
                    TargetIds = effectData.targetIds ?? new List<int>(),
                    CardTypeIds = effectData.cardTypeIds ?? new List<int>(),
                    Value = effectData.value
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

            if (Enum.TryParse(rawValue, true, out rarity))
            {
                return true;
            }

            switch (rawValue)
            {
                case "普通":
                    rarity = RogueCardRarity.Common;
                    return true;
                case "罕见":
                    rarity = RogueCardRarity.Rare;
                    return true;
                case "史诗":
                    rarity = RogueCardRarity.Epic;
                    return true;
                case "传说":
                    rarity = RogueCardRarity.Legendary;
                    return true;
                default:
                    rarity = RogueCardRarity.Common;
                    return false;
            }
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
            public string description;
            public List<RogueCardEffectData> effects;
        }

        [Serializable]
        private class RogueCardEffectData
        {
            public string effectType;
            public List<int> targetIds;
            public List<int> cardTypeIds;
            public float value;
        }
    }
}
