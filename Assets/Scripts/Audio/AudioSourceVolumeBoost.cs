using UnityEngine;

/// <summary>
/// Amplifies an <see cref="AudioSource"/> beyond Unity's 0–1 volume clamp
/// by multiplying PCM samples in <see cref="OnAudioFilterRead"/>.
/// Drop on the same GameObject as the AudioSource.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class AudioSourceVolumeBoost : MonoBehaviour
{
    const float MinMultiplier = 0f;
    const float MaxMultiplier = 5f;

    [SerializeField]
    [Range(MinMultiplier, MaxMultiplier)]
    [Tooltip("Linear gain applied to the AudioSource output. 1 = unchanged, 5 = five times louder.")]
    float multiplier = 1f;

    /// <summary> Linear gain applied to the AudioSource output (0–5). </summary>
    public float Multiplier
    {
        get => multiplier;
        set => multiplier = Mathf.Clamp(value, MinMultiplier, MaxMultiplier);
    }

    void OnValidate()
    {
        multiplier = Mathf.Clamp(multiplier, MinMultiplier, MaxMultiplier);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float gain = multiplier;
        if (Mathf.Approximately(gain, 1f))
            return;

        for (int i = 0; i < data.Length; i++)
            data[i] *= gain;
    }
}
