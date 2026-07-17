using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Intro")]
    [Tooltip("DialogueTrigger set to ExternalEvent + IntroSequence presentation.")]
    [SerializeField] private DialogueTrigger introTrigger;

    [Tooltip("Wait one frame so DialogueManager / presenters finish Start().")]
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
        if (introTrigger == null)
            introTrigger = FindIntroTrigger();

        if (introTrigger == null)
        {
            Debug.LogError(
                $"{name}: Intro DialogueTrigger not assigned and none found. " +
                "Create/select Intro_sequence and assign it on GameManager.",
                this);
            return;
        }

        // Ensure ExternalEvent + Intro presentation even if Inspector was left on defaults.
        introTrigger.ActivationMode = DialogueActivationMode.ExternalEvent;
        introTrigger.PresentationMode = DialoguePresentationMode.IntroSequence;

        if (GlobalVariableOperator.Instance != null
            && GlobalVariableOperator.Instance.GameProgression > 0)
        {
            Debug.LogWarning(
                $"{name}: gameProgression is {GlobalVariableOperator.Instance.GameProgression}. " +
                "Resetting to 0 so intro can start.",
                this);
            GlobalVariableOperator.Instance.GameProgression = 0;
        }

        if (!introTrigger.TryStartDialogue())
        {
            Debug.LogError(
                $"{name}: Failed to start intro. Check Console for DialogueManager / phases / ink warnings.",
                this);
        }
        else
        {
            Debug.Log($"{name}: Intro sequence started via {introTrigger.name}.", this);
        }
    }

    private static DialogueTrigger FindIntroTrigger()
    {
        DialogueTrigger[] triggers = FindObjectsByType<DialogueTrigger>(FindObjectsSortMode.None);
        for (int i = 0; i < triggers.Length; i++)
        {
            DialogueTrigger trigger = triggers[i];
            if (trigger == null)
                continue;

            if (trigger.PresentationMode == DialoguePresentationMode.IntroSequence)
                return trigger;

            if (trigger.name.IndexOf("intro", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return trigger;
        }

        return null;
    }
}
