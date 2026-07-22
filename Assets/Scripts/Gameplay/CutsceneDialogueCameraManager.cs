using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporary dialogue cutscene cameras driven by Ink EXTERNAL ChangeCamera(cameraId).
/// Only active during <see cref="DialoguePresentationMode.Standard"/> — ignored for
/// Internal Monologue and Pager. Activating a camera disables the player, holds for a
/// random duration, then jumps back to the player camera. Calling ChangeCamera again
/// cancels the current hold and restarts.
/// </summary>
public class CutsceneDialogueCameraManager : MonoBehaviour
{
    public static CutsceneDialogueCameraManager Instance { get; private set; }

    [Header("Cameras")]
    [Tooltip("Dialogue cutscene cameras. Ink ChangeCamera(\"id\") matches each camera's GameObject name.")]
    [SerializeField] List<Camera> cameras = new();

    [Tooltip("Player camera restored when the hold timer ends. Auto-found if empty.")]
    [SerializeField] Camera playerCamera;

    [Tooltip("Optional CameraManager name used when returning (keeps CameraManager in sync).")]
    [SerializeField] string playerCameraName = "Player";

    [Header("Player")]
    [Tooltip("Player root deactivated while a cutscene camera is active. Auto-found if empty.")]
    [SerializeField] GameObject playerObject;

    [Header("Hold Duration")]
    [Tooltip("Random seconds a cutscene camera stays active before returning to the player (min, max).")]
    [SerializeField] Vector2 holdDurationRange = new(10f, 25f);

    Coroutine _holdRoutine;
    Camera _activeCutsceneCamera;
    bool _subscribedToDialogue;

    public bool IsCutsceneCameraActive => _activeCutsceneCamera != null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: Duplicate CutsceneDialogueCameraManager — destroying this instance.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        EnsurePlayerRefs();
    }

    void Start()
    {
        SubscribeDialogue();
    }

    void OnDestroy()
    {
        UnsubscribeDialogue();
        StopHoldRoutine();

        if (_activeCutsceneCamera != null)
            ReturnToPlayerCamera();

        if (Instance == this)
            Instance = null;
    }

    public void BindInkExternals(Ink.Runtime.Story story)
    {
        if (story == null)
            return;

        story.BindExternalFunction("ChangeCamera", (string cameraId) => ChangeCamera(cameraId));
    }

    /// <summary>Ink EXTERNAL entry point. Cutscene cams only run during Standard dialogue.</summary>
    public void ChangeCamera(string cameraId)
    {
        if (string.IsNullOrWhiteSpace(cameraId))
        {
            Debug.LogWarning($"{name}: ChangeCamera id is empty.", this);
            return;
        }

        string trimmed = cameraId.Trim();
        bool returnToPlayer = string.Equals(trimmed, playerCameraName, StringComparison.OrdinalIgnoreCase);

        // Always allow cleanup back to the player if a cutscene cam is already up.
        // Otherwise ignore all ChangeCamera calls outside Standard dialogue
        // (Internal Monologue / Pager / Intro must not steal the camera).
        if (!IsStandardDialogueActive())
        {
            if (returnToPlayer && IsCutsceneCameraActive)
                ReturnToPlayerCamera();
            return;
        }

        if (returnToPlayer)
        {
            ReturnToPlayerCamera();
            return;
        }

        if (!TryGetCamera(trimmed, out Camera target))
        {
            Debug.LogWarning($"{name}: No dialogue camera named '{trimmed}'.", this);
            return;
        }

        ActivateCutsceneCamera(target);
    }

    static bool IsStandardDialogueActive()
    {
        DialogueManager dialogue = DialogueManager.GetInstance();
        return dialogue != null
            && dialogue.dialogueIsPlaying
            && dialogue.ActiveMode == DialoguePresentationMode.Standard;
    }

    /// <summary>Cancel any hold timer, disable cutscene cams, and restore the player.</summary>
    public void ReturnToPlayerCamera()
    {
        StopHoldRoutine();
        DisableAllCutsceneCameras();
        _activeCutsceneCamera = null;
        SetPlayerActive(true);
        RestorePlayerCamera();
    }

    void ActivateCutsceneCamera(Camera target)
    {
        StopHoldRoutine();
        EnsurePlayerRefs();

        DisableAllCutsceneCameras();
        SetCameraEnabled(target, true);
        _activeCutsceneCamera = target;

        SetPlayerActive(false);
        _holdRoutine = StartCoroutine(HoldThenReturnRoutine());
    }

    IEnumerator HoldThenReturnRoutine()
    {
        float min = Mathf.Min(holdDurationRange.x, holdDurationRange.y);
        float max = Mathf.Max(holdDurationRange.x, holdDurationRange.y);
        float duration = UnityEngine.Random.Range(min, max);

        yield return new WaitForSeconds(duration);

        _holdRoutine = null;
        ReturnToPlayerCamera();
    }

    void StopHoldRoutine()
    {
        if (_holdRoutine == null)
            return;

        StopCoroutine(_holdRoutine);
        _holdRoutine = null;
    }

    bool TryGetCamera(string cameraName, out Camera camera)
    {
        camera = null;
        if (cameras == null)
            return false;

        for (int i = 0; i < cameras.Count; i++)
        {
            Camera candidate = cameras[i];
            if (candidate == null)
                continue;

            if (!string.Equals(candidate.name, cameraName, StringComparison.OrdinalIgnoreCase))
                continue;

            camera = candidate;
            return true;
        }

        return false;
    }

    void DisableAllCutsceneCameras()
    {
        if (cameras == null)
            return;

        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] != null)
                SetCameraEnabled(cameras[i], false);
        }
    }

    void SetPlayerActive(bool active)
    {
        EnsurePlayerRefs();
        if (playerObject != null && playerObject.activeSelf != active)
            playerObject.SetActive(active);
    }

    void RestorePlayerCamera()
    {
        if (CameraManager.Instance != null
            && !string.IsNullOrWhiteSpace(playerCameraName)
            && CameraManager.Instance.SwapCamera(playerCameraName))
            return;

        EnsurePlayerRefs();
        if (playerCamera != null)
            SetCameraEnabled(playerCamera, true);
    }

    void EnsurePlayerRefs()
    {
        if (playerObject == null || playerCamera == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                if (playerObject == null)
                    playerObject = player.gameObject;
                if (playerCamera == null)
                    playerCamera = player.playerCamera;
            }
        }

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    static void SetCameraEnabled(Camera cam, bool enabled)
    {
        if (cam == null)
            return;

        if (enabled && !cam.gameObject.activeSelf)
            cam.gameObject.SetActive(true);

        cam.enabled = enabled;

        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = enabled;
    }

    void SubscribeDialogue()
    {
        DialogueManager dialogue = DialogueManager.GetInstance();
        if (dialogue == null || _subscribedToDialogue)
            return;

        dialogue.OnDialogueEnded += HandleDialogueEnded;
        _subscribedToDialogue = true;
    }

    void UnsubscribeDialogue()
    {
        DialogueManager dialogue = DialogueManager.GetInstance();
        if (dialogue != null && _subscribedToDialogue)
            dialogue.OnDialogueEnded -= HandleDialogueEnded;

        _subscribedToDialogue = false;
    }

    void HandleDialogueEnded(string _)
    {
        if (_activeCutsceneCamera != null || _holdRoutine != null)
            ReturnToPlayerCamera();
    }
}
