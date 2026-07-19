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

    [Header("Basket Slot Gate")]
    [Tooltip("If enabled, the named basket slot must be occupied before this can be interacted with.")]
    [SerializeField] private bool requireBasketSlotOccupied = false;

    [Tooltip("BasketSlot.key that must be occupied, e.g. Token_act_1 or Key_act_1.")]
    [SerializeField] private string requiredBasketSlotKey = "";

    private bool isMoving = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// True when progression and optional basket-slot gates allow interaction.
    /// </summary>
    public bool CanInteract()
    {
        if (!PassesProgressionGate())
            return false;

        if (!PassesBasketSlotGate())
            return false;

        return true;
    }

    bool PassesProgressionGate()
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

    bool PassesBasketSlotGate()
    {
        if (!requireBasketSlotOccupied)
            return true;

        if (string.IsNullOrEmpty(requiredBasketSlotKey))
            return false;

        if (BasketCollector.Instance == null)
            return false;

        return BasketCollector.Instance.IsSlotOccupied(requiredBasketSlotKey);
    }

    /// <summary>
    /// Player look+E path. Respects progression gates.
    /// </summary>
    public void Interact() => TryInteract(respectGates: true);

    /// <summary>
    /// External / UnityEvent entry point. Respects progression gates.
    /// </summary>
    public void Activate() => TryInteract(respectGates: true);

    /// <summary>
    /// External call that ignores progression and basket-slot gates (scripted sequences).
    /// </summary>
    public void ForceActivate() => TryInteract(respectGates: false);

    public bool TryInteract(bool respectGates = true)
    {
        if (respectGates && !CanInteract())
            return false;

        if (type == InteractionType.Animation)
        {
            if (animator != null && !string.IsNullOrEmpty(animationTrigger))
                animator.SetTrigger(animationTrigger);
        }
        else if (type == InteractionType.Transform)
        {
            if (targetTransform != null)
            {
                if (rb != null)
                    rb.isKinematic = true;
                isMoving = true;
            }
        }
        else if (type == InteractionType.Pickup)
        {
            if (BasketCollector.Instance == null)
            {
                Debug.LogWarning($"{name}: BasketCollector.Instance is null (is the basket active in the scene?).", this);
                return false;
            }

            CollectibleItem collectible = GetComponent<CollectibleItem>();
            if (collectible == null)
            {
                Debug.LogWarning($"{name}: Pickup interactable has no CollectibleItem component.", this);
                return false;
            }

            return BasketCollector.Instance.Collect(collectible);
        }
        else if (type == InteractionType.Event)
        {
            onInteract?.Invoke();
        }

        return true;
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
