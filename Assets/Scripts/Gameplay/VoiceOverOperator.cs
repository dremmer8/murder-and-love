using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PuppetFace;
using UnityEngine;

/// <summary>
/// Plays dialogue voice-over from <see cref="VoiceLineLibrary"/> and drives Mandy/Lau
/// <see cref="LipSync"/> (audio + Rhubarb phonemes). MC (<c>You</c>) gets audio only.
/// <para>
/// Lines are tagged in Ink as <c># vo:p_N_l_M</c>. Runtime reads that tag from
/// <c>story.currentTags</c> after <c>Continue()</c> — no ink-text matching required.
/// </para>
/// Mandy/Lau use early LipSync while <c>game_progression &lt; modelSwitchProgression</c>
/// (default 22), late afterward (prefers the hierarchy-active instance).
/// </summary>
public class VoiceOverOperator : MonoBehaviour
{
    public static VoiceOverOperator Instance { get; private set; }

    public const string VoiceTagPrefix = "vo:";

    static readonly Regex VoiceFileRegex = new(
        @"^vo:(p_(\d+)_l_(\d+))$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Header("Library")]
    [SerializeField] VoiceLineLibrary library;

    [Header("Mandy LipSync (lipsync + audio)")]
    [SerializeField] LipSync mandyLipSyncEarly;
    [SerializeField] LipSync mandyLipSyncLate;

    [Header("Lau LipSync (lipsync + audio)")]
    [SerializeField] LipSync lauLipSyncEarly;
    [SerializeField] LipSync lauLipSyncLate;

    [Header("MC audio only")]
    [Tooltip("Plays You/Vivian lines during normal dialogue. No LipSync.")]
    [SerializeField] AudioSource mcAudioSource;

    [Tooltip("Optional MC sources on the three ending cutscene cameras (player is inactive then).")]
    [SerializeField] AudioSource mcAudioSourceEnding1;
    [SerializeField] AudioSource mcAudioSourceEnding2;
    [SerializeField] AudioSource mcAudioSourceEnding3;

    [Header("Model switch")]
    [Tooltip("Early LipSync while game_progression is below this; late from this value up.")]
    [SerializeField] int modelSwitchProgression = 22;

    [Header("Debug")]
    [SerializeField] bool logMissingLines;

    LipSync _playingLipSync;
    AudioSource _playingMcSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: more than one VoiceOverOperator in scene.", this);
            return;
        }

        Instance = this;

