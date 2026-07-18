using System.Collections;
using UnityEngine;

public class CoinMachineOperator : MonoBehaviour
{
    enum Step { Fail1, Fail2, Success, Done }

    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Collider billSlit;
    [SerializeField] float failAnimDuration = 14f;

    [Tooltip("Seconds to wait after the success (win) trigger before firing the end trigger.")]
    [SerializeField] float successAnimDuration = 9f;

    [Tooltip("Seconds to wait after the end trigger before exiting the minigame.")]
    [SerializeField] float endExitDelay = 4f;

    [SerializeField] MinigameActivator minigameActivator;
    public DialogueTrigger afterSecondFailDialogue;

    Step step = Step.Fail1;
    bool busy;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!minigameActivator)
            minigameActivator = GetComponentInParent<MinigameActivator>();
    }

    void Update()
    {
        if (busy || step == Step.Done || !animator) return;
        if (!Input.GetMouseButtonDown(0)) return;
        TryBillSlitClick();
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
