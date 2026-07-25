using TMPro;
using UnityEngine;

/// <summary>
/// Caps the game at a target frame rate and shows a running average FPS on a TMP label.
/// Assign a TextMeshProUGUI in the Inspector (or leave empty to use one on this object / children).
/// </summary>
public class FpsCounter : MonoBehaviour
{
    [Header("Frame Cap")]
    [Tooltip("Maximum frames per second. VSync is disabled so this cap can take effect.")]
    [SerializeField] int targetFrameRate = 60;

    [Header("Display")]
    [SerializeField] TextMeshProUGUI fpsLabel;
    [SerializeField] string displayFormat = "FPS: {0:0.0}";
    [Tooltip("How often the label text is refreshed (seconds). Average is still sampled every frame.")]
    [SerializeField] float updateInterval = 0.5f;

    [Header("Average Window")]
    [Tooltip("Rolling window length in seconds used for the displayed average.")]
    [SerializeField] float averageWindowSeconds = 1f;

    float _accum;
    int _frames;
    float _timeLeft;
    float _avgFps;

    void Awake()
    {
        if (fpsLabel == null)
            fpsLabel = GetComponentInChildren<TextMeshProUGUI>(true);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;

        _timeLeft = updateInterval;
    }

    void OnValidate()
    {
        targetFrameRate = Mathf.Max(1, targetFrameRate);
        updateInterval = Mathf.Max(0.05f, updateInterval);
        averageWindowSeconds = Mathf.Max(0.1f, averageWindowSeconds);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f)
            return;

        _accum += dt;
        _frames++;
        _timeLeft -= dt;

        // Keep a rolling window so the average stays responsive.
        if (_accum >= averageWindowSeconds && _frames > 0)
        {
            _avgFps = _frames / _accum;
            // Shrink the window instead of resetting hard, so spikes don't vanish instantly.
            float keep = averageWindowSeconds * 0.5f;
            float scale = keep / _accum;
            _accum *= scale;
            _frames = Mathf.Max(1, Mathf.RoundToInt(_frames * scale));
        }

        if (_timeLeft > 0f)
            return;

        _timeLeft = updateInterval;

        if (_accum > 0f && _frames > 0)
            _avgFps = _frames / _accum;

        if (fpsLabel != null)
            fpsLabel.text = string.Format(displayFormat, _avgFps);
    }
}
