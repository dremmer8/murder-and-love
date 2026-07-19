using UnityEngine;

public class DoWorkTrigger : MonoBehaviour
{
    [SerializeField] Animator animator;
    [Tooltip("If true, fires DoWork when this object starts.")]
    [SerializeField] bool playOnStart;

    void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (playOnStart)
            DoWork();
    }

    /// <summary>Call from other scripts or UnityEvents.</summary>
    public void DoWork()
    {
        if (!animator) return;
        animator.SetTrigger("DoWork");
    }
}
