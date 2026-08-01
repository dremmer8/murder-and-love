using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Swaps the TMP font and applies a relative font-size offset on UI (<see cref="TextMeshProUGUI"/>)
/// labels when the language changes — dialogue, monologue, intro, menus, HUD.
/// <para>
/// World-space <see cref="TextMeshPro"/> (pager screens, 3D hints, etc.) is left untouched:
/// assigning <c>font</c> replaces the material with the font asset default and can make
/// those meshes invisible (wrong shader / atlas).
/// </para>
/// </summary>
[DefaultExecutionOrder(-45)]
public class LocalizedFontApplier : MonoBehaviour
{
    const float MinFontSize = 1f;

    struct OriginalStyle
    {
        public TMP_FontAsset Font;
        public Material SharedMaterial;
        public float FontSize;
    }

    static LocalizedFontApplier s_Instance;

    readonly Dictionary<int, OriginalStyle> _originals = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (s_Instance != null)
            return;

        s_Instance = FindFirstObjectByType<LocalizedFontApplier>();
        if (s_Instance != null)
            return;

        var go = new GameObject("[LocalizedFontApplier]");
        DontDestroyOnLoad(go);
        s_Instance = go.AddComponent<LocalizedFontApplier>();
    }

    /// <summary>
    /// Re-scan UI TMP labels and apply the active locale font / size offset (or restore originals).
    /// Safe to call after spawning new UI at runtime.
    /// </summary>
    public static void ApplyNow()
    {
        if (s_Instance == null)
            Bootstrap();

        if (s_Instance != null)
            s_Instance.ApplyAll();
    }

    void Awake()
    {
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(this);
            return;
        }

        s_Instance = this;
    }

    void OnEnable()
    {
        if (s_Instance != null && s_Instance != this)
            return;

        LocalizationService.EnsureInitialized();
        LocalizationService.LanguageChanged += ApplyAll;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyAll();
    }

    void OnDisable()
    {
        if (s_Instance != null && s_Instance != this)
            return;

        LocalizationService.LanguageChanged -= ApplyAll;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (s_Instance == this)
            s_Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyAll();
    }

    void ApplyAll()
    {
        LocalizationService.EnsureInitialized();
        TMP_FontAsset font = LocalizationService.CurrentFont;
        float sizeOffset = LocalizationService.CurrentFontSizeOffset;

        TMP_Text[] labels = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < labels.Length; i++)
        {
            TMP_Text label = labels[i];
            if (label == null)
                continue;

            ApplyTo(label, font, sizeOffset);
        }
    }

    void ApplyTo(TMP_Text label, TMP_FontAsset font, float sizeOffset)
    {
        int id = label.GetInstanceID();
        CaptureOriginal(label, id);
        OriginalStyle original = _originals[id];

        // World-space TMP (pager, 3D hints, …): never assign .font — it replaces the
        // authored material with the font default and often makes the mesh invisible.
        if (label is TextMeshPro)
        {
            RestoreOriginalFontAndMaterial(label, original);
            return;
        }

        TMP_FontAsset targetFont = font != null ? font : original.Font;
        if (label.font != targetFont)
            label.font = targetFont;

        // When using the scene font again, restore the authored material preset too.
        if (font == null || targetFont == original.Font)
            RestoreMaterial(label, original);

        if (Mathf.Abs(sizeOffset) > 0.0001f)
        {
            float targetSize = Mathf.Max(MinFontSize, original.FontSize + sizeOffset);
            if (!Mathf.Approximately(label.fontSize, targetSize))
                label.fontSize = targetSize;
        }
        else if (!Mathf.Approximately(label.fontSize, original.FontSize))
        {
            label.fontSize = original.FontSize;
        }
    }

    void CaptureOriginal(TMP_Text label, int id)
    {
        if (_originals.ContainsKey(id))
            return;

        _originals[id] = new OriginalStyle
        {
            Font = label.font,
            SharedMaterial = label.fontSharedMaterial,
            FontSize = label.fontSize
        };
    }

    static void RestoreOriginalFontAndMaterial(TMP_Text label, OriginalStyle original)
    {
        if (label.font != original.Font)
            label.font = original.Font;

        RestoreMaterial(label, original);
    }

    static void RestoreMaterial(TMP_Text label, OriginalStyle original)
    {
        if (original.SharedMaterial == null)
            return;

        if (label.fontSharedMaterial != original.SharedMaterial)
            label.fontSharedMaterial = original.SharedMaterial;
    }
}
