using System.Collections;
using System.Collections.Generic;
using Configs;
using Core;
using UnityEngine;

/// <summary>
/// 彩票控制层。
/// 负责连接彩票 Model 与 View，并处理基础交互状态流转。
/// </summary>
[RequireComponent(typeof(ScratchCardView))]
public class ScratchCardController : MonoBehaviour
{
    public event System.Action<ScratchCardController, bool> OnFocusStateChanged;
    public event System.Action<ScratchCardController, ScratchSettlementResult> OnRewardClaimed;
    public event System.Action<ScratchCardController, int> OnScratchToolScoreSettled;
    public event System.Action<ScratchCardController, int> OnRogueCardEffectTriggered;
    public event System.Action<ScratchCardController, int> OnPatternScored;
    public event System.Action<ScratchCardController, string> OnCoinRainEffectRequested;
    public event System.Action<ScratchCardController> OnGameOverSkullEffectRequested;
    public event System.Action<ScratchCardController, int, double> OnScratchCardTypeMultiplierBonusAdded;

    private ScratchCardView _view;
    private ScratchCardModel _model;
    private ScratchSettlementResult _settlementResult;
    private bool _rewardClaimed;
    private bool _isSettling;
    private bool _settlementBonusesApplied;
    private bool _settlementCardExtraEffectsApplied;
    private Coroutine _settlementCoroutine;
    private float _lastScratchInputTime = float.MinValue;
    private bool _isScratchSoundPlaying;
    private AudioCueId _currentScratchSoundCueId = AudioCueId.None;
    private int _currentRevealedReward;
    private int _scratchRevealOrder;
    private int _settlementDingIndex;
    private readonly HashSet<string> _rewardMultiplierAppliedScoreKeys = new HashSet<string>();
    private readonly HashSet<string> _animatedScoreKeys = new HashSet<string>();

    private const float ScratchLoopStopDelay = 0.2f;
    private const float ToolSettlementStepDelay = 0.3f;
    private const float PatternSettlementStepDelay = 0.15f;
    private const float RogueCardSettlementStartDelay = 0.12f;
    private const float ScratchDirectionSoundThreshold = 0.5f;
    private const float SettlementDingBasePitch = 1f;
    private const float SettlementDingPitchStep = 0.12f;
    private const float SettlementDingMaxPitch = 1.8f;
    private const int GoodJokerPatternId = 27;
    private const int GameOverScratchCardTypeId = 13;
    private const int SkullPatternId = 36;
    private const int CoinPilePatternId = 37;

    public ScratchCardModel Model => _model;
    public int CurrentRevealedReward => _currentRevealedReward;

    public void Initialize(ScratchCardModel model, Vector2 spawnFrom, Vector2 spawnTo)
    {
        UnbindModel();
        UnbindView();

        _model = model;
        _view = GetComponent<ScratchCardView>();

        if (_view == null || _model == null)
        {
            Debug.LogError("[ScratchCardController] Initialize failed: missing view or model.");
            enabled = false;
            return;
        }

        BindModel();
        BindView();

        _view.BindCardData(_model.Cells);
        _view.SetupInitialVisual();
        _view.PlaySpawnAnimation(spawnFrom, spawnTo);
        _settlementResult = null;
        _rewardClaimed = false;
        _isSettling = false;
        _settlementBonusesApplied = false;
        _settlementCardExtraEffectsApplied = false;
        _settlementCoroutine = null;
        _currentRevealedReward = 0;
        _scratchRevealOrder = 0;
        _settlementDingIndex = 0;
        _rewardMultiplierAppliedScoreKeys.Clear();
        _animatedScoreKeys.Clear();
    }

    private void Update()
    {
        if (TutorialPanel.BlocksScratchCardInput)
        {
            StopScratchSound();
            return;
        }

        if (!IsInFocusedState() || _view == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryExitFocus(Input.mousePosition);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            TryExitFocus(Input.GetTouch(0).position);
        }

        if (_isScratchSoundPlaying && Time.unscaledTime - _lastScratchInputTime > ScratchLoopStopDelay)
        {
            StopScratchSound();
        }
    }

    private void BindModel()
    {
        _model.OnScratchProgressChanged += HandleScratchProgressChanged;
        _model.OnStateChanged += HandleStateChanged;
        _model.OnScratchCompleted += HandleScratchCompleted;
        _model.OnRewardMultiplierChanged += HandleRewardMultiplierChanged;
    }

    private void BindView()
    {
        _view.OnCardClicked += HandleCardClicked;
        _view.OnScratchDragged += HandleScratchDragged;
        _view.OnScratchLayerCleared += HandleScratchLayerCleared;
        _view.OnScratchCellRevealed += HandleScratchCellRevealed;
        _view.OnSpawnAnimationFinished += HandleSpawnFinished;
        _view.OnClaimRewardClicked += HandleClaimRewardClicked;
    }

    private void HandleSpawnFinished()
    {
        AudioManager.Instance?.PlayCue(AudioCueId.ScratchCardSpawned);
        _model.SetState(ScratchCardModel.ScratchCardState.Idle);
    }

    private void HandleCardClicked()
    {
        if (TutorialPanel.BlocksScratchCardInput)
        {
            return;
        }

        if (_model.State == ScratchCardModel.ScratchCardState.Falling)
        {
            return;
        }

        _model.SetState(ScratchCardModel.ScratchCardState.Focused);
        AudioManager.Instance?.PlayCue(AudioCueId.ScratchCardFocused);
        EventBus.Publish(new TutorialEvent(TutorialEventType.ScratchCardClicked));
    }

