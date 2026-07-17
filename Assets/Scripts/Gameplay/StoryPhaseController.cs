using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to an interactable entity. Reads phase unlock data from a
/// <see cref="StoryCharacterPhasesSO"/> and resolves which Ink knot to play.
/// </summary>
public class StoryPhaseController : MonoBehaviour
{
    [SerializeField] private StoryCharacterPhasesSO characterPhases;

    readonly HashSet<string> _completedKnots = new();

    public StoryCharacterPhasesSO CharacterPhases => characterPhases;
    public TextAsset InkFile => characterPhases != null ? characterPhases.InkFile : null;

    public IReadOnlyList<StoryPhaseEntry> Phases =>
        characterPhases != null ? characterPhases.Phases : System.Array.Empty<StoryPhaseEntry>();

    /// <summary>
    /// Returns the latest unlocked knot this entity can play, or empty if none.
    /// </summary>
    public string ResolveKnot()
    {
        if (GlobalVariableOperator.Instance == null || characterPhases == null)
            return string.Empty;

        int progression = GlobalVariableOperator.Instance.GameProgression;
        StoryPhaseEntry best = null;

        foreach (StoryPhaseEntry entry in characterPhases.Phases)
        {
            if (entry == null || string.IsNullOrEmpty(entry.knotName))
                continue;

            if (!entry.IsAvailableAt(progression))
                continue;

            if (entry.playOnce && _completedKnots.Contains(entry.knotName))
                continue;

            if (best == null || entry.requiredProgression > best.requiredProgression)
                best = entry;
        }

        return best != null ? best.knotName : string.Empty;
    }

    /// <summary>
    /// Returns the knot whose <see cref="StoryPhaseEntry.storyPhase"/> matches
    /// <paramref name="storyPhase"/>, or empty if none.
    /// </summary>
    public string ResolveKnotForStoryPhase(int storyPhase)
    {
        if (characterPhases == null)
            return string.Empty;

        foreach (StoryPhaseEntry entry in characterPhases.Phases)
        {
            if (entry == null || string.IsNullOrEmpty(entry.knotName))
                continue;

            if (entry.storyPhase != storyPhase)
                continue;

            if (entry.playOnce && _completedKnots.Contains(entry.knotName))
                return string.Empty;

            return entry.knotName;
        }

        return string.Empty;
    }

    public void MarkKnotCompleted(string knotName)
    {
        if (string.IsNullOrEmpty(knotName))
            return;

        _completedKnots.Add(knotName);
    }

    public bool HasCompletedKnot(string knotName)
    {
        return !string.IsNullOrEmpty(knotName) && _completedKnots.Contains(knotName);
    }
}
