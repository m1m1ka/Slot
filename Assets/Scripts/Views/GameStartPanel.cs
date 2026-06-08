using Core;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameStartPanel : UIPanel
{
    [SerializeField] private Button _startBtn;
    [SerializeField] private float _startButtonHoverScale = 1.12f;
    [SerializeField] private float _startButtonHoverDuration = 0.16f;

    private Transform _startButtonTransform;
    private Vector3 _startButtonNormalScale = Vector3.one;
    private Tween _startButtonScaleTween;
    private bool _hoverEventsRegistered;

    public override void Init()
    {
        base.Init();

        if (_startBtn == null)
        {
            Transform startButtonTransform = transform.Find("StartBtn");
            if (startButtonTransform != null)
            {
                _startBtn = startButtonTransform.GetComponent<Button>();
            }
        }

        if (_startBtn == null)
        {
            Debug.LogError("[GameStartPanel] StartBtn is missing or has no Button component.");
            return;
        }

        _startBtn.onClick.RemoveListener(HandleStartButtonClicked);
        _startBtn.onClick.AddListener(HandleStartButtonClicked);

        _startButtonTransform = _startBtn.transform;
        _startButtonNormalScale = _startButtonTransform.localScale;
        RegisterHoverEvents();
    }

    protected override void OnHide()
    {
        base.OnHide();
        ResetStartButtonScale();
    }

    private void OnDestroy()
    {
        if (_startBtn != null)
        {
            _startBtn.onClick.RemoveListener(HandleStartButtonClicked);
        }

        _startButtonScaleTween?.Kill();
    }

    private void HandleStartButtonClicked()
    {
        UIManager.Instance.ShowPanel<MainGamePanel>(closeOthers: true);
        UIManager.Instance.ShowPanel<TutorialPanel>(data: TutorialPanel.CreateFirstTutorialSequence());
    }

    private void RegisterHoverEvents()
    {
        if (_hoverEventsRegistered || _startBtn == null)
        {
            return;
        }

        EventTrigger eventTrigger = _startBtn.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = _startBtn.gameObject.AddComponent<EventTrigger>();
        }

        AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, HandleStartButtonPointerEnter);
        AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerExit, HandleStartButtonPointerExit);
        _hoverEventsRegistered = true;
    }

    private void AddEventTriggerEntry(
        EventTrigger eventTrigger,
        EventTriggerType eventId,
        UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry
        {
            eventID = eventId
        };

        entry.callback.AddListener(callback);
        eventTrigger.triggers.Add(entry);
    }

    private void HandleStartButtonPointerEnter(BaseEventData eventData)
    {
        TweenStartButtonScale(_startButtonNormalScale * _startButtonHoverScale);
    }

    private void HandleStartButtonPointerExit(BaseEventData eventData)
    {
        TweenStartButtonScale(_startButtonNormalScale);
    }

    private void TweenStartButtonScale(Vector3 targetScale)
    {
        if (_startButtonTransform == null)
        {
            return;
        }

        _startButtonScaleTween?.Kill();
        _startButtonScaleTween = _startButtonTransform
            .DOScale(targetScale, _startButtonHoverDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void ResetStartButtonScale()
    {
        if (_startButtonTransform == null)
        {
            return;
        }

        _startButtonScaleTween?.Kill();
        _startButtonTransform.localScale = _startButtonNormalScale;
    }
}
