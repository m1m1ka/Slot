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
    public event System.Action<ScratchCardController, int, double, bool> OnScoreDisplayChanged;
    public event System.Action<ScratchCardController, RectTransform, int, bool, double> OnPatternScoreRevealed;

    private ScratchCardView _view;
    private ScratchCardModel _model;
    private ScratchSettlementResult _settlementResult;
    private bool _rewardClaimed;
    private bool _isSettling;
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
    private const float ToolSettlementStepDelay = 0.35f;
    private const float PatternSettlementStepDelay = 0.18f;
    private const float ScratchDirectionSoundThreshold = 0.5f;
    private const float SettlementDingBasePitch = 1f;
    private const float SettlementDingPitchStep = 0.12f;
    private const float SettlementDingMaxPitch = 1.8f;

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
        _settlementCoroutine = null;
        _currentRevealedReward = 0;
        _scratchRevealOrder = 0;
        _settlementDingIndex = 0;
        _rewardMultiplierAppliedScoreKeys.Clear();
        _animatedScoreKeys.Clear();
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
        if (_model.State == ScratchCardModel.ScratchCardState.Falling)
        {
            return;
        }

        _model.SetState(ScratchCardModel.ScratchCardState.Focused);
        AudioManager.Instance?.PlayCue(AudioCueId.ScratchCardFocused);
    }

    private void HandleScratchDragged(float amount, float horizontalDelta)
    {
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
                RaiseScoreDisplayChanged(true);
                break;
            case ScratchCardModel.ScratchCardState.Idle:
                StopScratchSound();
                OnFocusStateChanged?.Invoke(this, false);
                _view.SetFocused(false);
                _view.HideClaimRewardButton();
                RaiseScoreDisplayChanged(false);
                break;
            case ScratchCardModel.ScratchCardState.Completed:
                StopScratchSound();
                OnFocusStateChanged?.Invoke(this, true);
                _view.SetFocused(true);
                UpdateSettlementButtonView();
                RaiseScoreDisplayChanged(true);
                break;
        }
    }

    private void HandleScratchCompleted()
    {
        Debug.Log(
            $"[ScratchCardController] Scratch card {_model.CardId} completed. " +
            $"Type={_model.CardTypeName}, Base={_model.TotalBaseScore}");
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

        RaiseScoreDisplayChanged(IsInFocusedState());
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
        ApplyCellRevealEffects(cell);
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
            string scoreKey = BuildScoreKey(scoredCellIndex, scoreMultiplier);
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

                string groupedScoreKey = BuildScoreKey(groupedCellIndex, groupedScoreMultiplier);
                if (!_animatedScoreKeys.Add(groupedScoreKey))
                {
                    continue;
                }

                int cellScore = ScratchPatternScoreService.GetCellScore(_model, groupedCell);
                RectTransform sourceRect = _view.PlayPatternScorePulse(groupedCellIndex);
                OnPatternScoreRevealed?.Invoke(this, sourceRect, cellScore, groupedCell.IsBaseScoreEnhanced, groupedScoreMultiplier);
                playedPatternGroup = true;
            }

            if (playedPatternGroup)
            {
                PlaySettlementDing();
                yield return new WaitForSecondsRealtime(PatternSettlementStepDelay);
            }
        }
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
        _view.ShowSettlementInProgressButton(_currentRevealedReward, _model != null ? _model.RewardMultiplier : 1d);
        RaiseScoreDisplayChanged(IsInFocusedState());

        _settlementResult = new ScratchSettlementResult();
        List<ScratchSettlementResult> toolResults = ScratchToolSettlementService.EvaluateByToolOrder(_model);
        var summaries = new List<string>();

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
            _view.ShowSettlementInProgressButton(_currentRevealedReward, _model != null ? _model.RewardMultiplier : 1d);
            yield return PlayNewScoreAnimations(toolResult);
            RaiseScoreDisplayChanged(IsInFocusedState());

            if (i < toolResults.Count - 1)
            {
                yield return new WaitForSecondsRealtime(ToolSettlementStepDelay);
            }
        }

        _settlementResult.FinalScore = ScratchPatternScoreService.ApplyFinalScoreRules(
            _model,
            _settlementResult.ScoreBeforeRewardMultiplier);
        _settlementResult.Summary = summaries.Count > 0 ? string.Join(" ", summaries) : "\u6ca1\u6709\u89e6\u53d1\u522e\u5177\u8ba1\u5206\u3002";

        _isSettling = false;
        _settlementCoroutine = null;
        _view.ShowClaimRewardButton(_settlementResult.ScoreBeforeRewardMultiplier, _model.RewardMultiplier);
        RaiseScoreDisplayChanged(IsInFocusedState());

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
        RaiseScoreDisplayChanged(false);
        AudioManager.Instance?.PlayCue(AudioCueId.GainMoney);
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
            _view.ShowSettlementInProgressButton(_currentRevealedReward, _model != null ? _model.RewardMultiplier : 1d);
            _view.SetScratchInputEnabled(false);
            return;
        }

        if (_settlementResult != null)
        {
            _view.ShowClaimRewardButton(_settlementResult.ScoreBeforeRewardMultiplier, _model != null ? _model.RewardMultiplier : 1d);
            _view.SetScratchInputEnabled(false);
            return;
        }

        _view.ShowSettleButton(_model != null ? _model.RewardMultiplier : 1d);
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

    private void RaiseScoreDisplayChanged(bool visible)
    {
        OnScoreDisplayChanged?.Invoke(
            this,
            _currentRevealedReward,
            _model != null ? _model.RewardMultiplier : 1d,
            visible);
    }

    private void TryExitFocus(Vector2 screenPoint)
    {
        if (!_rewardClaimed && (_isSettling || _settlementResult != null ||
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
