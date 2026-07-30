using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

/// <summary>
/// Thin Unity Gaming Services Analytics wrapper for ending reaches and drop-off points.
/// Custom events must also be created in the Unity Dashboard Event Manager:
/// <list type="bullet">
/// <item><c>ending_reached</c> — ending_id (int), ending_name (string), seconds_to_ending (int), game_progression (int), story_phase (int)</item>
/// <item><c>session_stopped</c> — reason (string), game_progression (int), story_phase (int), seconds_played (int), reached_ending (bool)</item>
/// </list>
/// </summary>
public static class GameAnalytics
{
    public const string EndingReachedEvent = "ending_reached";
    public const string SessionStoppedEvent = "session_stopped";

    public const string StopReasonRestart = "restart";
    public const string StopReasonQuit = "quit";

    static bool _initStarted;
    static bool _ready;
    static bool _sessionActive;
    static bool _endingReportedThisSession;
    static float _sessionStartRealtime = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        Application.quitting += OnApplicationQuitting;
        _ = InitializeAsync();
    }

    static async Task InitializeAsync()
    {
        if (_initStarted)
            return;

        _initStarted = true;

        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            _ready = true;
            Debug.Log("[GameAnalytics] Unity Analytics ready.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameAnalytics] Init failed (events will be skipped): {e.Message}");
        }
    }

    /// <summary>Call when the player presses Start from the main menu.</summary>
    public static void StartSession()
    {
        _sessionActive = true;
        _endingReportedThisSession = false;
        _sessionStartRealtime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// Records that the player reached an ending cutscene (1 escape, 2 confession, 3 completion).
    /// </summary>
    public static void RecordEndingReached(int endingId)
    {
        if (endingId < GameManager.EscapeEndingCinematicIndex
            || endingId > GameManager.CompletionEndingCinematicIndex)
            return;

        if (!_sessionActive)
            StartSession();

        GetProgression(out int progression, out int phase);

        Record(EndingReachedEvent, new Dictionary<string, object>
        {
            { "ending_id", endingId },
            { "ending_name", EndingName(endingId) },
            { "seconds_to_ending", GetSessionSeconds() },
            { "game_progression", progression },
            { "story_phase", phase },
        });

        _endingReportedThisSession = true;
        Flush();
    }

    /// <summary>
    /// Records where the player stopped (restart / quit) using <c>game_progression</c> as stage.
    /// </summary>
    public static void RecordSessionStopped(string reason)
    {
        if (!_sessionActive)
            return;

        GetProgression(out int progression, out int phase);

        Record(SessionStoppedEvent, new Dictionary<string, object>
        {
            { "reason", reason ?? "unknown" },
            { "game_progression", progression },
            { "story_phase", phase },
            { "seconds_played", GetSessionSeconds() },
            { "reached_ending", _endingReportedThisSession },
        });

        Flush();
        _sessionActive = false;
    }

    static void OnApplicationQuitting()
    {
        RecordSessionStopped(StopReasonQuit);
    }

    static void Record(string eventName, Dictionary<string, object> parameters)
    {
        if (!_ready)
        {
            Debug.Log($"[GameAnalytics] (not ready) {eventName}: {FormatParams(parameters)}");
            return;
        }

        try
        {
            AnalyticsService.Instance.CustomData(eventName, parameters);
            Debug.Log($"[GameAnalytics] {eventName}: {FormatParams(parameters)}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameAnalytics] Failed to record '{eventName}': {e.Message}");
        }
    }

    static void Flush()
    {
        if (!_ready)
            return;

        try
        {
            AnalyticsService.Instance.Flush();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameAnalytics] Flush failed: {e.Message}");
        }
    }

    static int GetSessionSeconds()
    {
        if (_sessionStartRealtime < 0f)
            return 0;

        return Mathf.Max(0, Mathf.RoundToInt(Time.realtimeSinceStartup - _sessionStartRealtime));
    }

    static void GetProgression(out int gameProgression, out int storyPhase)
    {
        GlobalVariableOperator vars = GlobalVariableOperator.Instance;
        if (vars == null)
        {
            gameProgression = 0;
            storyPhase = 0;
            return;
        }

        gameProgression = vars.GameProgression;
        storyPhase = vars.GetInt(GlobalVariableOperator.StoryPhaseVar, 0);
    }

    static string EndingName(int endingId)
    {
        switch (endingId)
        {
            case GameManager.EscapeEndingCinematicIndex:
                return "escape";
            case GameManager.ConfessionEndingCinematicIndex:
                return "confession";
            case GameManager.CompletionEndingCinematicIndex:
                return "completion";
            default:
                return "unknown";
        }
    }

    static string FormatParams(Dictionary<string, object> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return "{}";

        var parts = new List<string>(parameters.Count);
        foreach (KeyValuePair<string, object> pair in parameters)
            parts.Add($"{pair.Key}={pair.Value}");

        return string.Join(", ", parts);
    }
}
