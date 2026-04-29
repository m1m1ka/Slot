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

    private ScratchCardView _view;
    private ScratchCardModel _model;
    private ScratchSettlementResult _settlementResult;
    private bool _rewardClaimed;
    private float _lastScratchInputTime = float.MinValue;
    private bool _isScratchLoopPlaying;
    private int _currentRevealedReward;
    private readonly HashSet<int> _scoredPairCellIndices = new HashSet<int>();

    private const float ScratchLoopStopDelay = 0.2f;

    public ScratchCardModel Model => _model;

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
        _view.SetClaimRewardMultiplier(_model.RewardMultiplier);
        _view.PlaySpawnAnimation(spawnFrom, spawnTo);
        _settlementResult = null;
        _rewardClaimed = false;
        _currentRevealedReward = 0;
        _scoredPairCellIndices.Clear();
    }

    private void Update()
    {
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

        if (_isScratchLoopPlaying && Time.unscaledTime - _lastScratchInputTime > ScratchLoopStopDelay)
        {
            StopScratchLoop();
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
        if (_model.State == ScratchCardModel.ScratchCardState.Falling)
        {
            return;
        }

        _model.SetState(ScratchCardModel.ScratchCardState.Focused);
        AudioManager.Instance?.PlayCue(AudioCueId.ScratchCardFocused);
    }

    private void HandleScratchDragged(float amount)
    {
        if (_model.State == ScratchCardModel.ScratchCardState.Focused ||
            _model.State == ScratchCardModel.ScratchCardState.Scratching)
        {
            _lastScratchInputTime = Time.unscaledTime;
            StartScratchLoop();
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
                _view.SetCurrentRewardText(_currentRevealedReward, true);
                break;
            case ScratchCardModel.ScratchCardState.Idle:
                StopScratchLoop();
                OnFocusStateChanged?.Invoke(this, false);
                _view.SetFocused(false);
                _view.SetCurrentRewardText(_currentRevealedReward, false);
                break;
            case ScratchCardModel.ScratchCardState.Completed:
                StopScratchLoop();
                OnFocusStateChanged?.Invoke(this, true);
                _view.SetFocused(true);
                _view.SetCurrentRewardText(_currentRevealedReward, true);
                break;
        }
    }

    private void HandleScratchCompleted()
    {
        IScratchSettlementEvaluator evaluator = ScratchSettlementEvaluatorFactory.Create(_model.SettlementType);
        _settlementResult = evaluator.Evaluate(_model);
        _view.ShowClaimRewardButton(_settlementResult.ScoreBeforeRewardMultiplier, _model.RewardMultiplier);

        Debug.Log(
            $"[ScratchCardController] Scratch card {_model.CardId} completed. " +
            $"Type={_model.CardTypeName}, Base={_model.TotalBaseScore}, Final={_settlementResult.FinalScore}, Summary={_settlementResult.Summary}");
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

        _view.SetClaimRewardMultiplier(rewardMultiplier);
    }

    private void HandleScratchLayerCleared()
    {
        AudioManager.Instance?.PlaySfx("Audio/Sfx/Pop");
    }

    private void HandleScratchCellRevealed(int cellIndex)
    {
        if (_model == null || _model.Cells == null || cellIndex < 0 || cellIndex >= _model.Cells.Count)
        {
            return;
        }

        ScratchCellModel cell = _model.Cells[cellIndex];
        if (cell == null || !cell.IsScratchable || cell.IsScratched)
        {
            return;
        }

        cell.MarkScratched();
        ApplyCellRevealEffects(cell);

        if (_model.SettlementType == ScratchSettlementType.MatchAnyPair)
        {
            TryScorePair(cellIndex, cell);
            return;
        }

        int cellScore = ScratchPatternScoreService.GetCellScore(_model, cell);
        _currentRevealedReward += cellScore;
        ApplyCellRewardMultiplierBonus(cell);
        _view.SetCurrentRewardText(_currentRevealedReward, IsInFocusedState());

        if (_model.CardTypeId == 1)
        {
            _view.PlayPatternScoreReveal(cellIndex, cellScore, cell.IsBaseScoreEnhanced);
        }
    }

    private void TryScorePair(int cellIndex, ScratchCellModel cell)
    {
        int pairedCellIndex = FindUnscoredRevealedMatch(cellIndex, cell.PatternId);
        if (pairedCellIndex < 0)
        {
            _view.SetCurrentRewardText(_currentRevealedReward, IsInFocusedState());
            return;
        }

        ScratchCellModel pairedCell = _model.Cells[pairedCellIndex];
        _scoredPairCellIndices.Add(cellIndex);
        _scoredPairCellIndices.Add(pairedCellIndex);

        const int pairScoreMultiplier = 2;
        int cellScore = ScratchPatternScoreService.GetCellScore(_model, cell);
        int pairedCellScore = ScratchPatternScoreService.GetCellScore(_model, pairedCell);
        _currentRevealedReward += (cellScore + pairedCellScore) * pairScoreMultiplier;
        ApplyCellRewardMultiplierBonus(cell);
        ApplyCellRewardMultiplierBonus(pairedCell);
        _view.SetCurrentRewardText(_currentRevealedReward, IsInFocusedState());
        _view.PlayPatternScoreReveal(cellIndex, cellScore, cell.IsBaseScoreEnhanced, pairScoreMultiplier);
        _view.PlayPatternScoreReveal(pairedCellIndex, pairedCellScore, pairedCell.IsBaseScoreEnhanced, pairScoreMultiplier);
    }

    private void ApplyCellRewardMultiplierBonus(ScratchCellModel cell)
    {
        double bonus = ScratchPatternScoreService.GetRewardMultiplierBonusOnScore(cell);
        if (_model == null || bonus <= 0d)
        {
            return;
        }

        _model.AddRewardMultiplierBonus(bonus);
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

    private int FindUnscoredRevealedMatch(int currentCellIndex, int patternId)
    {
        for (int i = 0; i < _model.Cells.Count; i++)
        {
            if (i == currentCellIndex || _scoredPairCellIndices.Contains(i))
            {
                continue;
            }

            ScratchCellModel candidate = _model.Cells[i];
            if (candidate != null && candidate.IsScratchable && candidate.IsScratched && candidate.PatternId == patternId)
            {
                return i;
            }
        }

        return -1;
    }

    private void HandleClaimRewardClicked()
    {
        if (_rewardClaimed || _settlementResult == null)
        {
            return;
        }

        _rewardClaimed = true;
        _view.HideClaimRewardButton();
        AudioManager.Instance?.PlayCue(AudioCueId.GainMoney);
        OnRewardClaimed?.Invoke(this, _settlementResult);
    }

    private void TryExitFocus(Vector2 screenPoint)
    {
        if (_model != null && _model.State == ScratchCardModel.ScratchCardState.Completed && !_rewardClaimed)
        {
            return;
        }

        if (_view.ContainsScreenPoint(screenPoint))
        {
            return;
        }

        _model.SetState(ScratchCardModel.ScratchCardState.Idle);
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
        StopScratchLoop();
        UnbindModel();
        UnbindView();
    }

    private void StartScratchLoop()
    {
        if (_isScratchLoopPlaying)
        {
            return;
        }

        AudioManager.Instance?.PlayLoopCue(AudioCueId.Sratching);
        _isScratchLoopPlaying = true;
    }

    private void StopScratchLoop()
    {
        if (!_isScratchLoopPlaying)
        {
            return;
        }

        AudioManager.Instance?.StopLoopCue(AudioCueId.Sratching);
        _isScratchLoopPlaying = false;
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
