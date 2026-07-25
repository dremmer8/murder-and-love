using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum CutsceneLookTarget
{
    Mandy1 = 0,
    Mandy2 = 1,
    Lau1 = 2,
    Lau2 = 3
}

[Serializable]
public class CutsceneLookPhaseBinding
{
    [Tooltip("Ink / GlobalVariableOperator story_phase value that selects this look target.")]
    public int storyPhase;

    [Tooltip("Which of the four look targets to face when this story phase starts.")]
    public CutsceneLookTarget target;
}

/// <summary>
/// Temporary dialogue cutscene cameras driven by Ink EXTERNAL ChangeCamera(cameraId).
/// Only active during <see cref="DialoguePresentationMode.Standard"/> — ignored for
/// Internal Monologue and Pager. Activating a camera hides the player visual (not the
/// player root), disables the player camera, holds for a random duration, then jumps
/// back. Calling ChangeCamera again cancels the current hold and restarts.
///
/// Also rotates the player toward Mandy/Lau look targets when a Standard dialogue starts
/// without a DialogueTrigger pose mark.
/// </summary>
public class CutsceneDialogueCameraManager : MonoBehaviour
{
    public static CutsceneDialogueCameraManager Instance { get; private set; }

    const string PlayerVisualChildName = "visual";

    [Header("Cameras")]
    [Tooltip("Dialogue cutscene cameras. Ink ChangeCamera(\"id\") matches each camera's GameObject name.")]
    [SerializeField] List<Camera> cameras = new();

    [Tooltip("Player camera restored when the hold timer ends. Auto-found if empty.")]
    [SerializeField] Camera playerCamera;

    [Tooltip("Optional CameraManager name used when returning (keeps CameraManager in sync).")]
    [SerializeField] string playerCameraName = "Player";

    [Header("Player")]
    [Tooltip("Player root. Cutscenes hide the child named 'visual' so logic/camera stay alive.")]
    [SerializeField] GameObject playerObject;

    [Tooltip("Optional visual override. Defaults to playerObject/visual.")]
    [SerializeField] GameObject playerVisual;

    [Tooltip("PlayerController rotated toward Mandy/Lau look targets on dialogue start. Assign explicitly — not the cutscene Player Object.")]
    public PlayerController facePlayer;

    [Header("Hold Duration")]
    [Tooltip("Random seconds a cutscene camera stays active before returning to the player (min, max).")]
    [SerializeField] Vector2 holdDurationRange = new(10f, 25f);

    [Header("Look Targets")]
    [SerializeField] Transform mandyTarget1;
    [SerializeField] Transform mandyTarget2;
    [SerializeField] Transform lauTarget1;
    [SerializeField] Transform lauTarget2;

    [Tooltip("Map story_phase values to one of the four look targets.")]
    [SerializeField] List<CutsceneLookPhaseBinding> phaseLookTargets = new();

    [Header("Face Animators")]
    [Tooltip("FaceAnimationOperator for Mandy slot 1 (phase targets using Mandy1).")]
    [SerializeField] FaceAnimationOperator mandyFace1;

    [Tooltip("FaceAnimationOperator for Mandy slot 2 (phase targets using Mandy2).")]
    [SerializeField] FaceAnimationOperator mandyFace2;

    [Tooltip("FaceAnimationOperator for Lau slot 1 (phase targets using Lau1).")]
    [SerializeField] FaceAnimationOperator lauFace1;

    [Tooltip("FaceAnimationOperator for Lau slot 2 (phase targets using Lau2).")]
    [SerializeField] FaceAnimationOperator lauFace2;

    [Tooltip("Seconds to rotate Face Player toward the look target. 0 = snap.")]
    [SerializeField] float faceTurnDuration = 0.75f;

    [SerializeField] Ease faceTurnEase = Ease.InOutCubic;

