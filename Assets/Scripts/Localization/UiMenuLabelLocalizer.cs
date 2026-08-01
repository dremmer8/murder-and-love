using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Applies localization keys to known menu TMP labels by GameObject name
/// (Play, Resume, Options, Sound, Music, Back, Exit, Exit to menu).
/// Attach once under a Canvas / menu root; also used by export to discover menu strings.
/// </summary>
public class UiMenuLabelLocalizer : MonoBehaviour
{
    static readonly Dictionary<string, string> NameToKey = new()
    {
        { "Play", LocalizationKeys.MenuPlay },
        { "Resume", LocalizationKeys.MenuResume },
        { "Options", LocalizationKeys.MenuOptions },
        { "Sound", LocalizationKeys.MenuSound },
        { "Sounds", LocalizationKeys.MenuSound },
        { "Music", LocalizationKeys.MenuMusic },
        { "Language", LocalizationKeys.MenuLanguage },
        { "Back", LocalizationKeys.MenuBack },
        { "Exit", LocalizationKeys.MenuExit },
        { "Exit to menu", LocalizationKeys.MenuExitToMenu },
    };

    [Tooltip("If empty, searches this transform hierarchy.")]
    [SerializeField] Transform root;

    readonly List<(TMP_Text label, string key, string fallback)> _bindings = new();

    void Awake()
    {
        CollectBindings();
    }

    void OnEnable()
    {
        LocalizationService.LanguageChanged += ApplyAll;
        ApplyAll();
    }

    void OnDisable()
    {
        LocalizationService.LanguageChanged -= ApplyAll;
    }

    void CollectBindings()
    {
        _bindings.Clear();
        Transform searchRoot = root != null ? root : transform;
        TMP_Text[] labels = searchRoot.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            TMP_Text label = labels[i];
            if (label == null)
                continue;

            // Prefer explicit LocalizedTmpText when present.
            if (label.GetComponent<LocalizedTmpText>() != null)
                continue;

            string goName = label.gameObject.name;
            if (!NameToKey.TryGetValue(goName, out string key))
            {
                // Common pattern: Text child under a button named Play / Resume / …
                Transform parent = label.transform.parent;
                if (parent == null || !NameToKey.TryGetValue(parent.name, out key))
                {
                    // Fallback: match known English caption text (Sound/Music TMP children).
                    if (!TryKeyFromCaption(label.text, out key))
                        continue;
                }
            }

            // Skip slider value readout TMP under Sounds/Music roots (those are not the labels).
            if (IsLikelySliderChrome(label))
                continue;

            _bindings.Add((label, key, label.text ?? ""));
        }
    }

    static bool TryKeyFromCaption(string caption, out string key)
    {
        key = null;
        if (string.IsNullOrWhiteSpace(caption))
            return false;

        string t = caption.Trim();
        if (t.Equals("Play", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuPlay;
            return true;
        }

        if (t.Equals("Resume", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuResume;
            return true;
        }

        if (t.Equals("Options", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuOptions;
            return true;
        }

        if (t.Equals("Sound", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuSound;
            return true;
        }

        if (t.Equals("Music", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuMusic;
            return true;
        }

        if (t.Equals("Language", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuLanguage;
            return true;
        }

        if (t.Equals("Back", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuBack;
            return true;
        }

        if (t.Equals("Exit", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuExit;
            return true;
        }

        if (t.Equals("Exit to menu", System.StringComparison.OrdinalIgnoreCase))
        {
            key = LocalizationKeys.MenuExitToMenu;
            return true;
        }

        return false;
    }

    static bool IsLikelySliderChrome(TMP_Text label)
    {
        // Sound/Music slider handle labels are tiny or numeric; menu captions are the word itself.
        string t = label.text != null ? label.text.Trim() : "";
        if (t.Length == 0)
            return true;

        // If the TMP lives under a Slider and isn't the caption text matching the key word, skip.
        SliderLikeAncestor(label.transform, out string ancestorName);
        if (ancestorName == "Sounds" || ancestorName == "Music")
        {
            string expected = ancestorName == "Sounds" ? "Sound" : "Music";
            return !string.Equals(t, expected, System.StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    static void SliderLikeAncestor(Transform t, out string name)
    {
        name = null;
        Transform cur = t;
        while (cur != null)
        {
            if (cur.name == "Sounds" || cur.name == "Music")
            {
                name = cur.name;
                return;
            }

            cur = cur.parent;
        }
    }

    void ApplyAll()
    {
        for (int i = 0; i < _bindings.Count; i++)
        {
            (TMP_Text label, string key, string fallback) = _bindings[i];
            if (label != null)
                label.text = LocalizationService.Get(key, fallback);
        }
    }

#if UNITY_EDITOR
    public IEnumerable<(string key, string text)> EditorHarvest()
    {
        CollectBindings();
        var seen = new HashSet<string>();
        for (int i = 0; i < _bindings.Count; i++)
        {
            (TMP_Text label, string key, string fallback) = _bindings[i];
            if (label == null || !seen.Add(key))
                continue;

            yield return (key, string.IsNullOrEmpty(fallback) ? label.text : fallback);
        }
    }
#endif
}
