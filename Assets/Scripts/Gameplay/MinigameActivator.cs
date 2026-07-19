using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Starts a minigame: smooth camera transition via CameraManager, then toggles object lists.
/// Does not handle raycasts — put an <see cref="Interactable"/> (type Event) on the same object
/// and wire On Interact → Activate, or leave auto-subscribe enabled.
/// Call <see cref="Exit"/> externally to reverse enter logic.
/// </summary>
public class MinigameActivator : MonoBehaviour
{
    [Header("Interactable")]
    [Tooltip("Interactable that receives the look+E raycast. Auto-found on this object if empty.")]
    [SerializeField] Interactable interactable;

    [Tooltip("If true, listens to Interactable.onInteract automatically.")]
    [SerializeField] bool autoSubscribeToInteractable = true;

    [Header("Camera")]
    [Tooltip("Name of the minigame camera in CameraManager's list.")]
    [SerializeField] string minigameCameraName = "Minigame";

    [Tooltip("Name of the player camera to return to on Exit.")]
    [SerializeField] string playerCameraName = "Player";

    [Tooltip("Optional override for transition length. Uses CameraManager default when <= 0.")]
    [SerializeField] float transitionDuration = -1f;

    [Header("Visibility")]
    [SerializeField] List<GameObject> objectsToHide = new();
    [SerializeField] List<GameObject> objectsToShow = new();

    [Tooltip("If true, wait until the camera transition finishes before toggling objects (enter and exit).")]
    [SerializeField] bool applyVisibilityAfterTransition;

    [Header("Collider")]
    [Tooltip("If true, disables the assigned box collider(s) on enter and re-enables them on exit.")]
    [SerializeField] bool disableColliderDuringMinigame = true;

    [Tooltip("Box colliders to toggle. Auto-fills this object's BoxCollider if empty.")]
    [SerializeField] List<BoxCollider> collidersToDisable = new();

    [Header("Behaviour")]
    [Tooltip("If true, show and unlock the mouse cursor while in the minigame.")]
    [SerializeField] bool showCursorInMinigame = true;

    [Tooltip("If true, pressing the exit key while in the minigame calls Exit.")]
    [SerializeField] bool allowExitWithKey = true;

    [SerializeField] KeyCode exitKey = KeyCode.Escape;

    [Tooltip("If true, automatically call Exit after Auto End Delay seconds once entered.")]
    [SerializeField] bool autoEndWithTimer;

    [Tooltip("Seconds to wait after enter before auto-exiting. Ignored unless Auto End With Timer is on.")]
    [SerializeField] float autoEndDelay = 3f;

    [Header("Control Hints")]
    [Tooltip("How-to-play text shown top-right by ControlHintsPresenter while this minigame is active. Leave empty to use the presenter's fallback.")]
    [TextArea]
    [SerializeField] string controlHints = "";

    bool _activated;
    bool _interactionLocked;
    Coroutine _visibilityRoutine;
    Coroutine _autoEndRoutine;

    static int s_ActiveCount;

    public bool IsActivated => _activated;

    /// <summary>True after <see cref="LockInteraction"/> — Activate is blocked permanently.</summary>
    public bool IsInteractionLocked => _interactionLocked;

    /// <summary>True while any MinigameActivator is in the minigame.</summary>
    public static bool IsAnyActive => s_ActiveCount > 0;

    /// <summary>The most recently activated minigame (null when none active). Used for context hints.</summary>
    public static MinigameActivator ActiveInstance { get; private set; }

    /// <summary>How-to-play hint text for this minigame (may be empty).</summary>
    public string ControlHints => controlHints;

    void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<Interactable>();

        if (autoSubscribeToInteractable && interactable != null)
            interactable.onInteract.AddListener(Activate);

