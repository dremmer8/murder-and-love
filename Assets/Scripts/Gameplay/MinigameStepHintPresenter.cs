using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// World-space step hints for minigames. Extension of the HUD control-hints system:
/// you build one <see cref="TextMeshPro"/> (3D / world-space — not UGUI) in the scene,
/// drag it in here, and this component moves/shows it at the current step's anchor
/// while a minigame is active.
///
/// Setup:
///   1. Create a 3D Object → Text - TextMeshPro (NOT UI → Text). Style it how you like.
///   2. Assign its root + TextMeshPro below. Keep it outside any Canvas.
///   3. On each <see cref="MinigameActivator"/>, fill Step Hints (step id, text, anchor).
///   4. Have the minigame operator implement <see cref="IMinigameStepHintSource"/>.
///
/// Billboards toward <see cref="CameraManager.ActiveCamera"/> (or the proxy during blends).
/// With Always On Top enabled, uses TMP's Overlay shader (ZTest Always) so the text is not
/// occluded by nearby meshes.
/// </summary>
public class MinigameStepHintPresenter : MonoBehaviour
{
    const string OverlayShaderName = "TextMeshPro/Distance Field Overlay";
    const string OverlayShaderNameMobile = "TextMeshPro/Mobile/Distance Field Overlay";

    [Header("Hint Object (world-space TextMeshPro — not UGUI)")]
    [Tooltip("Root toggled on/off and moved to the current step anchor. Must be a world object, not under a Canvas.")]
    [SerializeField] GameObject hintRoot;

    [Tooltip("3D TextMeshPro whose text is set from the active step entry. Auto-found under Hint Root if empty.")]
    [SerializeField] TextMeshPro hintLabel;

    [Tooltip("Offset from the step anchor, in the anchor's local space.")]
    [SerializeField] Vector3 anchorLocalOffset = new Vector3(0f, 0.15f, 0f);

    [Tooltip("If true, the hint root faces the active gameplay/minigame camera each frame.")]
    [SerializeField] bool faceCamera = true;

    [Tooltip("Optional camera override. When empty, uses CameraManager's active (or proxy) camera.")]
    [SerializeField] Camera targetCamera;

    [Header("Visibility (avoid mesh occlusion)")]
    [Tooltip("Switch the TMP material to the Overlay shader (ZTest Always) so other meshes cannot cover the hint.")]
    [SerializeField] bool alwaysOnTop = true;

    [Tooltip("Extra meters to pull the hint toward the camera so it sits in front of the anchor surface.")]
    [SerializeField] float pullTowardCamera = 0.08f;

    [Tooltip("MeshRenderer sorting order while shown. Higher draws later / on top of other transparent objects.")]
    [SerializeField] int sortingOrder = 5000;

    // Cached last-applied values so we only touch TMP / transform / SetActive when needed.
    bool _shown;
    string _text;
    Transform _anchor;
    Vector3 _lastPosition;
    Quaternion _lastRotation;
    bool _overlayConfigured;

    readonly List<MonoBehaviour> _behaviourBuffer = new();

    void OnEnable()
    {
        if (hintLabel == null && hintRoot != null)
            hintLabel = hintRoot.GetComponentInChildren<TextMeshPro>(true);

        _overlayConfigured = false;
        ConfigureVisibility();
        SetHint(false, null, null);
    }

    void LateUpdate()
    {
        if (!TryResolveActiveHint(out string text, out Transform anchor))
        {
            SetHint(false, null, null);
            return;
        }

        SetHint(true, text, anchor);
        ApplyTransform(anchor);
    }

    bool TryResolveActiveHint(out string text, out Transform anchor)
    {
        text = null;
        anchor = null;

        if (!MinigameActivator.IsAnyActive)
            return false;

        MinigameActivator active = MinigameActivator.ActiveInstance;
        if (active == null)
            return false;

        if (!TryFindSource(active, out IMinigameStepHintSource source))
            return false;

        if (!source.TryGetCurrentStepHintId(out string stepId) || string.IsNullOrEmpty(stepId))
            return false;

        if (!active.TryGetStepHint(stepId, out MinigameStepHintEntry entry))
            return false;

        Transform resolved = entry.ResolvedAnchor;
        string localized = active.GetLocalizedStepHintText(stepId);
        if (resolved == null || string.IsNullOrEmpty(localized))
            return false;

        text = localized;
        anchor = resolved;
        return true;
    }

