using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// World-space step hints for minigames. Extension of the HUD control-hints system:
/// you build one TMP (or empty + TMP child) in the scene, drag it in here, and this
/// component moves/shows it at the current step's anchor while a minigame is active.
///
/// Setup:
///   1. Create a world-space TextMeshPro (or World Space Canvas + TMP) styled how you like.
///   2. Assign its root + label below.
///   3. On each <see cref="MinigameActivator"/>, fill Step Hints (step id, text, anchor).
///   4. Have the minigame operator implement <see cref="IMinigameStepHintSource"/>.
/// </summary>
public class MinigameStepHintPresenter : MonoBehaviour
{
    [Header("Hint Object (set up yourself)")]
    [Tooltip("Root toggled on/off and moved to the current step anchor. Place/style it yourself.")]
    [SerializeField] GameObject hintRoot;

    [Tooltip("TMP label whose text is set from the active step entry. Auto-found under Hint Root if empty.")]
    [SerializeField] TMP_Text hintLabel;

    [Tooltip("Offset from the step anchor, in the anchor's local space.")]
    [SerializeField] Vector3 anchorLocalOffset = new Vector3(0f, 0.15f, 0f);

    [Tooltip("If true, the hint root faces the active camera each frame.")]
    [SerializeField] bool faceCamera = true;

    [Tooltip("Optional camera override. Uses Camera.main when empty.")]
    [SerializeField] Camera targetCamera;

    // Cached last-applied values so we only touch TMP / transform / SetActive when needed.
    bool _shown;
    string _text;
    Transform _anchor;
    Vector3 _lastPosition;
    Quaternion _lastRotation;

    readonly List<MonoBehaviour> _behaviourBuffer = new();

    void OnEnable()
    {
        if (hintLabel == null && hintRoot != null)
            hintLabel = hintRoot.GetComponentInChildren<TMP_Text>(true);

        SetHint(false, null, null);
    }

    void LateUpdate()
    {
        if (!TryResolveActiveHint(out string text, out Transform anchor))
        {
            SetHint(false, null, null);
            return;
        }

        SetHint(true, text, anchor);
        ApplyTransform(anchor);
    }

    bool TryResolveActiveHint(out string text, out Transform anchor)
    {
        text = null;
        anchor = null;

        if (!MinigameActivator.IsAnyActive)
            return false;

        MinigameActivator active = MinigameActivator.ActiveInstance;
        if (active == null)
            return false;

        if (!TryFindSource(active, out IMinigameStepHintSource source))
            return false;

        if (!source.TryGetCurrentStepHintId(out string stepId) || string.IsNullOrEmpty(stepId))
            return false;

        if (!active.TryGetStepHint(stepId, out MinigameStepHintEntry entry))
            return false;

        if (entry.anchor == null || string.IsNullOrEmpty(entry.hintText))
            return false;

        text = entry.hintText;
        anchor = entry.anchor;
        return true;
    }

    bool TryFindSource(MinigameActivator active, out IMinigameStepHintSource source)
    {
        source = null;
        _behaviourBuffer.Clear();
        active.GetComponentsInChildren(true, _behaviourBuffer);

        for (int i = 0; i < _behaviourBuffer.Count; i++)
        {
            if (_behaviourBuffer[i] is IMinigameStepHintSource candidate)
            {
                source = candidate;
                return true;
            }
        }

        return false;
    }

    void SetHint(bool show, string text, Transform anchor)
    {
        bool textChanged = show && text != _text;
        bool visibilityChanged = show != _shown;

        _shown = show;
        _text = text;
        _anchor = anchor;

        if (textChanged && hintLabel != null)
            hintLabel.text = text;

        if (visibilityChanged && hintRoot != null)
            hintRoot.SetActive(show);
    }

    void ApplyTransform(Transform anchor)
    {
        if (hintRoot == null || anchor == null)
            return;

        Vector3 position = anchor.TransformPoint(anchorLocalOffset);
        Quaternion rotation = hintRoot.transform.rotation;

        if (faceCamera)
        {
            Camera cam = ResolveCamera();
            if (cam != null)
            {
                Vector3 toCam = cam.transform.position - position;
                if (toCam.sqrMagnitude > 0.0001f)
                    rotation = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
            }
        }

        if (position == _lastPosition && rotation == _lastRotation && _anchor == anchor)
            return;

        _lastPosition = position;
        _lastRotation = rotation;
        hintRoot.transform.SetPositionAndRotation(position, rotation);
    }

    Camera ResolveCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        return targetCamera;
    }
}
