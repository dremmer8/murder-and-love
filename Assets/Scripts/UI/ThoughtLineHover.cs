using TMPro;
using UnityEngine;

/// <summary>
/// Soft hover bob for thought-line UI bubbles. Enable only while a
/// <c>Thoughts:</c> line is showing so the text reads as floating in air.
/// </summary>
[DisallowMultipleComponent]
public class ThoughtLineHover : MonoBehaviour
{
    const string ThoughtsSpeaker = "Thoughts";

    [Header("Motion")]
    [SerializeField]
    [Tooltip("Vertical bob amplitude in UI units (anchored Y).")]
    float m_Amplitude = 8f;

    [SerializeField]
    [Tooltip("Bob cycles per second.")]
    float m_Frequency = 0.85f;

    [SerializeField]
    [Tooltip("Optional horizontal drift amplitude.")]
    float m_HorizontalAmplitude = 2.5f;

    [SerializeField]
    [Tooltip("Optional Z sway in degrees.")]
    float m_SwayDegrees = 1.25f;

    RectTransform m_Rect;
    Vector2 m_RestAnchoredPosition;
    Quaternion m_RestLocalRotation;
    bool m_RestCaptured;
    bool m_Floating;
    float m_Phase;

    public bool IsFloating => m_Floating;

    void Awake()
    {
        m_Rect = transform as RectTransform;
        CaptureRestPose();
    }

    void OnDisable()
    {
        StopFloating(restorePose: true);
    }

    void LateUpdate()
    {
        if (!m_Floating || m_Rect == null)
            return;

        m_Phase += Time.unscaledDeltaTime * m_Frequency * Mathf.PI * 2f;
        float bob = Mathf.Sin(m_Phase);
        float sway = Mathf.Sin(m_Phase * 0.7f + 1.1f);

        m_Rect.anchoredPosition = m_RestAnchoredPosition + new Vector2(
            sway * m_HorizontalAmplitude,
            bob * m_Amplitude);
        m_Rect.localRotation = m_RestLocalRotation * Quaternion.Euler(0f, 0f, sway * m_SwayDegrees);
    }

    public void SetFloating(bool enabled)
    {
        if (enabled)
            StartFloating();
        else
            StopFloating(restorePose: true);
    }

    void StartFloating()
    {
        if (m_Rect == null)
            m_Rect = transform as RectTransform;

        if (m_Rect == null)
            return;

        if (!m_Floating)
        {
            CaptureRestPose();
            m_Phase = Random.Range(0f, Mathf.PI * 2f);
        }

        m_Floating = true;
        enabled = true;
    }

    void StopFloating(bool restorePose)
    {
        m_Floating = false;

        if (restorePose && m_Rect != null && m_RestCaptured)
        {
            m_Rect.anchoredPosition = m_RestAnchoredPosition;
            m_Rect.localRotation = m_RestLocalRotation;
        }
    }

    void CaptureRestPose()
    {
        if (m_Rect == null)
            m_Rect = transform as RectTransform;

        if (m_Rect == null)
            return;

        m_RestAnchoredPosition = m_Rect.anchoredPosition;
        m_RestLocalRotation = m_Rect.localRotation;
        m_RestCaptured = true;
    }

    /// <summary>
    /// Turns hover on/off for the parent bubble of <paramref name="textField"/>
    /// based on whether <paramref name="rawLine"/> is a Thoughts speaker line.
    /// </summary>
    public static void ApplyForLine(TextMeshProUGUI textField, string rawLine)
    {
        ThoughtLineHover hover = ResolveForText(textField);
        if (hover == null)
            return;

        hover.SetFloating(IsThoughtsLine(rawLine));
    }

    public static void StopFor(TextMeshProUGUI textField)
    {
        ThoughtLineHover hover = ResolveForText(textField, createIfMissing: false);
        if (hover != null)
            hover.SetFloating(false);
    }

    public static bool IsThoughtsLine(string rawLine)
    {
        if (string.IsNullOrWhiteSpace(rawLine))
            return false;

        int colon = rawLine.IndexOf(':');
        if (colon <= 0)
            return false;

        string speaker = rawLine.Substring(0, colon).Trim();
        if (speaker.EndsWith(":"))
            speaker = speaker.Substring(0, speaker.Length - 1).TrimEnd();

        return string.Equals(speaker, ThoughtsSpeaker, System.StringComparison.OrdinalIgnoreCase);
    }

    static ThoughtLineHover ResolveForText(TextMeshProUGUI textField, bool createIfMissing = true)
    {
        if (textField == null)
            return null;

        Transform parent = textField.transform.parent;
        if (parent == null)
            return null;

        ThoughtLineHover hover = parent.GetComponent<ThoughtLineHover>();
        if (hover == null && createIfMissing)
            hover = parent.gameObject.AddComponent<ThoughtLineHover>();

        return hover;
    }
}
