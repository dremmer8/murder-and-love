using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Swaps the TMP font on every label when the language changes — dialogue, monologue, intro, menus,
/// HUD, and world-space text such as minigame step hints. The relative font-size offset is UI-only.
/// <para>
/// Assigning <c>font</c> resets the material to the font asset default, so the shader variant the
/// label was rendering with (e.g. Distance Field Overlay) is re-applied afterwards. Labels whose font
/// the catalogue lists under preserved fonts — the pager's pixel screen and friends — are skipped, as
/// are labels under a <see cref="KeepAuthoredFont"/> marker.
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

        if (LocalizationService.IsFontPreserved(original.Font) || KeepsAuthoredFont(label))
        {
            RestoreOriginalFontAndMaterial(label, original);
            if (!Mathf.Approximately(label.fontSize, original.FontSize))
                label.fontSize = original.FontSize;
            return;
        }

        TMP_FontAsset targetFont = font != null ? font : original.Font;
        ApplyFont(label, targetFont);

        // When using the scene font again, restore the authored material preset too.
        if (font == null || targetFont == original.Font)
            RestoreMaterial(label, original);

        // The offset is authored in UI points; world-space labels are sized in metres, where the
        // same number would be a huge jump, so they keep their authored size.
        if (label is not TextMeshProUGUI)
            return;

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

    /// <summary>
    /// The marker sits on a parent so one component covers a whole screen; the search includes
    /// inactive objects because screens such as the credits are scanned while still hidden.
    /// </summary>
    static bool KeepsAuthoredFont(TMP_Text label)
    {
        return label.GetComponentInParent<KeepAuthoredFont>(true) != null;
    }

    void CaptureOriginal(TMP_Text label, int id)
    {
        if (_originals.ContainsKey(id))
            return;

        _originals[id] = new OriginalStyle
        {
            Font = label.font,
            SharedMaterial = ResolveUsableMaterial(label.font, label.fontSharedMaterial),
            FontSize = label.fontSize
        };
    }

    /// <summary>
    /// A material preset only draws correctly with the font asset whose atlas it samples. Labels
    /// that sit inactive at scan time have not run TMP's own font/material repair yet, so their
    /// serialized preset may still belong to a font the label no longer uses — reinstating that
    /// preset later would sample the wrong atlas and draw garbled, oversized glyphs.
    /// </summary>
    static Material ResolveUsableMaterial(TMP_FontAsset font, Material material)
    {
        if (font == null)
            return material;

        return SamplesAtlasOf(font, material) ? material : font.material;
    }

    static bool SamplesAtlasOf(TMP_FontAsset font, Material material)
    {
        if (font == null || material == null)
            return false;

        Texture atlas = font.atlasTexture;
        if (atlas == null || !material.HasProperty(ShaderUtilities.ID_MainTex))
            return false;

        return material.GetTexture(ShaderUtilities.ID_MainTex) == atlas;
    }

    static void ApplyFont(TMP_Text label, TMP_FontAsset targetFont)
    {
        if (targetFont == null || label.font == targetFont)
            return;

        Material previous = label.fontSharedMaterial;
        Shader previousShader = previous != null ? previous.shader : null;

        label.font = targetFont;

        KeepShaderVariant(label, previousShader);
    }

    /// <summary>
    /// World-space hints render through the Overlay shader so meshes cannot occlude them; the font
    /// asset's default material would silently drop that back to the plain Distance Field shader.
    /// </summary>
    static void KeepShaderVariant(TMP_Text label, Shader previousShader)
    {
        if (previousShader == null)
            return;

        Material applied = label.fontSharedMaterial;
        if (applied == null || applied.shader == previousShader)
            return;

        Material instance = label.fontMaterial;
        if (instance == null)
            return;

        instance.shader = previousShader;
        label.fontMaterial = instance;
    }

    static void RestoreOriginalFontAndMaterial(TMP_Text label, OriginalStyle original)
    {
        if (label.font != original.Font)
            label.font = original.Font;

        RestoreMaterial(label, original);
    }

    static void RestoreMaterial(TMP_Text label, OriginalStyle original)
    {
        Material target = ResolveUsableMaterial(label.font, original.SharedMaterial);
        if (target == null)
            return;

        if (label.fontSharedMaterial != target)
            label.fontSharedMaterial = target;
    }
}
