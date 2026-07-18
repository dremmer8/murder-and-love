using System.Collections;
using UnityEngine;

/// <summary>
/// Circuit box minigame: open door → flip main switch → close door.
/// Door clicks fire animator trigger "toggle"; switch fires "on".
/// Only that order is accepted.
/// </summary>
public class CircuitBoxAnimator : MonoBehaviour
{
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
        if (!Physics.Raycast(ray, out var hit, 200f)) return false;

        var c = hit.collider;
        switch (step)
        {
            case Step.OpenDoor when c == doorTrigger:
                StartCoroutine(PlayStep(toggleTrigger, openDoorDelay, Step.FlipSwitch));
                return true;

            case Step.FlipSwitch when c == mainSwitchTrigger:
                StartCoroutine(PlayStep(onTrigger, switchOnDelay, Step.CloseDoor));
                return true;

            case Step.CloseDoor when c == doorTrigger:
                StartCoroutine(CloseAndFinish());
                return true;
        }

        // Absorb clicks on our triggers so they don't fall through.
        return c == doorTrigger || c == mainSwitchTrigger;
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
}
