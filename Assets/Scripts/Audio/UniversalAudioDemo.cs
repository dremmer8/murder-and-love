using FMOD.Studio;
using UnityEngine;

/// <summary>
/// Demo helper: starts background music from <see cref="SoundManager"/> keys on Start,
/// and exposes a parameterless method for UI buttons to fire a one-shot SFX key.
/// Music is started at most once per application run; it is not restarted when this
/// component or scene is loaded again.
/// </summary>
public class UniversalAudioDemo : MonoBehaviour
{
    static EventInstance s_GlobalMusicInstance;
    static bool s_MusicWasStartedGlobally;
    static bool s_QuitCleanupDone;

    [Header("Music (looping event recommended in FMOD)")]
    [SerializeField] bool m_PlayMusicOnStart = true;

    [SerializeField]
    [Tooltip("SoundLibrary key for music — only the first successful start in the session runs; not started again on later scenes or duplicate objects.")]
    string m_MusicEventKey = "Music_Main";

    [Header("UI button sound")]
    [SerializeField]
    [Tooltip("SoundLibrary key for generic UI clicks — used by PlayUIButtonSound / UI_PlayUIButtonSound.")]
    string m_UIButtonSoundKey = "click";

    void Start()
    {
        if (s_MusicWasStartedGlobally)
            return;

        if (!m_PlayMusicOnStart || string.IsNullOrWhiteSpace(m_MusicEventKey))
            return;

        var mgr = SoundManager.Instance;
        if (mgr == null)
        {
            Debug.LogWarning("[UniversalAudioDemo] No SoundManager — cannot start music.", this);
            return;
        }

        if (!mgr.TryStartInstance(m_MusicEventKey.Trim(), out var instance))
            return;

        if (!instance.isValid())
            return;

        s_GlobalMusicInstance = instance;
        s_MusicWasStartedGlobally = true;
    }

    void OnApplicationQuit()
    {
        ReleaseGlobalMusicIfNeeded();
    }

    static void ReleaseGlobalMusicIfNeeded()
    {
        if (s_QuitCleanupDone)
            return;

        s_QuitCleanupDone = true;

        if (!s_GlobalMusicInstance.isValid())
            return;

        s_GlobalMusicInstance.stop(STOP_MODE.ALLOWFADEOUT);
        s_GlobalMusicInstance.release();
        s_GlobalMusicInstance.clearHandle();
    }

    /// <summary> Plays <see cref="m_UIButtonSoundKey"/> as a one-shot (hook to Button OnClick). </summary>
    public void PlayUIButtonSound()
    {
        PlayOneShotFromLibrary(m_UIButtonSoundKey);
    }

    /// <summary> Plays any library key — useful from scripts or extra buttons wired via separate behaviours. </summary>
    public void PlayOneShotFromLibrary(string soundLibraryKey)
    {
        if (string.IsNullOrWhiteSpace(soundLibraryKey))
            return;

        var mgr = SoundManager.Instance;
        if (mgr == null)
        {
            Debug.LogWarning("[UniversalAudioDemo] No SoundManager — cannot play sound.", this);
            return;
        }

        mgr.TryPlayOneShot(soundLibraryKey.Trim());
    }

    #region Unity UI — Button OnClick

    public void UI_PlayUIButtonSound() => PlayUIButtonSound();

    #endregion
}
