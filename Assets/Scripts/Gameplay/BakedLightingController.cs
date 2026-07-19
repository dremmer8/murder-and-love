using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Switches the scene between two baked lighting snapshots (lights on / blackout).
/// Bake each state in the Lighting window, then capture into
/// <see cref="BakedLightingScenario"/> assets with the editor buttons on this component.
/// </summary>
public class BakedLightingController : MonoBehaviour
{
    public static BakedLightingController Instance { get; private set; }

    public enum LightingState
    {
        LightsOn,
        Blackout
    }

    [Header("Scenarios")]
    [SerializeField] BakedLightingScenario lightsOnScenario;
    [SerializeField] BakedLightingScenario blackoutScenario;

    [Header("Start State")]
    [SerializeField] LightingState startState = LightingState.LightsOn;
    [SerializeField] bool applyOnStart = true;

    [Header("Realtime Lights / Objects")]
    [Tooltip("Enabled for lights-on, disabled for blackout (ceiling lamps, etc.).")]
    [SerializeField] List<Light> lightsActiveWhenLit = new();

    [Tooltip("Enabled for blackout only (flashlight, emergency bulbs, workingLight).")]
    [SerializeField] List<Light> lightsActiveWhenBlackout = new();

    [Tooltip("GameObjects enabled only when lit (emissive meshes, lamp props).")]
    [SerializeField] List<GameObject> objectsActiveWhenLit = new();

    [Tooltip("GameObjects enabled only during blackout.")]
    [SerializeField] List<GameObject> objectsActiveWhenBlackout = new();

    [Header("Transition")]
    [Tooltip("Optional full-screen CanvasGroup faded to black while swapping lightmaps.")]
    [SerializeField] CanvasGroup fadeOverlay;

    [SerializeField] float fadeOutDuration = 0.25f;
    [SerializeField] float fadeInDuration = 0.35f;

    [Header("Debug")]
    [SerializeField] bool logTransitions;

    LightingState _currentState;
    Coroutine _transitionRoutine;

    public LightingState CurrentState => _currentState;
    public bool IsBlackout => _currentState == LightingState.Blackout;
    public BakedLightingScenario LightsOnScenario => lightsOnScenario;
    public BakedLightingScenario BlackoutScenario => blackoutScenario;

    public event Action<LightingState> OnLightingStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("BakedLightingController: more than one instance in scene.");
            Destroy(this);
            return;
        }

        Instance = this;
        _currentState = startState;
    }

    void Start()
    {
        if (applyOnStart)
            ApplyState(_currentState, immediate: true);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ApplyLightsOn() => SetBlackout(false);
    public void ApplyBlackout() => SetBlackout(true);

    public void ToggleBlackout() => SetBlackout(!IsBlackout);

    /// <summary>UnityEvent / Ink-friendly: 0 = lights on, non-zero = blackout.</summary>
    public void SetBlackoutFromInt(int blackout) => SetBlackout(blackout != 0);

    public void SetBlackout(bool blackout)
    {
        LightingState target = blackout ? LightingState.Blackout : LightingState.LightsOn;
        if (target == _currentState && _transitionRoutine == null)
            return;

        ApplyState(target, immediate: fadeOverlay == null || fadeOutDuration <= 0f && fadeInDuration <= 0f);
    }

    public void BindInkExternals(Ink.Runtime.Story story)
    {
        if (story == null)
            return;

        story.BindExternalFunction("SetBlackout", (int blackout) => SetBlackoutFromInt(blackout));
        story.BindExternalFunction("ApplyLightsOn", () => ApplyLightsOn());
        story.BindExternalFunction("ApplyBlackout", () => ApplyBlackout());
    }

    public void ApplyState(LightingState state, bool immediate = true)
    {
        if (_transitionRoutine != null)
        {
            StopCoroutine(_transitionRoutine);
            _transitionRoutine = null;
            SetFadeAlpha(0f);
        }

        if (immediate)
        {
            ApplyStateImmediate(state);
            return;
        }

        _transitionRoutine = StartCoroutine(TransitionRoutine(state));
    }

    void ApplyStateImmediate(LightingState state)
    {
        BakedLightingScenario scenario = GetScenario(state);
        if (scenario == null)
        {
            Debug.LogWarning($"BakedLightingController: no scenario assigned for {state}. Applying object/light toggles only.");
        }
        else
        {
            ApplyScenario(scenario);
        }

        ApplyRealtimeToggles(state);
        ApplyWorkingMachines(state);
        _currentState = state;

        if (logTransitions)
            Debug.Log($"BakedLightingController: applied {state}");

        OnLightingStateChanged?.Invoke(state);
    }

    static void ApplyWorkingMachines(LightingState state)
    {
        if (state == LightingState.Blackout)
            DoWorkTrigger.PauseAllForBlackout();
        else
            DoWorkTrigger.ResumeAllAfterBlackout();
    }

    public static void ApplyScenario(BakedLightingScenario scenario)
    {
        if (scenario == null)
            return;

#if UNITY_EDITOR
        // In Edit Mode, prefer the full LightingData.asset (lightmaps + renderer bindings).
        if (!Application.isPlaying && TryApplyLightingDataAsset(scenario))
        {
            // The LightingData swap doesn't reliably refresh active light-probe SH in Edit Mode,
            // so force the scenario's captured probes on top of it.
            ApplyLightProbes(scenario);
            scenario.ApplyEnvironment();
            DynamicGI.UpdateEnvironment();
            return;
        }
#endif

        if (scenario.HasLightmaps)
        {
            LightmapSettings.lightmapsMode = scenario.lightmapsMode;
            LightmapSettings.lightmaps = scenario.ToLightmapData();
        }

        ApplyLightProbes(scenario);
        scenario.ApplyEnvironment();
        DynamicGI.UpdateEnvironment();
    }

#if UNITY_EDITOR
    static bool TryApplyLightingDataAsset(BakedLightingScenario scenario)
    {
        if (scenario.lightingDataAsset == null)
            return false;

        var lightingData = scenario.lightingDataAsset as UnityEditor.LightingDataAsset;
        if (lightingData == null)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(scenario.lightingDataAsset);
            lightingData = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.LightingDataAsset>(path);
        }

        if (lightingData == null)
            return false;

        UnityEditor.Lightmapping.lightingDataAsset = lightingData;
        Debug.Log($"BakedLightingController: applied LightingDataAsset '{lightingData.name}'.");
        return true;
    }