    bool TryFindSource(MinigameActivator active, out IMinigameStepHintSource source)
    {
        source = null;
        _behaviourBuffer.Clear();
        active.GetComponentsInChildren(true, _behaviourBuffer);

        for (int i = 0; i < _behaviourBuffer.Count; i++)
        {
            if (_behaviourBuffer[i] is IMinigameStepHintSource candidate)
            {
                source = candidate;
                return true;
            }
        }

        return false;
    }

    void SetHint(bool show, string text, Transform anchor)
    {
        bool textChanged = show && text != _text;
        bool visibilityChanged = show != _shown;

        _shown = show;
        _text = text;
        _anchor = anchor;

        if (textChanged && hintLabel != null)
            hintLabel.text = text;

        if (visibilityChanged && hintRoot != null)
            hintRoot.SetActive(show);

        // Material instances reset when the GO is re-enabled — force overlay re-apply.
        if (show && visibilityChanged)
        {
            _overlayConfigured = false;
            ConfigureVisibility();
        }
    }

    void ApplyTransform(Transform anchor)
    {
        if (hintRoot == null || anchor == null)
            return;

        Vector3 position = anchor.TransformPoint(anchorLocalOffset);
        Quaternion rotation = hintRoot.transform.rotation;
        Camera cam = ResolveCamera();

        if (cam != null)
        {
            Vector3 toCam = cam.transform.position - position;
            float sqr = toCam.sqrMagnitude;
            if (sqr > 0.0001f)
            {
                Vector3 toCamDir = toCam / Mathf.Sqrt(sqr);

                if (pullTowardCamera > 0f)
                    position += toCamDir * pullTowardCamera;

                if (faceCamera)
                    rotation = Quaternion.LookRotation(-toCamDir, Vector3.up);
            }
        }

        if (position == _lastPosition && rotation == _lastRotation && _anchor == anchor)
            return;

        _lastPosition = position;
        _lastRotation = rotation;
        hintRoot.transform.SetPositionAndRotation(position, rotation);
    }

    void ConfigureVisibility()
    {
        if (hintLabel == null)
            return;

        Renderer renderer = hintLabel.renderer;
        if (renderer != null)
        {
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            // Occlusion culling can skip the draw even with ZTest Always.
            renderer.allowOcclusionWhenDynamic = false;
        }

        if (!alwaysOnTop)
            return;

        // Skip Shader.Find only when we already verified Overlay is on this material.
        Material mat = hintLabel.fontMaterial;
        if (_overlayConfigured && mat != null && IsOverlayShader(mat.shader))
            return;

        Shader overlay = Shader.Find(OverlayShaderName);
        if (overlay == null)
            overlay = Shader.Find(OverlayShaderNameMobile);

        if (overlay == null)
        {
            Debug.LogWarning(
                $"{name}: Overlay TMP shader not found — hint may be occluded by meshes. " +
                $"Expected '{OverlayShaderName}'.",
                this);
            _overlayConfigured = true;
            return;
        }

        // fontMaterial returns a unique instance; swapping to Overlay keeps atlas/face props.
        if (mat != null && mat.shader != overlay)
        {
            mat.shader = overlay;
            hintLabel.fontMaterial = mat;
        }

        _overlayConfigured = true;
    }

    static bool IsOverlayShader(Shader shader)
    {
        if (shader == null)
            return false;

        string n = shader.name;
        return n == OverlayShaderName || n == OverlayShaderNameMobile;
    }

    Camera ResolveCamera()
    {
        if (targetCamera != null)
            return targetCamera;

        CameraManager cameras = CameraManager.Instance;
        if (cameras != null)
        {
            // During blends the proxy is what actually renders.
            if (cameras.IsTransitioning
                && cameras.ProxyCamera != null
                && cameras.ProxyCamera.enabled)
                return cameras.ProxyCamera;

            if (cameras.ActiveCamera != null)
                return cameras.ActiveCamera;
        }

        return Camera.main;
    }
}
