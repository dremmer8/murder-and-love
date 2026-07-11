using UnityEngine;

public class BasketSlot : MonoBehaviour
{
    public string key;
    public float maxOffset = 0.08f;
    public float maxTilt = 8f;
    public float smooth = 12f;
    public float returnSpeed = 6f;

    Transform _item;
    Vector3 _prevPos;
    Vector3 _offset;
    Vector3 _tilt;

    public bool IsOccupied => _item != null;

    void Start() => _prevPos = transform.position;

    public void Attach(Transform item)
    {
        _item = item;
        _offset = Vector3.zero;
        _tilt = Vector3.zero;
        item.SetParent(transform, false);
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;
    }

    public Transform Detach()
    {
        var item = _item;
        _item = null;
        if (item != null)
            item.SetParent(null, true);
        return item;
    }

    void LateUpdate()
    {
        if (_item == null)
        {
            _prevPos = transform.position;
            return;
        }

        Vector3 delta = transform.position - _prevPos;
        _prevPos = transform.position;

        if (delta.sqrMagnitude > 1e-6f)
        {
            Vector3 localDelta = transform.InverseTransformDirection(-delta);
            _offset = Vector3.ClampMagnitude(_offset + localDelta, maxOffset);
            _tilt = Vector3.ClampMagnitude(_tilt + new Vector3(localDelta.z, 0f, -localDelta.x) * (maxTilt / maxOffset), maxTilt);
        }

        _offset = Vector3.Lerp(_offset, Vector3.zero, Time.deltaTime * returnSpeed);
        _tilt = Vector3.Lerp(_tilt, Vector3.zero, Time.deltaTime * returnSpeed);

        _item.localPosition = Vector3.Lerp(_item.localPosition, _offset, Time.deltaTime * smooth);
        _item.localRotation = Quaternion.Slerp(_item.localRotation, Quaternion.Euler(_tilt), Time.deltaTime * smooth);
    }
}
