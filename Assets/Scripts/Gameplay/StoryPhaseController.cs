using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to an interactable entity. Reads phase unlock data from a
/// <see cref="StoryCharacterPhasesSO"/> and resolves which Ink knot to play.
/// </summary>
public class StoryPhaseController : MonoBehaviour
{
    // Hardcoded Mandy smoking chain (story phases 26/27/28). Shared across every Mandy
    // StoryPhaseController so auto-talk (26) and revisit (27) see the same checkpoint.
    public const string MandySmokingScene1Knot = "Mandy_smoking_scene_1";
    public const string MandySmokingScene2Knot = "Mandy_smoking_scene_2";
    public const string MandySmokingScene3Knot = "Mandy_smoking_scene_3";

    const int MandySmokingProgressionMin = 26;
    const int MandySmokingProgressionMax = 28;

    static bool s_hasMandySmokingResume;
    static int s_mandySmokingResumeProgression;
    static readonly HashSet<string> s_mandySmokingCompletedKnots = new();

    [SerializeField] private StoryCharacterPhasesSO characterPhases;

    readonly HashSet<string> _completedKnots = new();

    public StoryCharacterPhasesSO CharacterPhases => characterPhases;
    public TextAsset InkFile => characterPhases != null ? characterPhases.InkFile : null;

    public IReadOnlyList<StoryPhaseEntry> Phases =>
        characterPhases != null ? characterPhases.Phases : System.Array.Empty<StoryPhaseEntry>();

    public bool IsMandy =>
        characterPhases != null && characterPhases.CharacterId == StoryCharacterId.Mandy;

    /// <summary>
    /// Last Mandy smoking checkpoint (26–28) snapped when a smoking knot ends.
    /// Used to dial progression back if the pager jumps ahead.
    /// </summary>
    public static bool HasMandySmokingResume => s_hasMandySmokingResume;

    public static int MandySmokingResumeProgression => s_mandySmokingResumeProgression;

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

        // Mandy only: dial progression back to the smoking checkpoint before unlock checks.
        ApplyMandySmokingGuardrailBeforeResolve();

        int progression = GlobalVariableOperator.Instance.GameProgression;
        StoryPhaseEntry best = null;

        foreach (StoryPhaseEntry entry in characterPhases.Phases)
        {
            if (entry == null || string.IsNullOrEmpty(entry.knotName))
                continue;

            if (!entry.IsAvailableAt(progression))
                continue;

            if (entry.playOnce && IsKnotCompletedForResolve(entry.knotName))
                continue;

            if (best == null || entry.requiredProgression > best.requiredProgression)
                best = entry;
        }

        return best;
    }

    bool IsKnotCompletedForResolve(string knotName)
    {
        if (_completedKnots.Contains(knotName))
            return true;

        // Smoking play-once must survive across separate Mandy trigger objects (26 vs 27).
        return IsMandySmokingKnot(knotName) && s_mandySmokingCompletedKnots.Contains(knotName);
    }

    static bool IsMandySmokingKnot(string knotName)
    {
        return knotName == MandySmokingScene1Knot
            || knotName == MandySmokingScene2Knot
            || knotName == MandySmokingScene3Knot;
    }

    /// <summary>
    /// Snap a 26–28 resume checkpoint when a Mandy smoking knot ends, before chained
    /// triggers (e.g. boyfriend pager ending) can bump <c>game_progression</c> past 28.
    /// </summary>
    public void NoteMandySmokingCheckpoint(string completedKnot)
    {
        if (!IsMandy || string.IsNullOrEmpty(completedKnot) || !IsMandySmokingKnot(completedKnot))
            return;

        s_mandySmokingCompletedKnots.Add(completedKnot);

        if (GlobalVariableOperator.Instance == null)
            return;

        int progression = GlobalVariableOperator.Instance.GameProgression;
        if (progression < MandySmokingProgressionMin || progression > MandySmokingProgressionMax)
        {
            Debug.Log(
                $"[MandySmokingGuardrail] skip checkpoint knot={completedKnot} " +
                $"progression={progression} (outside {MandySmokingProgressionMin}-{MandySmokingProgressionMax})",
                this);
            return;
        }

        s_mandySmokingResumeProgression = progression;
        s_hasMandySmokingResume = true;
        Debug.Log(
            $"[MandySmokingGuardrail] checkpoint knot={completedKnot} resume={progression}",
            this);
    }

    /// <summary>
    /// If something (pager) jumped <c>game_progression</c> past the last Mandy smoking
    /// checkpoint, dial it back so unlock ranges still hit the next unseen smoking knot.
    /// </summary>
    public void ApplyMandySmokingGuardrailBeforeResolve()
    {
        if (!IsMandy || !s_hasMandySmokingResume)
            return;

        if (GlobalVariableOperator.Instance == null)
            return;

        int current = GlobalVariableOperator.Instance.GameProgression;
        int target = ResolveMandySmokingTargetProgression();
        if (current <= target)
            return;

        Debug.LogWarning(
            $"[MandySmokingGuardrail] dial-down game_progression {current} → {target} " +
            $"(resume={s_mandySmokingResumeProgression}, " +
            $"smokingCompleted=[{string.Join(", ", s_mandySmokingCompletedKnots)}])",
            this);

        GlobalVariableOperator.Instance.SetGameProgression(target, allowBelowMilestoneFloor: true);
    }

    /// <summary>
    /// Hardcoded target for the next unseen smoking beat:
    /// 26 intro → 27 admit/refuse revisit → 28 escape/stall.
    /// </summary>
    static int ResolveMandySmokingTargetProgression()
    {
        int target = s_mandySmokingResumeProgression;

        // Never offer scene_3 while scene_1 is the only completed smoking beat and
        // the checkpoint still says we left on the admit/refuse beat (27).
        if (!s_mandySmokingCompletedKnots.Contains(MandySmokingScene1Knot))
            return Mathf.Min(target, MandySmokingProgressionMin);

        // Scene 1 always chains into scene 2; if we never reached 28 via Mandy, stay on 27.
        if (target < MandySmokingProgressionMax)
            return Mathf.Clamp(target, MandySmokingProgressionMin, 27);

        return Mathf.Clamp(target, MandySmokingProgressionMin, MandySmokingProgressionMax);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetMandySmokingGuardrailStaticState()
    {
        s_hasMandySmokingResume = false;
        s_mandySmokingResumeProgression = 0;
        s_mandySmokingCompletedKnots.Clear();
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

        ApplyMandySmokingGuardrailBeforeResolve();

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

            if (entry.playOnce && IsKnotCompletedForResolve(entry.knotName))
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
        NoteMandySmokingCheckpoint(knotName);
    }

    public bool HasCompletedKnot(string knotName)
    {
        if (string.IsNullOrEmpty(knotName))
            return false;

        return IsKnotCompletedForResolve(knotName);
    }
}
