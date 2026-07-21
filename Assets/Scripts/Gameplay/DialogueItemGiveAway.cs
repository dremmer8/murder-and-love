using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueItemGiveAwayEntry
{
    [Tooltip("Id passed from Ink: GiveAwayItem(\"change_coin_1\").")]
    public string itemId;

    [Tooltip("BasketSlot.key to empty, e.g. coin_act_2_1.")]
    public string slotKey;

    [Tooltip("Optional override destination. Uses DialogueItemGiveAway default when empty.")]
    public ItemDestination destination;

    [Tooltip("Seconds to wait after the Ink call before GiveBack starts.")]
    public float timeOffset = 0f;

    [Tooltip("If true, disable the item GameObject when the arc finishes.")]
    public bool hideOnArrive = true;

    [Tooltip("If true, this id only fires once per play session.")]
    public bool playOnce = true;

    [NonSerialized] public bool fired;
}

/// <summary>
/// Ink EXTERNAL GiveAwayItem(itemId): after each entry's time offset, calls
/// BasketCollector.GiveBack for that slot toward Mrs Wong (or a per-entry destination).
/// </summary>
public class DialogueItemGiveAway : MonoBehaviour
{
    public static DialogueItemGiveAway Instance { get; private set; }

    [Tooltip("Used when an entry has no destination override.")]
    [SerializeField] ItemDestination defaultDestination;

    [SerializeField] List<DialogueItemGiveAwayEntry> items = new();

    readonly Dictionary<string, Coroutine> _pending = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: more than one DialogueItemGiveAway in scene.", this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Ink EXTERNAL entry point.</summary>
    public void GiveAwayItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning($"{name}: GiveAwayItem called with empty id.", this);
            return;
        }

        DialogueItemGiveAwayEntry entry = FindEntry(itemId);
        if (entry == null)
        {
            Debug.LogWarning($"{name}: No DialogueItemGiveAway entry for id '{itemId}'.", this);
            return;
        }

        if (entry.playOnce && entry.fired)
            return;

        if (string.IsNullOrEmpty(entry.slotKey))
        {
            Debug.LogWarning($"{name}: Entry '{itemId}' has no slotKey.", this);
            return;
        }

        ItemDestination destination = entry.destination != null ? entry.destination : defaultDestination;
        if (destination == null)
        {
            Debug.LogWarning($"{name}: Entry '{itemId}' has no destination (and no default).", this);
            return;
        }

        if (_pending.TryGetValue(itemId, out Coroutine running) && running != null)
            StopCoroutine(running);

        _pending[itemId] = StartCoroutine(GiveAwayAfterDelay(entry, destination));
    }

    IEnumerator GiveAwayAfterDelay(DialogueItemGiveAwayEntry entry, ItemDestination destination)
    {
        float delay = Mathf.Max(0f, entry.timeOffset);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (BasketCollector.Instance == null)
        {
            Debug.LogWarning($"{name}: BasketCollector.Instance is null — cannot give away '{entry.itemId}'.", this);
            _pending.Remove(entry.itemId);
            yield break;
        }

        bool started = BasketCollector.Instance.GiveBack(
            entry.slotKey,
            destination,
            arrived =>
            {
                if (entry.hideOnArrive && arrived != null)
                    arrived.gameObject.SetActive(false);
            });

        if (!started)
        {
            Debug.LogWarning(
                $"{name}: GiveBack failed for '{entry.itemId}' (slot '{entry.slotKey}' empty or busy?).",
                this);
        }
        else
        {
            entry.fired = true;
            SoundManager.PlayOneShot("getItem", destination.transform.position);
        }

        _pending.Remove(entry.itemId);
    }

    DialogueItemGiveAwayEntry FindEntry(string itemId)
    {
        if (items == null)
            return null;

        for (int i = 0; i < items.Count; i++)
        {
            DialogueItemGiveAwayEntry entry = items[i];
            if (entry != null && entry.itemId == itemId)
                return entry;
        }

        return null;
    }
}
