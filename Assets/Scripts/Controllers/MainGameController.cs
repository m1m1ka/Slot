using UnityEngine;
using UI; // 引入 UIManager 所在的空间
using Core; // 引入 PoolManager
using System.Collections.Generic;
using Configs;

/// <summary>
/// 控制层：挂载在 MainGamePanel 预制体上，获取同级 View 引用。
/// 包含核心的购买验证、升级验证逻辑。
/// </summary>
[RequireComponent(typeof(MainGamePanel))]
public class MainGameController : MonoBehaviour
{
    // View 引用：与 Controller 同属一个 GameObject
    private MainGamePanel _mainGamePanel;

    // Model 引用
    private PlayerModel _playerModel;
    private GameSession _gameSession;
    private LevelProgressModel _levelModel;
    private RogueCardInventoryModel _rogueCardInventory;
    private readonly RogueCardRewardService _rogueCardRewardService = new RogueCardRewardService();
    private readonly RogueCardEffectService _rogueCardEffectService = new RogueCardEffectService();
    private RogueCardRewardOfferModel _currentRogueRewardOffer;
    private bool _rogueRewardOfferedForCurrentLevel;

    // 存储当前动态加载的商店项引用
    private readonly List<ShopItemView> _shopItems = new List<ShopItemView>();
    private readonly Dictionary<ShopItemView, double> _shopItemPrices = new Dictionary<ShopItemView, double>();
    private readonly List<ScratchCardController> _activeScratchCards = new List<ScratchCardController>();

    private int _nextScratchCardId = 1;

    private void Awake()
    {
        // 1. 获取同级挂载的 View 组件（严格禁止在此处直接操作 UI 渲染组件）
        _mainGamePanel = GetComponent<MainGamePanel>();

        // 2. 从全局运行时上下文中获取玩家数据，而不是由当前界面自行创建
        AppRoot appRoot = AppRoot.Instance;
        if (appRoot == null || appRoot.PlayerContext == null || appRoot.PlayerContext.Player == null || appRoot.GameSession == null)
        {
            Debug.LogError("[MainGameController] PlayerContext 未初始化，无法绑定 PlayerModel。请先确认 GameBootstrapper 已完成启动。");
            enabled = false;
            return;
        }

        _playerModel = appRoot.PlayerContext.Player;
        _gameSession = appRoot.GameSession;
        _rogueCardInventory = appRoot.PlayerContext.RogueCards;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        EnsureCurrentLevel();

        // 1. 批量生成购物面板的购买按钮
        LoadShopItems();

        // 2. 将数据层的事件绑定到当前 Controller 的响应方法中
        _playerModel.OnCoinsChanged += HandleCoinsChanged;
        if (_mainGamePanel != null)
        {
            _mainGamePanel.OnRogueRewardCardSelected += HandleRogueRewardCardSelected;
        }

        if (_rogueCardInventory != null)
        {
            _rogueCardInventory.OnCardChanged += HandleRogueCardChanged;
            _mainGamePanel?.RefreshOwnedRogueCards(_rogueCardInventory.OwnedCards);
        }

        // 3. 初始刷新一次视图
        HandleCoinsChanged(_playerModel.Coins);
        RefreshLevelDisplay();
    }

    /// <summary>
    /// 从对象池动态批量实例化商店左侧的购买项
    /// </summary>
    private void LoadShopItems()
    {
        if (_mainGamePanel == null || _mainGamePanel.SlotListRoot == null) return;

        // 根据最新规范，从 Resources/UI 目录读取该预制体
        GameObject shopItemPrefab = AssetProvider.LoadPrefab("UI/ShopItemView");
        if (shopItemPrefab == null)
        {
            Debug.LogError("没有找到 UI/ShopItemView 预制体，无法生成购买列表！");
            return;
        }

        // 根据默认卡种配置生成彩票购买选项
        foreach (ScratchCardTypeConfig cardTypeConfig in ScratchCardDefaultsProvider.GetAllCardTypes())
        {
            if (cardTypeConfig == null)
            {
                continue;
            }

            // 通过架构自带的核心对象池 (PoolManager) 生成 View
            GameObject itemObj = PoolManager.Instance.Spawn(shopItemPrefab, _mainGamePanel.SlotListRoot);
            ShopItemView itemView = itemObj.GetComponent<ShopItemView>();

            if (itemView != null)
            {
                // View 只展示配置数据，购买校验和扣费留在 Controller。
                itemView.SetData(
                    cardTypeConfig.Id,
                    cardTypeConfig.Name,
                    cardTypeConfig.Price,
                    cardTypeConfig.ShopIconAtlasPath,
                    cardTypeConfig.ShopIconSpriteName);
                itemView.UpdateAffordability(_playerModel != null && _levelModel != null && _levelModel.CanPurchaseScratchCard && _playerModel.Coins >= cardTypeConfig.Price);

                // 核心：由统一的主 Controller 监听所有个体的购买点击意图
                itemView.OnBuyClicked += HandleBuyRequest;
                
                _shopItems.Add(itemView);
                _shopItemPrices[itemView] = cardTypeConfig.Price;
            }
        }
    }

