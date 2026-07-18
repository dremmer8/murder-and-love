using UnityEngine;

public enum DialogueActivationMode
{
    /// <summary>Player must be in the trigger and press the interact key.</summary>
    KeyPress = 0,

    /// <summary>Starts automatically when the player enters the trigger zone.</summary>
    TriggerZone = 1,

    /// <summary>Only starts when another script or UnityEvent calls TryStartDialogue / StartDialogue.</summary>
    ExternalEvent = 2
}

[RequireComponent(typeof(StoryPhaseController))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private DialogueActivationMode activationMode = DialogueActivationMode.KeyPress;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Tooltip("TriggerZone only: if the player is already inside when a knot unlocks, start it without re-entering.")]
    [SerializeField] private bool recheckWhileInside = true;

    [Tooltip("TriggerZone only: seconds the player must remain inside before dialogue can start. 0 = fire on enter.")]
    [SerializeField] private float requiredStayDuration = 0f;

    [Header("Progression Gate")]
    [Tooltip("Minimum game_progression (story phase) required. 0 = no minimum.")]
    [SerializeField] private int minStoryPhase = 0;

    [Tooltip("If enabled, game_progression must also be <= Max Story Phase.")]
    [SerializeField] private bool useMaxStoryPhase = false;

    [Tooltip("Inclusive maximum game_progression. Ignored unless Use Max Story Phase is enabled.")]
    [SerializeField] private int maxStoryPhase = 0;

    [Tooltip("When dialogue starts, set Ink story_phase and play the matching knot from Story Phase Controller.")]
    [SerializeField] private bool useForcedStoryPhase = false;

    [SerializeField] private int forcedStoryPhase = 1;

    [Header("Presentation")]
    [SerializeField] private DialoguePresentationMode presentationMode = DialoguePresentationMode.Standard;

    [Tooltip("If true, starting this trigger aborts any active dialogue and starts immediately.")]
    public bool forceCancelPrevious;

    [Tooltip("Pager only: after the inbound message, Space shows \"start typing\" and any key types the canned reply. When finished, plays the completion ending cutscene and fires Next Dialogue Trigger. Tab is the only way to leave the pager during this sequence.")]
    [SerializeField] private bool isRespondSupportPager;

    [Header("Player Pose")]
    [Tooltip("Optional mark the player tweens to when this dialogue starts. Leave empty to keep current pose.")]
    [SerializeField] private Transform playerPoseMark;

    [Tooltip("If true and Player Pose Mark is set, tween the player into that mark when dialogue starts.")]
    [SerializeField] private bool tweenPlayerToMark = true;

    [Tooltip("Tween length in seconds. Uses CameraManager default when < 0. 0 = snap.")]
    [SerializeField] private float playerTweenDuration = -1f;

    [Header("Sequence")]
    [Tooltip("If set, this trigger starts automatically when the current dialogue ends.")]
    [SerializeField] private DialogueTrigger nextDialogueTrigger;

    [Header("References")]
    [SerializeField] private StoryPhaseController storyPhaseController;

    private bool playerInRange;
    private bool waitingForDialogueEnd;
    private bool zoneFiredThisStay;
    private float stayTimer;
    private Coroutine pendingNextRoutine;
    private string waitingForKnot;

    public DialogueActivationMode ActivationMode
    {
        get => activationMode;
        set => activationMode = value;
    }

    public DialoguePresentationMode PresentationMode
    {
        get => presentationMode;
        set => presentationMode = value;
    }

    public bool IsRespondSupportPager
    {
        get => isRespondSupportPager;
        set => isRespondSupportPager = value;
    }

    public bool PlayerInRange => playerInRange;
    public bool IsWaitingForDialogueEnd => waitingForDialogueEnd;

    private void Awake()
    {
        playerInRange = false;

        if (storyPhaseController == null)
            storyPhaseController = GetComponent<StoryPhaseController>();
    }

    private void OnDestroy()
    {
        CancelPendingNext();
        UnsubscribeDialogueEnded();
    }

    private void Update()
    {
        if (GameStateManager.CurrentState == GameState.Paused)
            return;

        // Non-force triggers wait; forceCancelPrevious may interrupt Dialogue / Pager.
        if (!forceCancelPrevious
            && (GameStateManager.CurrentState == GameState.Dialogue
                || GameStateManager.CurrentState == GameState.Pager))
            return;

        if (!forceCancelPrevious
            && waitingForDialogueEnd
            && presentationMode != DialoguePresentationMode.Pager
            && !isRespondSupportPager)
            return;

        switch (activationMode)
        {
            case DialogueActivationMode.KeyPress:
                if (!playerInRange)
                    return;
                if (!Input.GetKeyDown(interactKey))
                    return;
                // Standing in a dialogue volume must not steal E from look-targeted
                // Interactables or DialogueZone NPCs — InteractionSystem owns those presses.
                if (IsAimingAtLookTarget())
                    return;
                TryStartDialogue();
                break;

            case DialogueActivationMode.TriggerZone:
                if (!playerInRange || zoneFiredThisStay)
                    return;

                stayTimer += Time.deltaTime;
                if (stayTimer < requiredStayDuration)
                    return;

                // Instant zones (duration 0) only recheck when enabled; timed zones keep trying once dwell is met.
                if (requiredStayDuration <= 0f && !recheckWhileInside)
                    return;

                if (TryStartDialogue())
                    zoneFiredThisStay = true;
                break;

            case DialogueActivationMode.ExternalEvent:
                break;
        }
    }

    /// <summary>
    /// Resolves the current knot from <see cref="StoryPhaseController"/> and starts dialogue.
    /// When forced story phase is enabled, plays that phase's knot instead of progression unlock.
    /// </summary>
    public bool TryStartDialogue()
    {
        if (!IsStoryPhaseAllowed())
            return false;

        if (storyPhaseController == null)
            return false;

        StoryPhaseEntry entry = useForcedStoryPhase
            ? storyPhaseController.FindEntryForStoryPhase(forcedStoryPhase)
            : storyPhaseController.ResolveBestEntry();

        if (entry == null || string.IsNullOrEmpty(entry.knotName))
            return false;

        return BeginDialogue(entry.knotName, entry.storyPhase, force: forceCancelPrevious);
    }

    public bool TryStartDialogue(string knotName)
    {
        if (!IsStoryPhaseAllowed())
            return false;

        if (string.IsNullOrEmpty(knotName))
            return TryStartDialogue();

        int storyPhase = -1;
        if (storyPhaseController != null)
        {
            StoryPhaseEntry entry = storyPhaseController.FindEntryForKnot(knotName);
            if (entry != null)
                storyPhase = entry.storyPhase;
        }

        return BeginDialogue(knotName, storyPhase, force: forceCancelPrevious);
    }

    /// <summary>
    /// True when current <see cref="GlobalVariableOperator.GameProgression"/> is within the configured range.
    /// </summary>
    public bool IsStoryPhaseAllowed()
    {
        int progression = GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;

        if (progression < minStoryPhase)
            return false;

        if (useMaxStoryPhase && progression > maxStoryPhase)
            return false;

        return true;
    }

    public void StartDialogue() => TryStartDialogue();

    public void StartDialogue(string knotName) => TryStartDialogue(knotName);

    /// <summary>
    /// Starts a knot even if another presentation is active (aborts the current one).
    /// </summary>
    public bool ForceStartDialogue(string knotName)
    {
        if (string.IsNullOrEmpty(knotName))
            return false;

        int storyPhase = -1;
        if (storyPhaseController != null)
        {
            StoryPhaseEntry entry = storyPhaseController.FindEntryForKnot(knotName);
            if (entry != null)
                storyPhase = entry.storyPhase;
        }

        return BeginDialogue(knotName, storyPhase, force: true);
    }

    private bool BeginDialogue(string knotToPlay, int storyPhaseToSet, bool force = false)
    {
        if (!force && presentationMode != DialoguePresentationMode.Pager && !isRespondSupportPager
            && waitingForDialogueEnd)
            return false;

        DialogueManager manager = DialogueManager.GetInstance();
        if (manager == null)
        {
            Debug.LogWarning($"{name}: No DialogueManager in scene.", this);
            return false;
        }

        if (force)
        {
            UnsubscribeDialogueEnded();
            manager.AbortActivePresentation();
        }
        else if (manager.IsBusy && presentationMode != DialoguePresentationMode.Pager && !isRespondSupportPager)
            return false;

        // Pager threads replace each other; don't block on IsBusy from NPCs.
        if ((presentationMode == DialoguePresentationMode.Pager || isRespondSupportPager)
            && manager.ActiveMode == DialoguePresentationMode.Pager
            && waitingForDialogueEnd)
        {
            // Allow replace — unsubscribe old wait first.
            UnsubscribeDialogueEnded();
        }

        TextAsset inkFile = storyPhaseController != null ? storyPhaseController.InkFile : null;
        if (inkFile == null)
        {
            Debug.LogWarning($"{name}: StoryCharacterPhasesSO has no ink file assigned.", this);
            return false;
        }

        // Keep Ink story_phase aligned with the knot we actually start (avoids sticky values
        // from a previous forced trigger looking like the wrong phase is playing).
        if (GlobalVariableOperator.Instance != null)
        {
            int phase = useForcedStoryPhase ? forcedStoryPhase : storyPhaseToSet;
            if (phase >= 0)
                GlobalVariableOperator.Instance.SetStoryPhase(phase);
        }

        SubscribeDialogueEnded(knotToPlay);

        DialoguePresentationMode mode = presentationMode;
        if (isRespondSupportPager)
            mode = DialoguePresentationMode.Pager;

        manager.EnterDialogue(inkFile, knotToPlay, mode, isRespondSupportPager);
        TryTweenPlayerToMark();
        return true;
    }

    void TryTweenPlayerToMark()
    {
        if (!tweenPlayerToMark || playerPoseMark == null)
            return;

        // Modes that keep free movement should not steal the player pose.
        if (presentationMode == DialoguePresentationMode.InternalMonologue
            || presentationMode == DialoguePresentationMode.Pager
            || isRespondSupportPager)
            return;

        if (CameraManager.Instance == null)
        {
            Debug.LogWarning($"{name}: No CameraManager in scene — cannot tween player pose.", this);
            return;
        }

        if (playerTweenDuration < 0f)
            CameraManager.Instance.TweenPlayerTo(playerPoseMark);
        else
            CameraManager.Instance.TweenPlayerTo(playerPoseMark, playerTweenDuration);
    }

    /// <summary>
    /// True when the crosshair is on an Interactable or DialogueZone target.
    /// Those presses are owned by <see cref="InteractionSystem"/> (separate layer masks).
    /// </summary>
    private bool IsAimingAtLookTarget()
    {
        InteractionSystem interaction = InteractionSystem.Instance;
        if (interaction == null)
            interaction = FindFirstObjectByType<InteractionSystem>();

        if (interaction == null)
            return false;

        return interaction.TryGetAimedInteractable(out _)
            || interaction.TryGetAimedDialogueTrigger(out _);
    }

    private void SubscribeDialogueEnded(string knotName)
    {
        DialogueManager manager = DialogueManager.GetInstance();
        if (manager == null)
            return;

        // Always re-bind so a replaced pager thread tracks the new knot.
        if (waitingForDialogueEnd)
            manager.OnDialogueEnded -= HandleDialogueEnded;

        waitingForKnot = knotName ?? "";
        manager.OnDialogueEnded += HandleDialogueEnded;
        waitingForDialogueEnd = true;
    }

    private void UnsubscribeDialogueEnded()
    {
        if (!waitingForDialogueEnd)
            return;

        DialogueManager manager = DialogueManager.GetInstance();
        if (manager != null)
            manager.OnDialogueEnded -= HandleDialogueEnded;

        waitingForDialogueEnd = false;
        waitingForKnot = null;
    }

    private void HandleDialogueEnded(string completedKnot)
    {
        // Pager stays subscribed while the player can still talk to NPCs.
        // Ignore other dialogues ending so nextDialogueTrigger does not bleed.
        if (!string.IsNullOrEmpty(waitingForKnot)
            && !string.IsNullOrEmpty(completedKnot)
            && completedKnot != waitingForKnot)
            return;

        UnsubscribeDialogueEnded();

        if (storyPhaseController != null && !string.IsNullOrEmpty(completedKnot))
            storyPhaseController.MarkKnotCompleted(completedKnot);

        if (nextDialogueTrigger == null || nextDialogueTrigger == this)
            return;

        // Wait a frame so DialogueManager finishes teardown before the next EnterDialogue.
        CancelPendingNext();
        pendingNextRoutine = StartCoroutine(StartNextDialogueNextFrame());
    }

    private System.Collections.IEnumerator StartNextDialogueNextFrame()
    {
        yield return null;
        pendingNextRoutine = null;

        if (nextDialogueTrigger == null)
            yield break;

        nextDialogueTrigger.TryStartDialogue();
    }

    private void CancelPendingNext()
    {
        if (pendingNextRoutine == null)
            return;

        StopCoroutine(pendingNextRoutine);
        pendingNextRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        stayTimer = 0f;

        if (activationMode != DialogueActivationMode.TriggerZone)
            return;

        // Timed zones wait in Update until requiredStayDuration elapses.
        if (requiredStayDuration > 0f)
            return;

        if (GameStateManager.CurrentState != GameState.Gameplay)
            return;

        if (TryStartDialogue())
            zoneFiredThisStay = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        zoneFiredThisStay = false;
        stayTimer = 0f;
    }
}
