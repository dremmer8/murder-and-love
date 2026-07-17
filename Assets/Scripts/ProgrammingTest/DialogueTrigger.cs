using UnityEngine;

[RequireComponent(typeof(StoryPhaseController))]
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private StoryPhaseController storyPhaseController;

    private bool playerInRange;
    private bool waitingForDialogueEnd;

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
        if (!playerInRange || GameStateManager.CurrentState != GameState.Gameplay)
            return;

        if (!Input.GetKey(KeyCode.E))
            return;

        if (storyPhaseController == null)
            return;

        string knotToPlay = storyPhaseController.ResolveKnot();
        if (string.IsNullOrEmpty(knotToPlay))
            return;

        TextAsset inkFile = storyPhaseController.InkFile;
        if (inkFile == null)
        {
            Debug.LogWarning($"{name}: StoryCharacterPhasesSO has no ink file assigned.", this);
            return;
        }

        SubscribeDialogueEnded();
        DialogueManager.GetInstance().EnterDialogue(inkFile, knotToPlay);
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
        if (other.gameObject.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            playerInRange = false;
    }
}
