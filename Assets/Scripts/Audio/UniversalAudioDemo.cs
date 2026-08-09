using FMOD.Studio;
using UnityEngine;

/// <summary>
/// Demo helper: starts background music from <see cref="SoundManager"/> keys on Start,
/// and exposes a parameterless method for UI buttons to fire a one-shot SFX key.
/// Music runs as a single instance shared across the session; a second component does not
/// start a competing track while one is already playing.
/// </summary>
public class UniversalAudioDemo : MonoBehaviour
{
    static EventInstance s_GlobalMusicInstance;
    static UniversalAudioDemo s_MusicOwner;
    static bool s_MusicWasStartedGlobally;
    static bool s_QuitCleanupDone;

    [Header("Music (looping event recommended in FMOD)")]
    [SerializeField] bool m_PlayMusicOnStart = true;

    [SerializeField]
    [Tooltip("SoundLibrary key for music — skipped while a track started by another component is still playing, so duplicate objects do not stack tracks.")]
    string m_MusicEventKey = "Music_Main";

    [SerializeField]
    [Tooltip("Stop the music this component started once it is destroyed, so a menu track does not keep playing under the next scene.")]
    bool m_StopMusicOnDestroy = true;

    [Header("UI button sound")]
    [SerializeField]
    [Tooltip("SoundLibrary key for generic UI clicks — used by PlayUIButtonSound / UI_PlayUIButtonSound.")]
    string m_UIButtonSoundKey = "click";

    void Start()
    {
        if (s_MusicWasStartedGlobally && s_GlobalMusicInstance.isValid())
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
        s_MusicOwner = this;
        s_MusicWasStartedGlobally = true;
    }

    void OnApplicationQuit()
    {
        ReleaseGlobalMusicIfNeeded();
    }

    void OnDestroy()
    {
        if (s_MusicOwner != this)
            return;

        s_MusicOwner = null;

        if (m_StopMusicOnDestroy)
            ResetGlobalMusicTracking();
    }

    /// <summary>
    /// Clears the session music handle after a bus-wide stop (e.g. game restart)
    /// so a later scene can start music again.
    /// </summary>
    public static void ResetGlobalMusicTracking()
    {
        StopAndReleaseGlobalMusic();

        s_MusicOwner = null;
        s_MusicWasStartedGlobally = false;
        s_QuitCleanupDone = false;
    }

    static void ReleaseGlobalMusicIfNeeded()
    {
        if (s_QuitCleanupDone)
            return;

        s_QuitCleanupDone = true;
        StopAndReleaseGlobalMusic();
    }

    static void StopAndReleaseGlobalMusic()
    {
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
