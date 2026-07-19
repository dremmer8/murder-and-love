using UnityEngine;

/// <summary>
/// Escape toggles pause: first press shows the pause menu, hides options, and freezes time (<c>timeScale = 0</c>).
/// Second press hides pause and options and restores time (<c>timeScale = 1</c>).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Main pause overlay (shown on first Escape while playing).")]
    GameObject m_PauseMenuRoot;

    [SerializeField]
    [Tooltip("Options overlay (hidden when opening pause via Escape).")]
    GameObject m_OptionsRoot;

    bool m_IsPaused;

    /// <summary> True while paused (pause or options may be visible). </summary>
    public bool IsPaused => m_IsPaused;

    void Start()
    {
        Time.timeScale = 1f;
        m_IsPaused = false;
        SetRootsVisible(false, false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Minigames no longer exit on Escape; also block pause so Escape is a no-op mid-minigame.
            if (MinigameActivator.IsAnyActive)
                return;

            HandleEscape();
        }
    }

    void HandleEscape()
    {
        if (!m_IsPaused)
        {
            m_IsPaused = true;
            SetRootsVisible(pauseMenu: true, options: false);
            Time.timeScale = 0f;
        }
        else
        {
            ResumeGameplay();
        }
    }

    /// <summary> Close overlays and unpause — same as second Escape while paused. Hook Resume buttons here. </summary>
    public void ResumeGameplay()
    {
        m_IsPaused = false;
        SetRootsVisible(false, false);
        Time.timeScale = 1f;
    }

    /// <summary> From pause menu → options (buttons). Stays paused. </summary>
    public void OpenOptionsFromPause()
    {
        if (!m_IsPaused)
            return;

        SetRootsVisible(false, true);
    }

    /// <summary> From options → pause menu (buttons). Stays paused. </summary>
    public void CloseOptionsToPauseMenu()
    {
        if (!m_IsPaused)
            return;

        SetRootsVisible(true, false);
    }

    void SetRootsVisible(bool pauseMenu, bool options)
    {
        if (m_PauseMenuRoot != null)
            m_PauseMenuRoot.SetActive(pauseMenu);

        if (m_OptionsRoot != null)
            m_OptionsRoot.SetActive(options);
    }

    #region Unity UI — Button OnClick

    public void UI_ResumeGameplay() => ResumeGameplay();

    public void UI_OpenOptionsFromPause() => OpenOptionsFromPause();

    public void UI_CloseOptionsToPauseMenu() => CloseOptionsToPauseMenu();

    #endregion
}
