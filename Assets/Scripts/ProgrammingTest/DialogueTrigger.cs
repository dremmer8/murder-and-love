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

    [Header("Presentation")]
    [SerializeField] private DialoguePresentationMode presentationMode = DialoguePresentationMode.Standard;

    [Header("References")]
    [SerializeField] private StoryPhaseController storyPhaseController;

    private bool playerInRange;
    private bool waitingForDialogueEnd;
    private bool zoneFiredThisStay;

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
        UnsubscribeDialogueEnded();
    }

    private void Update()
    {
        // Pager / internal can run while Gameplay; only block on hard locks.
        if (GameStateManager.CurrentState == GameState.Dialogue
            || GameStateManager.CurrentState == GameState.Pager
            || GameStateManager.CurrentState == GameState.Paused)
            return;

        if (waitingForDialogueEnd && presentationMode != DialoguePresentationMode.Pager)
            return;

        switch (activationMode)
        {
            case DialogueActivationMode.KeyPress:
                if (!playerInRange)
                    return;
                if (!Input.GetKeyDown(interactKey))
                    return;
                TryStartDialogue();
                break;

            case DialogueActivationMode.TriggerZone:
                if (!recheckWhileInside || !playerInRange || zoneFiredThisStay)
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
    /// </summary>
    public bool TryStartDialogue()
    {
        if (storyPhaseController == null)
            return false;

        string knotToPlay = storyPhaseController.ResolveKnot();
        if (string.IsNullOrEmpty(knotToPlay))
            return false;

        return BeginDialogue(knotToPlay);
    }

    public bool TryStartDialogue(string knotName)
    {
        if (string.IsNullOrEmpty(knotName))
            return TryStartDialogue();

        return BeginDialogue(knotName);
    }

    public void StartDialogue() => TryStartDialogue();

    public void StartDialogue(string knotName) => TryStartDialogue(knotName);

    private bool BeginDialogue(string knotToPlay)
    {
        if (presentationMode != DialoguePresentationMode.Pager && waitingForDialogueEnd)
            return false;

        DialogueManager manager = DialogueManager.GetInstance();
        if (manager == null)
        {
            Debug.LogWarning($"{name}: No DialogueManager in scene.", this);
            return false;
        }

        if (manager.IsBusy && presentationMode != DialoguePresentationMode.Pager)
            return false;

        // Pager threads replace each other; don't block on IsBusy from NPCs.
        if (presentationMode == DialoguePresentationMode.Pager
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

        SubscribeDialogueEnded();
        manager.EnterDialogue(inkFile, knotToPlay, presentationMode);
        return true;
    }

    private void SubscribeDialogueEnded()
    {
        if (waitingForDialogueEnd)
            return;

        DialogueManager manager = DialogueManager.GetInstance();
        if (manager == null)
            return;

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
    }

    private void HandleDialogueEnded(string completedKnot)
    {
        UnsubscribeDialogueEnded();

        if (storyPhaseController != null && !string.IsNullOrEmpty(completedKnot))
            storyPhaseController.MarkKnotCompleted(completedKnot);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (activationMode != DialogueActivationMode.TriggerZone)
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
    }
}
