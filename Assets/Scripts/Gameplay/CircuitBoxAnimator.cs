using System.Collections;
using UnityEngine;

/// <summary>
/// Circuit box minigame: open door → flip main switch → close door.
/// Door clicks fire animator trigger "toggle"; switch fires "on".
/// Flipping the switch restores baked lighting via <see cref="BakedLightingController"/>.
/// Only that order is accepted.
/// </summary>
public class CircuitBoxAnimator : MonoBehaviour, IMinigameStepHintSource
{
    public const string HintOpenDoor = "OpenDoor";
    public const string HintFlipSwitch = "FlipSwitch";
    public const string HintCloseDoor = "CloseDoor";

    enum Step { OpenDoor, FlipSwitch, CloseDoor, Done }

    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Collider doorTrigger;
    [SerializeField] Collider mainSwitchTrigger;

    [Header("Animator triggers")]
    [SerializeField] string toggleTrigger = "toggle";
    [SerializeField] string onTrigger = "on";

    [Header("Timing")]
    [Tooltip("Seconds to ignore further clicks after opening the door.")]
    [SerializeField] float openDoorDelay = 0.5f;

    [Tooltip("Seconds to ignore further clicks after flipping the switch.")]
    [SerializeField] float switchOnDelay = 0.5f;

    [Tooltip("Seconds to wait after closing the door before exiting the minigame.")]
    [SerializeField] float closeDoorDelay = 0.5f;

    [Header("Lighting")]
    [Tooltip("When the main switch 'on' trigger fires, restore the lights-on baked scenario.")]
    [SerializeField] bool restoreLightsOnSwitch = true;

    [Tooltip("Optional override. Uses BakedLightingController.Instance when empty.")]
    [SerializeField] BakedLightingController lightingController;

    [Header("Minigame")]
    [SerializeField] MinigameActivator minigameActivator;

    [Tooltip("If true, Exit + LockInteraction after the sequence completes.")]
    [SerializeField] bool exitAndLockOnComplete = true;

    [Tooltip("Optional object enabled when the minigame sequence completes.")]
    [SerializeField] GameObject objectToEnableOnComplete;

    Step step = Step.OpenDoor;
    bool busy;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!minigameActivator)
            minigameActivator = GetComponentInParent<MinigameActivator>();
        if (!animator)
            animator = GetComponentInChildren<Animator>();
        if (!lightingController)
            lightingController = BakedLightingController.Instance;
    }

    void Update()
    {
        if (busy || step == Step.Done || !animator) return;
        if (minigameActivator != null && !minigameActivator.IsActivated) return;
        if (!Input.GetMouseButtonDown(0)) return;
        TrySequenceClick();
    }

    bool TrySequenceClick()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 200f, GameLayers.MinigameZoneMask)) return false;

        var c = hit.collider;
        switch (step)
        {
            case Step.OpenDoor when c == doorTrigger:
                StartCoroutine(PlayStep(toggleTrigger, openDoorDelay, Step.FlipSwitch));
                return true;

            case Step.FlipSwitch when c == mainSwitchTrigger:
                StartCoroutine(FlipSwitchAndRestoreLights());
                return true;

            case Step.CloseDoor when c == doorTrigger:
                StartCoroutine(CloseAndFinish());
                return true;
        }

        // Absorb clicks on our triggers so they don't fall through.
        return c == doorTrigger || c == mainSwitchTrigger;
    }

    IEnumerator FlipSwitchAndRestoreLights()
    {
        busy = true;
        animator.SetTrigger(onTrigger);

        if (restoreLightsOnSwitch)
            RestoreLights();

        if (switchOnDelay > 0f)
            yield return new WaitForSeconds(switchOnDelay);

        step = Step.CloseDoor;
        busy = false;
    }

    void RestoreLights()
    {
        BakedLightingController lighting = lightingController;
        if (lighting == null)
            lighting = BakedLightingController.Instance;
        if (lighting == null)
            lighting = FindFirstObjectByType<BakedLightingController>();

        if (lighting == null)
        {
            Debug.LogWarning($"{name}: No BakedLightingController found — cannot restore lights.", this);
            return;
        }

        lighting.ApplyLightsOn();
    }

    IEnumerator PlayStep(string trigger, float delay, Step next)
    {
        busy = true;
        animator.SetTrigger(trigger);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        step = next;
        busy = false;
    }

    IEnumerator CloseAndFinish()
    {
        busy = true;
        animator.SetTrigger(toggleTrigger);
        if (closeDoorDelay > 0f)
            yield return new WaitForSeconds(closeDoorDelay);

        step = Step.Done;

        if (objectToEnableOnComplete != null)
            objectToEnableOnComplete.SetActive(true);

        if (exitAndLockOnComplete && minigameActivator != null)
        {
            if (minigameActivator.IsActivated)
                minigameActivator.Exit();
            minigameActivator.LockInteraction();
        }

        busy = false;
    }

    public bool TryGetCurrentStepHintId(out string stepId)
    {
        stepId = null;

        if (busy || step == Step.Done)
            return false;

        if (minigameActivator != null && !minigameActivator.IsActivated)
            return false;

        switch (step)
        {
            case Step.OpenDoor:
                stepId = HintOpenDoor;
                return true;
            case Step.FlipSwitch:
                stepId = HintFlipSwitch;
                return true;
            case Step.CloseDoor:
                stepId = HintCloseDoor;
                return true;
            default:
                return false;
        }
    }
}
