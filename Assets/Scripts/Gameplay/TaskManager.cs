using System.Collections.Generic;
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

    [Header("UI")]
    [SerializeField] private TMP_Text taskLabel;

    [Tooltip("Shown when no entry matches the current progression.")]
    [SerializeField] private string fallbackText = "";

    [Tooltip("Disable the label GameObject when the resolved text is empty.")]
    [SerializeField] private bool hideWhenEmpty = true;

    [Header("Tasks")]
    [SerializeField] private List<TaskPhaseEntry> tasks = new();

    int _lastSeenProgression = int.MinValue;
    string _currentText = "";

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
    }

    void OnDestroy()
    {
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
        ApplyText(ResolveTaskText(progression));
    }

    /// <summary>
    /// Highest entry with <see cref="TaskPhaseEntry.storyPhase"/> &lt;= <paramref name="progression"/>,
    /// or <see cref="fallbackText"/> when none match.
    /// </summary>
    public string ResolveTaskText(int progression)
    {
        if (tasks == null || tasks.Count == 0)
            return fallbackText ?? "";

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

        if (best == null)
            return fallbackText ?? "";

        return best.taskText ?? "";
    }

    void ApplyText(string text)
    {
        _currentText = text ?? "";

        if (taskLabel == null)
            return;

        taskLabel.text = _currentText;

        if (!hideWhenEmpty)
            return;

        bool visible = !string.IsNullOrWhiteSpace(_currentText);
        if (taskLabel.gameObject.activeSelf != visible)
            taskLabel.gameObject.SetActive(visible);
    }

    static int CurrentProgression()
    {
        return GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;
    }
}
