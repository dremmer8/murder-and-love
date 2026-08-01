using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Options language dropdown. Builds a TMP dropdown under the Options UI root when none is assigned.
/// </summary>
[DefaultExecutionOrder(-40)]
public class LanguageOptionsController : MonoBehaviour
{
    [SerializeField] TMP_Dropdown m_LanguageDropdown;
    [SerializeField] Transform m_DropdownParent;
    [SerializeField] string m_LanguageLabelKey = LocalizationKeys.MenuLanguage;
    [SerializeField] string m_LanguageLabelFallback = "Language";

    readonly List<string> m_LocaleIds = new();
    TMP_Text m_Label;
    bool m_BuiltRuntimeUi;

    void Awake()
    {
        LocalizationService.EnsureInitialized();
        EnsureDropdown();
        RefreshOptions();
    }

    void OnEnable()
    {
        LocalizationService.LanguageChanged += OnLanguageChanged;

        if (m_LanguageDropdown != null)
        {
            m_LanguageDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
            m_LanguageDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        SyncDropdownToCurrent();
        ApplyLabel();
    }

    void OnDisable()
    {
        LocalizationService.LanguageChanged -= OnLanguageChanged;

        if (m_LanguageDropdown != null)
            m_LanguageDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    void OnLanguageChanged()
    {
        RefreshOptions();
        SyncDropdownToCurrent();
        ApplyLabel();
    }

    void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= m_LocaleIds.Count)
            return;

        LocalizationService.SetLanguage(m_LocaleIds[index]);
    }

    void RefreshOptions()
    {
        if (m_LanguageDropdown == null)
            return;

        m_LocaleIds.Clear();
        var options = new List<TMP_Dropdown.OptionData>();
        IReadOnlyList<LocalizationLocale> locales = LocalizationService.Locales;
        for (int i = 0; i < locales.Count; i++)
        {
            LocalizationLocale locale = locales[i];
            if (locale == null || string.IsNullOrEmpty(locale.id))
                continue;

            m_LocaleIds.Add(locale.id);
            string name = string.IsNullOrEmpty(locale.displayName) ? locale.id : locale.displayName;
            options.Add(new TMP_Dropdown.OptionData(name));
        }

        m_LanguageDropdown.ClearOptions();
        m_LanguageDropdown.AddOptions(options);
        m_LanguageDropdown.interactable = m_LocaleIds.Count > 0;
    }

    void SyncDropdownToCurrent()
    {
        if (m_LanguageDropdown == null || m_LocaleIds.Count == 0)
            return;

        string current = LocalizationService.CurrentLanguageId;
        int index = 0;
        for (int i = 0; i < m_LocaleIds.Count; i++)
        {
            if (m_LocaleIds[i] == current)
            {
                index = i;
                break;
            }
        }

        m_LanguageDropdown.SetValueWithoutNotify(index);
        m_LanguageDropdown.RefreshShownValue();
    }

    void ApplyLabel()
    {
        if (m_Label == null)
            return;

        m_Label.text = LocalizationService.Get(m_LanguageLabelKey, m_LanguageLabelFallback);
    }

    void EnsureDropdown()
    {
        if (m_LanguageDropdown != null)
            return;

        Transform parent = m_DropdownParent != null ? m_DropdownParent : transform;
        var existing = parent.GetComponentInChildren<TMP_Dropdown>(true);
        if (existing != null)
        {
            m_LanguageDropdown = existing;
            return;
        }

        BuildRuntimeDropdown(parent);
    }

    void BuildRuntimeDropdown(Transform parent)
    {
        // Row root — sits under Options UI next to Sound / Music sliders.
        var row = new GameObject("Language", typeof(RectTransform));
        row.layer = parent.gameObject.layer;
        var rowRt = row.GetComponent<RectTransform>();
        rowRt.SetParent(parent, false);
        rowRt.anchorMin = new Vector2(0f, 1f);
        rowRt.anchorMax = new Vector2(0f, 1f);
        rowRt.pivot = new Vector2(0.5f, 0.5f);
        rowRt.anchoredPosition = new Vector2(50f, -77f);
        rowRt.sizeDelta = new Vector2(160f, 28f);

        // Label
        var labelGo = new GameObject("Label", typeof(RectTransform));
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.SetParent(rowRt, false);
        labelRt.anchorMin = new Vector2(0f, 0.5f);
        labelRt.anchorMax = new Vector2(0f, 0.5f);
        labelRt.pivot = new Vector2(0f, 0.5f);
        labelRt.anchoredPosition = new Vector2(-90f, 0f);
        labelRt.sizeDelta = new Vector2(90f, 24f);
        m_Label = labelGo.AddComponent<TextMeshProUGUI>();
        m_Label.fontSize = 16f;
        m_Label.alignment = TextAlignmentOptions.MidlineRight;
        m_Label.color = Color.white;
        ApplyLabel();

        // TMP default dropdown (includes template hierarchy).
        var resources = new TMP_DefaultControls.Resources();
        GameObject dropdownGo = TMP_DefaultControls.CreateDropdown(resources);
        dropdownGo.name = "LanguageDropdown";
        var dropdownRt = dropdownGo.GetComponent<RectTransform>();
        dropdownRt.SetParent(rowRt, false);
        dropdownRt.anchorMin = Vector2.zero;
        dropdownRt.anchorMax = Vector2.one;
        dropdownRt.offsetMin = Vector2.zero;
        dropdownRt.offsetMax = Vector2.zero;

        m_LanguageDropdown = dropdownGo.GetComponent<TMP_Dropdown>();
        m_BuiltRuntimeUi = true;

        // Keep dropdown readable on dark pause panels.
        Image[] images = dropdownGo.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
                images[i].color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        }

        // Runtime-spawned TMP (label + dropdown) missed the initial font pass.
        LocalizedFontApplier.ApplyNow();
    }
}
