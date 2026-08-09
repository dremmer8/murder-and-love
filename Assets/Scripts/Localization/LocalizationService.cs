using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Runtime language table + active Ink asset. Loads <see cref="LocalizationCatalog"/> from Resources.
/// </summary>
public static class LocalizationService
{
    public const string PrefLanguageId = "Localization.LanguageId";

    static LocalizationCatalog s_Catalog;
    static LocalizationLocale s_ActiveLocale;
    static Dictionary<string, string> s_Strings = new(StringComparer.Ordinal);
    static bool s_Initialized;

    public static event Action LanguageChanged;

    public static string CurrentLanguageId =>
        s_ActiveLocale != null ? s_ActiveLocale.id : "";

    public static string CurrentDisplayName =>
        s_ActiveLocale != null ? s_ActiveLocale.displayName : "";

    public static TextAsset CurrentInkAsset =>
        s_ActiveLocale != null ? s_ActiveLocale.inkStory : null;

    /// <summary>
    /// Active locale TMP font, or null to keep each label's original scene font.
    /// </summary>
    public static TMP_FontAsset CurrentFont =>
        s_ActiveLocale != null ? s_ActiveLocale.font : null;

    /// <summary>
    /// Added to each TMP label's original fontSize for the active locale (0 = unchanged).
    /// </summary>
    public static float CurrentFontSizeOffset =>
        s_ActiveLocale != null ? s_ActiveLocale.fontSizeOffset : 0f;

    /// <summary>
    /// Active locale voice-over library, or null to keep VoiceOverOperator's scene-assigned library.
    /// </summary>
    public static VoiceLineLibrary CurrentVoiceLineLibrary =>
        s_ActiveLocale != null ? s_ActiveLocale.voiceLineLibrary : null;

    /// <summary>
    /// True for fonts the catalogue marks as stylistic, which keep their authored font and size.
    /// </summary>
    public static bool IsFontPreserved(TMP_FontAsset font)
    {
        EnsureInitialized();
        return s_Catalog != null && s_Catalog.IsFontPreserved(font);
    }

    public static LocalizationCatalog Catalog
    {
        get
        {
            EnsureInitialized();
            return s_Catalog;
        }
    }

    public static IReadOnlyList<LocalizationLocale> Locales
    {
        get
        {
            EnsureInitialized();
            return s_Catalog != null
                ? s_Catalog.Locales
                : Array.Empty<LocalizationLocale>();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        EnsureInitialized();
    }

    public static void EnsureInitialized()
    {
        if (s_Initialized && s_Catalog != null)
            return;

        s_Catalog = Resources.Load<LocalizationCatalog>(LocalizationCatalog.ResourcesPath);
        s_Initialized = true;

        if (s_Catalog == null)
        {
            Debug.LogWarning(
                "[LocalizationService] No Resources/LocalizationCatalog.asset — UI falls back to Inspector English; Ink uses StoryCharacterPhasesSO.");
            s_ActiveLocale = null;
            s_Strings = new Dictionary<string, string>(StringComparer.Ordinal);
            return;
        }

        string saved = PlayerPrefs.GetString(PrefLanguageId, s_Catalog.DefaultLocaleId);
        ApplyLocale(ResolveLocale(saved), savePrefs: false, notify: false);
    }

    public static string Get(string key, string fallback = "")
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(key))
            return fallback ?? "";

        if (s_Strings != null && s_Strings.TryGetValue(key, out string value) && value != null)
            return value;

        return fallback ?? "";
    }

    public static bool HasKey(string key)
    {
        EnsureInitialized();
        return !string.IsNullOrEmpty(key) && s_Strings != null && s_Strings.ContainsKey(key);
    }

    public static void SetLanguage(string localeId)
    {
        EnsureInitialized();
        LocalizationLocale locale = ResolveLocale(localeId);
        if (locale == null)
        {
            Debug.LogWarning($"[LocalizationService] Unknown locale id '{localeId}'.");
            return;
        }

        if (s_ActiveLocale != null && s_ActiveLocale.id == locale.id)
            return;

        ApplyLocale(locale, savePrefs: true, notify: true);
    }

    public static void Reload()
    {
        s_Initialized = false;
        s_Catalog = null;
        EnsureInitialized();
        LanguageChanged?.Invoke();
    }

    static LocalizationLocale ResolveLocale(string id)
    {
        if (s_Catalog == null)
            return null;

        LocalizationLocale match = s_Catalog.FindById(id);
        return match ?? s_Catalog.FindDefault();
    }

    static void ApplyLocale(LocalizationLocale locale, bool savePrefs, bool notify)
    {
        s_ActiveLocale = locale;
        s_Strings = new Dictionary<string, string>(StringComparer.Ordinal);

        if (locale != null && locale.uiCsv != null && !string.IsNullOrEmpty(locale.uiCsv.text))
            s_Strings = LocalizationCsv.Parse(locale.uiCsv.text);

        if (savePrefs && locale != null && !string.IsNullOrEmpty(locale.id))
        {
            PlayerPrefs.SetString(PrefLanguageId, locale.id);
            PlayerPrefs.Save();
        }

        if (notify)
            LanguageChanged?.Invoke();
    }
}
