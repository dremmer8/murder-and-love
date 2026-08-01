using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Swaps the TMP font and applies a relative font-size offset on every <see cref="TMP_Text"/>
/// when the language changes — including dialogue, monologue, intro, pager, hints, and menus.
/// Prefer placing on the Game scene Root; falls back to a runtime singleton if missing.
/// </summary>
[DefaultExecutionOrder(-45)]
public class LocalizedFontApplier : MonoBehaviour
{
    const float MinFontSize = 1f;

    static LocalizedFontApplier s_Instance;

    readonly Dictionary<int, TMP_FontAsset> _originalFonts = new();
    readonly Dictionary<int, float> _originalFontSizes = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (s_Instance != null)
            return;

        // Prefer a scene-placed instance (visible in the Editor hierarchy).
        s_Instance = FindFirstObjectByType<LocalizedFontApplier>();
        if (s_Instance != null)
            return;

        var go = new GameObject("[LocalizedFontApplier]");
        DontDestroyOnLoad(go);
        s_Instance = go.AddComponent<LocalizedFontApplier>();
    }

    /// <summary>
    /// Re-scan all TMP labels and apply the active locale font / size offset (or restore originals).
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
        if (!_originalFonts.ContainsKey(id))
            _originalFonts[id] = label.font;
        if (!_originalFontSizes.ContainsKey(id))
            _originalFontSizes[id] = label.fontSize;

        TMP_FontAsset targetFont = font != null ? font : _originalFonts[id];
        if (label.font != targetFont)
            label.font = targetFont;

        float targetSize = Mathf.Max(MinFontSize, _originalFontSizes[id] + sizeOffset);
        if (!Mathf.Approximately(label.fontSize, targetSize))
            label.fontSize = targetSize;
    }
}
