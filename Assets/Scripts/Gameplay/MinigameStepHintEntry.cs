using System;
using UnityEngine;

/// <summary>
/// One world-space step hint for a minigame. Configure these on
/// <see cref="MinigameActivator"/>; the operator returns matching step ids via
/// <see cref="IMinigameStepHintSource"/>.
/// </summary>
[Serializable]
public class MinigameStepHintEntry
{
    [Tooltip("Must match the step id returned by IMinigameStepHintSource (e.g. OpenDoor).")]
    public string stepId;

    [Tooltip("How-to text shown on the world-space hint object for this step.")]
    [TextArea]
    public string hintText;

    [Tooltip("World transform the shared hint object snaps to while this step is active.")]
    public Transform anchor;

    [Tooltip("Optional. Used when Anchor is empty — snaps to this collider's transform.")]
    public Collider anchorCollider;

    /// <summary>Resolved world anchor for the shared hint object, or null.</summary>
    public Transform ResolvedAnchor
    {
        get
        {
            if (anchor != null)
                return anchor;
            return anchorCollider != null ? anchorCollider.transform : null;
        }
    }
}
