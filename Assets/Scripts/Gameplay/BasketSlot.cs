using UnityEngine;

public class BasketSlot : MonoBehaviour
{
    public string key;
    public float maxOffset = 0.15f;
    public float maxTilt = 12f;
    public float impulse = 1.8f;
    public float returnSpeed = 5f;

    [Tooltip("If set, starts when an item finishes landing in this slot.")]
    public DialogueTrigger onOccupiedDialogue;

    Transform _item;
    Vector3 _restLocalPos;
    Quaternion _restLocalRot;
    Vector3 _prevWorldPos;
    Quaternion _prevWorldRot;
    Vector3 _offset;
    Vector3 _tilt;
    bool _occupiedDialogueFired;

    public bool IsOccupied => _item != null;

    void Awake()
    {
        _restLocalPos = transform.localPosition;
        _restLocalRot = transform.localRotation;
        CacheWorld();
    }

    void CacheWorld()
    {
        _prevWorldPos = transform.position;
        _prevWorldRot = transform.rotation;
    }

    public void Attach(Transform item)
    {
        _item = item;
        _offset = Vector3.zero;
        _tilt = Vector3.zero;
        CacheWorld();
        item.SetParent(transform, false);
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;

        if (!_occupiedDialogueFired && onOccupiedDialogue != null
            && onOccupiedDialogue.TryStartDialogue())
        {
            _occupiedDialogueFired = true;
        }
    }

    public Transform Detach()
    {
        var item = _item;
        _item = null;
        _offset = Vector3.zero;
        _tilt = Vector3.zero;
        if (item != null)
            item.SetParent(null, true);
        return item;
    }

    void LateUpdate()
    {
        transform.localPosition = _restLocalPos;
        transform.localRotation = _restLocalRot;

        Vector3 worldDelta = transform.position - _prevWorldPos;
        Quaternion worldRotDelta = transform.rotation * Quaternion.Inverse(_prevWorldRot);
        _prevWorldPos = transform.position;
        _prevWorldRot = transform.rotation;

        if (worldDelta.sqrMagnitude > 1e-8f)
        {
            _offset += transform.InverseTransformDirection(-worldDelta) * impulse;
            _offset = Vector3.ClampMagnitude(_offset, maxOffset);
        }

        Vector3 euler = worldRotDelta.eulerAngles;
        euler.x = Mathf.DeltaAngle(0f, euler.x);
        euler.y = Mathf.DeltaAngle(0f, euler.y);
        euler.z = Mathf.DeltaAngle(0f, euler.z);
        if (euler.sqrMagnitude > 0.001f)
        {
            _tilt += -euler * (maxTilt / 30f) * impulse;
            _tilt = Vector3.ClampMagnitude(_tilt, maxTilt);
        }

        _offset = Vector3.Lerp(_offset, Vector3.zero, Time.deltaTime * returnSpeed);
        _tilt = Vector3.Lerp(_tilt, Vector3.zero, Time.deltaTime * returnSpeed);

        transform.localPosition = _restLocalPos + _offset;
        transform.localRotation = _restLocalRot * Quaternion.Euler(_tilt);
    }
}
