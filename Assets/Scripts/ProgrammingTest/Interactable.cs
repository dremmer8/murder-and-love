using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { Animation, Transform, Pickup, Event }
    public InteractionType type;

    public Animator animator;
    public string animationTrigger;

    public Transform targetTransform;
    public float transitionSpeed = 10f;

    [Tooltip("Used when type is Event. Wire MinigameActivator.Activate here, etc.")]
    public UnityEvent onInteract;

    [Header("Progression Gate")]
    [Tooltip("If enabled, game_progression must be >= Min Story Phase.")]
    [SerializeField] private bool useMinStoryPhase = false;

    [SerializeField] private int minStoryPhase = 0;

    [Tooltip("If enabled, game_progression must be <= Max Story Phase.")]
    [SerializeField] private bool useMaxStoryPhase = false;

    [SerializeField] private int maxStoryPhase = 0;

    private bool isMoving = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// True when current game_progression is within the optional min/max range.
    /// </summary>
    public bool CanInteract()
    {
        if (!useMinStoryPhase && !useMaxStoryPhase)
            return true;

        int progression = GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;

        if (useMinStoryPhase && progression < minStoryPhase)
            return false;

        if (useMaxStoryPhase && progression > maxStoryPhase)
            return false;

        return true;
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

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
        else if (type == InteractionType.Pickup)
        {
            if (BasketCollector.Instance != null)
            {
                BasketCollector.Instance.Collect(GetComponent<CollectibleItem>());
            }
        }
        else if (type == InteractionType.Event)
        {
            onInteract?.Invoke();
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
