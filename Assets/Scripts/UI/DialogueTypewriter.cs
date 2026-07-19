using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Channels that feed predetermined TMP fields on <see cref="DialogueTypewriter"/>.
/// Intro rows pass their own target via <see cref="DialogueTypewriter.PlayIntro"/>.
/// </summary>
public enum DialogueTextChannel
{
    Standard = 0,
    Internal = 1,
    Intro = 2
}

[Serializable]
public class SpeakerNameColorEntry
{
    [Tooltip("Speaker name as it appears before ':' in Ink (e.g. Mrs. Wong, You, Drunk Man). Colon optional.")]
    public string speakerName = "";

    public Color color = new Color(1f, 0.82f, 0.35f, 1f);
}

/// <summary>
/// Receives dialogue lines for standard, internal, and intro UI, colors speaker names,
/// and reveals text letter-by-letter into the assigned TMP fields.
/// </summary>
public class DialogueTypewriter : MonoBehaviour
{
    public static DialogueTypewriter Instance { get; private set; }

    [Header("Text Fields")]
    [SerializeField]
    [Tooltip("Standard / ordinal dialogue panel body text.")]
    TextMeshProUGUI m_StandardDialogueText;

    [SerializeField]
    [Tooltip("Internal monologue body text.")]
    TextMeshProUGUI m_InternalMonologueText;

    [SerializeField]
    [Tooltip("Optional default intro text field. Intro rows usually pass their own TextMeshProUGUI.")]
    TextMeshProUGUI m_IntroText;

    [Header("Speaker Name Colors")]
    [SerializeField]
    [Tooltip("Per-character colors. Match the Ink name before ':' (case-insensitive).")]
    List<SpeakerNameColorEntry> m_SpeakerColors = new();

    [SerializeField]
    [Tooltip("Used when the speaker is not in the list above.")]
    Color m_DefaultSpeakerColor = new Color(1f, 0.82f, 0.35f, 1f);

    [SerializeField]
    [Tooltip("Max characters allowed before ':' to count as a speaker name.")]
    int m_MaxSpeakerNameLength = 32;

    [Header("Typing")]
    [SerializeField]
    [Tooltip("Visible characters revealed per second.")]
    float m_CharactersPerSecond = 42f;

    [SerializeField]
    [Tooltip("If true, the first Space / Skip call finishes the current line immediately.")]
    bool m_AllowSkip = true;

    Coroutine m_TypingRoutine;
    TextMeshProUGUI m_ActiveTarget;
    Action m_OnComplete;
    bool m_SkipRequested;

    public bool IsTyping => m_TypingRoutine != null;

    public Color DefaultSpeakerColor
    {
        get => m_DefaultSpeakerColor;
        set => m_DefaultSpeakerColor = value;
    }

    public IReadOnlyList<SpeakerNameColorEntry> SpeakerColors => m_SpeakerColors;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: Duplicate DialogueTypewriter — keeping the first instance.", this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayStandard(string text, Action onComplete = null)
    {
        Play(DialogueTextChannel.Standard, text, null, onComplete);
    }

    public void PlayInternal(string text, Action onComplete = null)
    {
        Play(DialogueTextChannel.Internal, text, null, onComplete);
    }

    public void PlayIntro(string text, TextMeshProUGUI target = null, Action onComplete = null)
    {
        Play(DialogueTextChannel.Intro, text, target, onComplete);
    }

    public void Play(
        DialogueTextChannel channel,
        string text,
        TextMeshProUGUI overrideTarget = null,
        Action onComplete = null)
    {
        TextMeshProUGUI target = overrideTarget != null ? overrideTarget : ResolveTarget(channel);
        if (target == null)
        {
            Debug.LogWarning($"{name}: No TMP target for channel {channel}.", this);
            onComplete?.Invoke();
            return;
        }

        StopTyping(invokeComplete: false);
        m_OnComplete = onComplete;
        m_SkipRequested = false;
        m_ActiveTarget = target;
        m_TypingRoutine = StartCoroutine(TypeRoutine(target, text ?? string.Empty));
    }

    /// <summary>Finish the current line immediately (if skipping is allowed).</summary>
    public bool Skip()
    {
        if (!IsTyping)
            return false;

        if (!m_AllowSkip)
            return false;

        m_SkipRequested = true;
        return true;
    }

    /// <summary>Stop typing and optionally clear the active field.</summary>
    public void Stop(bool clearText = false)
    {
        TextMeshProUGUI target = m_ActiveTarget;
        StopTyping(invokeComplete: false);

        if (clearText && target != null)
            ClearTarget(target);
    }