    /// <summary>
    /// 响应任何一个 ShopItemView 提交上来的购买请求
    /// </summary>
    private void HandleBuyRequest(int slotId)
    {
        AudioManager.Instance?.PlayCue(AudioCueId.UiClick);
        Debug.Log($"收到请求：尝试购买编号为 {slotId} 的刮刮卡。");

        // 从配置读取价格，进入统一购买校验和扣费流程。
        ScratchCardTypeConfig cardTypeConfig = ScratchCardDefaultsProvider.GetCardTypeForShopSlot(slotId);
        if (cardTypeConfig == null)
        {
            Debug.LogWarning($"[MainGameController] Cannot buy scratch card: card type config not found for slotId={slotId}.");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            return;
        }

        RequestBuySlot(slotId, cardTypeConfig.Price);
    }

    /// <summary>
    /// 纯粹的事件响应方法：当玩家金币数值发生改变时，通知对应的 View 进行渲染更新
    /// </summary>
    private void HandleCoinsChanged(double newCoins)
    {
        if (_mainGamePanel != null)
        {
            _mainGamePanel.UpdateCoinDisplay(newCoins);
        }

        _levelModel?.EvaluatePass(newCoins);
        RefreshLevelDisplay();

        // TODO: 通知左侧 SlotShopItemView 和右侧 UpgradeItemView
        // 刷新它们各自按钮的置灰/高亮状态（通过比较 newCoins 与价格）
    }

    // -----------------------------------------------------
    // 以下为未来预留的业务逻辑桥梁
    // -----------------------------------------------------

    /// <summary>
    /// 当监听到玩家点击了“购买彩票”按钮时触发
    /// </summary>
    public void RequestBuySlot(int slotId, double cost)
    {
        if (_levelModel == null || !_levelModel.CanPurchaseScratchCard)
        {
            Debug.LogWarning("[MainGameController] Cannot buy scratch card: purchase limit reached or level passed.");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            return;
        }

        if (cost <= 0)
        {
            if (!_levelModel.TryRecordScratchCardPurchase())
            {
                Debug.LogWarning("[MainGameController] Cannot buy scratch card: purchase limit reached.");
                AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
                RefreshLevelDisplay();
                return;
            }

            RefreshLevelDisplay();
            AudioManager.Instance?.PlayCue(AudioCueId.BuyScratchCard);
            SpawnScratchCard(slotId);
            return;
        }

        // 核心判断均在 Controller 处理
        if (_playerModel.ConsumeCoins(cost))
        {
            if (!_levelModel.TryRecordScratchCardPurchase())
            {
                _playerModel.AddCoins(cost);
                Debug.LogWarning("[MainGameController] Purchase cancelled: purchase limit reached after cost check.");
                AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
                RefreshLevelDisplay();
                return;
            }

            Debug.Log($"成功花费 {cost} 购买刮刮卡 {slotId}");
            RefreshLevelDisplay();
            AudioManager.Instance?.PlayCue(AudioCueId.BuyScratchCard);
            SpawnScratchCard(slotId);
        }
        else
        {
            Debug.LogWarning("金币不足，无法购买！");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            // TODO: 通知 View 播放“余额不足”的飘字或震动动画
        }
    }

