using System.Collections;
using UnityEngine;

public class CoinMachineOperator : MonoBehaviour
{
    enum Step { KickOff, Fail1, Fail2, Success, Done }

    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Collider billSlit;

    [Tooltip("Seconds to wait after kickOff before bill-slit clicks are accepted.")]
    [SerializeField] float kickOffAnimDuration = 2f;

    [SerializeField] float failAnimDuration = 14f;

    [Tooltip("Seconds to wait after the success (win) trigger before firing the end trigger.")]
    [SerializeField] float successAnimDuration = 9f;

    [Tooltip("Seconds to wait after the end trigger before exiting the minigame.")]
    [SerializeField] float endExitDelay = 4f;

    [SerializeField] MinigameActivator minigameActivator;
    public DialogueTrigger afterSecondFailDialogue;

    Step step = Step.KickOff;
    bool busy;
    bool wasMinigameActive;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!minigameActivator)
            minigameActivator = GetComponentInParent<MinigameActivator>();
    }

    void Update()
    {
        WatchMinigameEnter();
        if (busy || step == Step.Done || step == Step.KickOff || !animator) return;
        if (minigameActivator != null && !minigameActivator.IsActivated) return;
        if (!Input.GetMouseButtonDown(0)) return;
        TryBillSlitClick();
    }

    void WatchMinigameEnter()
    {
        if (minigameActivator == null || !animator) return;
        bool active = minigameActivator.IsActivated;
        if (active && !wasMinigameActive && step == Step.KickOff && !busy)
            StartCoroutine(PlayKickOff());
        wasMinigameActive = active;
    }

    bool TryBillSlitClick()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 200f, GameLayers.MinigameZoneMask)) return false;
        if (hit.collider != billSlit) return false;

        switch (step)
        {
            case Step.Fail1:
                StartCoroutine(PlayFailThen(Step.Fail2));
                return true;
            case Step.Fail2:
                StartCoroutine(PlayFailThen(Step.Success));
                return true;
            case Step.Success:
                StartCoroutine(PlaySuccessAndEnd());
                return true;
        }

        return true;
    }

    IEnumerator PlayKickOff()
    {
        busy = true;
        animator.SetTrigger("kickOff");
        if (kickOffAnimDuration > 0f)
            yield return new WaitForSeconds(kickOffAnimDuration);
        step = Step.Fail1;
        busy = false;
    }

    IEnumerator PlayFailThen(Step next)
    {
        busy = true;
        animator.SetTrigger("fail");
        yield return new WaitForSeconds(failAnimDuration);
        step = next;
        busy = false;

        // Second fail completes when we advance to Success.
        if (next == Step.Success && afterSecondFailDialogue != null)
            afterSecondFailDialogue.TryStartDialogue();
    }

    IEnumerator PlaySuccessAndEnd()
    {
        busy = true;
        animator.SetTrigger("win");
        yield return new WaitForSeconds(successAnimDuration);
        animator.SetTrigger("end");
        yield return new WaitForSeconds(endExitDelay);
        step = Step.Done;

        if (minigameActivator != null)
        {
            if (minigameActivator.IsActivated)
                minigameActivator.Exit();
            minigameActivator.LockInteraction();
        }

        busy = false;
    }
}
