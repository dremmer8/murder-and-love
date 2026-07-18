using UnityEngine;

/// <summary>
/// When a specific Ink knot ends, ForceActivates (or Activates) a referenced Interactable.
/// Drop on any scene object and assign Knot Name + Target in the Inspector.
/// </summary>
public class DialogueEndInteractableTrigger : MonoBehaviour
{
    [Header("Filter")]
    [Tooltip("Ink knot that must complete, e.g. Mandy_story_phase_4 for story phase 18.")]
    [SerializeField] string knotName = "Mandy_story_phase_4";

    [Header("Target")]
    [SerializeField] Interactable target;

    [Tooltip("If true, call ForceActivate (skips progression gates). Otherwise call Activate.")]
    [SerializeField] bool forceActivate = true;

    [Tooltip("If true, fire only once per play session.")]
    [SerializeField] bool playOnce = true;

    bool _subscribed;
    bool _fired;

    void OnEnable() => TrySubscribe();

    void Start() => TrySubscribe();

    void OnDisable() => Unsubscribe();

    void TrySubscribe()
    {
        if (_subscribed)
            return;

        DialogueManager manager = DialogueManager.GetInstance();
        if (manager == null)
            return;

        manager.OnDialogueEnded += HandleDialogueEnded;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed)
            return;

        DialogueManager manager = DialogueManager.GetInstance();
        if (manager != null)
            manager.OnDialogueEnded -= HandleDialogueEnded;

        _subscribed = false;
    }

    void HandleDialogueEnded(string completedKnot)
    {
        if (_fired && playOnce)
            return;

        if (string.IsNullOrEmpty(knotName) || string.IsNullOrEmpty(completedKnot))
            return;

        if (completedKnot != knotName)
            return;

        if (target == null)
        {
            Debug.LogWarning($"{name}: DialogueEndInteractableTrigger has no target Interactable.", this);
            return;
        }

        if (forceActivate)
            target.ForceActivate();
        else
            target.Activate();

        if (playOnce)
            _fired = true;
    }
}
