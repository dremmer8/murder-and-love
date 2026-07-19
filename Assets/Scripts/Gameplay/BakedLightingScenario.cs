using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Snapshot of baked lighting for one state (lights on / blackout):
/// LightingData.asset, lightmaps, light probes, reflection probes, and ambient.
/// Capture via the editor tools on <see cref="BakedLightingController"/> after baking.
/// </summary>
[CreateAssetMenu(fileName = "BakedLightingScenario", menuName = "MurderAndLove/Baked Lighting Scenario")]
public class BakedLightingScenario : ScriptableObject
{
    [Serializable]
    public class LightmapEntry
    {
        public Texture2D lightmapColor;
        public Texture2D lightmapDir;
        public Texture2D shadowMask;
    }

    [Serializable]
    public class ReflectionProbeEntry
    {
        [Tooltip("Hierarchy path used to find the probe when applying (Root/Child/Probe).")]
        public string hierarchyPath;

        [Tooltip("Copied baked / custom cubemap for this probe.")]
        public Texture cubemap;

        public float intensity = 1f;
        public bool boxProjection;
        public Vector3 size = Vector3.one;
        public Vector3 center;
        public float blendDistance;
        public int importance = 1;
    }

    [Header("Lighting Data Asset")]
    [Tooltip("Copied LightingData.asset (Editor). Preview uses Lightmapping.lightingDataAsset. Builds use the lightmaps/probes below.")]
    public UnityEngine.Object lightingDataAsset;

    [Header("Lightmaps")]
    public LightmapsMode lightmapsMode = LightmapsMode.CombinedDirectional;
    public LightmapEntry[] lightmaps = Array.Empty<LightmapEntry>();

    [Header("Light Probes")]
    [Tooltip("Baked SH coefficients. Probe count must match the scene Light Probe Group when applied.")]
    public SphericalHarmonicsL2[] bakedProbes = Array.Empty<SphericalHarmonicsL2>();

    [Header("Reflection Probes")]
    [Tooltip("If true, assign captured cubemaps onto matching scene ReflectionProbes.")]
    public bool applyReflectionProbes = true;

    public ReflectionProbeEntry[] reflectionProbes = Array.Empty<ReflectionProbeEntry>();

    [Header("Ambient / Environment")]
    public bool applyAmbient = true;
    public AmbientMode ambientMode = AmbientMode.Skybox;
    public Color ambientSkyColor = new(0.212f, 0.227f, 0.259f);
    public Color ambientEquatorColor = new(0.114f, 0.125f, 0.133f);
    public Color ambientGroundColor = new(0.047f, 0.043f, 0.035f);
    public Color ambientLight = Color.gray;
    public float ambientIntensity = 1f;
    public float reflectionIntensity = 1f;
    public DefaultReflectionMode defaultReflectionMode = DefaultReflectionMode.Skybox;
    public int defaultReflectionResolution = 128;
    public Texture customReflection;

    [Header("Fog (optional)")]
    public bool applyFog;
    public bool fogEnabled;
    public Color fogColor = Color.gray;
    public FogMode fogMode = FogMode.Linear;
    public float fogDensity = 0.01f;
    public float fogStartDistance;
    public float fogEndDistance = 300f;

    public int LightmapCount => lightmaps != null ? lightmaps.Length : 0;
    public int LightProbeCount => bakedProbes != null ? bakedProbes.Length : 0;
    public int ReflectionProbeCount => reflectionProbes != null ? reflectionProbes.Length : 0;

    public bool HasLightingDataAsset => lightingDataAsset != null;

    [Obsolete("Use LightProbeCount")]
    public int ProbeCount => LightProbeCount;

    public bool HasLightmaps
    {
        get
        {
            if (lightmaps == null || lightmaps.Length == 0)
                return false;

            for (int i = 0; i < lightmaps.Length; i++)
            {
                if (lightmaps[i] != null && lightmaps[i].lightmapColor != null)
                    return true;
            }

            return false;
        }
    }

    public bool HasLightProbes => bakedProbes != null && bakedProbes.Length > 0;

    public bool HasReflectionProbes
    {
        get
        {
            if (reflectionProbes == null || reflectionProbes.Length == 0)
                return false;

            for (int i = 0; i < reflectionProbes.Length; i++)
            {
                if (reflectionProbes[i] != null && reflectionProbes[i].cubemap != null)
                    return true;
            }

            return false;
        }
    }

