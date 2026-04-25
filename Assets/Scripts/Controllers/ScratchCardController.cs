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
        _view.PlaySpawnAnimation(spawnFrom, spawnTo);
        _settlementResult = null;
        _rewardClaimed = false;
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
    }

    private void BindView()
    {
        _view.OnCardClicked += HandleCardClicked;
        _view.OnScratchDragged += HandleScratchDragged;
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
                break;
            case ScratchCardModel.ScratchCardState.Idle:
                StopScratchLoop();
                OnFocusStateChanged?.Invoke(this, false);
                _view.SetFocused(false);
                break;
            case ScratchCardModel.ScratchCardState.Completed:
                StopScratchLoop();
                OnFocusStateChanged?.Invoke(this, true);
                _view.SetFocused(true);
                break;
        }
    }

    private void HandleScratchCompleted()
    {
        IScratchSettlementEvaluator evaluator = ScratchSettlementEvaluatorFactory.Create(_model.SettlementType);
        _settlementResult = evaluator.Evaluate(_model);
        _view.ShowClaimRewardButton(_settlementResult.FinalScore);
        AudioManager.Instance?.PlayCue(AudioCueId.ScratchCardCompleted);

        Debug.Log(
            $"[ScratchCardController] Scratch card {_model.CardId} completed. " +
            $"Type={_model.CardTypeName}, Base={_model.TotalBaseScore}, Final={_settlementResult.FinalScore}, Summary={_settlementResult.Summary}");
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
    }

    private void UnbindView()
    {
        if (_view == null)
        {
            return;
        }

        _view.OnCardClicked -= HandleCardClicked;
        _view.OnScratchDragged -= HandleScratchDragged;
        _view.OnSpawnAnimationFinished -= HandleSpawnFinished;
        _view.OnClaimRewardClicked -= HandleClaimRewardClicked;
    }
}