    private void OnDestroy()
    {
        // 架构规范：务必在销毁时注销所有的事件委托，防止内存泄漏
        if (_playerModel != null)
        {
            _playerModel.OnCoinsChanged -= HandleCoinsChanged;
        }

        UnbindLevel();
        if (_mainGamePanel != null)
        {
            _mainGamePanel.OnRogueRewardCardSelected -= HandleRogueRewardCardSelected;
        }

        if (_rogueCardInventory != null)
        {
            _rogueCardInventory.OnCardChanged -= HandleRogueCardChanged;
        }

        // 清理由于事件带来的绑定关系及所有的子 View 对象池回收
        foreach (var item in _shopItems)
        {
            if (item != null)
            {
                item.OnBuyClicked -= HandleBuyRequest;
                _shopItemPrices.Remove(item);
                if (PoolManager.Instance != null && item.gameObject != null)
                {
                    PoolManager.Instance.Despawn(item.gameObject);
                }
            }
        }
        _shopItems.Clear();

        foreach (var scratchCard in _activeScratchCards)
        {
            if (scratchCard != null && PoolManager.Instance != null)
            {
                scratchCard.OnFocusStateChanged -= HandleScratchCardFocusStateChanged;
                scratchCard.OnRewardClaimed -= HandleScratchCardRewardClaimed;
                PoolManager.Instance.Despawn(scratchCard.gameObject);
            }
        }
        _activeScratchCards.Clear();
        
        // 由于现在 Controller 挂载在面板上，它随面板一起被销毁，不再需要调用 UIManager.ClosePanel。
    }

    private void SpawnScratchCard(int sourceSlotId)
    {
        if (_mainGamePanel == null || _mainGamePanel.ScratchCardRoot == null)
        {
            Debug.LogWarning("[MainGameController] ScratchCardRoot 未设置，无法生成彩票。");
            return;
        }

        var cardTypeConfig = ScratchCardDefaultsProvider.GetCardTypeForShopSlot(sourceSlotId);
        if (cardTypeConfig == null)
        {
            Debug.LogError($"[MainGameController] 未找到 sourceSlotId={sourceSlotId} 对应的刮刮卡类型配置。");
            return;
        }

        var areaTemplateConfig = ScratchCardDefaultsProvider.GetAreaTemplate(cardTypeConfig.AreaTemplateId);
        if (areaTemplateConfig == null)
        {
            Debug.LogError($"[MainGameController] 未找到卡种 {cardTypeConfig.Name} 对应的区域模板配置。");
            return;
        }

        GameObject scratchCardPrefab = AssetProvider.LoadPrefab(cardTypeConfig.PrefabPath);
        if (scratchCardPrefab == null)
        {
            Debug.LogError($"[MainGameController] 没有找到 {cardTypeConfig.PrefabPath} 预制体，无法生成彩票。");
            return;
        }

        GameObject cardObject = PoolManager.Instance.Spawn(scratchCardPrefab, _mainGamePanel.ScratchCardRoot);
        ScratchCardController scratchCardController = cardObject.GetComponent<ScratchCardController>();
        if (scratchCardController == null)
        {
            Debug.LogError("[MainGameController] ScratchCardView 预制体缺少 ScratchCardController。");
            PoolManager.Instance.Despawn(cardObject);
            return;
        }

        Vector2 targetPosition = _mainGamePanel.GetRandomScratchCardAnchoredPosition();
        Vector2 spawnFrom = _mainGamePanel.GetScratchCardSpawnFromTop(targetPosition.x);
        RogueCardRunModifierModel runModifiers = _gameSession != null ? _gameSession.RunModifiers : null;
        var generatedCells = ScratchCardGenerator.GenerateCells(cardTypeConfig, areaTemplateConfig, runModifiers);
        ScratchCardModel model = new ScratchCardModel(
            _nextScratchCardId++,
            sourceSlotId,
            cardTypeConfig,
            areaTemplateConfig,
            generatedCells,
            runModifiers != null ? runModifiers.ScratchCardMultiplier : 1d);

        scratchCardController.Initialize(model, spawnFrom, targetPosition);
        scratchCardController.OnFocusStateChanged += HandleScratchCardFocusStateChanged;
        scratchCardController.OnRewardClaimed += HandleScratchCardRewardClaimed;
        _activeScratchCards.Add(scratchCardController);
    }

    private void HandleScratchCardFocusStateChanged(ScratchCardController scratchCard, bool focused)
    {
        if (_mainGamePanel == null)
        {
            return;
        }

        if (focused)
        {
            RectTransform focusedTransform = scratchCard != null ? scratchCard.transform as RectTransform : null;
            _mainGamePanel.ShowScratchCardFocusOverlay(focusedTransform, BuildFocusPanelModel(scratchCard));
            return;
        }

        if (scratchCard != null)
        {
            _mainGamePanel.RestoreScratchCardToDefaultLayer(scratchCard.transform as RectTransform);
        }

        bool hasOtherFocusedCard = false;
        for (int i = 0; i < _activeScratchCards.Count; i++)
        {
            ScratchCardController card = _activeScratchCards[i];
            if (card == null || card == scratchCard || card.Model == null)
            {
                continue;
            }

            ScratchCardModel.ScratchCardState state = card.Model.State;
            if (state == ScratchCardModel.ScratchCardState.Focused ||
                state == ScratchCardModel.ScratchCardState.Scratching ||
                state == ScratchCardModel.ScratchCardState.Completed)
            {
                _mainGamePanel.ShowScratchCardFocusOverlay(card.transform as RectTransform, BuildFocusPanelModel(card));
                hasOtherFocusedCard = true;
                break;
            }
        }

        if (!hasOtherFocusedCard)
        {
            _mainGamePanel.HideScratchCardFocusOverlay();
        }
    }