    private void HandleScratchDragged(float amount, float horizontalDelta)
    {
        if (TutorialPanel.BlocksScratchCardInput)
        {
            StopScratchSound();
            return;
        }

        if (_isSettling || _settlementResult != null)
        {
            return;
        }

        if (_model.State == ScratchCardModel.ScratchCardState.Focused ||
            _model.State == ScratchCardModel.ScratchCardState.Scratching)
        {
            _lastScratchInputTime = Time.unscaledTime;
            StartDirectionalScratchSound(horizontalDelta);
            _model.SetScratchProgress(amount);
        }
    }

    private void HandleScratchProgressChanged(float progress)
    {
        _view.SetScratchProgress(progress);
    }

    private void HandleStateChanged(ScratchCardModel.ScratchCardState state)
    {
        switch (state)
        {
            case ScratchCardModel.ScratchCardState.Focused:
            case ScratchCardModel.ScratchCardState.Scratching:
                OnFocusStateChanged?.Invoke(this, true);
                _view.SetFocused(true);
                UpdateSettlementButtonView();
                break;
            case ScratchCardModel.ScratchCardState.Idle:
                StopScratchSound();
                OnFocusStateChanged?.Invoke(this, false);
                _view.SetFocused(false);
                _view.HideClaimRewardButton();
                break;
            case ScratchCardModel.ScratchCardState.Completed:
                StopScratchSound();
                OnFocusStateChanged?.Invoke(this, true);
                _view.SetFocused(true);
                UpdateSettlementButtonView();
                break;
        }
    }

    private void HandleScratchCompleted()
    {
        Debug.Log(
            $"[ScratchCardController] Scratch card {_model.CardId} completed. " +
            $"Type={_model.CardTypeName}, Base={_model.TotalBaseScore}");
        EventBus.Publish(new TutorialEvent(TutorialEventType.ScratchCardCompleted));
    }

    private void HandleRewardMultiplierChanged(double rewardMultiplier)
    {
        if (_view == null)
        {
            return;
        }

        if (_settlementResult != null)
        {
            _settlementResult.FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(
                _model,
                _settlementResult.ScoreBeforeRewardMultiplier);
        }

        UpdateSettlementButtonView();
    }

    private void HandleScratchLayerCleared()
    {
    }

    private void HandleScratchCellRevealed(int cellIndex)
    {
        if (_isSettling || _settlementResult != null)
        {
            return;
        }

        if (_model == null || _model.Cells == null || cellIndex < 0 || cellIndex >= _model.Cells.Count)
        {
            return;
        }

        ScratchCellModel cell = _model.Cells[cellIndex];
        if (cell == null || !cell.IsScratchable || cell.IsScratched)
        {
            return;
        }

        cell.MarkScratched(++_scratchRevealOrder);
        AudioManager.Instance?.PlaySfx("Audio/Sfx/Pop");
        _view.PlayPatternRevealHighlight(cellIndex);
        TryApplyRevealPatternConversion(cellIndex, cell);
        TryApplyAdjacentMetalPatternConversion(cell);
        ApplyCellRevealEffects(cell);
    }

    private void TryApplyRevealPatternConversion(int cellIndex, ScratchCellModel cell)
    {
        if (cell == null || _model?.PatternRevealConversionRules == null || _model.PatternRevealConversionRules.Count == 0)
        {
            return;
        }

        var triggeredSourceCardIds = new HashSet<int>();
        int targetPatternId = cell.PatternId;
        for (int i = 0; i < _model.PatternRevealConversionRules.Count; i++)
        {
            PatternRevealConversionRuleModel rule = _model.PatternRevealConversionRules[i];
            if (rule == null || !rule.MatchesPattern(cell.PatternId) || Random.value > (float)rule.Chance)
            {
                continue;
            }

            targetPatternId = rule.PickTargetPatternId(cell.PatternId);
            if (targetPatternId <= 0)
            {
                continue;
            }

            if (rule.SourceCardId > 0)
            {
                triggeredSourceCardIds.Add(rule.SourceCardId);
            }
        }

        if (targetPatternId == cell.PatternId)
        {
            return;
        }

        ScratchPatternConfig targetPattern = ScratchPatternDefaultProvider.GetById(targetPatternId);
        if (targetPattern == null)
        {
            return;
        }

        var cellSourceIds = new HashSet<int>();
        AddRogueCardEffectSourceIds(cellSourceIds, cell);
        foreach (int sourceCardId in triggeredSourceCardIds)
        {
            if (sourceCardId > 0)
            {
                cellSourceIds.Add(sourceCardId);
            }
        }

        cell.TransformPattern(targetPattern, cell.BaseScore, cell.IsBaseScoreEnhanced, new List<int>(cellSourceIds));
        _view.RefreshPatternVisual(cellIndex, cell);
        _view.PlayPatternRevealHighlight(cellIndex);
        _view.PlayPatternEffectTextReveal(cellIndex, targetPattern.Name);
        RaiseRogueCardEffectTriggered(triggeredSourceCardIds);
    }

