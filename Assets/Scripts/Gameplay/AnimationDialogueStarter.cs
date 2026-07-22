using UnityEngine;

/// <summary>
/// Drop on the same GameObject as an <see cref="Animator"/>. Wire Animation Events to
/// <see cref="StartDialogue"/> (or the knot overload) to fire a referenced
/// <see cref="DialogueTrigger"/> in <see cref="DialogueActivationMode.ExternalEvent"/> mode.
/// </summary>
public class AnimationDialogueStarter : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Dialogue to start from the Animation Event.")]
    DialogueTrigger dialogueTrigger;

    [SerializeField]
    [Tooltip("If true, sets the trigger to ExternalEvent mode before starting.")]
    bool setExternalEventMode = true;

    /// <summary>
    /// Animation Event target: optionally set ExternalEvent mode, then start dialogue.
    /// </summary>
    public void StartDialogue()
    {
        if (!TryPrepareTrigger())
            return;

        if (!dialogueTrigger.TryStartDialogue())
            Debug.LogWarning($"{name}: Failed to start dialogue on '{dialogueTrigger.name}'.", this);
    }

    /// <summary>
    /// Animation Event target with a knot name string parameter.
    /// </summary>
    public void StartDialogue(string knotName)
    {
        if (!TryPrepareTrigger())
            return;

        if (!dialogueTrigger.TryStartDialogue(knotName))
            Debug.LogWarning(
                $"{name}: Failed to start dialogue knot '{knotName}' on '{dialogueTrigger.name}'.",
                this);
    }

    bool TryPrepareTrigger()
    {
        if (dialogueTrigger == null)
        {
            Debug.LogWarning($"{name}: AnimationDialogueStarter has no DialogueTrigger assigned.", this);
            return false;
        }

        if (setExternalEventMode)
            dialogueTrigger.ActivationMode = DialogueActivationMode.ExternalEvent;

        return true;
    }
}
