using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PropPhaseEntry
{
    [Tooltip("Applies when game_progression >= this value (same numbering as Interactable / Story Phase gates).")]
    public int storyPhase;

    [Tooltip("Enabled when this phase threshold is reached.")]
    public List<GameObject> objectsToActivate = new();

    [Tooltip("Disabled when this phase threshold is reached.")]
    public List<GameObject> objectsToDeactivate = new();
}

/// <summary>
/// Activates / deactivates scene props based on <see cref="GlobalVariableOperator.GameProgression"/>.
/// Each phase entry runs at most once per play session when its threshold is first reached.
/// </summary>
public class PropProgressionManager : MonoBehaviour
{
    [SerializeField] private List<PropPhaseEntry> phases = new();

    [Tooltip("If true, apply newly reached phases on Start / when progression changes.")]
    [SerializeField] private bool applyOnProgressionChange = true;

    int _lastSeenProgression = int.MinValue;
    readonly HashSet<int> _appliedPhaseIndices = new();

    public IReadOnlyList<PropPhaseEntry> Phases => phases;

    void Start()
    {
        ApplyForCurrentProgression(force: true);
    }

    void Update()
    {
        if (!applyOnProgressionChange)
            return;

        int progression = CurrentProgression();
        if (progression == _lastSeenProgression)
            return;

        ApplyForProgression(progression);
    }

    /// <summary>
    /// Re-reads <see cref="GlobalVariableOperator.GameProgression"/> and applies any not-yet-fired entries.
    /// </summary>
    public void ApplyForCurrentProgression(bool force = false)
    {
        int progression = CurrentProgression();
        if (!force && progression == _lastSeenProgression)
            return;

        ApplyForProgression(progression);
    }

    /// <summary>
    /// Applies each entry whose <see cref="PropPhaseEntry.storyPhase"/> is &lt;= <paramref name="progression"/>
    /// and has not already been applied this session, sorted ascending.
    /// </summary>
    public void ApplyForProgression(int progression)
    {
        _lastSeenProgression = progression;

        if (phases == null || phases.Count == 0)
            return;

        List<int> pendingIndices = new(phases.Count);
        for (int i = 0; i < phases.Count; i++)
        {
            PropPhaseEntry entry = phases[i];
            if (entry == null)
                continue;

            if (progression < entry.storyPhase)
                continue;

            if (_appliedPhaseIndices.Contains(i))
                continue;

            pendingIndices.Add(i);
        }

        pendingIndices.Sort((a, b) => phases[a].storyPhase.CompareTo(phases[b].storyPhase));

        for (int i = 0; i < pendingIndices.Count; i++)
        {
            int index = pendingIndices[i];
            ApplyEntry(phases[index]);
            _appliedPhaseIndices.Add(index);
        }
    }

    static void ApplyEntry(PropPhaseEntry entry)
    {
        SetActiveList(entry.objectsToActivate, true);
        SetActiveList(entry.objectsToDeactivate, false);
    }

    static int CurrentProgression()
    {
        return GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;
    }

    static void SetActiveList(List<GameObject> list, bool active)
    {
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
                list[i].SetActive(active);
        }
    }
}
