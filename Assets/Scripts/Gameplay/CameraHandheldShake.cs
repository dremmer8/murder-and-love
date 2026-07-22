using UnityEngine;

/// <summary>
/// Subtle continuous Perlin-noise motion for a handheld look.
/// Add to a camera (or its parent). Stores the local pose on enable and
/// applies tiny position / rotation offsets in LateUpdate.
/// </summary>
[DisallowMultipleComponent]
public class CameraHandheldShake : MonoBehaviour
{
    [Header("Amount")]
    [Tooltip("Max local position offset (meters). Keep tiny for a subtle feel.")]
    [SerializeField] float positionAmount = 0.015f;

    [Tooltip("Max rotation offset in degrees (pitch/yaw). Roll uses half of this.")]
    [SerializeField] float rotationAmount = 0.35f;

    [Header("Motion")]
    [Tooltip("How fast the noise drifts. Lower = slower, more cinematic.")]
    [SerializeField] float speed = 0.35f;

    [Tooltip("Optional seed so multiple cameras don't move in sync. 0 = randomize on enable.")]
    [SerializeField] float seed;

    Vector3 _restLocalPosition;
    Quaternion _restLocalRotation;
    float _noiseOrigin;

    void OnEnable()
    {
        CaptureRestPose();
        _noiseOrigin = seed != 0f ? seed : Random.Range(0f, 1000f);
    }

    void OnDisable()
    {
        RestoreRestPose();
    }

    void LateUpdate()
    {
        float t = Time.time * speed;
        float nx = Sample(t, 0f);
        float ny = Sample(t, 17f);
        float nz = Sample(t, 31f);

        transform.localPosition = _restLocalPosition
            + new Vector3(nx, ny, nz * 0.5f) * positionAmount;

        transform.localRotation = _restLocalRotation
            * Quaternion.Euler(
                ny * rotationAmount,
                nx * rotationAmount,
                nz * rotationAmount * 0.5f);
    }

    /// <summary>Re-read the current local pose as the shake rest pose.</summary>
    public void CaptureRestPose()
    {
        _restLocalPosition = transform.localPosition;
        _restLocalRotation = transform.localRotation;
    }

    void RestoreRestPose()
    {
        transform.localPosition = _restLocalPosition;
        transform.localRotation = _restLocalRotation;
    }

    float Sample(float time, float channel)
    {
        return (Mathf.PerlinNoise(_noiseOrigin + time, _noiseOrigin + channel) - 0.5f) * 2f;
    }
}
