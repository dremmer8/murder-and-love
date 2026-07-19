using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class BasketPhaseCleanupEntry
{
    [Tooltip("Applies when game_progression >= this value.")]
    public int storyPhase;

    [Tooltip("Enabled when this phase threshold is reached.")]
    public List<GameObject> objectsToActivate = new();

    [Tooltip("Disabled when this phase threshold is reached.")]
    public List<GameObject> objectsToDeactivate = new();
}

public class BasketCollector : MonoBehaviour
{
    [SerializeField] float duration = 0.45f;
    [SerializeField] float archHeight = 0.35f;

    [Header("Phase Cleanup")]
    [SerializeField] List<BasketPhaseCleanupEntry> phaseCleanups = new();

    readonly Dictionary<string, BasketSlot> _slots = new();
    /// <summary>One independent flight tween per animated transform (concurrent collects).</summary>
    readonly Dictionary<Transform, Tween> _tweens = new();
    public static BasketCollector Instance;

    int _lastSeenProgression = int.MinValue;
    readonly HashSet<int> _appliedCleanupIndices = new();

    void Awake()
    {
        Instance = this;
        RescanSlots();
    }

    void OnEnable()
    {
        // Covers the case where this collector was inactive at scene start.
        if (Instance == null)
            Instance = this;
        RescanSlots();
        TryApplyPhaseCleanups();
    }

    void Start()
    {
        TryApplyPhaseCleanups();
    }

    void Update()
    {
        int progression = CurrentProgression();
        if (progression == _lastSeenProgression)
            return;

        TryApplyPhaseCleanups();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        // Copy first — OnKill mutates _tweens.
        var active = new List<Tween>(_tweens.Values);
        _tweens.Clear();
        for (int i = 0; i < active.Count; i++)
            active[i]?.Kill(false);
    }

    void TryApplyPhaseCleanups()
    {
        int progression = CurrentProgression();
        _lastSeenProgression = progression;

        if (phaseCleanups == null || phaseCleanups.Count == 0)
            return;

        for (int i = 0; i < phaseCleanups.Count; i++)
        {
            if (_appliedCleanupIndices.Contains(i))
                continue;

            BasketPhaseCleanupEntry entry = phaseCleanups[i];
            if (entry == null || progression < entry.storyPhase)
                continue;

            ApplyCleanupEntry(entry);
            _appliedCleanupIndices.Add(i);
        }
    }

    static void ApplyCleanupEntry(BasketPhaseCleanupEntry entry)
    {
        SetActiveList(entry.objectsToActivate, true);
        SetActiveList(entry.objectsToDeactivate, false);
    }

