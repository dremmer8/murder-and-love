using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NamedAnimator
{
    public string Name;
    public Animator Animator;

    [Tooltip("Used when Animator is empty: GetComponentInChildren on this object.")]
    public GameObject Root;
}

/// <summary>
/// Named animator registry for Ink EXTERNAL TriggerAnimation(targetId, animationName).
/// Also drives outside-dialogue character loops: Mandy doIdle while game_progression &lt;
/// idleUntilProgression, and Lau doStandLoop once game_progression reaches standLoopFromProgression.
/// </summary>
public class DialogueAnimationTargets : MonoBehaviour
{
    public static DialogueAnimationTargets Instance { get; private set; }

    public const string GiveItemTrigger = "doGiveItem";
    public const string IdleTrigger = "doIdle";
    public const string StandLoopTrigger = "doStandLoop";
    public const string DefaultIdleTargetId = "Mandy";
    public const string DefaultStandLoopTargetId = "Lau";

    [Header("Targets")]
    [SerializeField] List<NamedAnimator> animators = new();

    [Header("Idle (outside dialogue)")]
    [Tooltip("Fire doIdle on this target when not in dialogue and progression is below the cutoff.")]
    [SerializeField] string idleTargetId = DefaultIdleTargetId;

    [Tooltip("Stop auto-idle once game_progression reaches this value.")]
    [SerializeField] int idleUntilProgression = 22;

    [Header("Stand loop (outside dialogue)")]
    [Tooltip("Fire doStandLoop on this target when not in dialogue and progression has reached the threshold.")]
    [SerializeField] string standLoopTargetId = DefaultStandLoopTargetId;

    [Tooltip("Start auto stand-loop once game_progression reaches this value.")]
    [SerializeField] int standLoopFromProgression = 26;

    [Header("Give item lock")]
    [Tooltip("Fallback lock duration if the give clip length cannot be read.")]
    [SerializeField] float giveItemFallbackDuration = 9.33f;

    [Tooltip("Animator state / clip name used to resolve doGiveItem length.")]
    [SerializeField] string giveItemStateName = "M_stand_give_item_1";

    int _lastIdleProgression = int.MinValue;
    bool _subscribedToDialogue;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: more than one DialogueAnimationTargets in scene.", this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        UnsubscribeDialogue();
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        SubscribeDialogue();
        TryApplyOutsideDialoguePoses();
    }

    void Update()
    {
        if (!_subscribedToDialogue)
            SubscribeDialogue();

        int progression = GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;

        if (progression != _lastIdleProgression)
            TryApplyOutsideDialoguePoses();
    }

    void SubscribeDialogue()
    {
        DialogueManager dialogue = DialogueManager.GetInstance();
        if (dialogue == null || _subscribedToDialogue)
            return;

        dialogue.OnDialogueEnded += HandleDialogueEnded;
        _subscribedToDialogue = true;
    }

    void UnsubscribeDialogue()
    {
        DialogueManager dialogue = DialogueManager.GetInstance();
        if (dialogue != null && _subscribedToDialogue)
            dialogue.OnDialogueEnded -= HandleDialogueEnded;

        _subscribedToDialogue = false;
    }

    void HandleDialogueEnded(string _)
    {
        TryApplyOutsideDialoguePoses();
    }

    void TryApplyOutsideDialoguePoses()
    {
        int progression = GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;

        _lastIdleProgression = progression;

        DialogueManager dialogue = DialogueManager.GetInstance();
        if (dialogue != null && dialogue.dialogueIsPlaying)
            return;

        TryApplyIdle(progression);
        TryApplyStandLoop(progression);
    }

    /// <summary>Ink EXTERNAL entry point.</summary>
    public bool Trigger(string targetId, string animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName))
        {
            Debug.LogWarning($"{name}: TriggerAnimation animation name is empty.", this);
            return false;
        }

        if (!TryGetAnimator(targetId, out Animator animator))
        {
            Debug.LogWarning($"{name}: No animator for targetId '{targetId}'.", this);
            return false;
        }

        string trigger = animationName.Trim();
        animator.SetTrigger(trigger);

        if (string.Equals(trigger, GiveItemTrigger, StringComparison.OrdinalIgnoreCase))
            LockAdvanceForGive(animator);

        return true;
    }

    void LockAdvanceForGive(Animator animator)
    {
        DialogueManager dialogue = DialogueManager.GetInstance();
        if (dialogue == null)
            return;

        float duration = ResolveClipLength(animator, giveItemStateName, giveItemFallbackDuration);
        dialogue.LockAdvanceFor(duration);
    }

    static float ResolveClipLength(Animator animator, string clipOrStateName, float fallback)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return fallback;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        if (clips == null)
            return fallback;

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip != null
                && string.Equals(clip.name, clipOrStateName, StringComparison.OrdinalIgnoreCase))
            {
                return Mathf.Max(0.1f, clip.length);
            }
        }

        return fallback;
    }

    public bool TryGetAnimator(string targetId, out Animator animator)
    {
        animator = null;
        if (string.IsNullOrWhiteSpace(targetId))
            return false;

        string trimmed = targetId.Trim();
        Animator inactiveFallback = null;

        for (int i = 0; i < animators.Count; i++)
        {
            NamedAnimator entry = animators[i];
            if (entry == null || string.IsNullOrEmpty(entry.Name))
                continue;

            if (!string.Equals(entry.Name, trimmed, StringComparison.OrdinalIgnoreCase))
                continue;

            Animator candidate = ResolveAnimator(entry);
            if (candidate == null)
                continue;

            // Prefer an active instance when multiple share the same id (counter vs backroom).
            if (candidate.isActiveAndEnabled && candidate.gameObject.activeInHierarchy)
            {
                animator = candidate;
                return true;
            }

            if (inactiveFallback == null)
                inactiveFallback = candidate;
        }

        animator = inactiveFallback;
        return animator != null;
    }

    static Animator ResolveAnimator(NamedAnimator entry)
    {
        if (entry.Animator != null)
            return entry.Animator;

        if (entry.Root != null)
            return entry.Root.GetComponentInChildren<Animator>(true);

        return null;
    }

    void TryApplyIdle(int progression)
    {
        if (progression >= idleUntilProgression)
            return;

        if (string.IsNullOrEmpty(idleTargetId))
            return;

        if (!TryGetAnimator(idleTargetId, out Animator animator) || animator == null)
            return;

        animator.SetTrigger(IdleTrigger);
    }

    void TryApplyStandLoop(int progression)
    {
        if (progression < standLoopFromProgression)
            return;

        if (string.IsNullOrEmpty(standLoopTargetId))
            return;

        if (!TryGetAnimator(standLoopTargetId, out Animator animator) || animator == null)
            return;

        animator.SetTrigger(StandLoopTrigger);
    }
}
