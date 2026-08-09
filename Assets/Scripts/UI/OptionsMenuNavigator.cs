using UnityEngine;

/// <summary>
/// Routes the shared options Back button to the main menu or pause menu, depending on who opened it.
/// </summary>
public class OptionsMenuNavigator : MonoBehaviour
{
    [SerializeField] MainMenuController m_MainMenu;
    [SerializeField] PauseMenu m_PauseMenu;

    void Awake()
    {
        ResolveReferences();
    }

    public void UI_Close()
    {
        ResolveReferences();

        if (m_MainMenu != null && (m_MainMenu.IsOptionsOpen || IsTitleScreen()))
        {
            m_MainMenu.CloseOptions();
            return;
        }

        if (m_PauseMenu != null && m_PauseMenu.IsPaused)
        {
            m_PauseMenu.CloseOptionsToPauseMenu();
            return;
        }

        if (m_MainMenu != null)
        {
            m_MainMenu.CloseOptions();
            return;
        }

        gameObject.SetActive(false);
    }

    static bool IsTitleScreen()
    {
        GameManager gameManager = GameManager.Instance;
        return gameManager != null && !gameManager.HasStartedFromMainMenu;
    }

    void ResolveReferences()
    {
        if (m_MainMenu == null)
            m_MainMenu = FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);

        if (m_PauseMenu == null)
            m_PauseMenu = FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include);
    }
}
