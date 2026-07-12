using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WashingMachineAnimator : MonoBehaviour
{
    public GameObject token;
    public GameObject lightBulbWorking;
    public Animator animator;

    public void HideToken()
    {
        token.SetActive(false);
    }

    public void ShowLightBulbWorking()
    {
        lightBulbWorking.SetActive(true);
        animator.SetTrigger("DoWork");
    }
}
