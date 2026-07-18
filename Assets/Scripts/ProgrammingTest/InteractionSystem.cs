using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public static InteractionSystem Instance { get; private set; }

    public Camera playerCamera;
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.E;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("InteractionSystem: more than one instance in scene.");
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            CheckForInteraction();
        }
    }

    void CheckForInteraction()
    {
        if (TryGetAimedInteractable(out Interactable interactable))
            interactable.Interact();
    }

    /// <summary>
    /// True when the crosshair ray hits an <see cref="Interactable"/> within interaction distance.
    /// Used by <see cref="DialogueTrigger"/> so KeyPress zones do not steal E from look-targeted objects.
    /// </summary>
    public bool TryGetAimedInteractable(out Interactable interactable)
    {
        interactable = null;

        if (playerCamera == null)
            return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
            return false;

        interactable = hit.collider.GetComponentInParent<Interactable>();
        return interactable != null;
    }
    
    public InteractionSystem interactionSystem;
    public Color gizmoColor = Color.green;

    void OnDrawGizmos()
    {
        if (interactionSystem == null || interactionSystem.playerCamera == null)
        {
            return;
        }

        Gizmos.color = gizmoColor;
        
        Vector3 rayOrigin = interactionSystem.playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Vector3 rayDirection = interactionSystem.playerCamera.transform.forward;

        Gizmos.DrawRay(rayOrigin, rayDirection * interactionSystem.interactionDistance);
        Gizmos.DrawWireSphere(rayOrigin + rayDirection * interactionSystem.interactionDistance, 0.05f);
    }
}