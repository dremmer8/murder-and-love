using TMPro;
using UnityEngine;

/// <summary>
/// Sets a target <see cref="RectTransform"/> width from a TMP label's preferred text size.
/// Intended for choice buttons: assign the TMP child and the Choice root so the button
/// grows/shrinks to fit its label.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class TmpTextWidthFitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("TMP label to measure. If empty, uses the first TextMeshProUGUI in children.")]
    TextMeshProUGUI m_Text;

    [SerializeField]
    [Tooltip("RectTransform whose width is adjusted. Defaults to this object's RectTransform.")]
    RectTransform m_Target;

    [Header("Sizing")]
    [SerializeField]
    [Tooltip("Extra width added around the preferred text width (left + right combined).")]
    float m_HorizontalPadding;

    [SerializeField]
    float m_MinWidth;

    [SerializeField]
    [Tooltip("0 = no maximum.")]
    float m_MaxWidth;

    [Header("Update")]
    [SerializeField]
    [Tooltip("Re-measure when the TMP text string changes.")]
    bool m_FitOnTextChange = true;

    [SerializeField]
    bool m_FitOnEnable = true;

    string m_LastText;
    RectTransform m_ResolvedTarget;
    TextMeshProUGUI m_ResolvedText;

    void OnEnable()
    {
        ResolveReferences();
        if (m_FitOnEnable)
            Fit();
    }

    void OnValidate()
    {
        ResolveReferences();
        Fit();
    }

    void LateUpdate()
    {
        if (!Application.isPlaying || !m_FitOnTextChange)
            return;

        if (m_ResolvedText == null)
            ResolveReferences();

        if (m_ResolvedText == null)
            return;

        string current = m_ResolvedText.text;
        if (current == m_LastText)
            return;

        Fit();
    }

    /// <summary>Re-measure the TMP label and apply width to the target.</summary>
    public void Fit()
    {
        ResolveReferences();
        if (m_ResolvedText == null || m_ResolvedTarget == null)
            return;

        m_LastText = m_ResolvedText.text;

        // Unconstrained preferred size so wrapping/parent stretch does not clamp the measure.
        Vector2 preferred = m_ResolvedText.GetPreferredValues(m_LastText);
        float width = preferred.x + m_HorizontalPadding;

        if (m_MinWidth > 0f)
            width = Mathf.Max(width, m_MinWidth);

        if (m_MaxWidth > 0f)
            width = Mathf.Min(width, m_MaxWidth);

        Vector2 size = m_ResolvedTarget.sizeDelta;
        size.x = width;
        m_ResolvedTarget.sizeDelta = size;
    }

    void ResolveReferences()
    {
        if (m_Text == null)
            m_Text = GetComponentInChildren<TextMeshProUGUI>(true);

        m_ResolvedText = m_Text;

        if (m_Target == null)
            m_Target = transform as RectTransform;

        m_ResolvedTarget = m_Target;
    }
}