    private void HandleScratchCardRewardClaimed(ScratchCardController scratchCard, ScratchSettlementResult settlementResult)
    {
        if (settlementResult != null)
        {
            _playerModel.AddCoins(settlementResult.FinalScore);
        }

        if (scratchCard != null)
        {
            scratchCard.OnFocusStateChanged -= HandleScratchCardFocusStateChanged;
            scratchCard.OnRewardClaimed -= HandleScratchCardRewardClaimed;
            _activeScratchCards.Remove(scratchCard);
        }

        if (_mainGamePanel != null)
        {
            _mainGamePanel.HideScratchCardFocusOverlay();
        }

        if (scratchCard != null && PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(scratchCard.gameObject);
        }
    }

    private void EnsureCurrentLevel()
    {
        if (_gameSession == null)
        {
            return;
        }

        if (_gameSession.CurrentLevel == null)
        {
            _gameSession.StartLevel(LevelDefaultsProvider.GetFirstLevel());
        }

        BindLevel(_gameSession.CurrentLevel);
    }

    private void BindLevel(LevelProgressModel levelModel)
    {
        UnbindLevel();
        _levelModel = levelModel;

        if (_levelModel == null)
        {
            return;
        }

        _levelModel.OnScratchCardPurchasesChanged += HandleLevelPurchasesChanged;
        _levelModel.OnPassStateChanged += HandleLevelPassStateChanged;
        _levelModel.EvaluatePass(_playerModel != null ? _playerModel.Coins : 0);
    }

    private void UnbindLevel()
    {
        if (_levelModel == null)
        {
            return;
        }

        _levelModel.OnScratchCardPurchasesChanged -= HandleLevelPurchasesChanged;
        _levelModel.OnPassStateChanged -= HandleLevelPassStateChanged;
    }

    private void HandleLevelPurchasesChanged(int used, int limit)
    {
        RefreshLevelDisplay();
    }

    private void HandleLevelPassStateChanged(bool passed)
    {
        RefreshLevelDisplay();
        if (passed)
        {
            Debug.Log($"[MainGameController] Level passed: {_levelModel.LevelName}");
            ShowRogueRewardChoices();
        }
    }

    private void RefreshLevelDisplay()
    {
        if (_mainGamePanel != null && _levelModel != null && _playerModel != null)
        {
            _mainGamePanel.UpdateLevelDisplay(_levelModel, _playerModel.Coins);
        }

        if (_playerModel != null)
        {
            RefreshShopItemAffordability(_playerModel.Coins);
        }
    }

    private void RefreshShopItemAffordability(double coins)
    {
        bool canPurchase = _levelModel != null && _levelModel.CanPurchaseScratchCard;
        foreach (KeyValuePair<ShopItemView, double> itemPrice in _shopItemPrices)
        {
            if (itemPrice.Key != null)
            {
                itemPrice.Key.UpdateAffordability(canPurchase && coins >= itemPrice.Value);
            }
        }
    }

    private void ShowRogueRewardChoices()
    {
        if (_rogueRewardOfferedForCurrentLevel || _mainGamePanel == null)
        {
            return;
        }

        _currentRogueRewardOffer = _rogueCardRewardService.CreateRewardOffer(3);
        _rogueRewardOfferedForCurrentLevel = true;
        _mainGamePanel.ShowRogueCardChoices(_currentRogueRewardOffer.Choices, _rogueCardInventory?.OwnedCards);
    }

    private void HandleRogueRewardCardSelected(int cardId)
    {
        if (_currentRogueRewardOffer == null || _rogueCardInventory == null)
        {
            return;
        }

        RogueCardConfig selectedCard = FindRogueRewardChoice(cardId);
        if (selectedCard == null)
        {
            Debug.LogWarning($"[MainGameController] Selected rogue card id={cardId} was not found in current offer.");
            return;
        }

        _rogueCardInventory.AddCard(selectedCard);
        _rogueCardEffectService.RebuildRunModifiers(
            _rogueCardInventory.OwnedCards,
            new RogueCardEffectContext(AppRoot.Instance != null ? AppRoot.Instance.PlayerContext : null, _gameSession));

        _currentRogueRewardOffer = null;
        _mainGamePanel.HideRogueCardChoices();
        AdvanceToNextLevelAfterReward();
    }