    Coroutine _holdRoutine;
    Camera _activeCutsceneCamera;
    bool _subscribedToDialogue;
    Tween _faceTween;
    bool _hasDialogueFaceFocus;
    CutsceneLookTarget _dialogueFaceTarget;

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
        KillFaceTween(releasePoseDriven: true);
        ClearDialogueFaceFocus();

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

    /// <summary>
    /// Standard dialogue start: focuses the matching FaceAnimationOperator from the phase list,
    /// and optionally rotates <see cref="facePlayer"/> toward that look target.
    /// </summary>
    public void NotifyStandardDialogueStarted(int storyPhase, bool rotatePlayerTowardTarget)
    {
        ApplyDialogueFaceFocus(storyPhase);

        if (rotatePlayerTowardTarget)
            TryFaceTargetForStoryPhase(storyPhase);
    }

    /// <summary>
    /// True when <paramref name="face"/> belongs to the character currently being spoken to.
    /// Mandy1/Mandy2 share Mandy; Lau1/Lau2 share Lau.
    /// </summary>
    public bool IsDialogueFaceFocus(FaceAnimationOperator face)
    {
        if (!_hasDialogueFaceFocus || face == null)
            return false;

        switch (_dialogueFaceTarget)
        {
            case CutsceneLookTarget.Mandy1:
            case CutsceneLookTarget.Mandy2:
                return face == mandyFace1 || face == mandyFace2;
            case CutsceneLookTarget.Lau1:
            case CutsceneLookTarget.Lau2:
                return face == lauFace1 || face == lauFace2;
            default:
                return false;
        }
    }

    /// <summary>
    /// Called by <see cref="DialogueTrigger"/> when Standard dialogue starts and no pose mark is used.
    /// Rotates the player toward the look target bound to <paramref name="storyPhase"/>.
    /// </summary>
    public bool TryFaceTargetForStoryPhase(int storyPhase)
    {
        if (storyPhase < 0)
            return false;

        if (!TryResolveLookTarget(storyPhase, out Transform target) || target == null)
            return false;

        return FaceTarget(target, faceTurnDuration);
    }

    void ApplyDialogueFaceFocus(int storyPhase)
    {
        _hasDialogueFaceFocus = storyPhase >= 0
            && TryResolveLookTargetId(storyPhase, out _dialogueFaceTarget);
    }

    void ClearDialogueFaceFocus()
    {
        _hasDialogueFaceFocus = false;
    }