#endif

    static void ApplyLightProbes(BakedLightingScenario scenario)
    {
        if (!scenario.HasLightProbes)
            return;

        LightProbes probes = LightmapSettings.lightProbes;
        if (probes == null)
        {
            Debug.LogWarning("BakedLightingController: scenario has light probe data but the scene has no Light Probes.");
            return;
        }

        if (probes.count != scenario.bakedProbes.Length)
        {
            Debug.LogWarning(
                $"BakedLightingController: light probe count mismatch (scene {probes.count} vs scenario {scenario.LightProbeCount}). Skipping light probes.");
            return;
        }

        float scenarioEnergy = AverageProbeEnergy(scenario.bakedProbes);

        // Detach → write → reattach so Unity does not keep a stale probe cache.
        var copy = (SphericalHarmonicsL2[])scenario.bakedProbes.Clone();
        LightmapSettings.lightProbes = null;
        probes.bakedProbes = copy;
        LightmapSettings.lightProbes = probes;

        // Sync tetrahedralize so SH is available this frame (Async can lag a frame).
        LightProbes.Tetrahedralize();

        float liveEnergy = 0f;
        LightProbes live = LightmapSettings.lightProbes;
        if (live != null && live.bakedProbes != null)
            liveEnergy = AverageProbeEnergy(live.bakedProbes);

        if (Application.isPlaying)
        {
            Debug.Log(
                $"BakedLightingController: applied '{scenario.name}' light probes " +
                $"({scenario.bakedProbes.Length}). scenario energy={scenarioEnergy:F4}, live energy={liveEnergy:F4}");
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(probes);
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }

    static float AverageProbeEnergy(SphericalHarmonicsL2[] probes)
    {
        if (probes == null || probes.Length == 0)
            return 0f;

        double sum = 0;
        for (int i = 0; i < probes.Length; i++)
        {
            sum += probes[i][0, 0] + probes[i][1, 0] + probes[i][2, 0];
        }

        return (float)(sum / probes.Length);
    }

    void ApplyRealtimeToggles(LightingState state)
    {
        bool lit = state == LightingState.LightsOn;

        SetLightsEnabled(lightsActiveWhenLit, lit);
        SetLightsEnabled(lightsActiveWhenBlackout, !lit);
        SetObjectsActive(objectsActiveWhenLit, lit);
        SetObjectsActive(objectsActiveWhenBlackout, !lit);
    }

    static void SetLightsEnabled(List<Light> lights, bool enabled)
    {
        if (lights == null)
            return;

        for (int i = 0; i < lights.Count; i++)
        {
            Light light = lights[i];
            if (light == null || light.enabled == enabled)
                continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.Undo.RecordObject(light, enabled ? "Enable Light" : "Disable Light");
#endif
            light.enabled = enabled;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(light);
#endif
        }
    }

    static void SetObjectsActive(List<GameObject> objects, bool active)
    {
        if (objects == null)
            return;

        for (int i = 0; i < objects.Count; i++)
        {
            GameObject go = objects[i];
            if (go == null || go.activeSelf == active)
                continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.Undo.RecordObject(go, active ? "Enable Object" : "Disable Object");
#endif
            go.SetActive(active);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(go);
#endif
        }
    }

    BakedLightingScenario GetScenario(LightingState state)
    {
        return state == LightingState.Blackout ? blackoutScenario : lightsOnScenario;
    }

    IEnumerator TransitionRoutine(LightingState state)
    {
        if (fadeOverlay != null && fadeOutDuration > 0f)
            yield return FadeRoutine(0f, 1f, fadeOutDuration);
        else
            SetFadeAlpha(1f);

        ApplyStateImmediate(state);

        if (fadeOverlay != null && fadeInDuration > 0f)
            yield return FadeRoutine(1f, 0f, fadeInDuration);
        else
            SetFadeAlpha(0f);

        _transitionRoutine = null;
    }

    IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (fadeOverlay == null || duration <= 0f)
        {
            SetFadeAlpha(to);
            yield break;
        }

        fadeOverlay.gameObject.SetActive(true);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            SetFadeAlpha(a);
            yield return null;
        }

        SetFadeAlpha(to);
        if (to <= 0.001f)
            fadeOverlay.gameObject.SetActive(false);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null)
            return;

        fadeOverlay.alpha = alpha;
        fadeOverlay.blocksRaycasts = alpha > 0.01f;
    }

#if UNITY_EDITOR
    [ContextMenu("Apply Lights On (Editor)")]
    void ContextApplyLightsOn() => ApplyStateImmediate(LightingState.LightsOn);

    [ContextMenu("Apply Blackout (Editor)")]
    void ContextApplyBlackout() => ApplyStateImmediate(LightingState.Blackout);
#endif
}
