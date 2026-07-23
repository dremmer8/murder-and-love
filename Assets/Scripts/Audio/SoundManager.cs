using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Plays events from a <see cref="SoundLibrary"/> at runtime using FMOD <see cref="RuntimeManager"/>.
/// </summary>
[DefaultExecutionOrder(10)]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] SoundLibrary m_Library;

    public SoundLibrary Library
    {
        get => m_Library;
        set => m_Library = value;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SoundManager] Duplicate SoundManager — destroying this instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        AudioOptionSettings.Load();
        AudioOptionSettings.ApplyFmodVolumes();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary> Convenience for call sites that don't want to null-check <see cref="Instance"/>. </summary>
    public static bool PlayOneShot(string key, Vector3 worldPosition = default)
    {
        return Instance != null && Instance.TryPlayOneShot(key, worldPosition);
    }

    /// <summary> Convenience attached one-shot via <see cref="Instance"/>. </summary>
    public static bool PlayOneShotAttached(string key, GameObject target)
    {
        return Instance != null && Instance.TryPlayOneShotAttached(key, target);
    }

    /// <summary>
    /// Stops every active FMOD event on the master bus (looped music, soundscape, etc.).
    /// Call on game restart so audio does not carry into the next playthrough.
    /// </summary>
    public static void StopAllEvents(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        if (!RuntimeManager.IsInitialized)
            return;

        try
        {
            RuntimeManager.GetBus("bus:/").stopAllEvents(mode);
        }
        catch (BusNotFoundException)
        {
            RuntimeUtils.DebugLogWarning("[SoundManager] Master bus 'bus:/' not found — cannot stop all events.");
        }

        UniversalAudioDemo.ResetGlobalMusicTracking();
    }

    /// <summary> True if the library resolved <paramref name="key"/> to a non-null event. </summary>
    public bool TryPlayOneShot(string key, Vector3 worldPosition = default)
    {
        if (!Resolve(key, out var reference))
            return false;

        RuntimeManager.PlayOneShot(reference, worldPosition);
        return true;
    }

    /// <summary> Plays a one-shot attached to <paramref name="target"/> (follows transform / rigidbody when available). </summary>
    public bool TryPlayOneShotAttached(string key, GameObject target)
    {
        if (target == null || !Resolve(key, out var reference))
            return false;

        RuntimeManager.PlayOneShotAttached(reference, target);
        return true;
    }

    /// <summary>
    /// Creates a started instance you own — call <see cref="EventInstance.stop"/> / <see cref="EventInstance.release"/> when finished.
    /// </summary>
    public bool TryStartInstance(string key, out EventInstance instance, Vector3 worldPosition = default)
    {
        instance = default;
        if (!Resolve(key, out var reference))
            return false;

        try
        {
            instance = RuntimeManager.CreateInstance(reference);
        }
        catch (EventNotFoundException)
        {
            RuntimeUtils.DebugLogWarning($"[SoundManager] Event not found for key '{key}'");
            return false;
        }

        if (!instance.isValid())
            return false;

        instance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPosition));
        instance.start();
        return true;
    }

    /// <summary> Same as <see cref="TryStartInstance"/> but does not call <c>start()</c> (for parameter setup first). </summary>
    public bool TryCreateInstance(string key, out EventInstance instance, Vector3 worldPosition = default)
    {
        instance = default;
        if (!Resolve(key, out var reference))
            return false;

        try
        {
            instance = RuntimeManager.CreateInstance(reference);
        }
        catch (EventNotFoundException)
        {
            RuntimeUtils.DebugLogWarning($"[SoundManager] Event not found for key '{key}'");
            return false;
        }

        if (!instance.isValid())
            return false;

        instance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPosition));
        return true;
    }

    bool Resolve(string key, out EventReference reference)
    {
        reference = default;
        if (m_Library == null)
        {
            Debug.LogWarning("[SoundManager] No SoundLibrary assigned.", this);
            return false;
        }

        if (!m_Library.TryGet(key, out reference) || reference.IsNull)
        {
            RuntimeUtils.DebugLogWarning($"[SoundManager] Unknown or empty sound key: '{key}'");
            return false;
        }

        return true;
    }
}
