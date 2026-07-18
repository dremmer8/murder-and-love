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
        StoryPhaseEntry best = ResolveBestEntry();
        return best != null ? best.knotName : string.Empty;
    }

    /// <summary>
    /// Latest unlocked phase entry for current <see cref="GlobalVariableOperator.GameProgression"/>, or null.
    /// </summary>
    public StoryPhaseEntry ResolveBestEntry()
    {
        if (GlobalVariableOperator.Instance == null || characterPhases == null)
            return null;

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

        return best;
    }

    /// <summary>
    /// Returns the knot whose <see cref="StoryPhaseEntry.storyPhase"/> matches
    /// <paramref name="storyPhase"/>, or empty if none / not available at current progression.
    /// </summary>
    public string ResolveKnotForStoryPhase(int storyPhase)
    {
        StoryPhaseEntry entry = FindEntryForStoryPhase(storyPhase);
        return entry != null ? entry.knotName : string.Empty;
    }

    /// <summary>
    /// Phase catalogue entry for <paramref name="storyPhase"/>, if unlocked and not play-once completed.
    /// </summary>
    public StoryPhaseEntry FindEntryForStoryPhase(int storyPhase)
    {
        if (characterPhases == null)
            return null;

        int progression = GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;

        foreach (StoryPhaseEntry entry in characterPhases.Phases)
        {
            if (entry == null || string.IsNullOrEmpty(entry.knotName))
                continue;

            if (entry.storyPhase != storyPhase)
                continue;

            if (!entry.IsAvailableAt(progression))
                return null;

            if (entry.playOnce && _completedKnots.Contains(entry.knotName))
                return null;

            return entry;
        }

        return null;
    }

    /// <summary>
    /// Looks up the catalogue entry for a knot name (ignores progression / play-once).
    /// </summary>
    public StoryPhaseEntry FindEntryForKnot(string knotName)
    {
        if (characterPhases == null || string.IsNullOrEmpty(knotName))
            return null;

        foreach (StoryPhaseEntry entry in characterPhases.Phases)
        {
            if (entry == null || string.IsNullOrEmpty(entry.knotName))
                continue;

            if (entry.knotName == knotName)
                return entry;
        }

        return null;
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
