using System.Collections.Generic;
using UnityEngine;

public enum StoryCharacterId
{
    Mandy = 0,
    Lau = 1,
    Jason = 2,
    InternalDialogue = 3,
    IntroSequence = 4
}

[System.Serializable]
public class StoryPhaseEntry
{
    [Tooltip("Matches the Ink story_phase index for this dialogue.")]
    public int storyPhase;

    [Tooltip("Ink knot name to play, e.g. Mandy_story_phase_1")]
    public string knotName;

    [Tooltip("Unlock when GlobalVariableOperator.GameProgression >= this value.")]
    public int requiredProgression;

    [Tooltip("Exclusive upper bound: skip this knot when GameProgression > this. 0 or less = no max (never expires).")]
    public int maxProgression = 9999;

    [Tooltip("If true, this phase will not be offered again after it completes.")]
    public bool playOnce = true;

    /// <summary>
    /// True when <paramref name="progression"/> is within
    /// [requiredProgression, maxProgression] (max ignored when &lt;= 0).
    /// </summary>
    public bool IsAvailableAt(int progression)
    {
        if (progression < requiredProgression)
            return false;

        if (maxProgression > 0 && progression > maxProgression)
            return false;

        return true;
    }
}

/// <summary>
/// Phase catalogue for one story dialogue source (Mandy, Lau, Jason, internal, intro).
/// </summary>
[CreateAssetMenu(
    fileName = "StoryCharacterPhases",
    menuName = "Story/Character Phases",
    order = 0)]
public class StoryCharacterPhasesSO : ScriptableObject
{
    [SerializeField] private StoryCharacterId characterId;
    [SerializeField] private string displayName;
    [SerializeField] private TextAsset inkFile;
    [SerializeField] private List<StoryPhaseEntry> phases = new();

    public StoryCharacterId CharacterId => characterId;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public TextAsset InkFile => inkFile;
    public IReadOnlyList<StoryPhaseEntry> Phases => phases;
}
