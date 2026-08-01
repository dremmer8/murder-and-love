using TMPro;
using UnityEngine;

/// <summary>
/// Binds a TMP label to a localization key. Fallback is the text present when the component wakes
/// (usually the English scene value).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public class LocalizedTmpText : MonoBehaviour
{
    [SerializeField] string key;
    [SerializeField] TMP_Text label;

    string _fallback;

    public string Key => key;

    public string Fallback => _fallback;

    void Awake()
    {
        if (label == null)
            label = GetComponent<TMP_Text>();

        if (label != null && string.IsNullOrEmpty(_fallback))
            _fallback = label.text ?? "";
    }

    void OnEnable()
    {
        LocalizationService.LanguageChanged += Apply;
        Apply();
    }

    void OnDisable()
    {
        LocalizationService.LanguageChanged -= Apply;
    }

    public void SetKey(string nextKey)
    {
        key = nextKey ?? "";
        Apply();
    }

    public void Apply()
    {
        if (label == null)
            label = GetComponent<TMP_Text>();

        if (label == null || string.IsNullOrEmpty(key))
            return;

        if (string.IsNullOrEmpty(_fallback))
            _fallback = label.text ?? "";

        label.text = LocalizationService.Get(key, _fallback);
    }

#if UNITY_EDITOR
    public void EditorConfigure(string nextKey, string fallbackText)
    {
        key = nextKey ?? "";
        _fallback = fallbackText ?? "";
        if (label == null)
            label = GetComponent<TMP_Text>();
    }
#endif
}
