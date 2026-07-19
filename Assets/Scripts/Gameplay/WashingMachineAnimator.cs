using UnityEngine;

public class WashingMachineAnimator : MonoBehaviour
{
    public GameObject token;
    public GameObject lightBulbWorking;
    public Animator animator;

    [Tooltip("Preferred: drives isWorking through DoWorkTrigger (respects blackout).")]
    [SerializeField] DoWorkTrigger doWorkTrigger;

    [Tooltip("Fallback animator bool if no DoWorkTrigger is assigned.")]
    [SerializeField] string isWorkingParam = "isWorking";

    void Awake()
    {
        if (!doWorkTrigger)
            doWorkTrigger = GetComponent<DoWorkTrigger>()
                ?? GetComponentInParent<DoWorkTrigger>()
                ?? GetComponentInChildren<DoWorkTrigger>();
    }

    public void HideToken()
    {
        token.SetActive(false);
    }

    public void ShowLightBulbWorking()
    {
        lightBulbWorking.SetActive(true);
        SetWorking(true);
    }

    public void SetWorking(bool working)
    {
        if (doWorkTrigger != null)
        {
            doWorkTrigger.SetWorking(working);
            return;
        }

        if (animator)
            animator.SetBool(isWorkingParam, working);
    }
}
