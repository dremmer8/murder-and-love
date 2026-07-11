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
    {BasketCollector.Instance = this;
        foreach (var slot in GetComponentsInChildren<BasketSlot>())
            _slots[slot.key] = slot;
    }

    void OnDestroy()
    {
        foreach (var tween in _tweens.Values)
            tween?.Kill();
    }

    public bool IsSlotFree(string key) =>
        _slots.TryGetValue(key, out var slot) && !slot.IsOccupied;

    public bool Collect(CollectibleItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.slotKey))
            return false;
        if (!_slots.TryGetValue(item.slotKey, out var slot) || slot.IsOccupied)
            return false;

        var target = item.Animated;
        AnimateArc(target, () => (slot.transform.position, slot.transform.rotation), () => slot.Attach(target));
        return true;
    }

    public bool GiveBack(string slotKey, ItemDestination destination)
    {
        if (destination == null || !_slots.TryGetValue(slotKey, out var slot) || !slot.IsOccupied)
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
