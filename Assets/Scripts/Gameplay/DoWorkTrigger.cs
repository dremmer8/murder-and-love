using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives the washing-machine "working" loop via the animator bool <c>isWorking</c>
/// (true = working / spinning, false = idle).
/// Blackout pauses all instances; power restore resumes those that should be working.
/// </summary>
public class DoWorkTrigger : MonoBehaviour
{
    static readonly List<DoWorkTrigger> s_Instances = new();
    static bool s_PowerAvailable = true;

    [SerializeField] Animator animator;

    [Tooltip("Optional object enabled while working and disabled while idle (follows blackout pause).")]
    [SerializeField] GameObject activeWhileWorking;

    [Tooltip("Animator bool parameter name.")]
    [SerializeField] string isWorkingParam = "isWorking";

    [Tooltip("If true, sets isWorking on Start.")]
    [SerializeField] bool playOnStart;

    [Tooltip("Value applied when Play On Start is enabled.")]
    [SerializeField] bool startWorking = true;

    /// <summary>Desired working state (may be deferred while power is out).</summary>
    public bool IsWorking { get; private set; }

    void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (!s_Instances.Contains(this))
            s_Instances.Add(this);
        ApplyAnimatorState();
    }

    void OnDisable()
    {
        s_Instances.Remove(this);
    }

    void Start()
    {
        if (playOnStart)
            SetWorking(startWorking);
    }

    /// <summary>Start the working animation (isWorking = true). UnityEvent-friendly.</summary>
    public void DoWork() => SetWorking(true);

    /// <summary>Stop the working animation (isWorking = false). UnityEvent-friendly.</summary>
    public void StopWork() => SetWorking(false);

    /// <summary>Explicitly set the desired working state.</summary>
    public void SetWorking(bool working)
    {
        IsWorking = working;
        ApplyAnimatorState();
    }

    void ApplyAnimatorState()
    {
        // During blackout, machines visually stop even if they should keep running after power returns.
        bool live = s_PowerAvailable && IsWorking;

        if (animator)
            animator.SetBool(isWorkingParam, live);

        if (activeWhileWorking)
            activeWhileWorking.SetActive(live);
    }

    /// <summary>Global: stop every machine for blackout (keeps desired state for restore).</summary>
    public static void PauseAllForBlackout()
    {
        s_PowerAvailable = false;
        for (int i = 0; i < s_Instances.Count; i++)
        {
            if (s_Instances[i] != null)
                s_Instances[i].ApplyAnimatorState();
        }
    }

    /// <summary>Global: restore power — machines that were / should be working turn back on.</summary>
    public static void ResumeAllAfterBlackout()
    {
        s_PowerAvailable = true;
        for (int i = 0; i < s_Instances.Count; i++)
        {
            if (s_Instances[i] != null)
                s_Instances[i].ApplyAnimatorState();
        }
    }
}
