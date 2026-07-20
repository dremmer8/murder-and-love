using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class NamedCamera
{
    public string Name;
    public Camera Camera;
}

/// <summary>
/// Swaps or smoothly transitions between named cameras via a proxy camera.
/// Transition: current off → proxy on → blend proxy toward live end pose → proxy off → end on.
/// End camera position/rotation/FOV are sampled every frame so moving targets stay seamless.
/// Also tweens the player to dialogue pose marks (no automatic return).
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Cameras")]
    [Tooltip("Named cameras available for swap / transition.")]
    public List<NamedCamera> Cameras = new();

    [Tooltip("Temporary camera used during transitions. Created automatically if empty.")]
    [SerializeField] Camera proxyCamera;

    [Header("Transition")]
    [SerializeField] float defaultTransitionDuration = 1f;
    [SerializeField] Ease transitionEase = Ease.InOutCubic;

    [Header("Player Pose")]
    [Tooltip("Player to move into dialogue marks. Auto-found if empty.")]
    [SerializeField] PlayerController player;

    [SerializeField] float defaultPlayerTweenDuration = 1f;
    [SerializeField] Ease playerTweenEase = Ease.InOutCubic;

    Camera _activeCamera;
    Tween _transitionTween;
    bool _isTransitioning;

    Tween _playerTween;
    bool _isPlayerTweening;

    public Camera ActiveCamera => _activeCamera;
    public Camera ProxyCamera => proxyCamera;
    public bool IsTransitioning => _isTransitioning;
    public bool IsPlayerTweening => _isPlayerTweening;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{name}: Duplicate CameraManager — destroying this instance.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        EnsureProxyCamera();
        EnsurePlayer();
        InitializeActiveCamera();
    }

    void OnDestroy()
    {
        KillTransition(disableProxy: true);
        KillPlayerTween(releasePoseDriven: true);
        if (Instance == this)
            Instance = null;
    }

    /// <summary> Instantly activate the named camera and disable all others. </summary>
    public bool SwapCamera(string cameraName)
    {
        if (!TryGetCamera(cameraName, out Camera target))
            return false;

        if (_isTransitioning)
            KillTransition(disableProxy: true);

        SetActiveCamera(target);
        return true;
    }

    /// <summary> Smoothly transition to the named camera using the proxy camera. </summary>
    public bool TransitionToCamera(string cameraName)
    {
        return TransitionToCamera(cameraName, defaultTransitionDuration);
    }

    /// <summary> Smoothly transition to the named camera over <paramref name="duration"/> seconds. </summary>
    public bool TransitionToCamera(string cameraName, float duration)
    {
        if (!TryGetCamera(cameraName, out Camera endCamera))
            return false;

        if (_activeCamera == endCamera && !_isTransitioning)
            return true;

        if (_activeCamera == null)
        {
            SetActiveCamera(endCamera);
            return true;
        }

        if (duration <= 0f)
            return SwapCamera(cameraName);

        EnsureProxyCamera();

        // Continue from the current proxy view if a blend is already running (no pop).
        bool continueFromProxy = _isTransitioning && proxyCamera != null && proxyCamera.enabled;
        KillTransition(disableProxy: !continueFromProxy);

        if (!continueFromProxy)
        {
            CopyCameraPose(_activeCamera, proxyCamera);
            SetCameraEnabled(_activeCamera, false);
            SetCameraEnabled(proxyCamera, true);
            DisableAllListedCameras();
        }
        else
        {
            DisableAllListedCameras();
            SetCameraEnabled(proxyCamera, true);
        }

        _isTransitioning = true;

        Transform proxyTransform = proxyCamera.transform;
        Vector3 startPos = proxyTransform.position;
        Quaternion startRot = proxyTransform.rotation;
        float startFov = proxyCamera.fieldOfView;
        float startOrthoSize = proxyCamera.orthographicSize;
        bool startOrtho = proxyCamera.orthographic;

        _transitionTween = DOVirtual.Float(0f, 1f, duration, t =>
            {
                if (endCamera == null)
                    return;

                Transform endTransform = endCamera.transform;
                proxyTransform.SetPositionAndRotation(
                    Vector3.LerpUnclamped(startPos, endTransform.position, t),
                    Quaternion.SlerpUnclamped(startRot, endTransform.rotation, t));

                proxyCamera.orthographic = endCamera.orthographic;
                if (endCamera.orthographic || startOrtho)
                    proxyCamera.orthographicSize = Mathf.LerpUnclamped(startOrthoSize, endCamera.orthographicSize, t);
                else
                    proxyCamera.fieldOfView = Mathf.LerpUnclamped(startFov, endCamera.fieldOfView, t);
            })
            .SetEase(transitionEase)
            .OnComplete(() =>
            {
                if (endCamera != null)
                    CopyCameraPose(endCamera, proxyCamera);

                SetCameraEnabled(proxyCamera, false);
                SetActiveCamera(endCamera);
                _isTransitioning = false;
                _transitionTween = null;
            })
            .OnKill(() =>
            {
                _isTransitioning = false;
                _transitionTween = null;
            });

        return true;
    }

    /// <summary>
    /// Tweens the player to <paramref name="mark"/> position and rotation.
    /// Does not store a return pose — gameplay resumes from wherever the tween ends.
    /// </summary>
    public bool TweenPlayerTo(Transform mark)
    {
        return TweenPlayerTo(mark, defaultPlayerTweenDuration);
    }

    /// <summary>
    /// Tweens the player to <paramref name="mark"/> over <paramref name="duration"/> seconds.
    /// Duration &lt;= 0 snaps instantly. Does not tween back afterward.
    /// </summary>
    public bool TweenPlayerTo(Transform mark, float duration)
    {
        if (mark == null)
        {
            Debug.LogWarning($"{name}: TweenPlayerTo called with null mark.", this);
            return false;
        }

        if (!EnsurePlayer())
        {
            Debug.LogWarning($"{name}: No PlayerController found for TweenPlayerTo.", this);
            return false;
        }

        KillPlayerTween(releasePoseDriven: false);

        Vector3 startPos = player.transform.position;
        Quaternion startRot = GetPlayerWorldLookRotation();
        Vector3 endPos = mark.position;
        Quaternion endRot = mark.rotation;

        player.PoseDriven = true;

        if (duration <= 0f)
        {
            player.ApplyWorldPose(endPos, endRot);
            player.PoseDriven = false;
            _isPlayerTweening = false;
            return true;
        }

        _isPlayerTweening = true;
        _playerTween = DOVirtual.Float(0f, 1f, duration, t =>
            {
                if (player == null)
                    return;

                player.ApplyWorldPose(
                    Vector3.LerpUnclamped(startPos, endPos, t),
                    Quaternion.SlerpUnclamped(startRot, endRot, t));
            })
            .SetEase(playerTweenEase)
            .OnComplete(() =>
            {
                if (player != null)
                {
                    player.ApplyWorldPose(endPos, endRot);
                    player.PoseDriven = false;
                }

                _isPlayerTweening = false;
                _playerTween = null;
            })
            .OnKill(() =>
            {
                _isPlayerTweening = false;
                _playerTween = null;
            });

        return true;
    }

    /// <summary>Stops an in-progress player pose tween. Leaves the player where they are.</summary>
    public void StopPlayerTween()
    {
        KillPlayerTween(releasePoseDriven: true);
    }

    public bool TryGetCamera(string cameraName, out Camera camera)
    {
        camera = null;
        if (string.IsNullOrWhiteSpace(cameraName))
        {
            Debug.LogWarning($"{name}: Camera name is empty.", this);
            return false;
        }

        string trimmed = cameraName.Trim();
        for (int i = 0; i < Cameras.Count; i++)
        {
            NamedCamera entry = Cameras[i];
            if (entry == null || entry.Camera == null || string.IsNullOrEmpty(entry.Name))
                continue;

            if (string.Equals(entry.Name, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                camera = entry.Camera;
                return true;
            }
        }

        Debug.LogWarning($"{name}: No camera named '{trimmed}' in the list.", this);
        return false;
    }

    bool EnsurePlayer()
    {
        if (player != null)
            return true;

        player = FindFirstObjectByType<PlayerController>();
        return player != null;
    }

    Quaternion GetPlayerWorldLookRotation()
    {
        float yaw = player.transform.eulerAngles.y;
        float pitch = 0f;

        if (player.playerCamera != null)
        {
            pitch = player.playerCamera.transform.localEulerAngles.x;
            if (pitch > 180f)
                pitch -= 360f;
        }

        return Quaternion.Euler(pitch, yaw, 0f);
    }

    void KillPlayerTween(bool releasePoseDriven)
    {
        if (_playerTween != null && _playerTween.IsActive())
            _playerTween.Kill();

        _playerTween = null;
        _isPlayerTweening = false;

        if (releasePoseDriven && player != null)
            player.PoseDriven = false;
    }

    void InitializeActiveCamera()
    {
        Camera firstEnabled = null;
        for (int i = 0; i < Cameras.Count; i++)
        {
            NamedCamera entry = Cameras[i];
            if (entry?.Camera == null)
                continue;

            if (entry.Camera.enabled && entry.Camera.gameObject.activeInHierarchy)
            {
                firstEnabled = entry.Camera;
                break;
            }

            if (firstEnabled == null)
                firstEnabled = entry.Camera;
        }

        if (firstEnabled != null)
            SetActiveCamera(firstEnabled);
        else if (Camera.main != null)
            SetActiveCamera(Camera.main);
    }

    void SetActiveCamera(Camera target)
    {
        DisableAllListedCameras();
        if (proxyCamera != null)
            SetCameraEnabled(proxyCamera, false);

        _activeCamera = target;
        if (_activeCamera != null)
            SetCameraEnabled(_activeCamera, true);
    }

    void DisableAllListedCameras()
    {
        for (int i = 0; i < Cameras.Count; i++)
        {
            NamedCamera entry = Cameras[i];
            if (entry?.Camera != null)
                SetCameraEnabled(entry.Camera, false);
        }
    }

    static void SetCameraEnabled(Camera cam, bool enabled)
    {
        if (cam == null)
            return;

        cam.enabled = enabled;

        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = enabled;
    }

    static void CopyCameraPose(Camera from, Camera to)
    {
        to.transform.SetPositionAndRotation(from.transform.position, from.transform.rotation);
        to.fieldOfView = from.fieldOfView;
        to.orthographic = from.orthographic;
        to.orthographicSize = from.orthographicSize;
        to.nearClipPlane = from.nearClipPlane;
        to.farClipPlane = from.farClipPlane;
    }

    void EnsureProxyCamera()
    {
        if (proxyCamera != null)
            return;

        var go = new GameObject("ProxyCamera");
        go.transform.SetParent(transform, false);
        proxyCamera = go.AddComponent<Camera>();
        proxyCamera.enabled = false;

        if (go.GetComponent<AudioListener>() == null)
            go.AddComponent<AudioListener>().enabled = false;
    }

    void KillTransition(bool disableProxy)
    {
        if (_transitionTween != null && _transitionTween.IsActive())
            _transitionTween.Kill();

        _transitionTween = null;
        _isTransitioning = false;

        if (disableProxy && proxyCamera != null)
            SetCameraEnabled(proxyCamera, false);
    }
}
