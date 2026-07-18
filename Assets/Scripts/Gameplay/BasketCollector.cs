using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BasketCollector : MonoBehaviour
{
    [SerializeField] float duration = 0.45f;
    [SerializeField] float archHeight = 0.35f;

    readonly Dictionary<string, BasketSlot> _slots = new();
    readonly Dictionary<Transform, Tween> _tweens = new();
    public static BasketCollector Instance;

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
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        foreach (var tween in _tweens.Values)
            tween?.Kill();
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

    public bool Collect(CollectibleItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.slotKey))
            return false;

        if (!TryGetSlot(item.slotKey, out var slot))
        {
            Debug.LogWarning($"{name}: no basket slot registered for key '{item.slotKey}'.", this);
            return false;
        }

        if (slot.IsOccupied)
            return false;

        var target = item.Animated;
        AnimateArc(target, () => (slot.transform.position, slot.transform.rotation), () => slot.Attach(target));
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
        if (destination == null || !TryGetSlot(slotKey, out var slot) || !slot.IsOccupied)
            return false;

        var item = slot.Detach();
        var dest = destination.transform;
        AnimateArc(item, () => (dest.position, dest.rotation), () => item.SetPositionAndRotation(dest.position, dest.rotation));
        return true;
    }

    void AnimateArc(Transform item, Func<(Vector3 pos, Quaternion rot)> getTarget, Action onComplete)
    {
        KillTween(item);

        Vector3 startPos = item.position;
        Quaternion startRot = item.rotation;

        _tweens[item] = DOVirtual.Float(0f, 1f, duration, t =>
        {
            var (endPos, endRot) = getTarget();
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += archHeight * 4f * t * (1f - t);
            item.SetPositionAndRotation(pos, Quaternion.Slerp(startRot, endRot, t));
        })
        .SetEase(Ease.OutCubic)
        .OnComplete(() =>
        {
            _tweens.Remove(item);
            onComplete?.Invoke();
        });
    }

    void KillTween(Transform item)
    {
        if (_tweens.TryGetValue(item, out var tween))
        {
            tween.Kill();
            _tweens.Remove(item);
        }
    }
}
