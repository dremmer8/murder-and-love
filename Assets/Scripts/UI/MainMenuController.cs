using UnityEngine;

/// <summary>
/// Main-menu Start / Options flow. Reuses the shared <c>OptionsMenu</c> panel (sound, music, language, back).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public static bool OptionsOpenedFromMainMenu { get; private set; }

    [SerializeField]
    [Tooltip("Root with Start / Options / Exit buttons on the title screen.")]
    GameObject m_MainMenuButtonsRoot;

    [SerializeField]
    [Tooltip("Shared options overlay (sound, music, language, back).")]
    GameObject m_OptionsRoot;

    [SerializeField]
    [Tooltip("Canvas that hosts the options overlay (raised above the title canvas while open).")]
    Canvas m_OptionsCanvas;

    [SerializeField]
    int m_OptionsCanvasSortOrder = 5;

    int m_DefaultCanvasSortOrder = 2;
    bool m_OptionsOpen;

    public bool IsOptionsOpen => m_OptionsOpen;

    void Awake()
    {
        if (m_OptionsCanvas != null)
            m_DefaultCanvasSortOrder = m_OptionsCanvas.sortingOrder;

        CloseOptionsImmediate();
    }

    void OnEnable()
    {
        if (!m_OptionsOpen)
            CloseOptionsImmediate();
    }

    void OnDisable()
    {
        OptionsOpenedFromMainMenu = false;
        m_OptionsOpen = false;

        if (m_OptionsRoot != null)
            m_OptionsRoot.SetActive(false);

        if (m_OptionsCanvas != null)
            m_OptionsCanvas.sortingOrder = m_DefaultCanvasSortOrder;
    }

    public static void ClearMainMenuOptionsContext()
    {
        OptionsOpenedFromMainMenu = false;
    }

    public void OpenOptions()
    {
        OptionsOpenedFromMainMenu = true;
        m_OptionsOpen = true;

        if (m_MainMenuButtonsRoot != null)
            m_MainMenuButtonsRoot.SetActive(false);

        if (m_OptionsRoot != null)
            m_OptionsRoot.SetActive(true);

        if (m_OptionsCanvas != null)
            m_OptionsCanvas.sortingOrder = m_OptionsCanvasSortOrder;

        LocalizedFontApplier.ApplyNow();
    }

    public void CloseOptions()
    {
        OptionsOpenedFromMainMenu = false;
        m_OptionsOpen = false;
        CloseOptionsImmediate();
    }

    void CloseOptionsImmediate()
    {
        if (m_MainMenuButtonsRoot != null)
            m_MainMenuButtonsRoot.SetActive(true);

        if (m_OptionsRoot != null)
            m_OptionsRoot.SetActive(false);

        if (m_OptionsCanvas != null)
            m_OptionsCanvas.sortingOrder = m_DefaultCanvasSortOrder;
    }

    public void UI_OpenOptions() => OpenOptions();

    public void UI_CloseOptions() => CloseOptions();
}
