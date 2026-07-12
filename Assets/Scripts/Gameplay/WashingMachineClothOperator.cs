using UnityEngine;

public class WashingMachineClothOperator : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Collider basketZone, barrelZone;
    [SerializeField] Transform[] cloths, destinations, track;
    [SerializeField] float follow = 14f, settle = 10f, barrelT = 0.88f, directionSpeed = 0.8f;
    [SerializeField] string directionParam = "direction";

    int idx = -1, done;
    float t, tVel, direction, directionVel;
    Vector3 posVel;
    float totalLen;
    float[] segLen;
    Plane plane;
    bool snapping;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (animator) direction = animator.GetFloat(directionParam);
        RebuildTrack();
    }

    void OnValidate() => RebuildTrack();

    void RebuildTrack()
    {
        if (track == null || track.Length < 2) return;
        segLen = new float[track.Length - 1];
        totalLen = 0f;
        for (int i = 0; i < segLen.Length; i++)
            totalLen += segLen[i] = Vector3.Distance(track[i].position, track[i + 1].position);
        plane = new Plane(Vector3.up, track[track.Length / 2].position);
    }

    void Update()
    {
        UpdateDirection();
        if (snapping) { Snap(); return; }
        if (Input.GetMouseButtonDown(0)) TryGrab();
        if (idx < 0) return;
        if (Input.GetMouseButton(0)) Drag();
        else Release();
    }

    void UpdateDirection()
    {
        if (!animator) return;
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input -= 1f;
        if (Input.GetKey(KeyCode.D)) input += 1f;
        if (input != 0f)
            direction = Mathf.SmoothDamp(direction, input, ref directionVel, 1f / directionSpeed);
        else
            directionVel = 0f;
        animator.SetFloat(directionParam, direction);
    }

    void TryGrab()
    {
        if (done >= cloths.Length || idx >= 0) return;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 200f)) return;
        var c = cloths[done];
        if (hit.collider != basketZone && hit.transform != c && !hit.transform.IsChildOf(c)) return;
        idx = done;
        t = 0f;
        tVel = 0f;
        posVel = Vector3.zero;
        c.SetParent(null, true);
    }

    void Drag()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!plane.Raycast(ray, out var e)) return;
        float st = 1f / follow;
        float target = ClosestT(ray.GetPoint(e));
        t = Mathf.SmoothDamp(t, target, ref tVel, st);
        DampTo(cloths[idx], SamplePos(t), SampleRot(t), st);
    }

    void DampTo(Transform tr, Vector3 pos, Quaternion rot, float smoothTime)
    {
        tr.position = Vector3.SmoothDamp(tr.position, pos, ref posVel, smoothTime);
        tr.rotation = Quaternion.Slerp(tr.rotation, rot, 1f - Mathf.Exp(-Time.deltaTime / smoothTime));
    }

    void Release()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        bool ok = t >= barrelT || (barrelZone && barrelZone.Raycast(ray, out _, 200f));
        if (ok) snapping = true;
        else { cloths[idx].SetPositionAndRotation(track[0].position, track[0].rotation); idx = -1; posVel = Vector3.zero; }
    }

    void Snap()
    {
        var tr = cloths[idx];
        var dst = destinations[idx];
        float st = 1f / settle;
        DampTo(tr, dst.position, dst.rotation, st);
        if ((tr.position - dst.position).sqrMagnitude > 1e-4f || Quaternion.Angle(tr.rotation, dst.rotation) > 0.5f) return;
        tr.SetPositionAndRotation(dst.position, dst.rotation);
        tr.SetParent(dst, true);
        done++;
        idx = -1;
        posVel = Vector3.zero;
        snapping = false;
    }

    float ClosestT(Vector3 p)
    {
        float best = 0f, dist = float.MaxValue, walked = 0f;
        for (int i = 0; i < segLen.Length; i++)
        {
            Vector3 a = track[i].position, b = track[i + 1].position;
            Vector3 ab = b - a;
            float u = ab.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            float d = (p - (a + ab * u)).sqrMagnitude;
            if (d < dist) { dist = d; best = (walked + segLen[i] * u) / totalLen; }
            walked += segLen[i];
        }
        return best;
    }

    Vector3 SamplePos(float u)
    {
        float d = u * totalLen;
        for (int i = 0; i < segLen.Length; i++)
        {
            if (d > segLen[i]) { d -= segLen[i]; continue; }
            return Vector3.Lerp(track[i].position, track[i + 1].position, segLen[i] > 1e-6f ? d / segLen[i] : 0f);
        }
        return track[track.Length - 1].position;
    }

    Quaternion SampleRot(float u)
    {
        float d = u * totalLen;
        for (int i = 0; i < segLen.Length; i++)
        {
            if (d > segLen[i]) { d -= segLen[i]; continue; }
            return Quaternion.Slerp(track[i].rotation, track[i + 1].rotation, segLen[i] > 1e-6f ? d / segLen[i] : 0f);
        }
        return track[track.Length - 1].rotation;
    }

    void OnDrawGizmosSelected()
    {
        if (track == null || track.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < track.Length - 1; i++)
            if (track[i] && track[i + 1])
                Gizmos.DrawLine(track[i].position, track[i + 1].position);
    }
}
