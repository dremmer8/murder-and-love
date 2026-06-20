using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueEntry
{
    [Tooltip("Text Assets of ink File")]
    public TextAsset inkFile;

    [Tooltip("The event gonna be happen after dialogue")]
    public UnityEvent onDialogueEnd;
}

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Custom Data/Dialogue Container")]
public class DialogueContainerSO : ScriptableObject
{
    [Header("Dialogue List")]
    [Tooltip("You can create dialogue as much you want")]
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();

    public void TriggerEventAtIndex(int index)
    {
        if (index >= 0 && index < dialogues.Count)
        {
            dialogues[index].onDialogueEnd?.Invoke();
        }
        else
        {
            Debug.LogWarning("invalid index " + index);
        }
    }
}