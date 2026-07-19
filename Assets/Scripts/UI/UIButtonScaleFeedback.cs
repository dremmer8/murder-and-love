using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Smooth scale feedback for UI buttons: grow on hover, shrink on press.
/// Drop on a Button (or any raycastable UI object). Uses DOTween.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonScaleFeedback : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Target")]
    [SerializeField]
    [Tooltip("Transform to scale. Defaults to this object.")]
    Transform m_ScaleTarget;

    [Header("Scale")]
    [SerializeField]
    [Tooltip("Idle / resting scale multiplier (usually 1).")]
    float m_NormalScale = 1f;

    [SerializeField]
    [Tooltip("Scale while the pointer is over the button.")]
    float m_HoverScale = 1.08f;

    [SerializeField]
    [Tooltip("Scale while the button is held down.")]
    float m_PressedScale = 0.92f;

    [Header("Timing")]
    [SerializeField]
    float m_HoverDuration = 0.16f;

    [SerializeField]
    float m_PressDuration = 0.08f;

    [SerializeField]
    float m_ReleaseDuration = 0.12f;

    [Header("Easing")]
    [SerializeField]
    Ease m_HoverEase = Ease.OutBack;

    [SerializeField]
    Ease m_PressEase = Ease.OutQuad;

    [SerializeField]
    Ease m_ReleaseEase = Ease.OutCubic;

    Selectable m_Selectable;
    bool m_Hovered;
    bool m_Pressed;
    Tween m_ScaleTween;

    void Awake()
    {
        if (m_ScaleTarget == null)
            m_ScaleTarget = transform;

        m_Selectable = GetComponent<Selectable>();
        m_ScaleTarget.localScale = Vector3.one * m_NormalScale;
    }

    void OnDisable()
    {
        m_Hovered = false;
        m_Pressed = false;
        KillTween();

        if (m_ScaleTarget != null)
            m_ScaleTarget.localScale = Vector3.one * m_NormalScale;
    }

    void OnDestroy()
    {
        KillTween();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractive())
            return;

        m_Hovered = true;
        if (!m_Pressed)
            AnimateTo(m_HoverScale, m_HoverDuration, m_HoverEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_Hovered = false;
        m_Pressed = false;

        if (!IsInteractive())
        {
            AnimateTo(m_NormalScale, m_ReleaseDuration, m_ReleaseEase);
            return;
        }

        AnimateTo(m_NormalScale, m_ReleaseDuration, m_ReleaseEase);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractive())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_Pressed = true;
        AnimateTo(m_PressedScale, m_PressDuration, m_PressEase);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_Pressed = false;

        if (!IsInteractive())
        {
            AnimateTo(m_NormalScale, m_ReleaseDuration, m_ReleaseEase);
            return;
        }

        float target = m_Hovered ? m_HoverScale : m_NormalScale;
        Ease ease = m_Hovered ? m_HoverEase : m_ReleaseEase;
        float duration = m_Hovered ? m_HoverDuration : m_ReleaseDuration;
        AnimateTo(target, duration, ease);
    }

    bool IsInteractive()
    {
        if (!isActiveAndEnabled)
            return false;

        if (m_Selectable != null)
            return m_Selectable.IsInteractable();

        return true;
    }

    void AnimateTo(float scale, float duration, Ease ease)
    {
        if (m_ScaleTarget == null)
            return;

        KillTween();

        Vector3 end = Vector3.one * scale;
        if (duration <= 0f)
        {
            m_ScaleTarget.localScale = end;
            return;
        }

        m_ScaleTween = m_ScaleTarget
            .DOScale(end, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    void KillTween()
    {
        if (m_ScaleTween != null && m_ScaleTween.IsActive())
            m_ScaleTween.Kill();

        m_ScaleTween = null;
    }
}
