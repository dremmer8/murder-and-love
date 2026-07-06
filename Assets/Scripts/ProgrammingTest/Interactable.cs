using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { Animation, Transform }
    public InteractionType type;

    public Animator animator;
    public string animationTrigger;

    public Transform targetTransform;
    public float transitionSpeed = 10f;

    private bool isMoving = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Interact()
    {
        if (type == InteractionType.Animation)
        {
            if (animator != null && !string.IsNullOrEmpty(animationTrigger))
            {
                animator.SetTrigger(animationTrigger);
            }
        }
        else if (type == InteractionType.Transform)
        {
            if (targetTransform != null)
            {
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
                isMoving = true;
            }
        }
    }

    void Update()
    {
        if (isMoving && targetTransform != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation, Time.deltaTime * transitionSpeed);

            if (Vector3.Distance(transform.position, targetTransform.position) < 0.001f)
            {
                transform.position = targetTransform.position;
                transform.rotation = targetTransform.rotation;
                isMoving = false;
            }
        }
    }
}