    public LightmapData[] ToLightmapData()
    {
        if (lightmaps == null || lightmaps.Length == 0)
            return Array.Empty<LightmapData>();

        var data = new LightmapData[lightmaps.Length];
        for (int i = 0; i < lightmaps.Length; i++)
        {
            LightmapEntry entry = lightmaps[i];
            data[i] = new LightmapData
            {
                lightmapColor = entry != null ? entry.lightmapColor : null,
                lightmapDir = entry != null ? entry.lightmapDir : null,
                shadowMask = entry != null ? entry.shadowMask : null
            };
        }

        return data;
    }

    public void ApplyEnvironment()
    {
        if (applyAmbient)
        {
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.defaultReflectionMode = defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = defaultReflectionResolution;

            if (defaultReflectionMode == DefaultReflectionMode.Custom && customReflection is Cubemap cubemap)
                RenderSettings.customReflection = cubemap;
        }

        if (!applyFog)
            return;

        RenderSettings.fog = fogEnabled;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
    }

    public void CopyEnvironmentFromScene()
    {
        ambientMode = RenderSettings.ambientMode;
        ambientSkyColor = RenderSettings.ambientSkyColor;
        ambientEquatorColor = RenderSettings.ambientEquatorColor;
        ambientGroundColor = RenderSettings.ambientGroundColor;
        ambientLight = RenderSettings.ambientLight;
        ambientIntensity = RenderSettings.ambientIntensity;
        reflectionIntensity = RenderSettings.reflectionIntensity;
        defaultReflectionMode = RenderSettings.defaultReflectionMode;
        defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
        customReflection = TryGetCustomReflection();

        fogEnabled = RenderSettings.fog;
        fogColor = RenderSettings.fogColor;
        fogMode = RenderSettings.fogMode;
        fogDensity = RenderSettings.fogDensity;
        fogStartDistance = RenderSettings.fogStartDistance;
        fogEndDistance = RenderSettings.fogEndDistance;
    }

    /// <summary>
    /// Unity throws if customReflection is set to a non-cubemap texture.
    /// </summary>
    public static Cubemap TryGetCustomReflection()
    {
        try
        {
            return RenderSettings.customReflection;
        }
        catch (System.ArgumentException)
        {
            return null;
        }
    }

    public void ApplyReflectionProbes()
    {
        if (!applyReflectionProbes || reflectionProbes == null || reflectionProbes.Length == 0)
            return;

        ReflectionProbe[] sceneProbes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        var byPath = new System.Collections.Generic.Dictionary<string, ReflectionProbe>(sceneProbes.Length);
        for (int i = 0; i < sceneProbes.Length; i++)
        {
            if (sceneProbes[i] == null)
                continue;

            string path = GetHierarchyPath(sceneProbes[i].transform);
            byPath[path] = sceneProbes[i];
        }

        int applied = 0;
        for (int i = 0; i < reflectionProbes.Length; i++)
        {
            ReflectionProbeEntry entry = reflectionProbes[i];
            if (entry == null || entry.cubemap == null || string.IsNullOrEmpty(entry.hierarchyPath))
                continue;

            if (!byPath.TryGetValue(entry.hierarchyPath, out ReflectionProbe probe) || probe == null)
            {
                Debug.LogWarning($"BakedLightingScenario: no ReflectionProbe at path '{entry.hierarchyPath}'.");
                continue;
            }

            probe.mode = ReflectionProbeMode.Custom;
            probe.customBakedTexture = entry.cubemap;
            probe.intensity = entry.intensity;
            probe.boxProjection = entry.boxProjection;
            probe.size = entry.size;
            probe.center = entry.center;
            probe.blendDistance = entry.blendDistance;
            probe.importance = entry.importance;
            applied++;
        }

        if (applied > 0)
            Debug.Log($"BakedLightingScenario: applied {applied} reflection probe cubemap(s).");
    }

    public static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
            return string.Empty;

        var parts = new System.Collections.Generic.List<string>(8);
        Transform current = transform;
        while (current != null)
        {
            parts.Add(current.name);
            current = current.parent;
        }

        parts.Reverse();
        return string.Join("/", parts);
    }
}
