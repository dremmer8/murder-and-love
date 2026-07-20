using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CoinMachineOperator : MonoBehaviour, IMinigameStepHintSource
{
    public const string HintInsertBill = "InsertBill";

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

    [Tooltip("Phase 16 Interaction_with_coin_machine — fired after each fail and after success.")]
    [FormerlySerializedAs("afterSecondFailDialogue")]
    [SerializeField] DialogueTrigger attemptDialogue;

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
        if (IsDialogueBlocking())
            return;
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

        // Visit 1 after first fail, visit 2 after second fail.
        yield return FireAttemptDialogueAndWait();

        busy = false;
    }

    IEnumerator PlaySuccessAndEnd()
    {
        busy = true;
        animator.SetTrigger("win");
        yield return new WaitForSeconds(successAnimDuration);
        animator.SetTrigger("end");
        yield return new WaitForSeconds(endExitDelay);

        // Visit 3 (else branch): "Finally..."
        yield return FireAttemptDialogueAndWait();

        step = Step.Done;

        if (minigameActivator != null)
        {
            if (minigameActivator.IsActivated)
                minigameActivator.Exit();
            minigameActivator.LockInteraction();
        }

        busy = false;
    }

    IEnumerator FireAttemptDialogueAndWait()
    {
        if (attemptDialogue == null)
            yield break;

        if (!attemptDialogue.TryStartDialogue())
            yield break;

        // Internal monologue stays in Gameplay — wait on DialogueManager, not GameState.
        yield return null;

        DialogueManager dialogue = DialogueManager.GetInstance();
        while (dialogue != null && (dialogue.dialogueIsPlaying || dialogue.IsBusy))
            yield return null;
    }

    static bool IsDialogueBlocking()
    {
        if (GameStateManager.CurrentState == GameState.Dialogue
            || GameStateManager.CurrentState == GameState.Pager
            || GameStateManager.CurrentState == GameState.Paused)
            return true;

        DialogueManager dialogue = DialogueManager.GetInstance();
        return dialogue != null && (dialogue.dialogueIsPlaying || dialogue.IsBusy);
    }

    public bool TryGetCurrentStepHintId(out string stepId)
    {
        stepId = null;

        if (busy || step == Step.Done || step == Step.KickOff)
            return false;

        if (minigameActivator != null && !minigameActivator.IsActivated)
            return false;

        if (IsDialogueBlocking())
            return false;

        switch (step)
        {
            case Step.Fail1:
            case Step.Fail2:
            case Step.Success:
                stepId = HintInsertBill;
                return true;
            default:
                return false;
        }
    }
}