    bool TryResolveLookTargetId(int storyPhase, out CutsceneLookTarget id)
    {
        id = default;
        if (phaseLookTargets == null)
            return false;

        for (int i = 0; i < phaseLookTargets.Count; i++)
        {
            CutsceneLookPhaseBinding binding = phaseLookTargets[i];
            if (binding == null || binding.storyPhase != storyPhase)
                continue;

            id = binding.target;
            return true;
        }

        return false;
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

    /// <summary>Cancel any hold timer, disable cutscene cams, and restore the player visual/camera.</summary>
    public void ReturnToPlayerCamera()
    {
        StopHoldRoutine();
        DisableAllCutsceneCameras();
        _activeCutsceneCamera = null;
        SetPlayerVisualActive(true);
        RestorePlayerCamera();
    }

    void ActivateCutsceneCamera(Camera target)
    {
        KillFaceTween(releasePoseDriven: true);
        StopHoldRoutine();
        EnsurePlayerRefs();

        DisableAllCutsceneCameras();
        // Player root stays active; only hide visuals and mute the player cam/listener.
        SetCameraEnabled(playerCamera, false);
        SetCameraEnabled(target, true);
        _activeCutsceneCamera = target;

        SetPlayerVisualActive(false);
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

    bool TryResolveLookTarget(int storyPhase, out Transform target)
    {
        target = null;
        if (!TryResolveLookTargetId(storyPhase, out CutsceneLookTarget id))
            return false;

        target = GetLookTarget(id);
        return target != null;
    }

    Transform GetLookTarget(CutsceneLookTarget id)
    {
        switch (id)
        {
            case CutsceneLookTarget.Mandy1: return mandyTarget1;
            case CutsceneLookTarget.Mandy2: return mandyTarget2;
            case CutsceneLookTarget.Lau1: return lauTarget1;
            case CutsceneLookTarget.Lau2: return lauTarget2;
            default: return null;
        }
    }

    bool FaceTarget(Transform target, float duration)
    {
        if (!EnsureFacePlayer())
            return false;

        KillFaceTween(releasePoseDriven: false);

        Vector3 eye = GetFaceOriginPosition();
        Vector3 to = target.position - eye;
        if (to.sqrMagnitude < 0.0001f)
            return false;

        Quaternion endRot = Quaternion.LookRotation(to.normalized);
        Vector3 pos = facePlayer.transform.position;
        Quaternion startRot = GetPlayerWorldLookRotation();

        facePlayer.PoseDriven = true;

        if (duration <= 0f)
        {
            facePlayer.ApplyWorldPose(pos, endRot);
            facePlayer.PoseDriven = false;
            return true;
        }

        _faceTween = DOVirtual.Float(0f, 1f, duration, t =>
            {
                if (facePlayer == null)
                    return;

                facePlayer.ApplyWorldPose(pos, Quaternion.SlerpUnclamped(startRot, endRot, t));
            })
            .SetEase(faceTurnEase)
            .OnComplete(() =>
            {
                if (facePlayer != null)
                {
                    facePlayer.ApplyWorldPose(pos, endRot);
                    facePlayer.PoseDriven = false;
                }

                _faceTween = null;
            })
            .OnKill(() =>
            {
                _faceTween = null;
            });

        return true;
    }

    Vector3 GetFaceOriginPosition()
    {
        if (facePlayer.dialogueFaceOrigin != null)
            return facePlayer.dialogueFaceOrigin.position;

        if (facePlayer.playerCamera != null)
            return facePlayer.playerCamera.transform.position;

        return facePlayer.transform.position;
    }

    Quaternion GetPlayerWorldLookRotation()
    {
        float yaw = facePlayer.transform.eulerAngles.y;
        float pitch = 0f;

        if (facePlayer.playerCamera != null)
        {
            pitch = facePlayer.playerCamera.transform.localEulerAngles.x;
            if (pitch > 180f)
                pitch -= 360f;
        }

        return Quaternion.Euler(pitch, yaw, 0f);
    }

    void KillFaceTween(bool releasePoseDriven)
    {
        if (_faceTween != null && _faceTween.IsActive())
            _faceTween.Kill();

        _faceTween = null;

        if (releasePoseDriven && facePlayer != null)
            facePlayer.PoseDriven = false;
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

    void SetPlayerVisualActive(bool active)
    {
        GameObject visual = ResolvePlayerVisual();
        if (visual != null && visual.activeSelf != active)
            visual.SetActive(active);
    }

    GameObject ResolvePlayerVisual()
    {
        if (playerVisual != null)
            return playerVisual;

        if (playerObject != null)
        {
            Transform child = playerObject.transform.Find(PlayerVisualChildName);
            if (child != null)
                return child.gameObject;
        }

        if (EnsureFacePlayer())
        {
            Transform root = facePlayer.transform.root;
            Transform child = root.Find(PlayerVisualChildName);
            if (child != null)
                return child.gameObject;
        }

        return null;
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

    bool EnsureFacePlayer()
    {
        if (facePlayer != null)
            return true;

        facePlayer = FindFirstObjectByType<PlayerController>();
        return facePlayer != null;
    }

    void EnsurePlayerRefs()
    {
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
        // Pager knot completion can fire while Standard dialogue is still active
        // (inbox suppress during PlayEndingCutscene). Don't steal the cutscene cam.
        if (IsStandardDialogueActive())
            return;

        ClearDialogueFaceFocus();
        KillFaceTween(releasePoseDriven: true);

        if (_activeCutsceneCamera != null || _holdRoutine != null)
            ReturnToPlayerCamera();
    }
}
