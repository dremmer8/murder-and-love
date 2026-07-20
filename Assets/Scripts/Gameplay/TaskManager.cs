using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;

[System.Serializable]
public class TaskPhaseEntry
{
    [Tooltip("Applies when game_progression >= this value (same numbering as Interactable / Story Phase gates).")]
    public int storyPhase;

    [Tooltip("Objective text shown while this phase is the latest matched entry.")]
    [TextArea(1, 3)]
    public string taskText;
}

/// <summary>
/// HUD task text driven by <see cref="GlobalVariableOperator.GameProgression"/>.
/// Picks the highest entry whose <see cref="TaskPhaseEntry.storyPhase"/> is &lt;= current progression.
/// </summary>
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    const string DefaultDecodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$%&*";

    [Header("UI")]
    [SerializeField] private TMP_Text taskLabel;

    [Tooltip("Shown when no entry matches the current progression.")]
    [SerializeField] private string fallbackText = "";

    [Tooltip("Disable the label GameObject when the resolved text is empty.")]
    [SerializeField] private bool hideWhenEmpty = true;

    [Header("Animation")]
    [Tooltip("Fade out duration when the task changes. 0 = snap.")]
    [SerializeField] private float fadeOutDuration = 0.2f;

    [Tooltip("Fade in duration after the new text is applied. 0 = snap.")]
    [SerializeField] private float fadeInDuration = 0.35f;

    [SerializeField] private Ease fadeOutEase = Ease.InQuad;
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;

    [Tooltip("Letter scramble → resolve left-to-right when a new task appears. 0 = skip.")]
    private float decodeDuration = 0.7f;

    [Tooltip("Glyphs used while undecoded letters are scrambling.")]
    [SerializeField] private string decodeChars = DefaultDecodeChars;

    [Header("Tasks")]
    [SerializeField] private List<TaskPhaseEntry> tasks = new();

    int _lastSeenProgression = int.MinValue;
    string _currentText = "";
    Tween _textTween;
    Color _labelBaseColor = Color.white;
    readonly StringBuilder _decodeBuilder = new StringBuilder(64);

    public string CurrentTaskText => _currentText;
    public IReadOnlyList<TaskPhaseEntry> Tasks => tasks;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("TaskManager: more than one instance in scene.", this);
            Destroy(this);
            return;
        }

        Instance = this;

        if (taskLabel != null)
            _labelBaseColor = taskLabel.color;
    }

    void OnDestroy()
    {
        KillTextTween();

        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        Refresh(force: true);
    }

    void Update()
    {
        int progression = CurrentProgression();
        if (progression == _lastSeenProgression)
            return;

        Refresh(force: false);
    }

    /// <summary>
    /// Re-reads progression and updates the label if needed.
    /// </summary>
    public void Refresh(bool force = false)
    {
        int progression = CurrentProgression();
        if (!force && progression == _lastSeenProgression)
            return;

        _lastSeenProgression = progression;
        ApplyText(ResolveTaskText(progression), animate: !force);
    }

    /// <summary>
    /// Highest entry with <see cref="TaskPhaseEntry.storyPhase"/> &lt;= <paramref name="progression"/>,
    /// or <see cref="fallbackText"/> when none match.
    /// </summary>
    public string ResolveTaskText(int progression)
    {
        TaskPhaseEntry best = FindReachedMilestone(progression);
        if (best == null)
            return fallbackText ?? "";

        return best.taskText ?? "";
    }

    /// <summary>
    /// Highest task milestone (<see cref="TaskPhaseEntry.storyPhase"/>) reached at
    /// <paramref name="progression"/>, or 0 if none. Used as a progression floor lock.
    /// </summary>
    public int GetReachedMilestoneFloor(int progression)
    {
        TaskPhaseEntry best = FindReachedMilestone(progression);
        return best != null ? best.storyPhase : 0;
    }

    TaskPhaseEntry FindReachedMilestone(int progression)
    {
        if (tasks == null || tasks.Count == 0)
            return null;

        TaskPhaseEntry best = null;
        for (int i = 0; i < tasks.Count; i++)
        {
            TaskPhaseEntry entry = tasks[i];
            if (entry == null)
                continue;

            if (progression < entry.storyPhase)
                continue;

            if (best == null || entry.storyPhase > best.storyPhase)
                best = entry;
        }

        return best;
    }

    void ApplyText(string text, bool animate)
    {
        string next = text ?? "";
        if (!animate || taskLabel == null || next == _currentText)
        {
            SetTextImmediate(next);
            return;
        }

        KillTextTween();

        bool hadVisibleText = !string.IsNullOrWhiteSpace(_currentText);
        bool willBeVisible = !string.IsNullOrWhiteSpace(next);
        bool runDecode = willBeVisible && decodeDuration > 0f;
        _currentText = next;

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetTarget(taskLabel);

        if (hadVisibleText && fadeOutDuration > 0f)
        {
            EnsureLabelVisible();
            sequence.Append(
                DOTween.To(GetLabelAlpha, SetLabelAlpha, 0f, fadeOutDuration)
                    .SetEase(fadeOutEase));
        }
        else
        {
            SetLabelAlpha(0f);
        }

        sequence.AppendCallback(() =>
        {
            taskLabel.text = runDecode ? BuildDecodedText(_currentText, 0f) : _currentText;
            ApplyVisibilityForEmpty();
            if (willBeVisible)
                EnsureLabelVisible();
        });

        if (willBeVisible)
            SoundManager.PlayOneShot("newTask");

        if (willBeVisible)
        {
            Sequence reveal = DOTween.Sequence().SetUpdate(true);

            if (fadeInDuration > 0f)
            {
                reveal.Join(
                    DOTween.To(GetLabelAlpha, SetLabelAlpha, _labelBaseColor.a, fadeInDuration)
                        .SetEase(fadeInEase));
            }
            else
            {
                reveal.AppendCallback(() => SetLabelAlpha(_labelBaseColor.a));
            }

            if (runDecode)
            {
                reveal.Join(
                    DOVirtual.Float(0f, 1f, decodeDuration, t =>
                    {
                        if (taskLabel != null)
                            taskLabel.text = BuildDecodedText(_currentText, t);
                    })
                        .SetEase(Ease.Linear)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            if (taskLabel != null)
                                taskLabel.text = _currentText;
                        }));
            }

            sequence.Append(reveal);
        }

        _textTween = sequence;
    }

    /// <summary>
    /// Scrambles undecoded glyphs; locks final characters left-to-right as <paramref name="progress"/> goes 0→1.
    /// Whitespace is never scrambled.
    /// </summary>
    string BuildDecodedText(string target, float progress)
    {
        if (string.IsNullOrEmpty(target))
            return "";

        progress = Mathf.Clamp01(progress);
        int length = target.Length;
        int resolvedCount = progress >= 1f
            ? length
            : Mathf.FloorToInt(progress * length);

        string glyphs = string.IsNullOrEmpty(decodeChars) ? DefaultDecodeChars : decodeChars;
        int glyphCount = glyphs.Length;

        _decodeBuilder.Clear();
        _decodeBuilder.EnsureCapacity(length);

        for (int i = 0; i < length; i++)
        {
            char c = target[i];
            if (char.IsWhiteSpace(c) || i < resolvedCount)
            {
                _decodeBuilder.Append(c);
                continue;
            }

            _decodeBuilder.Append(glyphs[Random.Range(0, glyphCount)]);
        }

        return _decodeBuilder.ToString();
    }

    void SetTextImmediate(string text)
    {
        KillTextTween();

        _currentText = text ?? "";

        if (taskLabel == null)
            return;

        taskLabel.text = _currentText;
        SetLabelAlpha(_labelBaseColor.a);
        ApplyVisibilityForEmpty();
    }

    void ApplyVisibilityForEmpty()
    {
        if (!hideWhenEmpty || taskLabel == null)
            return;

        bool visible = !string.IsNullOrWhiteSpace(_currentText);
        if (taskLabel.gameObject.activeSelf != visible)
            taskLabel.gameObject.SetActive(visible);
    }

    void EnsureLabelVisible()
    {
        if (taskLabel != null && !taskLabel.gameObject.activeSelf)
            taskLabel.gameObject.SetActive(true);
    }

    float GetLabelAlpha()
    {
        return taskLabel != null ? taskLabel.color.a : 0f;
    }

    void SetLabelAlpha(float alpha)
    {
        if (taskLabel == null)
            return;

        Color c = taskLabel.color;
        c.r = _labelBaseColor.r;
        c.g = _labelBaseColor.g;
        c.b = _labelBaseColor.b;
        c.a = alpha;
        taskLabel.color = c;
    }

    void KillTextTween()
    {
        if (_textTween == null)
            return;

        if (_textTween.IsActive())
            _textTween.Kill();

        _textTween = null;
    }

    static int CurrentProgression()
    {
        return GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;
    }
}