    public void Clear(DialogueTextChannel channel)
    {
        TextMeshProUGUI target = ResolveTarget(channel);
        if (target != null)
            ClearTarget(target);
    }

    public void ClearAll()
    {
        StopTyping(invokeComplete: false);
        ClearTarget(m_StandardDialogueText);
        ClearTarget(m_InternalMonologueText);
        ClearTarget(m_IntroText);
    }

    /// <summary>
    /// Wraps a leading speaker name (text before the first ':') in a TMP color tag.
    /// Color comes from the speaker list, or the default speaker color.
    /// </summary>
    public string ApplySpeakerColor(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return string.Empty;

        int colon = raw.IndexOf(':');
        if (colon <= 0 || colon > m_MaxSpeakerNameLength)
            return raw;

        string namePart = raw.Substring(0, colon).Trim();
        if (!ContainsLetter(namePart))
            return raw;

        Color color = ResolveSpeakerColor(namePart);
        string hex = ColorUtility.ToHtmlStringRGB(color);
        string prefix = raw.Substring(0, colon + 1);
        string body = raw.Substring(colon + 1);
        return $"<color=#{hex}>{prefix}</color>{body}";
    }

    public Color ResolveSpeakerColor(string speakerName)
    {
        if (string.IsNullOrWhiteSpace(speakerName))
            return m_DefaultSpeakerColor;

        string key = NormalizeSpeakerName(speakerName);
        if (m_SpeakerColors != null)
        {
            for (int i = 0; i < m_SpeakerColors.Count; i++)
            {
                SpeakerNameColorEntry entry = m_SpeakerColors[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.speakerName))
                    continue;

                if (string.Equals(NormalizeSpeakerName(entry.speakerName), key, StringComparison.OrdinalIgnoreCase))
                    return entry.color;
            }
        }

        return m_DefaultSpeakerColor;
    }

    static string NormalizeSpeakerName(string name)
    {
        string trimmed = name.Trim();
        if (trimmed.EndsWith(":"))
            trimmed = trimmed.Substring(0, trimmed.Length - 1).TrimEnd();
        return trimmed;
    }

    TextMeshProUGUI ResolveTarget(DialogueTextChannel channel)
    {
        switch (channel)
        {
            case DialogueTextChannel.Standard:
                return m_StandardDialogueText;
            case DialogueTextChannel.Internal:
                return m_InternalMonologueText;
            case DialogueTextChannel.Intro:
                return m_IntroText;
            default:
                return null;
        }
    }

    IEnumerator TypeRoutine(TextMeshProUGUI target, string raw)
    {
        string rich = ApplySpeakerColor(raw.Trim());
        target.text = rich;
        target.maxVisibleCharacters = 0;
        target.ForceMeshUpdate();

        int total = target.textInfo != null ? target.textInfo.characterCount : 0;
        if (total <= 0 || m_CharactersPerSecond <= 0f)
        {
            target.maxVisibleCharacters = int.MaxValue;
            FinishTyping();
            yield break;
        }

        float visible = 0f;
        while (visible < total)
        {
            if (m_SkipRequested)
                break;

            visible += m_CharactersPerSecond * Time.deltaTime;
            target.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(visible));
            yield return null;
        }

        target.maxVisibleCharacters = int.MaxValue;
        FinishTyping();
    }

    void FinishTyping()
    {
        m_TypingRoutine = null;
        m_ActiveTarget = null;
        m_SkipRequested = false;

        Action callback = m_OnComplete;
        m_OnComplete = null;
        callback?.Invoke();
    }

    void StopTyping(bool invokeComplete)
    {
        if (m_TypingRoutine != null)
        {
            StopCoroutine(m_TypingRoutine);
            m_TypingRoutine = null;
        }

        if (m_ActiveTarget != null)
            m_ActiveTarget.maxVisibleCharacters = int.MaxValue;

        m_ActiveTarget = null;
        m_SkipRequested = false;

        Action callback = m_OnComplete;
        m_OnComplete = null;

        if (invokeComplete)
            callback?.Invoke();
    }

    static void ClearTarget(TextMeshProUGUI target)
    {
        if (target == null)
            return;

        target.text = string.Empty;
        target.maxVisibleCharacters = int.MaxValue;
    }

    static bool ContainsLetter(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsLetter(value[i]))
                return true;
        }

        return false;
    }
}