        if (collidersToDisable.Count == 0)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
                collidersToDisable.Add(box);
        }
    }

    void OnDestroy()
    {
        if (autoSubscribeToInteractable && interactable != null)
            interactable.onInteract.RemoveListener(Activate);

        StopAutoEndTimer();
        StopVisibilityRoutine();

        if (_activated)
            SetActivated(false);
    }

    void Update()
    {
        if (!_activated || !allowExitWithKey)
            return;

        if (Input.GetKeyDown(exitKey))
            Exit();
    }

    /// <summary>
    /// Permanently blocks re-entry: Activate no-ops and colliders stay disabled.
    /// Call after a successful run so the player cannot start this minigame again.
    /// </summary>
    public void LockInteraction()
    {
        _interactionLocked = true;

        for (int i = 0; i < collidersToDisable.Count; i++)
        {
            if (collidersToDisable[i] != null)
                collidersToDisable[i].enabled = false;
        }

        if (interactable != null)
            interactable.enabled = false;
    }

    /// <summary>Starts the minigame. Called from Interactable.onInteract or externally.</summary>
    public void Activate()
    {
        if (_activated || _interactionLocked)
            return;

        if (GameStateManager.CurrentState != GameState.Gameplay)
            return;

        if (CameraManager.Instance == null)
        {
            Debug.LogWarning($"{name}: No CameraManager in scene.", this);
            return;
        }

        if (CameraManager.Instance.IsTransitioning)
            return;

        if (!TransitionTo(minigameCameraName))
            return;

        SetActivated(true);
        ApplyVisibilityForEnter();
        StartAutoEndTimer();
    }

    /// <summary>
    /// External exit: reverse enter logic — camera back to player,
    /// hide the show-list, show the hide-list.
    /// </summary>
    public void Exit()
    {
        if (!_activated)
            return;

        StopAutoEndTimer();
        StopVisibilityRoutine();

        bool cameraOk = TransitionTo(playerCameraName);
        if (!cameraOk)
            ApplyExitVisibility();
        else
            ApplyVisibilityForExit();

        SetActivated(false);
    }

    void SetActivated(bool value)
    {
        if (_activated == value)
            return;

        _activated = value;
        if (value)
        {
            s_ActiveCount++;
            ActiveInstance = this;
            if (showCursorInMinigame)
                ShowCursor();
            SetCollidersEnabled(false);
        }
        else
        {
            s_ActiveCount = Mathf.Max(0, s_ActiveCount - 1);
            if (ActiveInstance == this)
                ActiveInstance = null;
            if (s_ActiveCount == 0)
                HideCursor();
            if (!_interactionLocked)
                SetCollidersEnabled(true);
        }
    }

    void SetCollidersEnabled(bool enabled)
    {
        if (!disableColliderDuringMinigame)
            return;

        for (int i = 0; i < collidersToDisable.Count; i++)
        {
            if (collidersToDisable[i] != null)
                collidersToDisable[i].enabled = enabled;
        }
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

    /// <summary>Alias for <see cref="Exit"/> (UnityEvent-friendly).</summary>
    public void ExitMinigame() => Exit();

    /// <summary>Alias for <see cref="Exit"/>.</summary>
    public void Deactivate() => Exit();

    bool TransitionTo(string cameraName)
    {
        if (CameraManager.Instance == null)
        {
            Debug.LogWarning($"{name}: No CameraManager in scene.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(cameraName))
        {
            Debug.LogWarning($"{name}: Camera name is empty.", this);
            return false;
        }

        return transitionDuration > 0f
            ? CameraManager.Instance.TransitionToCamera(cameraName, transitionDuration)
            : CameraManager.Instance.TransitionToCamera(cameraName);
    }

    void ApplyVisibilityForEnter()
    {
        if (applyVisibilityAfterTransition)
            _visibilityRoutine = StartCoroutine(ApplyVisibilityWhenTransitionEnds(enter: true));
        else
            ApplyEnterVisibility();
    }

    void ApplyVisibilityForExit()
    {
        if (applyVisibilityAfterTransition)
            _visibilityRoutine = StartCoroutine(ApplyVisibilityWhenTransitionEnds(enter: false));
        else
            ApplyExitVisibility();
    }

    void ApplyEnterVisibility()
    {
        SetActiveList(objectsToHide, false);
        SetActiveList(objectsToShow, true);
    }

    void ApplyExitVisibility()
    {
        SetActiveList(objectsToShow, false);
        SetActiveList(objectsToHide, true);
    }

    IEnumerator ApplyVisibilityWhenTransitionEnds(bool enter)
    {
        while (CameraManager.Instance != null && CameraManager.Instance.IsTransitioning)
            yield return null;

        _visibilityRoutine = null;

        if (enter)
            ApplyEnterVisibility();
        else
            ApplyExitVisibility();
    }

    void StopVisibilityRoutine()
    {
        if (_visibilityRoutine == null)
            return;

        StopCoroutine(_visibilityRoutine);
        _visibilityRoutine = null;
    }

    void StartAutoEndTimer()
    {
        StopAutoEndTimer();

        if (!autoEndWithTimer || autoEndDelay < 0f)
            return;

        _autoEndRoutine = StartCoroutine(AutoEndAfterDelay());
    }

    void StopAutoEndTimer()
    {
        if (_autoEndRoutine == null)
            return;

        StopCoroutine(_autoEndRoutine);
        _autoEndRoutine = null;
    }

    IEnumerator AutoEndAfterDelay()
    {
        if (autoEndDelay > 0f)
            yield return new WaitForSeconds(autoEndDelay);

        _autoEndRoutine = null;
        Exit();
    }

    static void SetActiveList(List<GameObject> list, bool active)
    {
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
                list[i].SetActive(active);
        }
    }
}
