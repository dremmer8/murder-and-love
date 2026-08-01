using System;
using TMPro;
using UnityEngine;

/// <summary>
/// One playable language: UI string CSV + compiled Ink story JSON + optional TMP font / VO library.
/// </summary>
[Serializable]
public class LocalizationLocale
{
    [Tooltip("Stable id stored in PlayerPrefs, e.g. en, pl.")]
    public string id = "en";

    [Tooltip("Shown in the Options language dropdown.")]
    public string displayName = "English";

    [Tooltip("CSV TextAsset with key,text columns for non-Ink UI strings.")]
    public TextAsset uiCsv;

    [Tooltip("Compiled Ink JSON TextAsset (same knot names as English).")]
    public TextAsset inkStory;

    [Tooltip("TMP font for this language. Applied to every TMP_Text (menus, HUD, dialogue, monologue). Leave empty to keep each text's original scene font.")]
    public TMP_FontAsset font;

    [Tooltip("Added to each TMP label's original fontSize (e.g. -2 shrinks, +1.5 grows). 0 = no change.")]
    public float fontSizeOffset;

    [Tooltip("Voice-over clip library for this language. Applied to VoiceOverOperator at runtime. Leave empty to keep the scene-assigned library.")]
    public VoiceLineLibrary voiceLineLibrary;
}
