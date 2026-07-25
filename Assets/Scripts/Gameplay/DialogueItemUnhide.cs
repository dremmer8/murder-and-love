using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueItemUnhideEntry
{
    [Tooltip("Id passed from Ink: UnhideItem(\"first_laundry_coin\").")]
    public string itemId;

    [Tooltip("Object to SetActive(true). Leave inactive in the scene until this fires.")]
    public GameObject target;

    [Tooltip("Seconds to wait after the Ink call (i.e. after the line starts) before unhiding.")]
    public float timeOffset = 0.5f;

    [Tooltip("Optional hand-held prop on Mandy's skeleton. Active for the give-item anim, then hidden.")]
    public GameObject handProp;

    [Tooltip("How long the hand prop stays visible (match M_stand_give_item_1 length).")]
    public float handPropDuration = 9.33f;

    [Tooltip("If true, this id only unhides once per play session.")]
    public bool playOnce = true;

    [NonSerialized] public bool fired;
}

/// <summary>
/// Ink EXTERNAL UnhideItem(itemId): after each entry's time offset, activates its target.
/// Optional handProp activates immediately (give-item anim start) and hides after handPropDuration.
/// Wire three entries for: first laundry coin, backroom key, second laundry coin.
/// </summary>
public class DialogueItemUnhide : MonoBehaviour
{
    public static DialogueItemUnhide Instance { get; private set; }

    [SerializeField] List<DialogueItemUnhideEntry> items = new();

    readonly Dictionary<string, Coroutine> _pending = new();
    readonly Dictionary<string, Coroutine> _handPropHide = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: more than one DialogueItemUnhide in scene.", this);
            return;
        }

        Instance = this;
        HideAllHandProps();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Ink EXTERNAL entry point. Uses the matching entry's <see cref="DialogueItemUnhideEntry.timeOffset"/>.
    /// </summary>
    public void UnhideItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning($"{name}: UnhideItem called with empty id.", this);
            return;
        }

        DialogueItemUnhideEntry entry = FindEntry(itemId);
        if (entry == null)
        {
            Debug.LogWarning($"{name}: No DialogueItemUnhide entry for id '{itemId}'.", this);
            return;
        }

        if (entry.playOnce && entry.fired)
            return;

        if (entry.target == null && entry.handProp == null)
        {
            Debug.LogWarning($"{name}: Entry '{itemId}' has no target or handProp GameObject.", this);
            return;
        }

        if (_pending.TryGetValue(itemId, out Coroutine running) && running != null)
            StopCoroutine(running);

        ShowHandProp(entry);
        _pending[itemId] = StartCoroutine(UnhideAfterDelay(entry));
    }

    void ShowHandProp(DialogueItemUnhideEntry entry)
    {
        if (entry.handProp == null)
            return;

        entry.handProp.SetActive(true);

        if (_handPropHide.TryGetValue(entry.itemId, out Coroutine hiding) && hiding != null)
            StopCoroutine(hiding);

        _handPropHide[entry.itemId] = StartCoroutine(HideHandPropAfter(entry));
    }

    IEnumerator HideHandPropAfter(DialogueItemUnhideEntry entry)
    {
        float delay = Mathf.Max(0.1f, entry.handPropDuration);
        yield return new WaitForSeconds(delay);

        if (entry.handProp != null)
            entry.handProp.SetActive(false);

        _handPropHide.Remove(entry.itemId);
    }

    IEnumerator UnhideAfterDelay(DialogueItemUnhideEntry entry)
    {
        float delay = Mathf.Max(0f, entry.timeOffset);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (entry.target != null)
        {
            entry.target.SetActive(true);

            // If this visual lives under a basket slot, occupy the slot so GiveBack works.
            BasketSlot slot = entry.target.GetComponentInParent<BasketSlot>();
            if (slot != null && !slot.IsOccupied)
                slot.Attach(entry.target.transform);
        }

        entry.fired = true;
        _pending.Remove(entry.itemId);
    }

    void HideAllHandProps()
    {
        if (items == null)
            return;

        for (int i = 0; i < items.Count; i++)
        {
            DialogueItemUnhideEntry entry = items[i];
            if (entry?.handProp != null)
                entry.handProp.SetActive(false);
        }
    }

    DialogueItemUnhideEntry FindEntry(string itemId)
    {
        if (items == null)
            return null;

        for (int i = 0; i < items.Count; i++)
        {
            DialogueItemUnhideEntry entry = items[i];
            if (entry != null && entry.itemId == itemId)
                return entry;
        }

        return null;
    }
}
