using UnityEngine;

/// <summary>
/// Escape toggles pause: first press shows the pause menu, hides options, freezes time,
/// unlocks the cursor, and sets <see cref="GameState.Paused"/>. Second press (or Resume) restores.
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
    GameState m_StateBeforePause = GameState.Gameplay;

    /// <summary> True while paused (pause or options may be visible). </summary>
    public bool IsPaused => m_IsPaused;

    void Start()
    {
        Time.timeScale = 1f;
        m_IsPaused = false;
        SetRootsVisible(false, false);

        if (GameStateManager.CurrentState == GameState.Paused)
            GameStateManager.ChangeState(GameState.Gameplay);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Minigames no longer exit on Escape; also block pause so Escape is a no-op mid-minigame.
            if (MinigameActivator.IsAnyActive)
                return;

            // Main menu owns the screen until Start — don't open pause over it.
            if (GameManager.Instance != null && !GameManager.Instance.HasStartedFromMainMenu)
                return;

            HandleEscape();
        }
    }

    void HandleEscape()
    {
        if (!m_IsPaused)
            PauseGameplay();
        else
            ResumeGameplay();
    }

    void PauseGameplay()
    {
        m_IsPaused = true;
        m_StateBeforePause = GameStateManager.CurrentState;
        if (m_StateBeforePause == GameState.Paused)
            m_StateBeforePause = GameState.Gameplay;

        GameStateManager.ChangeState(GameState.Paused);
        SetRootsVisible(pauseMenu: true, options: false);
        Time.timeScale = 0f;
        ShowCursor();
    }

    /// <summary> Close overlays and unpause — same as second Escape while paused. Hook Resume buttons here. </summary>
    public void ResumeGameplay()
    {
        if (!m_IsPaused)
            return;

        m_IsPaused = false;
        SetRootsVisible(false, false);
        Time.timeScale = 1f;
        GameStateManager.ChangeState(m_StateBeforePause);
        RestoreCursorForState(m_StateBeforePause);
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

    static void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    static void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    static void RestoreCursorForState(GameState state)
    {
        // Dialogue (and intro) keep the cursor free for UI; gameplay / pager keep it locked for look.
        if (state == GameState.Dialogue)
            ShowCursor();
        else
            HideCursor();
    }

    #region Unity UI — Button OnClick

    public void UI_ResumeGameplay() => ResumeGameplay();

    public void UI_OpenOptionsFromPause() => OpenOptionsFromPause();

    public void UI_CloseOptionsToPauseMenu() => CloseOptionsToPauseMenu();

    #endregion
}
