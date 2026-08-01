using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catalogue of available languages (UI CSV + Ink). Load from Resources as
/// <c>LocalizationCatalog</c> so gameplay can resolve strings before scene UI wakes.
/// </summary>
[CreateAssetMenu(
    fileName = "LocalizationCatalog",
    menuName = "Localization/Catalog",
    order = 0)]
public class LocalizationCatalog : ScriptableObject
{
    public const string ResourcesPath = "LocalizationCatalog";

    [SerializeField] List<LocalizationLocale> locales = new();

    [Tooltip("Used when PlayerPrefs has no language or the saved id is missing.")]
    [SerializeField] string defaultLocaleId = "en";

    public IReadOnlyList<LocalizationLocale> Locales => locales;
    public string DefaultLocaleId => defaultLocaleId;

    public LocalizationLocale FindById(string id)
    {
        if (string.IsNullOrEmpty(id) || locales == null)
            return null;

        for (int i = 0; i < locales.Count; i++)
        {
            LocalizationLocale locale = locales[i];
            if (locale != null && locale.id == id)
                return locale;
        }

        return null;
    }

    public LocalizationLocale FindDefault()
    {
        LocalizationLocale match = FindById(defaultLocaleId);
        if (match != null)
            return match;

        if (locales != null)
        {
            for (int i = 0; i < locales.Count; i++)
            {
                if (locales[i] != null && !string.IsNullOrEmpty(locales[i].id))
                    return locales[i];
            }
        }

        return null;
    }

#if UNITY_EDITOR
    public void EditorSetLocales(List<LocalizationLocale> next, string defaultId)
    {
        locales = next ?? new List<LocalizationLocale>();
        if (!string.IsNullOrEmpty(defaultId))
            defaultLocaleId = defaultId;
    }

    public void EditorUpsertLocale(LocalizationLocale locale)
    {
        if (locale == null || string.IsNullOrEmpty(locale.id))
            return;

        if (locales == null)
            locales = new List<LocalizationLocale>();

        for (int i = 0; i < locales.Count; i++)
        {
            if (locales[i] != null && locales[i].id == locale.id)
            {
                locales[i] = locale;
                return;
            }
        }

        locales.Add(locale);
    }
#endif
}
