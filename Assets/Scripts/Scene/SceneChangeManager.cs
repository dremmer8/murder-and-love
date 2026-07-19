using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent scene transitions. Use a single instance on your bootstrap / first scene GameObject
/// (<see cref="DontDestroyOnLoad"/>). For Button OnClick, prefer <see cref="SceneChangeUIButtons"/>
/// on that same persistent object so scene copies of this component do not break references.
/// </summary>
public class SceneChangeManager : MonoBehaviour
{
    public static SceneChangeManager Instance { get; private set; }

    [Header("Game")]
    [Tooltip("Main gameplay scene loaded by StartGame (must be in Build Settings).")]
    [SerializeField] string m_GameSceneName = "SampleScene";

    [Header("Restart flow")]
    [Tooltip("Scene that contains SceneBootstrapAutoLoader (loads the real game scene).")]
    [SerializeField] string m_BootstrapSceneName = "Bootstrap";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "[SceneChangeManager] Duplicate SceneChangeManager removed from this GameObject only. " +
                "Remove extra managers from gameplay/menu scenes (keep one on bootstrap DontDestroyOnLoad). " +
                "If your UI buttons broke, add SceneChangeUIButtons next to the persistent manager and wire buttons to that instead.",
                this);
          //  Destroy(this);
            return;
        }

        Instance = this;
      //  DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary> Loads a scene by name (must be in File → Build Settings → Scenes In Build). </summary>
    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (!TryLoadScene(sceneName, mode))
            Debug.LogError($"[SceneChangeManager] Cannot load scene '{sceneName}'. Add it to Build Settings.", this);
    }

    public bool TryLoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        var trimmed = sceneName.Trim();
        if (!Application.CanStreamedLevelBeLoaded(trimmed))
            return false;

        SceneManager.LoadScene(trimmed, mode);
        return true;
    }

    /// <summary> Async load with optional completion callback on the Unity main thread. </summary>
    public bool TryLoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, System.Action onLoaded = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        var trimmed = sceneName.Trim();
        if (!Application.CanStreamedLevelBeLoaded(trimmed))
            return false;

        StartCoroutine(LoadAsyncRoutine(trimmed, mode, onLoaded));
        return true;
    }

    IEnumerator LoadAsyncRoutine(string sceneName, LoadSceneMode mode, System.Action onLoaded)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, mode);
        while (!op.isDone)
            yield return null;

        onLoaded?.Invoke();
    }

    /// <summary> Loads <see cref="GameSceneName"/> (inspector field <c>m_GameSceneName</c>). </summary>
    public void StartGame(LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (!TryStartGame(mode))
            Debug.LogError($"[SceneChangeManager] StartGame failed — scene '{m_GameSceneName}' missing or invalid. Add it to Build Settings.", this);
    }

    public bool TryStartGame(LoadSceneMode mode = LoadSceneMode.Single)
    {
        return TryLoadScene(m_GameSceneName, mode);
    }

    /// <summary> Async load of <see cref="GameSceneName"/>. </summary>
    public bool TryStartGameAsync(LoadSceneMode mode = LoadSceneMode.Single, System.Action onLoaded = null)
    {
        return TryLoadSceneAsync(m_GameSceneName, mode, onLoaded);
    }

    /// <summary>
    /// Reloads via the bootstrap scene so Awake/Start on bootstrap runs again and
    /// <see cref="SceneBootstrapAutoLoader"/> pushes the player into the game scene (clean restart).
    /// </summary>
    public void RestartGame()
    {
        // Pause leaves timeScale at 0; reset so the next scene is not frozen.
        Time.timeScale = 1f;
        if (GameStateManager.CurrentState == GameState.Paused)
            GameStateManager.ChangeState(GameState.Gameplay);

        if (!TryLoadScene(m_BootstrapSceneName))
            Debug.LogError($"[SceneChangeManager] Restart failed — bootstrap scene '{m_BootstrapSceneName}' missing from Build Settings.", this);
    }

    /// <summary> Bootstrap scene used by <see cref="RestartGame"/>. </summary>
    public string BootstrapSceneName => m_BootstrapSceneName;

    /// <summary> Scene loaded by <see cref="StartGame"/> / <see cref="TryStartGame"/>. </summary>
    public string GameSceneName => m_GameSceneName;

    /// <summary>
    /// Quits the application in a standalone build; stops Play Mode when called from the Editor.
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #region Unity UI — Button OnClick (no parameters)

    /// <summary>
    /// Hook to Button OnClick — prefer adding <see cref="SceneChangeUIButtons"/> on the persistent bootstrap object and wiring there so references survive scene reloads.
    /// </summary>
    public void UI_StartGame()
    {
        StartGame();
    }

    /// <summary> Hook to Button OnClick — loads <see cref="BootstrapSceneName"/> (clean restart via bootstrap). Prefer <see cref="SceneChangeUIButtons"/> on your persistent bootstrap object. </summary>
    public void UI_RestartGame()
    {
        RestartGame();
    }

    /// <summary> Hook to Button OnClick — quit build / exit Play Mode in Editor. Prefer <see cref="SceneChangeUIButtons"/> on your persistent bootstrap object. </summary>
    public void UI_ExitGame()
    {
        ExitGame();
    }

    #endregion
}