    private void TryApplyAdjacentMetalPatternConversion(ScratchCellModel sourceCell)
    {
        if (sourceCell == null ||
            _model?.AdjacentPatternMetalConversionRules == null ||
            _model.AdjacentPatternMetalConversionRules.Count == 0 ||
            _model.Cells == null)
        {
            return;
        }

        var triggeredSourceCardIds = new HashSet<int>();
        for (int ruleIndex = 0; ruleIndex < _model.AdjacentPatternMetalConversionRules.Count; ruleIndex++)
        {
            AdjacentPatternMetalConversionRuleModel rule = _model.AdjacentPatternMetalConversionRules[ruleIndex];
            if (rule == null || !rule.IsMetalPattern(sourceCell.PatternId))
            {
                continue;
            }

            for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
                {
                    if (rowOffset == 0 && columnOffset == 0)
                    {
                        continue;
                    }

                    TryConvertAdjacentCellToMetal(
                        sourceCell.Row + rowOffset,
                        sourceCell.Column + columnOffset,
                        rule,
                        triggeredSourceCardIds);
                }
            }
        }

        RaiseRogueCardEffectTriggered(triggeredSourceCardIds);
    }

    private void TryConvertAdjacentCellToMetal(
        int row,
        int column,
        AdjacentPatternMetalConversionRuleModel rule,
        HashSet<int> triggeredSourceCardIds)
    {
        if (rule == null ||
            row < 0 ||
            column < 0 ||
            row >= _model.GridHeight ||
            column >= _model.GridWidth ||
            Random.value > (float)rule.Chance)
        {
            return;
        }

        int cellIndex = row * _model.GridWidth + column;
        if (cellIndex < 0 || cellIndex >= _model.Cells.Count)
        {
            return;
        }

        ScratchCellModel cell = _model.Cells[cellIndex];
        if (cell == null || !cell.IsScratchable || rule.IsMetalPattern(cell.PatternId))
        {
            return;
        }

        int targetPatternId = rule.PickMetalPatternId();
        ScratchPatternConfig targetPattern = ScratchPatternDefaultProvider.GetById(targetPatternId);
        if (targetPattern == null)
        {
            return;
        }

        var cellSourceIds = new HashSet<int>();
        AddRogueCardEffectSourceIds(cellSourceIds, cell);
        if (rule.SourceCardId > 0)
        {
            cellSourceIds.Add(rule.SourceCardId);
            triggeredSourceCardIds?.Add(rule.SourceCardId);
        }

        cell.TransformPattern(targetPattern, cell.BaseScore, cell.IsBaseScoreEnhanced, new List<int>(cellSourceIds));
        _view.RefreshPatternVisual(cellIndex, cell);
        _view.PlayPatternRevealHighlight(cellIndex);
        if (cell.IsScratched)
        {
            _view.PlayPatternEffectTextReveal(cellIndex, targetPattern.Name);
        }
    }

    private void ApplyScoredCellRewardMultiplierBonuses(ScratchSettlementResult result)
    {
        if (result?.ScoredCellIndices == null || _model?.Cells == null)
        {
            return;
        }

        for (int i = 0; i < result.ScoredCellIndices.Count; i++)
        {
            int scoredCellIndex = result.ScoredCellIndices[i];
            double scoreMultiplier = GetScoredCellMultiplier(result, i);
            string scoreKey = BuildScoreKey(scoredCellIndex, scoreMultiplier);
            if (!_rewardMultiplierAppliedScoreKeys.Add(scoreKey))
            {
                continue;
            }

            ScratchCellModel cell = scoredCellIndex >= 0 && scoredCellIndex < _model.Cells.Count
                ? _model.Cells[scoredCellIndex]
                : null;
            double bonus = ScratchPatternScoreService.GetRewardMultiplierBonusOnScore(cell);
            if (bonus > 0d)
            {
                _model.AddRewardMultiplierBonus(bonus);
            }
        }
    }

    private IEnumerator PlayNewScoreAnimations(ScratchSettlementResult result)
    {
        if (result?.ScoredCellIndices == null || _model?.Cells == null)
        {
            yield break;
        }

        var settledPatternIds = new HashSet<int>();
        for (int i = 0; i < result.ScoredCellIndices.Count; i++)
        {
            int scoredCellIndex = result.ScoredCellIndices[i];
            double scoreMultiplier = GetScoredCellMultiplier(result, i);
            string scoreKey = BuildAnimationScoreKey(result, scoredCellIndex, scoreMultiplier);
            if (_animatedScoreKeys.Contains(scoreKey))
            {
                continue;
            }

            if (scoredCellIndex < 0 || scoredCellIndex >= _model.Cells.Count)
            {
                continue;
            }

            ScratchCellModel scoredCell = _model.Cells[scoredCellIndex];
            if (scoredCell == null)
            {
                continue;
            }

            if (!settledPatternIds.Add(scoredCell.PatternId))
            {
                continue;
            }

            bool playedPatternGroup = false;
            bool hasGiantFruit = false;
            var triggeredRogueCardIds = new HashSet<int>();
            for (int j = i; j < result.ScoredCellIndices.Count; j++)
            {
                int groupedCellIndex = result.ScoredCellIndices[j];
                double groupedScoreMultiplier = GetScoredCellMultiplier(result, j);
                if (groupedCellIndex < 0 || groupedCellIndex >= _model.Cells.Count)
                {
                    continue;
                }

                ScratchCellModel groupedCell = _model.Cells[groupedCellIndex];
                if (groupedCell == null || groupedCell.PatternId != scoredCell.PatternId)
                {
                    continue;
                }

                string groupedScoreKey = BuildAnimationScoreKey(result, groupedCellIndex, groupedScoreMultiplier);
                if (!_animatedScoreKeys.Add(groupedScoreKey))
                {
                    continue;
                }

                OnPatternScored?.Invoke(this, groupedCell.PatternId);
                string floatText = GetScoredCellFloatText(result, j);
                if (!string.IsNullOrWhiteSpace(floatText))
                {
                    _view.PlayPatternEffectTextReveal(groupedCellIndex, floatText);
                }
                else
                {
                    int cellScore = ScratchPatternScoreService.GetCellScore(_model, groupedCell);
                    _view.PlayPatternScoreReveal(groupedCellIndex, cellScore, groupedCell.IsBaseScoreEnhanced, groupedScoreMultiplier);
                }

                AddRogueCardEffectSourceIds(triggeredRogueCardIds, groupedCell);
                hasGiantFruit |= groupedCell.IsGiantFruit;
                playedPatternGroup = true;
            }

            if (playedPatternGroup)
            {
                RaisePatternCoinRainEffectIfNeeded(scoredCell.PatternId, hasGiantFruit);
                RaiseRogueCardEffectTriggered(triggeredRogueCardIds);
                PlaySettlementDing();
                yield return new WaitForSecondsRealtime(PatternSettlementStepDelay);
            }
        }
    }

    private void RaisePatternCoinRainEffectIfNeeded(int patternId, bool hasGiantFruit)
    {
        if (ScratchCardDefaultsProvider.IsHighestBaseScorePatternInCardPool(_model != null ? _model.CardTypeId : 0, patternId))
        {
            RaiseCoinRainEffectRequested("头奖");
            return;
        }

        if (patternId == GoodJokerPatternId)
        {
            RaiseCoinRainEffectRequested("小丑惊喜");
            return;
        }

        if (hasGiantFruit)
        {
            RaiseCoinRainEffectRequested("巨型水果");
        }
    }

    private bool HasSpecialCoinRainTrigger(ScratchSettlementResult result)
    {
        if (result?.ScoredCellIndices == null || _model?.Cells == null)
        {
            return false;
        }

        for (int i = 0; i < result.ScoredCellIndices.Count; i++)
        {
            int cellIndex = result.ScoredCellIndices[i];
            if (cellIndex < 0 || cellIndex >= _model.Cells.Count)
            {
                continue;
            }

            ScratchCellModel cell = _model.Cells[cellIndex];
            if (cell == null)
            {
                continue;
            }

            if (ScratchCardDefaultsProvider.IsHighestBaseScorePatternInCardPool(_model.CardTypeId, cell.PatternId) ||
                cell.PatternId == GoodJokerPatternId ||
                cell.IsGiantFruit)
            {
                return true;
            }
        }

        return false;
    }

    private void RaiseCoinRainEffectRequested(string text)
    {
        OnCoinRainEffectRequested?.Invoke(this, text);
    }

    private void RaiseGameOverCoinPileCoinRainIfNeeded()
    {
        if (!HasScratchedGameOverPattern(SkullPatternId) &&
            HasScratchedGameOverPattern(CoinPilePatternId))
        {
            RaiseCoinRainEffectRequested("\u5168\u90e8\u5e26\u8d70");
        }
    }

    private void RaiseGameOverSkullEffectIfNeeded()
    {
        if (HasScratchedGameOverPattern(SkullPatternId))
        {
            OnGameOverSkullEffectRequested?.Invoke(this);
        }
    }

    private bool HasScratchedGameOverPattern(int patternId)
    {
        if (_model == null ||
            _model.CardTypeId != GameOverScratchCardTypeId ||
            _model.Cells == null)
        {
            return false;
        }

        for (int i = 0; i < _model.Cells.Count; i++)
        {
            ScratchCellModel cell = _model.Cells[i];
            if (cell != null &&
                cell.IsScratchable &&
                cell.IsScratched &&
                cell.PatternId == patternId)
            {
                return true;
            }
        }

        return false;
    }

    private void PlaySettlementDing()
    {
        float pitch = Mathf.Min(
            SettlementDingMaxPitch,
            SettlementDingBasePitch + _settlementDingIndex * SettlementDingPitchStep);
        _settlementDingIndex++;
        AudioManager.Instance?.PlaySfx("Audio/Sfx/Ding", 0.9f, pitch);
    }

    private void StartDirectionalScratchSound(float horizontalDelta)
    {
        if (_isScratchSoundPlaying)
        {
            return;
        }

        if (Mathf.Abs(horizontalDelta) < ScratchDirectionSoundThreshold)
        {
            return;
        }

        AudioCueId nextCueId = horizontalDelta > 0f
            ? AudioCueId.ScratchRight
            : AudioCueId.ScratchLeft;

        AudioManager.Instance?.PlayLoopCue(nextCueId);
        _currentScratchSoundCueId = nextCueId;
        _isScratchSoundPlaying = true;
    }

    private static double GetScoredCellMultiplier(ScratchSettlementResult result, int index)
    {
        if (result?.ScoredCellScoreMultipliers != null && index >= 0 && index < result.ScoredCellScoreMultipliers.Count)
        {
            return result.ScoredCellScoreMultipliers[index];
        }

        return 1d;
    }

    private static string BuildScoreKey(int cellIndex, double scoreMultiplier)
    {
        return $"{cellIndex}:{scoreMultiplier:0.####}";
    }

    private static string BuildAnimationScoreKey(ScratchSettlementResult result, int cellIndex, double scoreMultiplier)
    {
        int sourceToolId = result != null ? result.SourceScratchToolId : -1;
        return $"{sourceToolId}:{cellIndex}:{scoreMultiplier:0.####}";
    }

    private static string GetScoredCellFloatText(ScratchSettlementResult result, int index)
    {
        if (result?.ScoredCellFloatTexts != null && index >= 0 && index < result.ScoredCellFloatTexts.Count)
        {
            return result.ScoredCellFloatTexts[index];
        }

        return null;
    }

    private void ApplyCellRevealEffects(ScratchCellModel cell)
    {
        double bonus = ScratchPatternScoreService.GetRewardMultiplierBonusOnReveal(cell);
        if (_model == null || bonus <= 0d)
        {
            return;
        }

        _model.AddRewardMultiplierBonus(bonus);
    }

    private void HandleClaimRewardClicked()
    {
        if (_rewardClaimed || _isSettling)
        {
            return;
        }

        if (_settlementResult == null)
        {
            EventBus.Publish(new TutorialEvent(TutorialEventType.SettlementButtonClicked));
            _settlementCoroutine = StartCoroutine(SettleRevealedPatternsByToolOrder());
            return;
        }

        ClaimReward();
    }

    private IEnumerator SettleRevealedPatternsByToolOrder()
    {
        _isSettling = true;
        StopScratchSound();
        _view.SetScratchInputEnabled(false);
        _currentRevealedReward = 0;
        _settlementDingIndex = 0;
        _settlementCardExtraEffectsApplied = false;
        _view.ShowSettlementInProgressButton(_currentRevealedReward, GetDisplayRewardMultiplier());

        _settlementResult = new ScratchSettlementResult();
        List<ScratchSettlementResult> intrinsicWinResults = ScratchCardIntrinsicWinSettlementService.EvaluateByRuleOrder(_model);
        List<ScratchSettlementResult> toolResults = ScratchToolSettlementService.EvaluateByToolOrder(_model);
        var summaries = new List<string>();

        for (int i = 0; i < intrinsicWinResults.Count; i++)
        {
            ScratchSettlementResult intrinsicResult = intrinsicWinResults[i];
            if (intrinsicResult == null)
            {
                continue;
            }

            MergeSettlementResult(_settlementResult, intrinsicResult);
            if (intrinsicResult.ScoreBeforeRewardMultiplier > 0 && !HasSpecialCoinRainTrigger(intrinsicResult))
            {
                RaiseCoinRainEffectRequested("中奖");
            }

            if (!string.IsNullOrWhiteSpace(intrinsicResult.Summary))
            {
                summaries.Add(intrinsicResult.Summary);
            }

            _currentRevealedReward = _settlementResult.ScoreBeforeRewardMultiplier;
            ApplyScoredCellRewardMultiplierBonuses(intrinsicResult);
            _view.ShowSettlementInProgressButton(_currentRevealedReward, GetDisplayRewardMultiplier());
            yield return PlayNewScoreAnimations(intrinsicResult);

            if (i < intrinsicWinResults.Count - 1 || toolResults.Count > 0)
            {
                yield return new WaitForSecondsRealtime(ToolSettlementStepDelay);
            }
        }

        for (int i = 0; i < toolResults.Count; i++)
        {
            ScratchSettlementResult toolResult = toolResults[i];
            if (toolResult == null)
            {
                continue;
            }

            MergeSettlementResult(_settlementResult, toolResult);
            if (!string.IsNullOrWhiteSpace(toolResult.Summary))
            {
                summaries.Add(toolResult.Summary);
            }

            _currentRevealedReward = _settlementResult.ScoreBeforeRewardMultiplier;
            ApplyScoredCellRewardMultiplierBonuses(toolResult);
            _view.ShowSettlementInProgressButton(_currentRevealedReward, GetDisplayRewardMultiplier());
            RaiseScratchToolScoreSettled(toolResult);
            yield return PlayNewScoreAnimations(toolResult);

            if (i < toolResults.Count - 1)
            {
                yield return new WaitForSecondsRealtime(ToolSettlementStepDelay);
            }
        }

        yield return ApplySettlementRogueCardBonuses();
        ApplySettlementCardExtraEffects();

        _settlementResult.FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(
            _model,
            _settlementResult.ScoreBeforeRewardMultiplier);
        _settlementResult.Summary = summaries.Count > 0 ? string.Join(" ", summaries) : "\u6ca1\u6709\u89e6\u53d1\u522e\u5177\u8ba1\u5206\u3002";
        RaiseGameOverCoinPileCoinRainIfNeeded();
        RaiseGameOverSkullEffectIfNeeded();
        _isSettling = false;
        _settlementCoroutine = null;
        _view.ShowClaimRewardButton(GetDisplayRewardBeforeMultiplier(), GetDisplayRewardMultiplier());

        Debug.Log(
            $"[ScratchCardController] Scratch card {_model.CardId} settled. " +
            $"Type={_model.CardTypeName}, Base={_model.TotalBaseScore}, Final={_settlementResult.FinalScore}, Summary={_settlementResult.Summary}");
    }

    private void ClaimReward()
    {
        if (_rewardClaimed || _settlementResult == null)
        {
            return;
        }

        _rewardClaimed = true;
        _view.HideClaimRewardButton();
        AudioManager.Instance?.PlayCue(AudioCueId.GainMoney);
        EventBus.Publish(new TutorialEvent(TutorialEventType.RewardClaimed));
        OnRewardClaimed?.Invoke(this, _settlementResult);
    }

    private void UpdateSettlementButtonView()
    {
        if (_view == null || _rewardClaimed)
        {
            return;
        }

        if (_isSettling)
        {
            _view.ShowSettlementInProgressButton(_currentRevealedReward, GetDisplayRewardMultiplier());
            _view.SetScratchInputEnabled(false);
            return;
        }

        if (_settlementResult != null)
        {
            _view.ShowClaimRewardButton(GetDisplayRewardBeforeMultiplier(), GetDisplayRewardMultiplier());
            _view.SetScratchInputEnabled(false);
            return;
        }

        _view.ShowSettleButton(GetDisplayRewardMultiplier());
        _view.SetScratchInputEnabled(true);
    }

    private static void MergeSettlementResult(ScratchSettlementResult target, ScratchSettlementResult source)
    {
        if (target == null || source == null)
        {
            return;
        }

        target.ScoreBeforeRewardMultiplier += source.ScoreBeforeRewardMultiplier;
        AddRange(target.WinningPatternIds, source.WinningPatternIds);
        AddRange(target.ScoredCellIndices, source.ScoredCellIndices);
        AddRange(target.ScoredCellScoreMultipliers, source.ScoredCellScoreMultipliers);
        AddRange(target.ScoredCellFloatTexts, source.ScoredCellFloatTexts);
    }

    private void RaiseScratchToolScoreSettled(ScratchSettlementResult toolResult)
    {
        if (toolResult == null || toolResult.SourceScratchToolId < 0)
        {
            return;
        }

        bool hasPatternScore = toolResult.ScoredCellIndices != null && toolResult.ScoredCellIndices.Count > 0;
        if (!hasPatternScore && toolResult.ScoreBeforeRewardMultiplier <= 0)
        {
            return;
        }

        OnScratchToolScoreSettled?.Invoke(this, toolResult.SourceScratchToolId);
    }

    private void AddRogueCardEffectSourceIds(HashSet<int> target, ScratchCellModel cell)
    {
        if (target == null || cell?.RogueCardEffectSourceIds == null)
        {
            return;
        }

        for (int i = 0; i < cell.RogueCardEffectSourceIds.Count; i++)
        {
            int rogueCardId = cell.RogueCardEffectSourceIds[i];
            if (rogueCardId > 0)
            {
                target.Add(rogueCardId);
            }
        }
    }

    private void RaiseRogueCardEffectTriggered(HashSet<int> rogueCardIds)
    {
        if (rogueCardIds == null || rogueCardIds.Count == 0)
        {
            return;
        }

        foreach (int rogueCardId in rogueCardIds)
        {
            OnRogueCardEffectTriggered?.Invoke(this, rogueCardId);
        }
    }

    private static void AddRange<T>(List<T> target, List<T> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            target.Add(source[i]);
        }
    }

    private int GetDisplayRewardBeforeMultiplier()
    {
        if (_settlementResult != null && (!_isSettling || _settlementBonusesApplied))
        {
            return ScratchPatternScoreService.GetScoreBeforeFinalMultiplier(
                _model,
                _settlementResult.ScoreBeforeRewardMultiplier);
        }

        return _currentRevealedReward;
    }

    private double GetDisplayRewardMultiplier()
    {
        if (_model == null)
        {
            return 1d;
        }

        return _settlementResult != null && (!_isSettling || _settlementBonusesApplied)
            ? ScratchPatternScoreService.GetFinalRewardMultiplier(_model)
            : _model.RewardMultiplier;
    }

    private IEnumerator ApplySettlementRogueCardBonuses()
    {
        if (!HasSettlementRogueCardBonuses())
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(RogueCardSettlementStartDelay);
        int patternSettlementScoreBonus = CalculatePatternSettlementScoreBonus(out HashSet<int> patternSettlementScoreBonusSourceCardIds);
        double patternSettlementMultiplierBonus = CalculatePatternSettlementMultiplierBonus(out HashSet<int> patternSettlementMultiplierBonusSourceCardIds);
        double allPatternsScratchedMultiplierBonus = CalculateAllPatternsScratchedSettlementMultiplierBonus(out HashSet<int> allPatternsScratchedMultiplierBonusSourceCardIds);
        if (_settlementResult != null && patternSettlementScoreBonus != 0)
        {
            _settlementResult.ScoreBeforeRewardMultiplier += patternSettlementScoreBonus;
            _currentRevealedReward = _settlementResult.ScoreBeforeRewardMultiplier;
        }

        double totalMultiplierBonus = patternSettlementMultiplierBonus + allPatternsScratchedMultiplierBonus;
        if (totalMultiplierBonus > 0d)
        {
            _model.AddRewardMultiplierBonus(totalMultiplierBonus);
        }

        _settlementBonusesApplied = true;
        RaiseSettlementBonusEffects(
            patternSettlementScoreBonusSourceCardIds,
            patternSettlementMultiplierBonusSourceCardIds,
            allPatternsScratchedMultiplierBonusSourceCardIds);
        _view.ShowSettlementInProgressButton(GetDisplayRewardBeforeMultiplier(), GetDisplayRewardMultiplier());
        PlaySettlementDing();
        yield return new WaitForSecondsRealtime(ToolSettlementStepDelay);
    }

    private bool HasSettlementRogueCardBonuses()
    {
        HashSet<int> ignoredSourceCardIds;
        HashSet<int> ignoredMultiplierSourceCardIds;
        return _model != null &&
            (_model.SettlementScoreBonus != 0 ||
                _model.SettlementMultiplierBonus > 0d ||
                CalculatePatternSettlementScoreBonus(out ignoredSourceCardIds) != 0 ||
                CalculatePatternSettlementMultiplierBonus(out ignoredMultiplierSourceCardIds) > 0d ||
                CalculateAllPatternsScratchedSettlementMultiplierBonus(out ignoredMultiplierSourceCardIds) > 0d);
    }

    private int CalculatePatternSettlementScoreBonus(out HashSet<int> sourceCardIds)
    {
        sourceCardIds = new HashSet<int>();
        if (_model?.PatternSettlementScoreBonusRules == null ||
            _model.PatternSettlementScoreBonusRules.Count == 0 ||
            _model.Cells == null)
        {
            return 0;
        }

        int totalBonus = 0;
        for (int ruleIndex = 0; ruleIndex < _model.PatternSettlementScoreBonusRules.Count; ruleIndex++)
        {
            PatternSettlementScoreBonusRuleModel rule = _model.PatternSettlementScoreBonusRules[ruleIndex];
            if (rule == null || rule.ScorePerPattern == 0)
            {
                continue;
            }

            int matchedCount = 0;
            for (int cellIndex = 0; cellIndex < _model.Cells.Count; cellIndex++)
            {
                ScratchCellModel cell = _model.Cells[cellIndex];
                if (cell != null && cell.IsScratchable && cell.IsScratched && rule.MatchesPattern(cell.PatternId))
                {
                    matchedCount++;
                }
            }

            if (matchedCount <= 0)
            {
                continue;
            }

            totalBonus += matchedCount * rule.ScorePerPattern;
            if (rule.SourceCardId > 0)
            {
                sourceCardIds.Add(rule.SourceCardId);
            }
        }

        return totalBonus;
    }

    private double CalculatePatternSettlementMultiplierBonus(out HashSet<int> sourceCardIds)
    {
        sourceCardIds = new HashSet<int>();
        if (_model?.PatternSettlementMultiplierBonusRules == null ||
            _model.PatternSettlementMultiplierBonusRules.Count == 0 ||
            _model.Cells == null)
        {
            return 0d;
        }

        double totalBonus = 0d;
        for (int ruleIndex = 0; ruleIndex < _model.PatternSettlementMultiplierBonusRules.Count; ruleIndex++)
        {
            PatternSettlementMultiplierBonusRuleModel rule = _model.PatternSettlementMultiplierBonusRules[ruleIndex];
            if (rule == null || rule.MultiplierBonusPerPattern <= 0d)
            {
                continue;
            }

            int matchedCount = 0;
            for (int cellIndex = 0; cellIndex < _model.Cells.Count; cellIndex++)
            {
                ScratchCellModel cell = _model.Cells[cellIndex];
                if (cell != null && cell.IsScratchable && cell.IsScratched && rule.MatchesPattern(cell.PatternId))
                {
                    matchedCount++;
                }
            }

            if (matchedCount <= 0)
            {
                continue;
            }

            totalBonus += matchedCount * rule.MultiplierBonusPerPattern;
            if (rule.SourceCardId > 0)
            {
                sourceCardIds.Add(rule.SourceCardId);
            }
        }

        return totalBonus;
    }

    private double CalculateAllPatternsScratchedSettlementMultiplierBonus(out HashSet<int> sourceCardIds)
    {
        sourceCardIds = new HashSet<int>();
        if (_model?.AllPatternsScratchedSettlementMultiplierBonusRules == null ||
            _model.AllPatternsScratchedSettlementMultiplierBonusRules.Count == 0 ||
            _model.Cells == null ||
            !AreAllScratchableCellsScratched())
        {
            return 0d;
        }

        double totalBonus = 0d;
        for (int ruleIndex = 0; ruleIndex < _model.AllPatternsScratchedSettlementMultiplierBonusRules.Count; ruleIndex++)
        {
            AllPatternsScratchedSettlementMultiplierBonusRuleModel rule = _model.AllPatternsScratchedSettlementMultiplierBonusRules[ruleIndex];
            if (rule == null || rule.MultiplierBonus <= 0d)
            {
                continue;
            }

            totalBonus += rule.MultiplierBonus;
            if (rule.SourceCardId > 0)
            {
                sourceCardIds.Add(rule.SourceCardId);
            }
        }

        return totalBonus;
    }

    private bool AreAllScratchableCellsScratched()
    {
        if (_model?.Cells == null)
        {
            return false;
        }

        bool hasScratchableCell = false;
        for (int cellIndex = 0; cellIndex < _model.Cells.Count; cellIndex++)
        {
            ScratchCellModel cell = _model.Cells[cellIndex];
            if (cell == null || !cell.IsScratchable)
            {
                continue;
            }

            hasScratchableCell = true;
            if (!cell.IsScratched)
            {
                return false;
            }
        }

        return hasScratchableCell;
    }

    private void ApplySettlementCardExtraEffects()
    {
        if (_settlementCardExtraEffectsApplied || _model?.ExtraEffects == null)
        {
            return;
        }

        _settlementCardExtraEffectsApplied = true;
        for (int i = 0; i < _model.ExtraEffects.Count; i++)
        {
            ScratchCardExtraEffectConfig effect = _model.ExtraEffects[i];
            if (effect == null || effect.EffectType != ScratchCardExtraEffectType.AddRewardMultiplierOnSettlement)
            {
                continue;
            }

            double bonus = effect.Value > 0d ? effect.Value : 0.1d;
            _model.AddRewardMultiplierBonus(bonus);
            OnScratchCardTypeMultiplierBonusAdded?.Invoke(this, _model.CardTypeId, bonus);
        }
    }

    private void RaiseSettlementBonusEffects(
        HashSet<int> patternSettlementScoreBonusSourceCardIds = null,
        HashSet<int> patternSettlementMultiplierBonusSourceCardIds = null,
        HashSet<int> allPatternsScratchedMultiplierBonusSourceCardIds = null)
    {
        if (_model == null)
        {
            return;
        }

        var triggeredCardIds = new HashSet<int>();
        AddRogueCardEffectSourceIds(triggeredCardIds, _model.SettlementScoreBonusSourceCardIds, _model.SettlementScoreBonus != 0);
        AddRogueCardEffectSourceIds(triggeredCardIds, _model.SettlementMultiplierBonusSourceCardIds, _model.SettlementMultiplierBonus > 0d);
        if (patternSettlementScoreBonusSourceCardIds != null)
        {
            foreach (int sourceCardId in patternSettlementScoreBonusSourceCardIds)
            {
                if (sourceCardId > 0)
                {
                    triggeredCardIds.Add(sourceCardId);
                }
            }
        }

        if (patternSettlementMultiplierBonusSourceCardIds != null)
        {
            foreach (int sourceCardId in patternSettlementMultiplierBonusSourceCardIds)
            {
                if (sourceCardId > 0)
                {
                    triggeredCardIds.Add(sourceCardId);
                }
            }
        }

        if (allPatternsScratchedMultiplierBonusSourceCardIds != null)
        {
            foreach (int sourceCardId in allPatternsScratchedMultiplierBonusSourceCardIds)
            {
                if (sourceCardId > 0)
                {
                    triggeredCardIds.Add(sourceCardId);
                }
            }
        }

        RaiseRogueCardEffectTriggered(triggeredCardIds);
    }

    private static void AddRogueCardEffectSourceIds(HashSet<int> target, IReadOnlyList<int> sourceCardIds, bool shouldAdd)
    {
        if (!shouldAdd || target == null || sourceCardIds == null)
        {
            return;
        }

        for (int i = 0; i < sourceCardIds.Count; i++)
        {
            int sourceCardId = sourceCardIds[i];
            if (sourceCardId > 0)
            {
                target.Add(sourceCardId);
            }
        }
    }

    private void TryExitFocus(Vector2 screenPoint)
    {
        if (!_rewardClaimed && (_isSettling || _settlementResult != null || HasStartedScratching() ||
            (_model != null && _model.State == ScratchCardModel.ScratchCardState.Completed)))
        {
            return;
        }

        if (_view.ContainsScreenPoint(screenPoint) ||
            _view.ContainsClaimRewardButtonScreenPoint(screenPoint))
        {
            return;
        }

        _model.SetState(ScratchCardModel.ScratchCardState.Idle);
    }

    private bool HasStartedScratching()
    {
        if (_model == null)
        {
            return false;
        }

        if (_model.ScratchProgress > 0f ||
            _model.State == ScratchCardModel.ScratchCardState.Scratching)
        {
            return true;
        }

        IReadOnlyList<ScratchCellModel> cells = _model.Cells;
        if (cells == null)
        {
            return false;
        }

        for (int i = 0; i < cells.Count; i++)
        {
            ScratchCellModel cell = cells[i];
            if (cell != null && cell.IsScratchable && cell.IsScratched)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInFocusedState()
    {
        if (_model == null)
        {
            return false;
        }

        return _model.State == ScratchCardModel.ScratchCardState.Focused ||
               _model.State == ScratchCardModel.ScratchCardState.Scratching ||
               _model.State == ScratchCardModel.ScratchCardState.Completed;
    }

    private void OnDestroy()
    {
        if (_settlementCoroutine != null)
        {
            StopCoroutine(_settlementCoroutine);
            _settlementCoroutine = null;
        }

        StopScratchSound();
        UnbindModel();
        UnbindView();
    }

    private void StopScratchSound()
    {
        if (!_isScratchSoundPlaying)
        {
            return;
        }

        AudioManager.Instance?.StopLoopCue(_currentScratchSoundCueId);
        _currentScratchSoundCueId = AudioCueId.None;
        _isScratchSoundPlaying = false;
    }

    private void UnbindModel()
    {
        if (_model == null)
        {
            return;
        }

        _model.OnScratchProgressChanged -= HandleScratchProgressChanged;
        _model.OnStateChanged -= HandleStateChanged;
        _model.OnScratchCompleted -= HandleScratchCompleted;
        _model.OnRewardMultiplierChanged -= HandleRewardMultiplierChanged;
    }

    private void UnbindView()
    {
        if (_view == null)
        {
            return;
        }

        _view.OnCardClicked -= HandleCardClicked;
        _view.OnScratchDragged -= HandleScratchDragged;
        _view.OnScratchLayerCleared -= HandleScratchLayerCleared;
        _view.OnScratchCellRevealed -= HandleScratchCellRevealed;
        _view.OnSpawnAnimationFinished -= HandleSpawnFinished;
        _view.OnClaimRewardClicked -= HandleClaimRewardClicked;
    }
}
