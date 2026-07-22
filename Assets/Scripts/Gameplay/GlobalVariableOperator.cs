using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

public class GlobalVariableOperator : MonoBehaviour
{
    public static GlobalVariableOperator Instance { get; private set; }

    public const string GameProgressionVar = "game_progression";
    public const string StoryPhaseVar = "story_phase";

    static readonly string[] TrackedVariables =
    {
        "mahjong_mentioned",
        "kitchen_knife",
        "gun_chosen",
        "has_detergent",
        "lied_about_cat",
        "black_out_happened",
        "did_insult",
        "told_lie_sick",
        "told_lie_busy",
        "lied_about_wine",
        "lied_about_hand",
        "coin_machine_attempt",
        StoryPhaseVar,
        GameProgressionVar
    };

    [Header("Debug")]
    [Tooltip("Edit in Play Mode to jump story progression. Synced with Ink game_progression.")]
    public int gameProgression;

    readonly Dictionary<string, object> _variables = new();
    readonly List<string> _choiceHistory = new();

    Story _boundStory;
    int _lastSeenProgression;

    public int GameProgression
    {
        get => gameProgression;
        set => SetGameProgression(value);
    }

    public IReadOnlyList<string> ChoiceHistory => _choiceHistory;
    public IReadOnlyDictionary<string, object> Variables => _variables;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GlobalVariableOperator: more than one instance in scene.");
            Destroy(this);
            return;
        }

        Instance = this;

        foreach (string name in TrackedVariables)
        {
            if (_variables.ContainsKey(name))
                continue;

            if (name == GameProgressionVar)
                _variables[name] = gameProgression;
            else if (name == StoryPhaseVar)
                _variables[name] = 1;
            else if (name == "coin_machine_attempt")
                _variables[name] = 0;
            else
                _variables[name] = false;
        }

        _lastSeenProgression = gameProgression;
    }

    void Update()
    {
        // Inspector tweaks in Play Mode don't always call OnValidate; pick them up here.
        if (gameProgression != _lastSeenProgression)
            SetGameProgression(gameProgression);
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            _lastSeenProgression = gameProgression;
            return;
        }

        if (_variables.Count == 0)
            return;

        SetGameProgression(gameProgression);
    }

    void OnDestroy()
    {
        UnbindStory();
        if (Instance == this)
            Instance = null;
    }

    public void SetGameProgression(int value)
    {
        SetGameProgression(value, allowBelowMilestoneFloor: false);
    }

    /// <summary>
    /// Sets <see cref="GameProgression"/>. When <paramref name="allowBelowMilestoneFloor"/> is true,
    /// skips the monotonic clamp (used by Mandy smoking guardrails to dial back after the pager
    /// jumps ahead).
    /// </summary>
    public void SetGameProgression(int value, bool allowBelowMilestoneFloor)
    {
        value = allowBelowMilestoneFloor ? value : ClampProgression(value);
        gameProgression = value;
        _lastSeenProgression = value;
        _variables[GameProgressionVar] = value;

        if (_boundStory != null
            && _boundStory.variablesState.GlobalVariableExistsWithName(GameProgressionVar))
        {
            _boundStory.variablesState[GameProgressionVar] = value;
        }
    }

    public void SetStoryPhase(int value)
    {
        _variables[StoryPhaseVar] = value;

        if (_boundStory != null
            && _boundStory.variablesState.GlobalVariableExistsWithName(StoryPhaseVar))
        {
            _boundStory.variablesState[StoryPhaseVar] = value;
        }
    }

    public void BindStory(Story story)
    {
        UnbindStory();
        if (story == null)
            return;

        _boundStory = story;
        story.ObserveVariables(TrackedVariables, OnVariableChanged);
    }

    public void UnbindStory()
    {
        if (_boundStory == null)
            return;

        _boundStory.RemoveVariableObserver(OnVariableChanged);
        _boundStory = null;
    }

    public void ApplyVariablesToStory(Story story)
    {
        if (story == null)
            return;

        _variables[GameProgressionVar] = gameProgression;

        foreach (var pair in _variables)
        {
            if (!story.variablesState.GlobalVariableExistsWithName(pair.Key))
                continue;

            story.variablesState[pair.Key] = pair.Value;
        }
    }

    public void SyncFromStory(Story story)
    {
        if (story == null)
            return;

        foreach (string name in TrackedVariables)
        {
            if (!story.variablesState.GlobalVariableExistsWithName(name))
                continue;

            object value = story.variablesState[name];
            _variables[name] = value;

            if (name == GameProgressionVar)
                ApplyProgressionFromValue(value);
        }
    }

    public void RecordChoice(string choiceText)
    {
        if (string.IsNullOrEmpty(choiceText))
            return;

        _choiceHistory.Add(choiceText);
    }

    public bool GetBool(string variableName, bool defaultValue = false)
    {
        if (_variables.TryGetValue(variableName, out object value) && value is bool b)
            return b;
        return defaultValue;
    }

    public int GetInt(string variableName, int defaultValue = 0)
    {
        if (variableName == GameProgressionVar)
            return gameProgression;

        if (!_variables.TryGetValue(variableName, out object value) || value == null)
            return defaultValue;

        return ToInt(value, defaultValue);
    }

    public bool TryGetVariable(string variableName, out object value)
    {
        if (variableName == GameProgressionVar)
        {
            value = gameProgression;
            return true;
        }

        return _variables.TryGetValue(variableName, out value);
    }

    void OnVariableChanged(string variableName, object newValue)
    {
        _variables[variableName] = newValue;

        if (variableName == GameProgressionVar)
            ApplyProgressionFromValue(newValue);
    }

    void ApplyProgressionFromValue(object value)
    {
        int requested = ToInt(value, gameProgression);
        int progression = ClampProgression(requested);

        gameProgression = progression;
        _lastSeenProgression = progression;
        _variables[GameProgressionVar] = progression;

        // Ink may have written a lower value (e.g. re-entering an earlier knot);
        // push the clamped value back so story state stays consistent.
        if (progression != requested
            && _boundStory != null
            && _boundStory.variablesState.GlobalVariableExistsWithName(GameProgressionVar))
        {
            _boundStory.variablesState[GameProgressionVar] = progression;
        }
    }

    /// <summary>
    /// Progression is monotonic: Ink (including pager knots that rewrite an older
    /// absolute value) must not pull <see cref="GameProgression"/> backward.
    /// Use <see cref="SetGameProgression(int, bool)"/> with allowBelowMilestoneFloor
    /// for intentional dial-backs (e.g. Mandy smoking guardrail).
    /// </summary>
    int ClampProgression(int value)
    {
        if (value < gameProgression)
            return gameProgression;

        return value;
    }

    static int ToInt(object value, int defaultValue = 0)
    {
        switch (value)
        {
            case int i:
                return i;
            case long l:
                return (int)l;
            case bool b:
                return b ? 1 : 0;
            default:
                return defaultValue;
        }
    }
}
