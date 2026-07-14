using System.Collections;
using UnityEngine;

public class CoinMachineOperator : MonoBehaviour
{
    enum Step { Fail1, Fail2, Success, Done }

    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Collider billSlit;
    [SerializeField] float failAnimDuration = 14f;
    [SerializeField] float successAnimDuration = 9f;

    Step step = Step.Fail1;
    bool busy;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (busy || step == Step.Done || !animator) return;
        if (!Input.GetMouseButtonDown(0)) return;
        TryBillSlitClick();
    }

    bool TryBillSlitClick()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 200f)) return false;
        if (hit.collider != billSlit) return false;

        switch (step)
        {
            case Step.Fail1:
                StartCoroutine(PlayFailThen(Step.Fail2));
                return true;
            case Step.Fail2:
                StartCoroutine(PlayFailThen(Step.Success));
                return true;
            case Step.Success:
                StartCoroutine(PlaySuccessAndEnd());
                return true;
        }

        return true;
    }

    IEnumerator PlayFailThen(Step next)
    {
        busy = true;
        animator.SetTrigger("fail");
        yield return new WaitForSeconds(failAnimDuration);
        step = next;
        busy = false;
    }

    IEnumerator PlaySuccessAndEnd()
    {
        busy = true;
        animator.SetTrigger("win");
        yield return new WaitForSeconds(successAnimDuration);
        animator.SetTrigger("end");
        step = Step.Done;
        busy = false;
    }
}