    static void SetActiveList(List<GameObject> list, bool active)
    {
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
                list[i].SetActive(active);
        }
    }

    static int CurrentProgression()
    {
        return GlobalVariableOperator.Instance != null
            ? GlobalVariableOperator.Instance.GameProgression
            : 0;
    }

    /// <summary>
    /// Rebuilds the slot map from every BasketSlot in the scene,
    /// including inactive objects and slots that are not children of this collector.
    /// </summary>
    public void RescanSlots()
    {
        _slots.Clear();

#if UNITY_2023_1_OR_NEWER
        BasketSlot[] found = FindObjectsByType<BasketSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        BasketSlot[] found = FindObjectsOfType<BasketSlot>(true);
#endif
        for (int i = 0; i < found.Length; i++)
            RegisterSlot(found[i]);
    }

    /// <summary>Called by <see cref="BasketSlot"/> so activation order no longer matters.</summary>
    public void RegisterSlot(BasketSlot slot)
    {
        if (slot == null || string.IsNullOrEmpty(slot.key))
            return;

        _slots[slot.key] = slot;
    }

    public void UnregisterSlot(BasketSlot slot)
    {
        if (slot == null || string.IsNullOrEmpty(slot.key))
            return;

        if (_slots.TryGetValue(slot.key, out var existing) && existing == slot)
            _slots.Remove(slot.key);
    }

    public bool IsSlotFree(string key) =>
        _slots.TryGetValue(key, out var slot) && slot != null && !slot.IsOccupied;

    public bool IsSlotOccupied(string key) =>
        TryGetSlot(key, out var slot) && slot != null && slot.IsOccupied;

    public bool Collect(CollectibleItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.slotKey))
            return false;

        if (!TryGetSlot(item.slotKey, out var slot))
        {
            Debug.LogWarning($"{name}: no basket slot registered for key '{item.slotKey}'.", this);
            return false;
        }

        var target = item.Animated;
        if (target == null || IsAnimating(target))
            return false;

        if (!slot.TryReserve())
            return false;

        ClaimForFlight(item, target);

        // Slight per-flight variation so parallel arcs stay visually distinct.
        float flightArch = archHeight * UnityEngine.Random.Range(0.85f, 1.2f);
        Vector3 lateral = UnityEngine.Random.insideUnitSphere * (archHeight * 0.35f);
        lateral.y = 0f;

        AnimateArc(
            target,
            () => (slot.transform.position, slot.transform.rotation),
            onComplete: () => slot.Attach(target),
            onInterrupted: () => slot.CancelReserve(),
            flightArch,
            lateral);
        return true;
    }

    bool TryGetSlot(string key, out BasketSlot slot)
    {
        if (_slots.TryGetValue(key, out slot) && slot != null)
            return true;

        // Fallback: slots may live outside this hierarchy or appear after Awake.
        RescanSlots();
        return _slots.TryGetValue(key, out slot) && slot != null;
    }

    public bool GiveBack(string slotKey, ItemDestination destination)
    {
        if (destination == null || !TryGetSlot(slotKey, out var slot))
            return false;

        // Detach only succeeds when an item has finished landing (not merely reserved).
        var item = slot.Detach();
        if (item == null || IsAnimating(item))
            return false;

        var dest = destination.transform;
        AnimateArc(
            item,
            () => (dest.position, dest.rotation),
            onComplete: () => item.SetPositionAndRotation(dest.position, dest.rotation),
            onInterrupted: null,
            archHeight,
            Vector3.zero);
        return true;
    }

    bool IsAnimating(Transform item) =>
        item != null && _tweens.ContainsKey(item);

    static void ClaimForFlight(CollectibleItem item, Transform target)
    {
        // World-space flight so parent motion cannot yank other in-flight items.
        target.SetParent(null, true);

        if (item != null)
        {
            var interactable = item.GetComponent<Interactable>();
            if (interactable != null)
                interactable.enabled = false;

            var col = item.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
        }

        var targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.isKinematic = true;
            targetRb.detectCollisions = false;
        }
    }

    void AnimateArc(
        Transform item,
        Func<(Vector3 pos, Quaternion rot)> getTarget,
        Action onComplete,
        Action onInterrupted,
        float flightArch,
        Vector3 lateralOffset)
    {
        KillTween(item);

        Vector3 startPos = item.position;
        Quaternion startRot = item.rotation;
        bool completed = false;

        Tween tween = DOVirtual.Float(0f, 1f, duration, t =>
        {
            if (item == null)
                return;

            var (endPos, endRot) = getTarget();
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            // Parabola + sideways bulge so concurrent arcs do not share one path.
            float archT = 4f * t * (1f - t);
            pos.y += flightArch * archT;
            pos += lateralOffset * archT;
            item.SetPositionAndRotation(pos, Quaternion.Slerp(startRot, endRot, t));
        })
        .SetEase(Ease.OutCubic)
        .SetId(item)
        .OnComplete(() =>
        {
            completed = true;
            _tweens.Remove(item);
            onComplete?.Invoke();
        })
        .OnKill(() =>
        {
            _tweens.Remove(item);
            if (!completed)
                onInterrupted?.Invoke();
        });

        _tweens[item] = tween;
    }

    void KillTween(Transform item)
    {
        if (item == null || !_tweens.TryGetValue(item, out var tween))
            return;

        // OnKill removes from _tweens and runs onInterrupted when not completed.
        tween.Kill(false);
    }
}
