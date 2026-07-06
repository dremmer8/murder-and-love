using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.E;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            CheckForInteraction();
        }
    }

    void CheckForInteraction()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
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