        // LipSync components often keep leftover editor test clips with PlayOnAwake.
        // When PropProgression enables late models at progression 22, those would
        // auto-play into internal monologues (which correctly have no VO).
        SanitizePlaybackSource(mcAudioSource);
        SanitizePlaybackSource(mcAudioSourceEnding1);
        SanitizePlaybackSource(mcAudioSourceEnding2);
        SanitizePlaybackSource(mcAudioSourceEnding3);
        SanitizeLipSyncSource(mandyLipSyncEarly);
        SanitizeLipSyncSource(mandyLipSyncLate);
        SanitizeLipSyncSource(lauLipSyncEarly);
        SanitizeLipSyncSource(lauLipSyncLate);
    }

    static void SanitizeLipSyncSource(LipSync lipSync)
    {
        if (lipSync == null)
            return;

        lipSync.PlayOnAwake = false;
        SanitizePlaybackSource(lipSync.GetComponent<AudioSource>());
    }

    static void SanitizePlaybackSource(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.Stop();
        source.clip = null;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void BeginDialogue(string knotName, int storyPhase, TextAsset inkFile = null)
    {
        StopPlayback();
    }

    public void EndDialogue()
    {
        // Ending cutscenes keep the last line's VO playing so credits can wait for it.
        if (GameManager.Instance != null && GameManager.Instance.IsEndingCutscenePlaying)
            return;

        StopPlayback();
    }

    /// <summary>
    /// Hold time for ending-cutscene auto-advance: active VO length, else library clip length,
    /// else <paramref name="thoughtDuration"/> for Thoughts lines or
    /// <paramref name="unvoicedFallback"/> for other unvoiced lines.
    /// </summary>
    public float ResolveLineHoldDuration(
        string rawLine,
        IReadOnlyList<string> inkTags,
        float thoughtDuration,
        float unvoicedFallback)
    {
        float remaining = GetRemainingPlaybackSeconds();
        if (remaining > 0f)
            return remaining;

        if (TryResolveClipDuration(rawLine, inkTags, out float clipDuration))
            return clipDuration;

        if (TryParseSpeakerLine(rawLine, out string speaker, out _)
            && string.Equals(speaker, "Thoughts", StringComparison.OrdinalIgnoreCase))
            return Mathf.Max(0f, thoughtDuration);

        return Mathf.Max(0f, unvoicedFallback);
    }

    bool TryResolveClipDuration(string rawLine, IReadOnlyList<string> inkTags, out float duration)
    {
        duration = 0f;
        if (library == null)
            return false;

        if (!TryGetVoiceFileId(inkTags, out _, out int phase, out int line))
            return false;

        if (!TryParseSpeakerLine(rawLine, out string speaker, out _))
            return false;

        if (!TryMapSpeaker(speaker, out VoiceLineLibrary.VoiceCharacter character))
            return false;

        if (!library.TryGet(character, phase, line, out VoiceLineLibrary.Entry entry)
            || entry == null
            || entry.Clip == null)
            return false;

        duration = entry.Clip.length;
        return duration > 0f;
    }

    /// <summary>
    /// Seconds left on the active MC or LipSync VO clip (0 if nothing is playing).
    /// </summary>
    public float GetRemainingPlaybackSeconds()
    {
        if (_playingMcSource != null && _playingMcSource.isPlaying && _playingMcSource.clip != null)
            return Mathf.Max(0f, _playingMcSource.clip.length - _playingMcSource.time);

        if (_playingLipSync != null)
        {
            AudioSource lipSource = _playingLipSync.GetComponent<AudioSource>();
            if (lipSource != null && lipSource.isPlaying && lipSource.clip != null)
                return Mathf.Max(0f, lipSource.clip.length - lipSource.time);
        }

        return 0f;
    }

    /// <summary>
    /// Play VO for a spoken line using Ink tags (<c># vo:p_N_l_M</c>) collected after Continue.
    /// </summary>
    public void PlayForLine(string rawLine, IReadOnlyList<string> inkTags)
    {
        StopPlayback();

        if (library == null)
            return;

        if (!TryGetVoiceFileId(inkTags, out string fileId, out int phase, out int line))
        {
            if (logMissingLines && !string.IsNullOrWhiteSpace(rawLine))
            {
                Debug.Log(
                    $"[VoiceOver] No # vo:p_N_l_M tag on line: '{Truncate(rawLine)}'",
                    this);
            }

            return;
        }

        if (!TryParseSpeakerLine(rawLine, out string speaker, out _))
        {
            if (logMissingLines)
                Debug.Log($"[VoiceOver] Tag {fileId} but could not parse speaker on '{Truncate(rawLine)}'", this);
            return;
        }

        if (!TryMapSpeaker(speaker, out VoiceLineLibrary.VoiceCharacter character))
        {
            // Thoughts / Jason / etc. — tagged for future use, but not voiced yet.
            return;
        }

        if (!library.TryGet(character, phase, line, out VoiceLineLibrary.Entry entry))
        {
            if (logMissingLines)
            {
                Debug.Log(
                    $"[VoiceOver] Missing clip {character}/{fileId}.wav (tag present)",
                    this);
            }

            return;
        }

        switch (character)
        {
            case VoiceLineLibrary.VoiceCharacter.MC:
                PlayMc(entry.Clip);
                break;
            case VoiceLineLibrary.VoiceCharacter.Mandy:
                PlayLipSync(ResolveLipSync(mandyLipSyncEarly, mandyLipSyncLate), entry);
                break;
            case VoiceLineLibrary.VoiceCharacter.Lau:
                PlayLipSync(ResolveLipSync(lauLipSyncEarly, lauLipSyncLate), entry);
                break;
        }
    }

    /// <summary>Legacy overload — looks for an inline <c># vo:</c> in the text (usually stripped by Ink).</summary>
    public void PlayForLine(string rawLine)
    {
        PlayForLine(rawLine, ExtractInlineVoiceTags(rawLine));
    }

    public void StopPlayback()
    {
        if (_playingLipSync != null)
        {
            if (_playingLipSync.isActiveAndEnabled)
                _playingLipSync.Stop();
            _playingLipSync = null;
        }

        if (_playingMcSource != null)
        {
            _playingMcSource.Stop();
            _playingMcSource.clip = null;
            _playingMcSource = null;
        }
    }

    public static bool TryGetVoiceFileId(
        IReadOnlyList<string> tags,
        out string fileId,
        out int phase,
        out int line)
    {
        fileId = null;
        phase = 0;
        line = 0;
        if (tags == null)
            return false;

        for (int i = 0; i < tags.Count; i++)
        {
            if (TryParseVoiceTag(tags[i], out fileId, out phase, out line))
                return true;
        }

        return false;
    }

    public static bool TryParseVoiceTag(string tag, out string fileId, out int phase, out int line)
    {
        fileId = null;
        phase = 0;
        line = 0;
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        Match match = VoiceFileRegex.Match(tag.Trim());
        if (!match.Success)
            return false;

        fileId = match.Groups[1].Value;
        phase = int.Parse(match.Groups[2].Value);
        line = int.Parse(match.Groups[3].Value);
        return phase > 0 && line > 0;
    }

    static List<string> ExtractInlineVoiceTags(string rawLine)
    {
        var tags = new List<string>();
        if (string.IsNullOrEmpty(rawLine))
            return tags;

        foreach (Match match in Regex.Matches(rawLine, @"#\s*(vo:p_\d+_l_\d+)\b", RegexOptions.IgnoreCase))
            tags.Add(match.Groups[1].Value);

        return tags;
    }

    void PlayMc(AudioClip clip)
    {
        if (clip == null)
            return;

        AudioSource source = ResolveMcAudioSource();
        if (source == null)
            return;

        source.clip = clip;
        source.Play();
        _playingMcSource = source;
    }

    /// <summary>
    /// Prefers the first hierarchy-active MC source. Ending-camera sources cover
    /// cutscenes where the player object (and its main AudioSource) is disabled.
    /// </summary>
    AudioSource ResolveMcAudioSource()
    {
        if (IsUsable(mcAudioSource))
            return mcAudioSource;
        if (IsUsable(mcAudioSourceEnding1))
            return mcAudioSourceEnding1;
        if (IsUsable(mcAudioSourceEnding2))
            return mcAudioSourceEnding2;
        if (IsUsable(mcAudioSourceEnding3))
            return mcAudioSourceEnding3;

        // Last resort: assigned but inactive (still better than silence if Unity allows it).
        if (mcAudioSource != null)
            return mcAudioSource;
        if (mcAudioSourceEnding1 != null)
            return mcAudioSourceEnding1;
        if (mcAudioSourceEnding2 != null)
            return mcAudioSourceEnding2;
        return mcAudioSourceEnding3;
    }

    static bool IsUsable(AudioSource source)
    {
        return source != null && source.isActiveAndEnabled && source.gameObject.activeInHierarchy;
    }

    void PlayLipSync(LipSync lipSync, VoiceLineLibrary.Entry entry)
    {
        if (lipSync == null || entry == null || entry.Clip == null)
            return;

        lipSync.AudioClips = new[] { entry.Clip };
        lipSync.LipSyncFiles = entry.PhonemeXml != null
            ? new[] { entry.PhonemeXml }
            : Array.Empty<TextAsset>();
        lipSync.LipSyncIndex = 0;
        lipSync.PlayAll = false;
        lipSync.Repeat = false;
        lipSync.PlayOnAwake = false;
        lipSync.InitializeFromFile();
        lipSync.Play(0);
        _playingLipSync = lipSync;
    }

    LipSync ResolveLipSync(LipSync early, LipSync late)
    {
        int progression = GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;

        LipSync preferred = progression < modelSwitchProgression ? early : late;
        LipSync fallback = progression < modelSwitchProgression ? late : early;

        if (IsUsable(preferred))
            return preferred;
        if (IsUsable(fallback))
            return fallback;
        if (IsUsable(early))
            return early;
        if (IsUsable(late))
            return late;

        return preferred != null ? preferred : fallback;
    }

    static bool IsUsable(LipSync lipSync)
    {
        return lipSync != null && lipSync.isActiveAndEnabled && lipSync.gameObject.activeInHierarchy;
    }

    public static bool TryParseSpeakerLine(string raw, out string speaker, out string body)
    {
        speaker = null;
        body = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        string line = raw.Trim();
        // Strip ink tags that might still be present in editor previews.
        line = Regex.Replace(line, @"\s*#\s*\S+", "").Trim();

        int colon = line.IndexOf(':');
        if (colon <= 0 || colon > 40)
            return false;

        speaker = line.Substring(0, colon).Trim();
        if (speaker.Length == 0 || !HasLetter(speaker))
            return false;

        body = line.Substring(colon + 1).Trim();
        body = Regex.Replace(body, @"\s*->\s*\S+\s*$", "").Trim();
        return true;
    }

    public static bool TryMapSpeaker(string speaker, out VoiceLineLibrary.VoiceCharacter character)
    {
        character = default;
        if (string.IsNullOrWhiteSpace(speaker))
            return false;

        switch (speaker.Trim().ToLowerInvariant())
        {
            case "you":
            case "vivian":
            case "vi":
                character = VoiceLineLibrary.VoiceCharacter.MC;
                return true;
            case "mrs wong":
            case "mrs. wong":
            case "mandy":
                character = VoiceLineLibrary.VoiceCharacter.Mandy;
                return true;
            case "drunk man":
            case "drunk cop":
            case "lau":
            case "cop":
            case "police officer":
                character = VoiceLineLibrary.VoiceCharacter.Lau;
                return true;
            default:
                return false;
        }
    }

    static bool HasLetter(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsLetter(value[i]))
                return true;
        }

        return false;
    }

    static string Truncate(string value, int max = 64)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
            return value;
        return value.Substring(0, max) + "…";
    }
}