    private void AdvanceToNextLevelAfterReward()
    {
        if (_gameSession == null || _levelModel == null)
        {
            return;
        }

        int completedLevelId = _levelModel.LevelId;
        if (!_gameSession.StartNextLevel())
        {
            Debug.Log($"[MainGameController] No next level configured after level id={completedLevelId}.");
            RefreshLevelDisplay();
            return;
        }

        _rogueRewardOfferedForCurrentLevel = false;
        BindLevel(_gameSession.CurrentLevel);
        RefreshLevelDisplay();
    }

    private RogueCardConfig FindRogueRewardChoice(int cardId)
    {
        IReadOnlyList<RogueCardConfig> choices = _currentRogueRewardOffer != null ? _currentRogueRewardOffer.Choices : null;
        int count = choices != null ? choices.Count : 0;
        for (int i = 0; i < count; i++)
        {
            if (choices[i] != null && choices[i].Id == cardId)
            {
                return choices[i];
            }
        }

        return null;
    }

    private void HandleRogueCardChanged(RogueCardInventoryEntryModel card)
    {
        if (_mainGamePanel != null && _rogueCardInventory != null)
        {
            _mainGamePanel.RefreshOwnedRogueCards(_rogueCardInventory.OwnedCards);
        }
    }

    private ScratchCardFocusPanelModel BuildFocusPanelModel(ScratchCardController scratchCard)
    {
        ScratchCardModel model = scratchCard != null ? scratchCard.Model : null;
        if (model == null)
        {
            return null;
        }

        ScratchCardTypeConfig cardTypeConfig = ScratchCardDefaultsProvider.GetCardType(model.CardTypeId);
        if (cardTypeConfig == null)
        {
            return null;
        }

        ScratchPatternPoolConfig poolConfig = ScratchCardDefaultsProvider.GetPatternPool(cardTypeConfig.PatternPoolId);
        RogueCardRunModifierModel runModifiers = _gameSession != null ? _gameSession.RunModifiers : null;
        List<ScratchPatternWeightEntry> effectiveWeights = ScratchCardGenerator.BuildEffectivePatternWeights(
            poolConfig,
            cardTypeConfig.Id,
            runModifiers);
        if (effectiveWeights.Count == 0)
        {
            return new ScratchCardFocusPanelModel(
                cardTypeConfig.Name,
                poolConfig != null ? poolConfig.Name : "无图案池",
                new List<ScratchCardFocusPatternInfo>(),
                cardTypeConfig.WinDescription);
        }

        float totalWeight = 0f;
        for (int i = 0; i < effectiveWeights.Count; i++)
        {
            ScratchPatternWeightEntry entry = effectiveWeights[i];
            if (entry != null)
            {
                totalWeight += entry.Weight;
            }
        }

        var patterns = new List<ScratchCardFocusPatternInfo>();
        for (int i = 0; i < effectiveWeights.Count; i++)
        {
            ScratchPatternWeightEntry entry = effectiveWeights[i];
            if (entry == null)
            {
                continue;
            }

            float weight = entry.Weight;
            if (weight <= 0f)
            {
                continue;
            }

            ScratchPatternConfig patternConfig = ScratchPatternDefaultProvider.GetById(entry.PatternId);
            if (patternConfig == null)
            {
                continue;
            }

            int baseScoreBonus = runModifiers != null ? runModifiers.GetPatternBaseScoreBonus(patternConfig.Id) : 0;
            bool isProbabilityEnhanced = entry.IsDynamicAdded || (runModifiers != null && runModifiers.GetPatternWeightBonus(patternConfig.Id) != 0d);
            float probability = totalWeight > 0 ? (float)weight / totalWeight : 0f;
            patterns.Add(new ScratchCardFocusPatternInfo(
                patternConfig.Id,
                patternConfig.Name,
                patternConfig.BaseScore + baseScoreBonus,
                baseScoreBonus != 0,
                isProbabilityEnhanced,
                Mathf.RoundToInt(weight),
                probability,
                patternConfig.AtlasPath,
                patternConfig.SpriteName));
        }

        return new ScratchCardFocusPanelModel(
            cardTypeConfig.Name,
            poolConfig != null ? poolConfig.Name : "动态图案池",
            patterns,
            cardTypeConfig.WinDescription);
    }
}
