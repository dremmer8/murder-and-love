using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DialogueTrigger introTrigger;
    [SerializeField] private bool delayOneFrame = true;

    private void Start()
    {
        if (delayOneFrame)
            StartCoroutine(StartIntroNextFrame());
        else
            StartIntroSequence();
    }

    private IEnumerator StartIntroNextFrame()
    {
        yield return null;
        StartIntroSequence();
    }

    public void StartIntroSequence()
    {
        if (introTrigger == null
            || GlobalVariableOperator.Instance == null
            || GlobalVariableOperator.Instance.GameProgression != 0)
            return;

        introTrigger.ActivationMode = DialogueActivationMode.ExternalEvent;
        introTrigger.PresentationMode = DialoguePresentationMode.IntroSequence;
        introTrigger.TryStartDialogue();
    }
}
