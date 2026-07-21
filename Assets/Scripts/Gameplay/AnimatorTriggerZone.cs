using UnityEngine;

/// <summary>
/// On player enter, fires a named trigger on a target <see cref="Animator"/>.
/// Requires a trigger collider on this object (or a child that shares this component).
/// </summary>
public class AnimatorTriggerZone : MonoBehaviour
{
    [SerializeField] Animator animator;

    [Tooltip("Animator trigger parameter name.")]
    [SerializeField] string triggerName;

    [Tooltip("If true, only fires the first time the player enters.")]
    [SerializeField] bool fireOnce = true;

    bool hasFired;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (fireOnce && hasFired)
            return;

        if (animator == null || string.IsNullOrEmpty(triggerName))
            return;

        animator.SetTrigger(triggerName);
        hasFired = true;
    }
}
