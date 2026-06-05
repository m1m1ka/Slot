using UnityEngine;
using UI; // 引入 UIManager 所在的空间
using Core; // 引入 PoolManager
using System.Collections;
using System.Collections.Generic;
using Configs;
using UnityEngine.UI;

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
    private ScratchToolInventoryModel _scratchToolInventory;
    private ScratchCardInventoryModel _scratchCardInventory;
    private readonly RogueCardRewardService _rogueCardRewardService = new RogueCardRewardService();
    private readonly RogueCardEffectService _rogueCardEffectService = new RogueCardEffectService();
    private RogueCardRewardOfferModel _currentRogueRewardOffer;
    private List<ScratchToolConfig> _currentScratchToolRewardChoices;
    private List<ScratchCardTypeConfig> _currentScratchCardRewardChoices;
    private LevelConfig _pendingNextLevelConfig;
    private bool _rogueRewardOfferedForCurrentLevel;
    private bool _scratchToolRewardOfferedForCurrentLevel;
    private bool _scratchCardRewardOfferedForCurrentLevel;
    private bool _winPanelShownForCurrentLevel;
    private bool _rogueRewardClaimedForCurrentLevel;
    private bool _scratchToolRewardClaimedForCurrentLevel;
    private bool _scratchCardRewardClaimedForCurrentLevel;
    private Coroutine _showWinPanelRoutine;

    // 存储当前动态加载的商店项引用
    private readonly List<ShopItemView> _shopItems = new List<ShopItemView>();
    private readonly Dictionary<ShopItemView, double> _shopItemPrices = new Dictionary<ShopItemView, double>();
    private readonly List<ScratchCardController> _activeScratchCards = new List<ScratchCardController>();
    private readonly List<ScratchToolView> _scratchToolViews = new List<ScratchToolView>();

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
        _scratchToolInventory = appRoot.PlayerContext.ScratchTools;
        _scratchCardInventory = appRoot.PlayerContext.ScratchCards;
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
        LoadScratchToolViews();

        // 2. 将数据层的事件绑定到当前 Controller 的响应方法中
        _playerModel.OnCoinsChanged += HandleCoinsChanged;
        if (_mainGamePanel != null)
        {
            _mainGamePanel.OnRogueRewardCardSelected += HandleRogueRewardCardSelected;
            _mainGamePanel.OnRogueRewardRequested += HandleRogueRewardRequested;
            _mainGamePanel.OnScratchToolRewardSelected += HandleScratchToolRewardSelected;
            _mainGamePanel.OnScratchToolRewardRequested += HandleScratchToolRewardRequested;
            _mainGamePanel.OnScratchCardRewardSelected += HandleScratchCardRewardSelected;
            _mainGamePanel.OnScratchCardRewardRequested += HandleScratchCardRewardRequested;
            _mainGamePanel.OnWinContinueRequested += HandleWinContinueRequested;
            _mainGamePanel.OnNewLevelStartRequested += HandleNewLevelStartRequested;
        }

        if (_rogueCardInventory != null)
        {
            _rogueCardInventory.OnCardChanged += HandleRogueCardChanged;
            _mainGamePanel?.RefreshOwnedRogueCards(_rogueCardInventory.OwnedCards);
        }

        if (_scratchToolInventory != null)
        {
            _scratchToolInventory.OnToolAdded += HandleScratchToolAdded;
        }

        if (_scratchCardInventory != null)
        {
            _scratchCardInventory.OnCardTypeAdded += HandleScratchCardTypeAdded;
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

        ClearShopItems();

        // 根据最新规范，从 Resources/UI 目录读取该预制体
        GameObject shopItemPrefab = AssetProvider.LoadPrefab("UI/ShopItemView");
        if (shopItemPrefab == null)
        {
            Debug.LogError("没有找到 UI/ShopItemView 预制体，无法生成购买列表！");
            return;
        }

        IReadOnlyList<ScratchCardTypeConfig> ownedCardTypes = _scratchCardInventory != null
            ? _scratchCardInventory.OwnedCardTypes
            : ScratchCardInventoryModel.GetStarterCardTypes();
        int ownedCount = ownedCardTypes != null ? ownedCardTypes.Count : 0;
        for (int i = 0; i < ownedCount; i++)
        {
            ScratchCardTypeConfig cardTypeConfig = ownedCardTypes[i];
            if (cardTypeConfig == null)
            {
                continue;
            }

            // 通过架构自带的核心对象池 (PoolManager) 生成 View
            GameObject itemObj = PoolManager.Instance.Spawn(shopItemPrefab, _mainGamePanel.SlotListRoot);
            ResetShopItemTransform(itemObj);
            ShopItemView itemView = itemObj.GetComponent<ShopItemView>();

            if (itemView != null)
            {
                double cardPrice = GetScratchCardPrice(cardTypeConfig.Id);
                // View 只展示配置数据，购买校验和扣费留在 Controller。
                itemView.SetData(
                    cardTypeConfig.Id,
                    cardTypeConfig.Name,
                    cardPrice,
                    cardTypeConfig.ShopIconPath);
                itemView.UpdateAffordability(_playerModel != null && _levelModel != null && _levelModel.CanPurchaseScratchCard && _playerModel.Coins >= cardPrice);

                // 核心：由统一的主 Controller 监听所有个体的购买点击意图
                itemView.OnBuyClicked += HandleBuyRequest;
                
                _shopItems.Add(itemView);
                _shopItemPrices[itemView] = cardPrice;
            }
        }

        RefreshShopListLayout();
    }

    private void ResetShopItemTransform(GameObject itemObj)
    {
        if (itemObj == null || _mainGamePanel == null || _mainGamePanel.SlotListRoot == null)
        {
            return;
        }

        Transform itemTransform = itemObj.transform;
        itemTransform.SetParent(_mainGamePanel.SlotListRoot, false);
        itemTransform.localScale = Vector3.one;
        itemTransform.localRotation = Quaternion.identity;

        RectTransform itemRect = itemTransform as RectTransform;
        if (itemRect != null)
        {
            itemRect.anchoredPosition3D = Vector3.zero;
        }
    }

    private void ClearShopItems()
    {
        foreach (var item in _shopItems)
        {
            if (item != null)
            {
                item.OnBuyClicked -= HandleBuyRequest;
                if (PoolManager.Instance != null && item.gameObject != null)
                {
                    PoolManager.Instance.Despawn(item.gameObject);
                }
            }
        }

        _shopItems.Clear();
        _shopItemPrices.Clear();
        RefreshShopListLayout();
    }

    private void RefreshShopListLayout()
    {
        RectTransform slotListRect = _mainGamePanel != null ? _mainGamePanel.SlotListRoot as RectTransform : null;
        if (slotListRect == null)
        {
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(slotListRect);
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

        if (_scratchCardInventory != null && !_scratchCardInventory.HasCardType(cardTypeConfig.Id))
        {
            Debug.LogWarning($"[MainGameController] Cannot buy scratch card: card type id={cardTypeConfig.Id} is not unlocked.");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            return;
        }

        if (RequestBuySlot(slotId, GetScratchCardPrice(cardTypeConfig.Id)))
        {
            PlayShopItemPurchaseFeedback(slotId);
        }
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
    public bool RequestBuySlot(int slotId, double cost)
    {
        if (_levelModel == null || !_levelModel.CanPurchaseScratchCard)
        {
            Debug.LogWarning("[MainGameController] Cannot buy scratch card: level passed or level model missing.");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            return false;
        }

        ScratchCardTypeConfig cardTypeConfig = ScratchCardDefaultsProvider.GetCardTypeForShopSlot(slotId);
        if (_scratchCardInventory != null && (cardTypeConfig == null || !_scratchCardInventory.HasCardType(cardTypeConfig.Id)))
        {
            Debug.LogWarning($"[MainGameController] Cannot buy scratch card: card type id={slotId} is not unlocked.");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            return false;
        }

        if (cost <= 0)
        {
            if (!_levelModel.TryRecordScratchCardPurchase())
            {
                Debug.LogWarning("[MainGameController] Cannot buy scratch card: level passed or level model missing.");
                AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
                RefreshLevelDisplay();
                return false;
            }

            RefreshLevelDisplay();
            AudioManager.Instance?.PlayCue(AudioCueId.BuyScratchCard);
            SpawnScratchCard(slotId);
            return true;
        }

        // 核心判断均在 Controller 处理
        if (_playerModel.ConsumeCoins(cost))
        {
            if (!_levelModel.TryRecordScratchCardPurchase())
            {
                _playerModel.AddCoins(cost);
                Debug.LogWarning("[MainGameController] Purchase cancelled: level passed after cost check.");
                AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
                RefreshLevelDisplay();
                return false;
            }

            Debug.Log($"成功花费 {cost} 购买刮刮卡 {slotId}");
            RefreshLevelDisplay();
            AudioManager.Instance?.PlayCue(AudioCueId.BuyScratchCard);
            SpawnScratchCard(slotId);
            return true;
        }
        else
        {
            Debug.LogWarning("金币不足，无法购买！");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            // TODO: 通知 View 播放“余额不足”的飘字或震动动画
            return false;
        }
    }

    private void PlayShopItemPurchaseFeedback(int slotId)
    {
        for (int i = 0; i < _shopItems.Count; i++)
        {
            ShopItemView item = _shopItems[i];
            if (item != null && item.SlotId == slotId)
            {
                item.PlayPurchaseFeedback();
                return;
            }
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
            _mainGamePanel.OnRogueRewardRequested -= HandleRogueRewardRequested;
            _mainGamePanel.OnScratchToolRewardSelected -= HandleScratchToolRewardSelected;
            _mainGamePanel.OnScratchToolRewardRequested -= HandleScratchToolRewardRequested;
            _mainGamePanel.OnScratchCardRewardSelected -= HandleScratchCardRewardSelected;
            _mainGamePanel.OnScratchCardRewardRequested -= HandleScratchCardRewardRequested;
            _mainGamePanel.OnWinContinueRequested -= HandleWinContinueRequested;
            _mainGamePanel.OnNewLevelStartRequested -= HandleNewLevelStartRequested;
        }

        if (_rogueCardInventory != null)
        {
            _rogueCardInventory.OnCardChanged -= HandleRogueCardChanged;
        }

        if (_scratchToolInventory != null)
        {
            _scratchToolInventory.OnToolAdded -= HandleScratchToolAdded;
        }

        if (_scratchCardInventory != null)
        {
            _scratchCardInventory.OnCardTypeAdded -= HandleScratchCardTypeAdded;
        }

        // 清理由于事件带来的绑定关系及所有的子 View 对象池回收
        ClearShopItems();

        ClearScratchToolViews();

        foreach (var scratchCard in _activeScratchCards)
        {
            if (scratchCard != null && PoolManager.Instance != null)
            {
                scratchCard.OnFocusStateChanged -= HandleScratchCardFocusStateChanged;
                scratchCard.OnRewardClaimed -= HandleScratchCardRewardClaimed;
                scratchCard.OnScratchToolScoreSettled -= HandleScratchToolScoreSettled;
                scratchCard.OnRogueCardEffectTriggered -= HandleRogueCardEffectTriggered;
                scratchCard.OnPatternScored -= HandleScratchCardPatternScored;
                scratchCard.OnCoinRainEffectRequested -= HandleScratchCardCoinRainEffectRequested;
                scratchCard.OnScratchCardTypeMultiplierBonusAdded -= HandleScratchCardTypeMultiplierBonusAdded;
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
            _scratchToolInventory?.OwnedTools,
            runModifiers != null ? runModifiers.GetScratchCardMultiplierForCardType(cardTypeConfig.Id) : 1d,
            runModifiers != null ? runModifiers.SettlementScoreBonus : 0,
            runModifiers != null ? new List<int>(runModifiers.GetSettlementScoreBonusSourceCardIds()) : null,
            runModifiers != null ? runModifiers.SettlementMultiplierBonus : 0d,
            runModifiers != null ? new List<int>(runModifiers.GetSettlementMultiplierBonusSourceCardIds()) : null,
            runModifiers != null ? runModifiers.GetPatternConversionRulesForCardType(cardTypeConfig.Id) : null,
            runModifiers != null ? runModifiers.GetAdjacentPatternMetalConversionRulesForCardType(cardTypeConfig.Id) : null,
            runModifiers != null ? runModifiers.GetPatternSettlementScoreBonusRulesForCardType(cardTypeConfig.Id) : null,
            runModifiers != null ? runModifiers.GetPatternSettlementMultiplierBonusRulesForCardType(cardTypeConfig.Id) : null);

        scratchCardController.Initialize(model, spawnFrom, targetPosition);
        scratchCardController.OnFocusStateChanged += HandleScratchCardFocusStateChanged;
        scratchCardController.OnRewardClaimed += HandleScratchCardRewardClaimed;
        scratchCardController.OnScratchToolScoreSettled += HandleScratchToolScoreSettled;
        scratchCardController.OnRogueCardEffectTriggered += HandleRogueCardEffectTriggered;
        scratchCardController.OnPatternScored += HandleScratchCardPatternScored;
        scratchCardController.OnCoinRainEffectRequested += HandleScratchCardCoinRainEffectRequested;
        scratchCardController.OnScratchCardTypeMultiplierBonusAdded += HandleScratchCardTypeMultiplierBonusAdded;
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
            scratchCard.OnScratchToolScoreSettled -= HandleScratchToolScoreSettled;
            scratchCard.OnRogueCardEffectTriggered -= HandleRogueCardEffectTriggered;
            scratchCard.OnPatternScored -= HandleScratchCardPatternScored;
            scratchCard.OnCoinRainEffectRequested -= HandleScratchCardCoinRainEffectRequested;
            scratchCard.OnScratchCardTypeMultiplierBonusAdded -= HandleScratchCardTypeMultiplierBonusAdded;
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
            ShowWinPanelForCurrentLevel();
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

    private void LoadScratchToolViews()
    {
        if (_mainGamePanel == null || _mainGamePanel.ScratchToolsListRoot == null)
        {
            return;
        }

        ClearScratchToolViews();

        GameObject scratchToolPrefab = AssetProvider.LoadPrefab("UI/ScratchToolView");
        if (scratchToolPrefab == null)
        {
            Debug.LogError("没有找到 UI/ScratchToolView 预制体，无法生成刮具列表！");
            return;
        }

        IReadOnlyList<ScratchToolConfig> ownedTools = _scratchToolInventory?.OwnedTools;
        int count = ownedTools != null ? ownedTools.Count : 0;
        for (int i = 0; i < count; i++)
        {
            ScratchToolConfig toolConfig = ownedTools[i];
            if (toolConfig == null)
            {
                continue;
            }

            GameObject itemObj = PoolManager.Instance.Spawn(scratchToolPrefab, _mainGamePanel.ScratchToolsListRoot);
            itemObj.transform.localScale = Vector3.one;
            ScratchToolView toolView = itemObj.GetComponent<ScratchToolView>();
            if (toolView == null)
            {
                Debug.LogWarning("[MainGameController] ScratchToolView 预制体缺少 ScratchToolView 组件。");
                PoolManager.Instance.Despawn(itemObj);
                continue;
            }

            toolView.Bind(toolConfig);
            _scratchToolViews.Add(toolView);
        }
    }

    private void ClearScratchToolViews()
    {
        for (int i = 0; i < _scratchToolViews.Count; i++)
        {
            ScratchToolView toolView = _scratchToolViews[i];
            if (toolView != null && PoolManager.Instance != null)
            {
                PoolManager.Instance.Despawn(toolView.gameObject);
            }
        }

        _scratchToolViews.Clear();
    }

    private void HandleScratchToolAdded(ScratchToolConfig toolConfig)
    {
        LoadScratchToolViews();
    }

    private double GetScratchCardPrice(int cardTypeId)
    {
        int levelId = _levelModel != null ? _levelModel.LevelId : 1;
        return ScratchCardDefaultsProvider.GetCardTypePrice(cardTypeId, levelId);
    }

    private void HandleScratchCardTypeAdded(ScratchCardTypeConfig cardTypeConfig)
    {
        LoadShopItems();
        if (_playerModel != null)
        {
            RefreshShopItemAffordability(_playerModel.Coins);
        }
    }

    private void HandleScratchToolScoreSettled(ScratchCardController scratchCard, int scratchToolId)
    {
        for (int i = 0; i < _scratchToolViews.Count; i++)
        {
            ScratchToolView toolView = _scratchToolViews[i];
            if (toolView != null && toolView.ToolId == scratchToolId)
            {
                toolView.PlaySettlementPulse();
                return;
            }
        }
    }

    private void HandleRogueCardEffectTriggered(ScratchCardController scratchCard, int rogueCardId)
    {
        _mainGamePanel?.PlayOwnedRogueCardEffect(rogueCardId);
    }

    private void HandleScratchCardPatternScored(ScratchCardController scratchCard, int patternId)
    {
        RogueCardRunModifierModel runModifiers = _gameSession != null ? _gameSession.RunModifiers : null;
        runModifiers?.ApplyPatternBaseScoreGrowthOnScore(patternId);
    }

    private void HandleScratchCardCoinRainEffectRequested(ScratchCardController scratchCard, string text)
    {
        _mainGamePanel?.PlayCoinRainEffect(text);
    }

    private void HandleScratchCardTypeMultiplierBonusAdded(ScratchCardController scratchCard, int cardTypeId, double bonus)
    {
        RogueCardRunModifierModel runModifiers = _gameSession != null ? _gameSession.RunModifiers : null;
        runModifiers?.AddScratchCardTypeMultiplierBonus(cardTypeId, bonus);
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

        int levelId = _levelModel != null ? _levelModel.LevelId : 1;
        _currentRogueRewardOffer = _rogueCardRewardService.CreateRewardOffer(levelId, _rogueCardInventory?.OwnedCards, 3);
        _rogueRewardOfferedForCurrentLevel = true;
        RefreshWinPanelRewardState();
        _mainGamePanel.ShowRogueCardChoices(_currentRogueRewardOffer.Choices, _rogueCardInventory?.OwnedCards);

        int choiceCount = _currentRogueRewardOffer != null && _currentRogueRewardOffer.Choices != null
            ? _currentRogueRewardOffer.Choices.Count
            : 0;
        if (choiceCount == 0)
        {
            _rogueRewardClaimedForCurrentLevel = true;
            RefreshWinPanelRewardState();
        }
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
        _rogueRewardClaimedForCurrentLevel = true;
        RefreshWinPanelRewardState();
    }

    private void ShowWinPanelForCurrentLevel()
    {
        if (_winPanelShownForCurrentLevel || _mainGamePanel == null)
        {
            return;
        }

        _winPanelShownForCurrentLevel = true;
        _pendingNextLevelConfig = null;
        _rogueRewardClaimedForCurrentLevel = !HasRogueRewardForCurrentLevel();
        _scratchToolRewardClaimedForCurrentLevel = !HasScratchToolRewardForCurrentLevel();
        _scratchCardRewardClaimedForCurrentLevel = !HasScratchCardRewardForCurrentLevel();
        _mainGamePanel.HideRogueCardChoices();
        _mainGamePanel.HideScratchToolChoices();
        _mainGamePanel.HideScratchCardChoices();
        if (_showWinPanelRoutine != null)
        {
            StopCoroutine(_showWinPanelRoutine);
        }

        _showWinPanelRoutine = StartCoroutine(ShowWinPanelAfterLevelGoalEffect());
    }

    private IEnumerator ShowWinPanelAfterLevelGoalEffect()
    {
        AudioManager.Instance?.PlayLoopCue(AudioCueId.LevelPassCharging);

        bool effectCompleted = false;
        _mainGamePanel.PlayLevelGoalWinEffect(() => effectCompleted = true);
        while (!effectCompleted)
        {
            yield return null;
        }

        AudioManager.Instance?.StopLoopCue(AudioCueId.LevelPassCharging);
        AudioManager.Instance?.PlayCue(AudioCueId.LevelPassWin);

        _mainGamePanel.ShowWinPanel(
            HasRogueRewardForCurrentLevel() && !_rogueRewardClaimedForCurrentLevel,
            false,
            HasScratchToolRewardForCurrentLevel() && !_scratchToolRewardClaimedForCurrentLevel,
            HasScratchCardRewardForCurrentLevel() && !_scratchCardRewardClaimedForCurrentLevel);
        _showWinPanelRoutine = null;
    }

    private void HandleRogueRewardRequested()
    {
        if (_levelModel == null || !_levelModel.IsPassed || !HasRogueRewardForCurrentLevel())
        {
            return;
        }

        AudioManager.Instance?.PlayCue(AudioCueId.UiClick);
        ShowRogueRewardChoices();
    }

    private void HandleScratchToolRewardRequested()
    {
        if (_levelModel == null || !_levelModel.IsPassed || !HasScratchToolRewardForCurrentLevel())
        {
            return;
        }

        AudioManager.Instance?.PlayCue(AudioCueId.UiClick);
        ShowScratchToolRewardChoices();
    }

    private void HandleScratchCardRewardRequested()
    {
        if (_levelModel == null || !_levelModel.IsPassed || !HasScratchCardRewardForCurrentLevel())
        {
            return;
        }

        AudioManager.Instance?.PlayCue(AudioCueId.UiClick);
        ShowScratchCardRewardChoices();
    }

    private void ShowScratchToolRewardChoices()
    {
        if (_scratchToolRewardOfferedForCurrentLevel || _mainGamePanel == null)
        {
            return;
        }

        _currentScratchToolRewardChoices = CreateScratchToolRewardChoices(3);
        _scratchToolRewardOfferedForCurrentLevel = true;
        RefreshWinPanelRewardState();

        int choiceCount = _currentScratchToolRewardChoices != null ? _currentScratchToolRewardChoices.Count : 0;
        if (choiceCount == 0)
        {
            _scratchToolRewardClaimedForCurrentLevel = true;
            RefreshWinPanelRewardState();
            return;
        }

        _mainGamePanel.ShowScratchToolChoices(_currentScratchToolRewardChoices);
    }

    private void ShowScratchCardRewardChoices()
    {
        if (_scratchCardRewardOfferedForCurrentLevel || _mainGamePanel == null)
        {
            return;
        }

        _currentScratchCardRewardChoices = CreateScratchCardRewardChoices(3);
        _scratchCardRewardOfferedForCurrentLevel = true;
        RefreshWinPanelRewardState();

        int choiceCount = _currentScratchCardRewardChoices != null ? _currentScratchCardRewardChoices.Count : 0;
        if (choiceCount == 0)
        {
            _scratchCardRewardClaimedForCurrentLevel = true;
            RefreshWinPanelRewardState();
            return;
        }

        _mainGamePanel.ShowScratchCardChoices(_currentScratchCardRewardChoices);
    }

    private void HandleScratchToolRewardSelected(int toolId)
    {
        if (_currentScratchToolRewardChoices == null || _scratchToolInventory == null)
        {
            return;
        }

        ScratchToolConfig selectedTool = FindScratchToolRewardChoice(toolId);
        if (selectedTool == null)
        {
            Debug.LogWarning($"[MainGameController] Selected scratch tool id={toolId} was not found in current offer.");
            return;
        }

        _scratchToolInventory.AddTool(selectedTool);
        _currentScratchToolRewardChoices = null;
        _mainGamePanel.HideScratchToolChoices();
        _scratchToolRewardClaimedForCurrentLevel = true;
        RefreshWinPanelRewardState();
    }

    private void HandleScratchCardRewardSelected(int cardTypeId)
    {
        if (_currentScratchCardRewardChoices == null || _scratchCardInventory == null)
        {
            return;
        }

        ScratchCardTypeConfig selectedCardType = FindScratchCardRewardChoice(cardTypeId);
        if (selectedCardType == null)
        {
            Debug.LogWarning($"[MainGameController] Selected scratch card type id={cardTypeId} was not found in current offer.");
            return;
        }

        _scratchCardInventory.AddCardType(selectedCardType);
        _currentScratchCardRewardChoices = null;
        _mainGamePanel.HideScratchCardChoices();
        _scratchCardRewardClaimedForCurrentLevel = true;
        RefreshWinPanelRewardState();
    }

    private void HandleWinContinueRequested()
    {
        if (_levelModel == null || !_levelModel.IsPassed)
        {
            return;
        }

        if (!_rogueRewardClaimedForCurrentLevel || !_scratchToolRewardClaimedForCurrentLevel || !_scratchCardRewardClaimedForCurrentLevel)
        {
            Debug.LogWarning("[MainGameController] Cannot continue: level rewards have not all been claimed yet.");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            return;
        }

        AudioManager.Instance?.PlayCue(AudioCueId.UiClick);
        _mainGamePanel?.HideWinPanel();
        ShowNewLevelPanelAfterReward();
    }

    private void ShowNewLevelPanelAfterReward()
    {
        if (_gameSession == null || _levelModel == null)
        {
            return;
        }

        if (_pendingNextLevelConfig != null)
        {
            _mainGamePanel?.ShowNewLevelPanel(_pendingNextLevelConfig);
            return;
        }

        int completedLevelId = _levelModel.LevelId;
        _pendingNextLevelConfig = LevelDefaultsProvider.GetNextLevel(completedLevelId);
        if (_pendingNextLevelConfig == null)
        {
            Debug.Log($"[MainGameController] No next level configured after level id={completedLevelId}.");
            _mainGamePanel?.ShowWinPanel(false, false, false);
            RefreshLevelDisplay();
            return;
        }

        _mainGamePanel?.ShowNewLevelPanel(_pendingNextLevelConfig);
    }

    private void HandleNewLevelStartRequested()
    {
        if (_gameSession == null || _pendingNextLevelConfig == null)
        {
            Debug.LogWarning("[MainGameController] Cannot start next level: pending level config is missing.");
            AudioManager.Instance?.PlayCue(AudioCueId.UiDenied);
            return;
        }

        AudioManager.Instance?.PlayCue(AudioCueId.UiClick);
        _mainGamePanel?.HideNewLevelPanel();
        _gameSession.StartLevel(_pendingNextLevelConfig);
        _pendingNextLevelConfig = null;

        _rogueRewardOfferedForCurrentLevel = false;
        _scratchToolRewardOfferedForCurrentLevel = false;
        _scratchCardRewardOfferedForCurrentLevel = false;
        _winPanelShownForCurrentLevel = false;
        _rogueRewardClaimedForCurrentLevel = false;
        _scratchToolRewardClaimedForCurrentLevel = false;
        _scratchCardRewardClaimedForCurrentLevel = false;
        _currentScratchToolRewardChoices = null;
        _currentScratchCardRewardChoices = null;
        BindLevel(_gameSession.CurrentLevel);
        LoadShopItems();
        RefreshLevelDisplay();
    }

    private void RefreshWinPanelRewardState()
    {
        if (_mainGamePanel == null || !_winPanelShownForCurrentLevel)
        {
            return;
        }

        bool canRequestRogueReward = HasRogueRewardForCurrentLevel() && !_rogueRewardOfferedForCurrentLevel && !_rogueRewardClaimedForCurrentLevel;
        bool canRequestScratchToolReward = HasScratchToolRewardForCurrentLevel() && !_scratchToolRewardOfferedForCurrentLevel && !_scratchToolRewardClaimedForCurrentLevel;
        bool canRequestScratchCardReward = HasScratchCardRewardForCurrentLevel() && !_scratchCardRewardOfferedForCurrentLevel && !_scratchCardRewardClaimedForCurrentLevel;
        bool allRewardsClaimed = _rogueRewardClaimedForCurrentLevel &&
            _scratchToolRewardClaimedForCurrentLevel &&
            _scratchCardRewardClaimedForCurrentLevel;
        if (allRewardsClaimed)
        {
            _mainGamePanel.HideWinPanel();
            ShowNewLevelPanelAfterReward();
            return;
        }

        _mainGamePanel.ShowWinPanel(canRequestRogueReward, false, canRequestScratchToolReward, canRequestScratchCardReward);
    }

    private bool HasRogueRewardForCurrentLevel()
    {
        return _levelModel != null && LevelDefaultsProvider.HasRogueCardReward(_levelModel.LevelId);
    }

    private bool HasScratchCardRewardForCurrentLevel()
    {
        return _levelModel != null && LevelDefaultsProvider.HasScratchCardReward(_levelModel.LevelId);
    }

    private bool HasScratchToolRewardForCurrentLevel()
    {
        return _levelModel != null && LevelDefaultsProvider.HasScratchToolReward(_levelModel.LevelId);
    }

    private List<ScratchToolConfig> CreateScratchToolRewardChoices(int choiceCount)
    {
        IReadOnlyList<ScratchToolConfig> allTools = ScratchToolDefaultsProvider.GetAll();
        var candidates = new List<ScratchToolConfig>();
        int count = allTools != null ? allTools.Count : 0;
        for (int i = 0; i < count; i++)
        {
            ScratchToolConfig toolConfig = allTools[i];
            if (toolConfig == null || (_scratchToolInventory != null && _scratchToolInventory.HasTool(toolConfig.Id)))
            {
                continue;
            }

            candidates.Add(toolConfig);
        }

        var choices = new List<ScratchToolConfig>();
        int resolvedChoiceCount = Mathf.Max(0, choiceCount);
        while (choices.Count < resolvedChoiceCount && candidates.Count > 0)
        {
            int index = Random.Range(0, candidates.Count);
            choices.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        return choices;
    }

    private ScratchToolConfig FindScratchToolRewardChoice(int toolId)
    {
        int count = _currentScratchToolRewardChoices != null ? _currentScratchToolRewardChoices.Count : 0;
        for (int i = 0; i < count; i++)
        {
            ScratchToolConfig toolConfig = _currentScratchToolRewardChoices[i];
            if (toolConfig != null && toolConfig.Id == toolId)
            {
                return toolConfig;
            }
        }

        return null;
    }

    private List<ScratchCardTypeConfig> CreateScratchCardRewardChoices(int choiceCount)
    {
        int levelId = _levelModel != null ? _levelModel.LevelId : 1;
        IReadOnlyList<ScratchCardTypeConfig> allCardTypes = ScratchCardDefaultsProvider.GetAvailableCardTypesForLevel(levelId);
        var candidates = new List<ScratchCardTypeConfig>();
        int count = allCardTypes != null ? allCardTypes.Count : 0;
        for (int i = 0; i < count; i++)
        {
            ScratchCardTypeConfig cardTypeConfig = allCardTypes[i];
            if (cardTypeConfig == null || (_scratchCardInventory != null && _scratchCardInventory.HasCardType(cardTypeConfig.Id)))
            {
                continue;
            }

            candidates.Add(cardTypeConfig);
        }

        var choices = new List<ScratchCardTypeConfig>();
        int resolvedChoiceCount = Mathf.Max(0, choiceCount);
        while (choices.Count < resolvedChoiceCount && candidates.Count > 0)
        {
            int index = Random.Range(0, candidates.Count);
            choices.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        return choices;
    }

    private ScratchCardTypeConfig FindScratchCardRewardChoice(int cardTypeId)
    {
        int count = _currentScratchCardRewardChoices != null ? _currentScratchCardRewardChoices.Count : 0;
        for (int i = 0; i < count; i++)
        {
            ScratchCardTypeConfig cardTypeConfig = _currentScratchCardRewardChoices[i];
            if (cardTypeConfig != null && cardTypeConfig.Id == cardTypeId)
            {
                return cardTypeConfig;
            }
        }

        return null;
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

        RogueCardRunModifierModel runModifiers = _gameSession != null ? _gameSession.RunModifiers : null;
        List<ScratchPatternWeightEntry> effectiveWeights = ScratchCardGenerator.BuildEffectivePatternWeights(cardTypeConfig, runModifiers);
        if (effectiveWeights.Count == 0)
        {
            return new ScratchCardFocusPanelModel(
                cardTypeConfig.Name,
                "全局图案池",
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
            bool isProbabilityEnhanced = entry.IsDynamicAdded ||
                entry.IsCardExtraEffectApplied ||
                (runModifiers != null && runModifiers.GetPatternWeightBonus(patternConfig.Id) != 0d);
            float probability = totalWeight > 0 ? (float)weight / totalWeight : 0f;
            patterns.Add(new ScratchCardFocusPatternInfo(
                patternConfig.Id,
                patternConfig.Name,
                entry.BaseScore + baseScoreBonus,
                baseScoreBonus != 0,
                isProbabilityEnhanced,
                Mathf.RoundToInt(weight),
                probability,
                patternConfig.AtlasPath,
                patternConfig.SpriteName,
                patternConfig.SpritePath));
        }

        return new ScratchCardFocusPanelModel(
            cardTypeConfig.Name,
            "全局图案池",
            patterns,
            cardTypeConfig.WinDescription);
    }
}
