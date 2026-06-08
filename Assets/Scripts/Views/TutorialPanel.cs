using System.Collections;
using System.Collections.Generic;
using Core;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialPanel : UIPanel, IPointerClickHandler
{
    public enum TutorialStepResponse
    {
        ClickDialogue = 0,
        BuyFirstScratchCard = 1,
        ClickSettlementButton = 2,
        ClickReceiveMoneyButton = 3,
        ClickScratchCardRewardButton = 4,
        ClickRogueCardRewardButton = 5
    }

    public class TutorialStep
    {
        public string Text;
        public string HighlightName;
        public TutorialStepResponse Response;

        public TutorialStep(string text, string highlightName, TutorialStepResponse response)
        {
            Text = text;
            HighlightName = highlightName;
            Response = response;
        }
    }

    public class TutorialSequence
    {
        public readonly List<TutorialStep> Steps = new List<TutorialStep>();
        public int TutorialId;
        public bool BlockScratchCardInput;
    }

    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _next;
    [SerializeField] private RectTransform _dialogueClickRoot;
    [SerializeField] private float _characterInterval = 0.045f;
    [SerializeField] private float _scratchToolHighlightScaleDuration = 0.25f;
    [SerializeField] private float _scratchToolHighlightTargetScale = 3.5f;
    [SerializeField] private List<string> _defaultDialogues = new List<string>();

    private readonly List<string> _dialogues = new List<string>();
    private readonly List<TutorialStep> _steps = new List<TutorialStep>();
    private readonly List<GameObject> _highlightObjects = new List<GameObject>();
    private Coroutine _typingRoutine;
    private Coroutine _highlightScaleRoutine;
    private GameObject _scalingHighlight;
    private int _dialogueIndex;
    private string _currentDialogue;
    private TutorialStepResponse _currentResponse = TutorialStepResponse.ClickDialogue;
    private bool _isTyping;
    private bool _lineCompleted;
    private bool _startSequenceOnEnable;
    private bool _waitingForTutorialEvent;
    private bool _blocksScratchCardInput;
    private int _currentTutorialId;

    public static bool BlocksScratchCardInput { get; private set; }
    public int CurrentTutorialId => _currentTutorialId;

    public static TutorialSequence CreateFirstTutorialSequence()
    {
        var sequence = new TutorialSequence
        {
            TutorialId = 1
        };
        sequence.Steps.Add(new TutorialStep("\u4f60\u597d\uff0c\u6b22\u8fce\u5149\u4e34\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u4f60\u8bf4\u4f60\u5feb\u7834\u4ea7\u4e86\uff1f", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u90a3\u4f60\u6765\u5bf9\u5730\u65b9\u4e86\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u5148\u6765\u8bd5\u8bd5\u624b\u6c14\u522e\u4e00\u5f20\u5f69\u7968\u5427\u3002", "ScratchCardHighLight", TutorialStepResponse.BuyFirstScratchCard));
        return sequence;
    }

    public static TutorialSequence CreateSecondTutorialSequence()
    {
        var sequence = new TutorialSequence
        {
            TutorialId = 2,
            BlockScratchCardInput = true
        };
        sequence.Steps.Add(new TutorialStep("\u8fd9\u91cc\u5199\u660e\u4e86\u522e\u522e\u5361\u7684\u4e2d\u5956\u89c4\u5219\u3002", "WinRuleHighLight", TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u8fd9\u662f\u8fd9\u5f20\u522e\u522e\u5361\u53ef\u4ee5\u522e\u51fa\u7684\u56fe\u6848\u3002", "PatternInfoHighLight", TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u73b0\u5728\u4f60\u53ef\u4ee5\u8bd5\u7740\u522e\u4e00\u522e\u3002", null, TutorialStepResponse.ClickDialogue));
        return sequence;
    }

    public static TutorialSequence CreateThirdTutorialSequence()
    {
        var sequence = new TutorialSequence
        {
            TutorialId = 3
        };
        sequence.Steps.Add(new TutorialStep("\u73b0\u5728\uff0c\u4f60\u9700\u8981\u70b9\u51fb\u7ed3\u7b97\u3002\u6211\u4f1a\u81ea\u52a8\u5e2e\u4f60\u7ed3\u7b97\u4e2d\u5956\u91d1\u989d\u3002\u4e2d\u5956\u4f1a\u7ed3\u7b97\u4e00\u6b21\u6240\u6709\u56fe\u6848\u5206\u6570", null, TutorialStepResponse.ClickDialogue));
        return sequence;
    }

    public static TutorialSequence CreateSettlementClickedTutorialSequence()
    {
        var sequence = new TutorialSequence
        {
            TutorialId = 4
        };
        sequence.Steps.Add(new TutorialStep("\u5f88\u9057\u61be\uff0c\u4f60\u6ca1\u6709\u4e2d\u5956\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u53e6\u5916\uff0c<color=red>\u522e\u522e\u5361\u4e0d\u9700\u8981\u522e\u5b8c\u6240\u6709\u56fe\u6848\u5373\u53ef\u7ed3\u7b97</color>\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u4f46\u662f\u4e00\u65e6\u522e\u4e86\uff0c\u4f60\u5c31\u5fc5\u987b\u7ed3\u7b97\u8fd9\u5f20\u522e\u522e\u5361\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u6bd5\u7adf\uff0c\u6211\u4e0d\u80fd\u5356\u4e00\u5f20\u5df2\u7ecf\u522e\u4e86\u7684\u522e\u522e\u5361\u5427\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u597d\u4e86\uff0c\u5f00\u59cb\u4f60\u7684\u522e\u522e\u5361\u5427\u3002", null, TutorialStepResponse.ClickDialogue));
        return sequence;
    }

    public static TutorialSequence CreateFifthTutorialSequence()
    {
        var sequence = new TutorialSequence
        {
            TutorialId = 5
        };
        sequence.Steps.Add(new TutorialStep("\u6ca1\u94b1\u4e86?\u6216\u8bb8\u4f60\u8981\u8bd5\u8bd5\u8fd9\u4e2a\u3002", "ScratchToolHighLight", TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u8fd9\u4e2a\u522e\u5177\u53ef\u4ee5\u8ba9\u4f60\u5728\u6ca1\u4e2d\u5956\u7684\u60c5\u51b5\u4e0b\u4f9d\u65e7\u80fd\u83b7\u5f97\u91d1\u5e01\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u4f60\u95ee\u6211\u4e3a\u4ec0\u4e48\u8981\u5e2e\u4f60\uff1f\u563f\u563f\u563f....\u4e4b\u540e\u4f60\u5c31\u77e5\u9053\u4e86\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u4f60\u53ef\u4ee5\u5728\u53f3\u4fa7\u770b\u5230\u4f60\u7684\u522e\u5177\u5217\u8868\u3002\u6211\u4f1a\u7ed9\u4f60\u4e00\u4e9b\u94b1\uff0c\u7ee7\u7eed\u4f60\u7684\u522e\u522e\u4e50\u5427\u3002", "ScratchToolListHighLight", TutorialStepResponse.ClickDialogue));
        return sequence;
    }

    public static TutorialSequence CreateSixthTutorialSequence()
    {
        var sequence = new TutorialSequence
        {
            TutorialId = 6
        };
        sequence.Steps.Add(new TutorialStep("\u606d\u559c\u4f60\u3002\u901a\u8fc7\u4e86\u7b2c\u4e00\u5173\u3002", null, TutorialStepResponse.ClickDialogue));
        sequence.Steps.Add(new TutorialStep("\u6bcf\u5173\u7ed3\u675f\u540e\u6709\u76f8\u5e94\u7684\u5956\u52b1\u3002\u4f60\u73b0\u5728\u53ef\u4ee5\u9886\u53d6\u65b0\u7684\u5361\u5305\u4e86\u3002", "NewScratchCardRewardHighLight", TutorialStepResponse.ClickScratchCardRewardButton));
        return sequence;
    }

    public static TutorialSequence CreateSeventhTutorialSequence()
    {
        var sequence = new TutorialSequence
        {
            TutorialId = 7
        };
        sequence.Steps.Add(new TutorialStep("\u73b0\u5728\uff0c\u4f60\u53ef\u4ee5\u9886\u53d6\u5361\u724c\u5956\u52b1\u4e86\u3002\u4f60\u53ef\u4ee5\u9009\u62e9\u4e00\u5f20\u4f5c\u4e3a\u5956\u52b1\u3002\u4f46\u4f60\u6700\u591a\u53ea\u80fd\u62e5\u67096\u5f20\u5361\u724c\u3002", "RogueCardRewardHighLight", TutorialStepResponse.ClickRogueCardRewardButton));
        return sequence;
    }

    public override void Init()
    {
        base.Init();

        if (_dialogueText == null)
        {
            Transform dialogueTextTransform = FindChildRecursive(transform, "DialogueText");
            _dialogueText = dialogueTextTransform != null
                ? dialogueTextTransform.GetComponent<TextMeshProUGUI>()
                : null;
        }

        if (_next == null)
        {
            Transform nextTransform = FindChildRecursive(transform, "Next");
            _next = nextTransform != null ? nextTransform.gameObject : null;
        }

        if (_dialogueClickRoot == null)
        {
            Transform dialogueFrameTransform = FindChildRecursive(transform, "DialogueFrame");
            Transform dialogueTransform = dialogueFrameTransform != null
                ? dialogueFrameTransform
                : FindChildRecursive(transform, "Dialogue");
            _dialogueClickRoot = dialogueTransform as RectTransform;
        }

        if (_dialogueText == null)
        {
            Debug.LogError("[TutorialPanel] DialogueText is missing or has no TextMeshProUGUI component.");
            return;
        }

        PrepareMaskAndHighlights();

        if (_defaultDialogues.Count == 0 && !string.IsNullOrEmpty(_dialogueText.text))
        {
            _defaultDialogues.Add(_dialogueText.text);
        }

        SetNextVisible(false);
        _dialogueText.text = string.Empty;
    }

    public void ShowDialogues(IList<string> dialogues)
    {
        SetDialogues(dialogues);
        BeginSequenceWhenReady();
    }

    protected override void OnShow(object data)
    {
        base.OnShow(data);
        SetDialoguesFromData(data);
        BeginSequenceWhenReady();
    }

    protected override void OnHide()
    {
        base.OnHide();
        _startSequenceOnEnable = false;
        SetScratchCardInputBlocked(false);
        StopWaitingForTutorialEvent();
        StopTyping();
        HideAllHighlights();
        SetNextVisible(false);
    }

    private void OnEnable()
    {
        if (!_startSequenceOnEnable)
        {
            return;
        }

        _startSequenceOnEnable = false;
        StartCurrentSequence();
    }

    private void OnDestroy()
    {
        SetScratchCardInputBlocked(false);
        StopWaitingForTutorialEvent();
        StopTyping();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_dialogues.Count == 0)
        {
            return;
        }

        if (_isTyping)
        {
            CompleteCurrentLineImmediately();
            return;
        }

        if (_lineCompleted && _currentResponse == TutorialStepResponse.ClickDialogue)
        {
            ShowNextDialogue();
        }
    }

    private void SetDialoguesFromData(object data)
    {
        if (data is TutorialSequence sequence)
        {
            SetSequence(sequence);
            return;
        }

        if (data is string singleDialogue)
        {
            SetDialogues(new[] { singleDialogue });
            return;
        }

        if (data is string[] dialogueArray)
        {
            SetDialogues(dialogueArray);
            return;
        }

        if (data is IList<string> dialogueList)
        {
            SetDialogues(dialogueList);
            return;
        }

        SetDialogues(_defaultDialogues);
    }

    private void SetDialogues(IList<string> dialogues)
    {
        _currentTutorialId = 0;
        _blocksScratchCardInput = false;
        _steps.Clear();
        _dialogues.Clear();

        if (dialogues == null)
        {
            return;
        }

        for (int i = 0; i < dialogues.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(dialogues[i]))
            {
                _dialogues.Add(dialogues[i]);
            }
        }
    }

    private void SetSequence(TutorialSequence sequence)
    {
        _currentTutorialId = sequence != null ? sequence.TutorialId : 0;
        _blocksScratchCardInput = sequence != null && sequence.BlockScratchCardInput;
        SetSteps(sequence != null ? sequence.Steps : null);
        _blocksScratchCardInput = sequence != null && sequence.BlockScratchCardInput;
    }

    private void SetSteps(IList<TutorialStep> steps)
    {
        _steps.Clear();
        _dialogues.Clear();

        if (steps == null)
        {
            return;
        }

        for (int i = 0; i < steps.Count; i++)
        {
            TutorialStep step = steps[i];
            if (step != null && !string.IsNullOrWhiteSpace(step.Text))
            {
                _steps.Add(step);
                _dialogues.Add(step.Text);
            }
        }
    }

    private void BeginSequenceWhenReady()
    {
        if (isActiveAndEnabled)
        {
            StartCurrentSequence();
            return;
        }

        _startSequenceOnEnable = true;
    }

    private void StartCurrentSequence()
    {
        _dialogueIndex = 0;
        SetScratchCardInputBlocked(_blocksScratchCardInput);
        StopWaitingForTutorialEvent();
        HideAllHighlights();

        if (_dialogues.Count == 0)
        {
            if (_dialogueText != null)
            {
                _dialogueText.text = string.Empty;
            }

            SetNextVisible(false);
            return;
        }

        PlayDialogue(_dialogues[_dialogueIndex]);
    }

    private void ShowNextDialogue()
    {
        StopWaitingForTutorialEvent();
        HideAllHighlights();

        _dialogueIndex++;
        if (_dialogueIndex >= _dialogues.Count)
        {
            CompleteSequence();
            return;
        }

        PlayDialogue(_dialogues[_dialogueIndex]);
    }

    private void PlayDialogue(string dialogue)
    {
        StopTyping();
        StopWaitingForTutorialEvent();
        ApplyCurrentStep();

        _currentDialogue = dialogue ?? string.Empty;
        _lineCompleted = false;
        SetNextVisible(false);

        if (_dialogueText != null)
        {
            _dialogueText.text = string.Empty;
            _dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        _typingRoutine = StartCoroutine(TypeDialogueRoutine());
    }

    private IEnumerator TypeDialogueRoutine()
    {
        _isTyping = true;
        AudioManager.Instance?.PlayLoopCue(AudioCueId.Speak);

        _dialogueText.text = _currentDialogue;
        _dialogueText.maxVisibleCharacters = 0;
        _dialogueText.ForceMeshUpdate();

        int visibleCharacterCount = _dialogueText.textInfo.characterCount;
        for (int i = 0; i < visibleCharacterCount; i++)
        {
            _dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSecondsRealtime(_characterInterval);
        }

        FinishCurrentLine();
    }

    private void CompleteCurrentLineImmediately()
    {
        StopTyping();
        if (_dialogueText != null)
        {
            _dialogueText.text = _currentDialogue;
            _dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        FinishCurrentLine();
    }

    private void FinishCurrentLine()
    {
        _isTyping = false;
        _typingRoutine = null;
        AudioManager.Instance?.StopLoopCue(AudioCueId.Speak);
        _lineCompleted = true;
        SetNextVisible(_currentResponse == TutorialStepResponse.ClickDialogue);
        ShowCurrentStepHighlight();

        if (_currentResponse == TutorialStepResponse.BuyFirstScratchCard ||
            _currentResponse == TutorialStepResponse.ClickSettlementButton ||
            _currentResponse == TutorialStepResponse.ClickReceiveMoneyButton ||
            _currentResponse == TutorialStepResponse.ClickScratchCardRewardButton ||
            _currentResponse == TutorialStepResponse.ClickRogueCardRewardButton)
        {
            StartWaitingForTutorialEvent();
        }
    }

    private void StopTyping()
    {
        if (_typingRoutine != null)
        {
            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }

        _isTyping = false;
        AudioManager.Instance?.StopLoopCue(AudioCueId.Speak);
    }

    private void ApplyCurrentStep()
    {
        _currentResponse = TutorialStepResponse.ClickDialogue;
        HideAllHighlights();

        if (_dialogueIndex < 0 || _dialogueIndex >= _steps.Count)
        {
            return;
        }

        TutorialStep step = _steps[_dialogueIndex];
        _currentResponse = step.Response;
    }

    private void ShowCurrentStepHighlight()
    {
        if (_dialogueIndex < 0 || _dialogueIndex >= _steps.Count)
        {
            return;
        }

        ShowHighlight(_steps[_dialogueIndex].HighlightName);
    }

    private void StartWaitingForTutorialEvent()
    {
        if (_waitingForTutorialEvent)
        {
            return;
        }

        EventBus.Subscribe<TutorialEvent>(HandleTutorialEvent);
        _waitingForTutorialEvent = true;
    }

    private void StopWaitingForTutorialEvent()
    {
        if (!_waitingForTutorialEvent)
        {
            return;
        }

        EventBus.Unsubscribe<TutorialEvent>(HandleTutorialEvent);
        _waitingForTutorialEvent = false;
    }

    private void HandleTutorialEvent(TutorialEvent tutorialEvent)
    {
        if (_currentResponse == TutorialStepResponse.BuyFirstScratchCard &&
            tutorialEvent.Type == TutorialEventType.ScratchCardBought)
        {
            StopWaitingForTutorialEvent();
            CompleteSequence();
            return;
        }

        if (_currentResponse == TutorialStepResponse.ClickSettlementButton &&
            tutorialEvent.Type == TutorialEventType.SettlementButtonClicked)
        {
            StopWaitingForTutorialEvent();
            ShowNextDialogue();
            return;
        }

        if (_currentResponse == TutorialStepResponse.ClickReceiveMoneyButton &&
            tutorialEvent.Type == TutorialEventType.RewardClaimed)
        {
            StopWaitingForTutorialEvent();
            CompleteSequence();
            return;
        }

        if (_currentResponse == TutorialStepResponse.ClickScratchCardRewardButton &&
            tutorialEvent.Type == TutorialEventType.ScratchCardRewardButtonClicked)
        {
            StopWaitingForTutorialEvent();
            CompleteSequence();
            return;
        }

        if (_currentResponse == TutorialStepResponse.ClickRogueCardRewardButton &&
            tutorialEvent.Type == TutorialEventType.RogueCardRewardButtonClicked)
        {
            StopWaitingForTutorialEvent();
            CompleteSequence();
        }
    }

    private void CompleteSequence()
    {
        int completedTutorialId = _currentTutorialId;
        SetScratchCardInputBlocked(false);
        HideAllHighlights();
        if (completedTutorialId > 0)
        {
            EventBus.Publish(new TutorialEvent(TutorialEventType.TutorialCompleted, completedTutorialId));
        }

        UIManager.Instance?.ClosePanel<TutorialPanel>();
    }

    private void PrepareMaskAndHighlights()
    {
        Transform maskTransform = FindChildRecursive(transform, "Mask");
        if (maskTransform != null)
        {
            Image maskImage = maskTransform.GetComponent<Image>();
            if (maskImage != null)
            {
                maskImage.raycastTarget = false;
            }
        }

        _highlightObjects.Clear();
        CollectHighlightObjects(transform);
        HideAllHighlights();
    }

    private void CollectHighlightObjects(Transform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (IsHighlightName(child.name))
            {
                if (!_highlightObjects.Contains(child.gameObject))
                {
                    _highlightObjects.Add(child.gameObject);
                }

                SetGraphicRaycastTarget(child, false);
            }

            CollectHighlightObjects(child);
        }
    }

    private void ShowHighlight(string highlightName)
    {
        if (string.IsNullOrWhiteSpace(highlightName))
        {
            return;
        }

        GameObject highlight = FindHighlightObject(highlightName);
        if (highlight == null && (highlightName == "ScratchCardHighLight" || highlightName == "ScratchCardHightLight"))
        {
            highlight = FindHighlightObject(highlightName == "ScratchCardHighLight"
                ? "ScratchCardHightLight"
                : "ScratchCardHighLight");
        }

        if (highlight == null && (highlightName == "ScratchCardHighLight" || highlightName == "ScratchCardHightLight"))
        {
            highlight = FindHighlightObject("HightLight1");
        }

        if (highlight == null && (highlightName == "PatternInfoHighLight" || highlightName == "PatternInfoHightLight"))
        {
            highlight = FindHighlightObject(highlightName == "PatternInfoHighLight"
                ? "PatternInfoHightLight"
                : "PatternInfoHighLight");
        }

        if (highlight == null)
        {
            Debug.LogWarning($"[TutorialPanel] Highlight '{highlightName}' was not found.");
            return;
        }

        SetGraphicRaycastTarget(highlight.transform, false);
        highlight.SetActive(true);

        if (highlight.name == "ScratchToolHighLight")
        {
            PlayScratchToolHighlightScale(highlight);
        }
    }

    private GameObject FindHighlightObject(string highlightName)
    {
        Transform found = FindChildRecursive(transform, highlightName);
        if (found != null)
        {
            SetGraphicRaycastTarget(found, false);
            if (!_highlightObjects.Contains(found.gameObject))
            {
                _highlightObjects.Add(found.gameObject);
            }

            return found.gameObject;
        }

        for (int i = 0; i < _highlightObjects.Count; i++)
        {
            GameObject highlight = _highlightObjects[i];
            if (highlight != null && string.Equals(
                highlight.name.Trim(),
                highlightName.Trim(),
                System.StringComparison.Ordinal))
            {
                return highlight;
            }
        }

        return null;
    }

    private void HideAllHighlights()
    {
        StopHighlightScaleAnimation();

        for (int i = 0; i < _highlightObjects.Count; i++)
        {
            if (_highlightObjects[i] != null)
            {
                _highlightObjects[i].SetActive(false);
            }
        }
    }

    private void PlayScratchToolHighlightScale(GameObject highlight)
    {
        StopHighlightScaleAnimation();
        if (highlight == null)
        {
            return;
        }

        _scalingHighlight = highlight;
        _scalingHighlight.transform.localScale = Vector3.zero;
        _highlightScaleRoutine = StartCoroutine(ScaleHighlightRoutine(_scalingHighlight.transform));
    }

    private IEnumerator ScaleHighlightRoutine(Transform target)
    {
        float duration = Mathf.Max(0.01f, _scratchToolHighlightScaleDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            target.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one * _scratchToolHighlightTargetScale, eased);
            yield return null;
        }

        target.localScale = Vector3.one * _scratchToolHighlightTargetScale;
        _highlightScaleRoutine = null;
        _scalingHighlight = null;
    }

    private void StopHighlightScaleAnimation()
    {
        if (_highlightScaleRoutine != null)
        {
            StopCoroutine(_highlightScaleRoutine);
            _highlightScaleRoutine = null;
        }

        if (_scalingHighlight != null)
        {
            _scalingHighlight.transform.localScale = Vector3.one;
            _scalingHighlight = null;
        }
    }

    private void SetNextVisible(bool visible)
    {
        if (_next != null)
        {
            _next.SetActive(visible);
        }
    }

    private static bool IsHighlightName(string objectName)
    {
        return !string.IsNullOrEmpty(objectName) &&
            (objectName.Contains("HightLight") ||
             objectName.Contains("HighLight") ||
             objectName.Contains("Highlight"));
    }

    private static void SetGraphicRaycastTarget(Transform root, bool raycastTarget)
    {
        Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].raycastTarget = raycastTarget;
        }
    }

    private static void SetScratchCardInputBlocked(bool blocked)
    {
        BlocksScratchCardInput = blocked;
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (string.Equals(child.name.Trim(), childName.Trim(), System.StringComparison.Ordinal))
            {
                return child;
            }

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
