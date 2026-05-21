using System.Collections.Generic;
using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders the persistent pattern info Scroll View shown while a scratch card is focused.
/// The panel itself must exist in the scene/prefab; this class only toggles it and fills rows.
/// </summary>
public class ScratchCardFocusPanelView : MonoBehaviour
{
    [Header("Persistent Panel References")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private ScrollRect _patternInfoScrollView;
    [SerializeField] private RectTransform _patternInfoContentRoot;
    [SerializeField] private GameObject _patternInfoPrefab;
    [SerializeField] private string _patternInfoPrefabPath = "UI/PatternInfo";
    [SerializeField] private bool _clearExistingContentOnBind = true;

    [Header("Animation")]
    [SerializeField] private float _showFadeDuration = 0.16f;
    [SerializeField] private float _hideFadeDuration = 0.12f;

    private CanvasGroup _canvasGroup;
    private Tween _fadeTween;
    private readonly List<GameObject> _spawnedPatternInfos = new List<GameObject>();

    private void Awake()
    {
        EnsureReferences();
        Hide(true);
    }

    public void Bind(ScratchCardFocusPanelModel model)
    {
        EnsureReferences();
        ClearPatternInfos();

        if (model == null || _patternInfoContentRoot == null)
        {
            return;
        }

        IReadOnlyList<ScratchCardFocusPatternInfo> patterns = model.Patterns;
        int count = patterns != null ? patterns.Count : 0;
        for (int i = 0; i < count; i++)
        {
            ScratchCardFocusPatternInfo pattern = patterns[i];
            if (pattern == null)
            {
                continue;
            }

            PatternInfoView itemView = CreatePatternInfoView();
            if (itemView == null)
            {
                continue;
            }

            itemView.Bind(pattern);
        }

        if (_patternInfoScrollView != null)
        {
            _patternInfoScrollView.normalizedPosition = new Vector2(0f, 1f);
        }
    }

    public void Show()
    {
        EnsureReferences();
        GameObject targetRoot = GetPanelRoot();
        if (targetRoot == null)
        {
            return;
        }

        targetRoot.SetActive(true);
        if (_canvasGroup == null)
        {
            return;
        }

        _canvasGroup.blocksRaycasts = true;
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup
            .DOFade(1f, _showFadeDuration)
            .SetUpdate(true)
            .OnComplete(() => _fadeTween = null);
    }

    public void Hide(bool instant = false)
    {
        GameObject targetRoot = GetPanelRoot();
        if (targetRoot == null)
        {
            return;
        }

        _fadeTween?.Kill();
        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = false;
        }

        if (instant || _canvasGroup == null)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            targetRoot.SetActive(false);
            return;
        }

        _fadeTween = _canvasGroup
            .DOFade(0f, _hideFadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                targetRoot.SetActive(false);
                _fadeTween = null;
            });
    }

    private PatternInfoView CreatePatternInfoView()
    {
        if (_patternInfoContentRoot == null)
        {
            return null;
        }

        GameObject prefab = GetPatternInfoPrefab();
        if (prefab == null)
        {
            Debug.LogError("[ScratchCardFocusPanelView] PatternInfo prefab is missing.");
            return null;
        }

        GameObject itemObject = Instantiate(prefab, _patternInfoContentRoot, false);
        itemObject.name = "PatternInfo";
        _spawnedPatternInfos.Add(itemObject);

        PatternInfoView itemView = itemObject.GetComponent<PatternInfoView>();
        if (itemView == null)
        {
            itemView = itemObject.AddComponent<PatternInfoView>();
        }

        return itemView;
    }

    private GameObject GetPatternInfoPrefab()
    {
        if (_patternInfoPrefab != null)
        {
            return _patternInfoPrefab;
        }

        _patternInfoPrefab = AssetProvider.LoadPrefab(_patternInfoPrefabPath);
        return _patternInfoPrefab;
    }

    private void ClearPatternInfos()
    {
        if (_clearExistingContentOnBind && _patternInfoContentRoot != null)
        {
            for (int i = _patternInfoContentRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = _patternInfoContentRoot.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            _spawnedPatternInfos.Clear();
            return;
        }

        for (int i = 0; i < _spawnedPatternInfos.Count; i++)
        {
            if (_spawnedPatternInfos[i] != null)
            {
                _spawnedPatternInfos[i].SetActive(false);
                Destroy(_spawnedPatternInfos[i]);
            }
        }

        _spawnedPatternInfos.Clear();
    }

    private void EnsureReferences()
    {
        if (_panelRoot == null)
        {
            _panelRoot = gameObject;
        }

        if (_patternInfoScrollView == null)
        {
            _patternInfoScrollView = GetComponentInChildren<ScrollRect>(true);
        }

        if (_patternInfoContentRoot == null && _patternInfoScrollView != null)
        {
            _patternInfoContentRoot = _patternInfoScrollView.content;
        }

        if (_canvasGroup == null)
        {
            _canvasGroup = GetPanelRoot()?.GetComponent<CanvasGroup>();
            if (_canvasGroup == null && GetPanelRoot() != null)
            {
                _canvasGroup = GetPanelRoot().AddComponent<CanvasGroup>();
            }
        }
    }

    private GameObject GetPanelRoot()
    {
        return _panelRoot != null ? _panelRoot : gameObject;
    }

    private void OnDestroy()
    {
        _fadeTween?.Kill();
    }
}
