using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public static InteractionSystem Instance { get; private set; }

    public Camera playerCamera;
    public float interactionDistance = 3f;

    [Tooltip("Only hits the Interactable layer. Leave empty to use GameLayers.InteractableMask.")]
    public LayerMask interactableLayer;

    [Tooltip("Only hits the DialogueZone layer. Leave empty to use GameLayers.DialogueZoneMask.")]
    public LayerMask dialogueLayer;

    public KeyCode interactKey = KeyCode.E;

    /// <summary>
    /// When both layers are hit within this distance of each other, prefer dialogue
    /// so similarly-sized overlapping colliders do not let Interactable blanket DialogueZone.
    /// </summary>
    const float OverlapTieEpsilon = 0.05f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("InteractionSystem: more than one instance in scene.");
            return;
        }

        Instance = this;

        if (interactableLayer == 0)
            interactableLayer = GameLayers.InteractableMask;

        if (dialogueLayer == 0)
            dialogueLayer = GameLayers.DialogueZoneMask;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
            CheckForInteraction();
    }

    void CheckForInteraction()
    {
        bool hasInteractable = TryGetAimedInteractable(out Interactable interactable, out float interactableDist);
        bool hasDialogue = TryGetAimedDialogueTrigger(out DialogueTrigger dialogue, out float dialogueDist);

        if (hasInteractable && hasDialogue)
        {
            // Closer collider wins. Near-ties go to dialogue so similar overlaps stay talkable.
            if (dialogueDist <= interactableDist + OverlapTieEpsilon)
            {
                dialogue.TryStartDialogue();
                return;
            }

            interactable.Interact();
            return;
        }

        if (hasInteractable)
        {
            interactable.Interact();
            return;
        }

        if (hasDialogue)
            dialogue.TryStartDialogue();
    }

    /// <summary>
    /// True when the crosshair ray hits a usable <see cref="Interactable"/> within interaction distance.
    /// Used by <see cref="DialogueTrigger"/> so KeyPress zones do not steal E from look-targeted objects.
    /// </summary>
    public bool TryGetAimedInteractable(out Interactable interactable)
        => TryGetAimedInteractable(out interactable, out _);

    public bool TryGetAimedInteractable(out Interactable interactable, out float hitDistance)
    {
        interactable = null;
        hitDistance = float.PositiveInfinity;

        if (playerCamera == null)
            return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer,
                QueryTriggerInteraction.Collide))
            return false;

        interactable = hit.collider.GetComponentInParent<Interactable>();
        if (interactable == null || !interactable.CanInteract())
        {
            interactable = null;
            return false;
        }

        hitDistance = hit.distance;
        return true;
    }

    /// <summary>
    /// True when the crosshair ray hits a <see cref="DialogueTrigger"/> on the DialogueZone layer.
    /// </summary>
    public bool TryGetAimedDialogueTrigger(out DialogueTrigger dialogue)
        => TryGetAimedDialogueTrigger(out dialogue, out _);

    public bool TryGetAimedDialogueTrigger(out DialogueTrigger dialogue, out float hitDistance)
    {
        dialogue = null;
        hitDistance = float.PositiveInfinity;

        if (playerCamera == null)
            return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, dialogueLayer,
                QueryTriggerInteraction.Collide))
            return false;

        dialogue = hit.collider.GetComponentInParent<DialogueTrigger>();
        if (dialogue == null)
            return false;

        hitDistance = hit.distance;
        return true;
    }

    public InteractionSystem interactionSystem;
    public Color gizmoColor = Color.green;

    void OnDrawGizmos()
    {
        if (interactionSystem == null || interactionSystem.playerCamera == null)
            return;

        Gizmos.color = gizmoColor;

        Vector3 rayOrigin = interactionSystem.playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Vector3 rayDirection = interactionSystem.playerCamera.transform.forward;

        Gizmos.DrawRay(rayOrigin, rayDirection * interactionSystem.interactionDistance);
        Gizmos.DrawWireSphere(rayOrigin + rayDirection * interactionSystem.interactionDistance, 0.05f);
    }
}
