using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WashingMachineId
{
    /// <summary>First washer: second-cloth grab + after start/exit.</summary>
    A = 0,
    /// <summary>Second washer: first-cloth grab + last-cloth grab.</summary>
    B = 1
}

public class WashingMachineClothOperator : MonoBehaviour, IMinigameStepHintSource
{
    public const string HintOpenDoor = "OpenDoor";
    public const string HintClothes = "Clothes";
    public const string HintCloseDoor = "CloseDoor";
    public const string HintDetergent = "Detergent";
    public const string HintToken = "Token";
    public const string HintStart = "Start";

    enum Step { OpenDoor, Clothes, CloseDoor, Detergent, Token, Start, Done }

    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Collider basketZone, barrelZone, door, tray, coinSlit, startButton;
    [SerializeField] Transform clothSet;
    [SerializeField] Transform[] cloths, destinations, track;
    [SerializeField] float follow = 14f, settle = 10f, barrelT = 0.88f, directionSpeed = 0.8f, emptyScaleY = 0.2f, dragPad = 0.25f;
    [SerializeField] string directionParam = "direction";

    [Header("Minigame")]
    [SerializeField] WashingMachineId machineId = WashingMachineId.A;
    [SerializeField] MinigameActivator minigameActivator;
    [SerializeField] float exitDelayAfterStart = 1.5f;

    [Tooltip("Washer B only: seconds after last-cloth (and blackout SFX) before exit + blackout routine.")]
    [SerializeField] float exitDelayAfterLastCloth = 2f;

    [Tooltip("Fired when this washing machine minigame exits.")]
    public DoWorkTrigger doWorkTrigger;

    [Header("Blackout cleanup")]
    [Tooltip("If true, blackout exits this minigame and hides held / transferable clothes.")]
    [SerializeField] bool cleanupClothesOnBlackout = true;

    [Tooltip("Extra cloth / prop objects to hide with the transferables (not in the Cloths array).")]
    [SerializeField] List<GameObject> additionalTransferableClothes = new();

    [Header("Washer A — dialogues")]
    [Tooltip("Fired when the player grabs the second cloth.")]
    public DialogueTrigger secondClothDialogue;
    [Tooltip("Fired after the wash cycle starts and the minigame exits.")]
    public DialogueTrigger afterMachineDialogue;

    [Header("Washer B — dialogues")]
    [Tooltip("Fired when the player grabs the first cloth.")]
    public DialogueTrigger firstClothDialogue;
    [Tooltip("Fired when the player grabs the last cloth.")]
    public DialogueTrigger lastClothDialogue;

    int idx = -1, snapIdx = -1, done;
    Step step = Step.OpenDoor;
    float t, tVel, direction, directionVel, fullScaleY, targetScaleY, scaleYVel;
    Vector3 posVel;
    float totalLen;
    float[] segLen;
    bool snapping;
    bool secondClothDialogueFired;
    bool firstClothDialogueFired;
    bool lastClothDialogueFired;
    bool completing;
    bool wasMinigameActive;
    BakedLightingController _lighting;

    public WashingMachineId MachineId => machineId;

    int ClothCount
    {
        get
        {
            if (cloths == null) return 0;
            int n = cloths.Length;
            while (n > 0 && !cloths[n - 1]) n--;
            return n;
        }
    }

    Transform GetDest(int i)
    {
        if (destinations == null || destinations.Length == 0) return null;
        return destinations[Mathf.Clamp(i, 0, destinations.Length - 1)];
    }

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (animator) direction = animator.GetFloat(directionParam);
        if (clothSet) { fullScaleY = targetScaleY = clothSet.localScale.y; }
        if (!minigameActivator)
            minigameActivator = GetComponentInParent<MinigameActivator>();
        RebuildTrack();
    }

    void OnEnable()
    {
        BindLighting();
    }

    void OnDisable()
    {
        UnbindLighting();
    }

    void BindLighting()
    {
        UnbindLighting();
        _lighting = BakedLightingController.Instance;
        if (_lighting == null)
            _lighting = FindFirstObjectByType<BakedLightingController>();
        if (_lighting != null)
            _lighting.OnLightingStateChanged += OnLightingStateChanged;
    }

    void UnbindLighting()
    {
        if (_lighting != null)
        {
            _lighting.OnLightingStateChanged -= OnLightingStateChanged;
            _lighting = null;
        }
    }

    void OnLightingStateChanged(BakedLightingController.LightingState state)
    {
        if (!cleanupClothesOnBlackout)
            return;
        if (state != BakedLightingController.LightingState.Blackout)
            return;

        // Blackout can start mid-drag — drop held cloth, hide transferables, leave minigame.
        AbortClothInteraction();
        HideAllTransferableClothes();

        if (minigameActivator != null)
        {
            if (minigameActivator.IsActivated)
                minigameActivator.Exit();
            minigameActivator.LockInteraction();
        }
    }

    void OnValidate() => RebuildTrack();

    void RebuildTrack()
    {
        if (track == null || track.Length < 2) return;
        segLen = new float[track.Length - 1];
        totalLen = 0f;
        for (int i = 0; i < segLen.Length; i++)
            totalLen += segLen[i] = Vector3.Distance(track[i].position, track[i + 1].position);
    }

    void Update()
    {
        UpdateDirection();
        UpdateSetScale();
        WatchMinigameExit();
        if (completing || step == Step.Done) return;
        if (snapping) { Snap(); return; }
        if (Input.GetMouseButtonDown(0) && !TrySequenceClick()) TryGrab();
        if (idx < 0) return;
        if (Input.GetMouseButton(0)) Drag();
        else Release();
    }

    void WatchMinigameExit()
    {
        if (minigameActivator == null) return;
        bool active = minigameActivator.IsActivated;
        if (wasMinigameActive && !active)
        {
            AbortClothInteraction();
            HideAllTransferableClothes();
        }
        wasMinigameActive = active;
    }

    /// <summary>Stops mid-drag / snap so a floating held cloth does not keep updating.</summary>
    void AbortClothInteraction()
    {
        completing = true;
        snapping = false;
        idx = -1;
        snapIdx = -1;
        posVel = Vector3.zero;
        tVel = 0f;
    }

    void HideAllTransferableClothes()
    {
        if (cloths != null)
        {
            for (int i = 0; i < cloths.Length; i++)
            {
                if (cloths[i])
                    cloths[i].gameObject.SetActive(false);
            }
        }

        if (clothSet)
            clothSet.gameObject.SetActive(false);

        if (additionalTransferableClothes != null)
        {
            for (int i = 0; i < additionalTransferableClothes.Count; i++)
            {
                if (additionalTransferableClothes[i] != null)
                    additionalTransferableClothes[i].SetActive(false);
            }
        }
    }

    void UpdateDirection()
    {
        if (!animator) return;
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input -= 1f;
        if (Input.GetKey(KeyCode.D)) input += 1f;
        if (input != 0f)
            direction = Mathf.SmoothDamp(direction, input, ref directionVel, 1f / directionSpeed);
        else
            directionVel = 0f;
        animator.SetFloat(directionParam, direction);
    }

    void UpdateSetScale()
    {
        if (!clothSet || !clothSet.gameObject.activeInHierarchy) return;
        var s = clothSet.localScale;
        s.y = Mathf.SmoothDamp(s.y, targetScaleY, ref scaleYVel, 1f / settle);
        clothSet.localScale = s;
    }

    bool TrySequenceClick()
    {
        if (step == Step.Done || !animator) return false;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 200f, GameLayers.MinigameZoneMask)) return false;
        var c = hit.collider;
        switch (step)
        {
            case Step.OpenDoor when c == door:
                animator.SetTrigger("DoOpen");
                step = Step.Clothes;
                return true;
            case Step.CloseDoor when c == door:
                animator.SetTrigger("DoClose");
                step = Step.Detergent;
                return true;
            case Step.Detergent when c == tray:
                animator.SetTrigger("DoDetergent");
                step = Step.Token;
                return true;
            case Step.Token when c == coinSlit:
                animator.SetTrigger("DoToken");
                step = Step.Start;
                return true;
            case Step.Start when c == startButton:
                animator.SetTrigger("DoStart");
                step = Step.Done;
                StartCoroutine(CompleteAfterStart());
                return true;
        }
        return c == door || c == tray || c == coinSlit || c == startButton;
    }

    IEnumerator CompleteAfterStart()
    {
        completing = true;
        yield return new WaitForSeconds(exitDelayAfterStart);

        if (minigameActivator != null)
        {
            if (minigameActivator.IsActivated)
                minigameActivator.Exit();
            minigameActivator.LockInteraction();
        }

        if (doWorkTrigger != null)
            doWorkTrigger.DoWork();

        if (machineId == WashingMachineId.A)
            FireDialogue(afterMachineDialogue);
    }

    void TryGrab()
    {
        if (step != Step.Clothes || idx >= 0) return;
        int count = ClothCount;
        if (done >= count || !cloths[done]) return;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, 200f, GameLayers.MinigameZoneMask);
        bool ok = false;
        var c = cloths[done];
        foreach (var hit in hits)
        {
            if (hit.collider == basketZone) { ok = true; break; }
            if (hit.transform == c || hit.transform.IsChildOf(c)) { ok = true; break; }
        }
        if (!ok) return;
        int clothIndex = done;
        idx = done;
        t = 0f;
        tVel = 0f;
        posVel = Vector3.zero;
        c.SetParent(null, true);
        if (clothSet && count > 0)
            targetScaleY = Mathf.Lerp(fullScaleY, emptyScaleY, (done + 1) / (float)count);

        SoundManager.PlayOneShot("getCloth", c.position);
        OnClothGrabbed(clothIndex, count);
    }

    void OnClothGrabbed(int clothIndex, int count)
    {
        switch (machineId)
        {
            case WashingMachineId.A:
                // Second cloth is index 1 (0-based).
                if (clothIndex == 1 && !secondClothDialogueFired)
                {
                    secondClothDialogueFired = true;
                    FireDialogue(secondClothDialogue);
                }
                break;

            case WashingMachineId.B:
                if (clothIndex == 0 && !firstClothDialogueFired)
                {
                    firstClothDialogueFired = true;
                    FireDialogue(firstClothDialogue);
                }

                if (count > 0 && clothIndex == count - 1 && !lastClothDialogueFired)
                {
                    lastClothDialogueFired = true;
                    // Blackout SFX starts the 2s lead-in; exit + lighting happen after the delay.
                    SoundManager.PlayOneShot("blackout", transform.position);
                    StartCoroutine(ExitAfterLastCloth());
                }
                break;
        }
    }

    IEnumerator ExitAfterLastCloth()
    {
        if (exitDelayAfterLastCloth > 0f)
            yield return new WaitForSeconds(exitDelayAfterLastCloth);

        completing = true;

        if (minigameActivator != null)
        {
            if (minigameActivator.IsActivated)
                minigameActivator.Exit();
            minigameActivator.LockInteraction();
        }

        if (doWorkTrigger != null)
            doWorkTrigger.DoWork();

        // Usual blackout: swap baked lighting (dialogue may also call SetBlackout).
        BakedLightingController lighting = _lighting != null
            ? _lighting
            : BakedLightingController.Instance;
        if (lighting == null)
            lighting = FindFirstObjectByType<BakedLightingController>();
        if (lighting != null)
            lighting.ApplyBlackout();

        FireDialogue(lastClothDialogue);
    }

    void FireDialogue(DialogueTrigger trigger)
    {
        if (trigger == null)
            return;

        trigger.TryStartDialogue();
    }

    void Drag()
    {
        if (idx < 0 || cloths == null || idx >= cloths.Length || !cloths[idx])
        {
            AbortClothInteraction();
            return;
        }

        float st = 1f / follow;
        float target = ScreenTrackT();
        t = Mathf.SmoothDamp(t, target, ref tVel, st);
        DampTo(cloths[idx], SamplePos(t), SampleRot(t), st);
    }

    float ScreenTrackT()
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        for (int i = 0; i < track.Length; i++)
        {
            if (!track[i]) continue;
            float x = cam.WorldToScreenPoint(track[i].position).x;
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
        }
        float span = Mathf.Max(maxX - minX, 1f);
        float pad = span * dragPad;
        return Mathf.Clamp01(Mathf.InverseLerp(minX - pad, maxX + pad, Input.mousePosition.x));
    }

    void DampTo(Transform tr, Vector3 pos, Quaternion rot, float smoothTime)
    {
        tr.position = Vector3.SmoothDamp(tr.position, pos, ref posVel, smoothTime);
        tr.rotation = Quaternion.Slerp(tr.rotation, rot, 1f - Mathf.Exp(-Time.deltaTime / smoothTime));
    }

    void Release()
    {
        if (idx < 0 || cloths == null || idx >= cloths.Length || !cloths[idx])
        {
            AbortClothInteraction();
            return;
        }

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        bool ok = t >= barrelT || (barrelZone && barrelZone.Raycast(ray, out _, 200f));
        if (ok && snapIdx < 0) { snapIdx = idx; snapping = true; }
        else if (!ok) { cloths[idx].SetPositionAndRotation(track[0].position, track[0].rotation); idx = -1; posVel = Vector3.zero; if (clothSet && ClothCount > 0) targetScaleY = Mathf.Lerp(fullScaleY, emptyScaleY, done / (float)ClothCount); }
    }

    void Snap()
    {
        var dst = GetDest(snapIdx);
        if (snapIdx < 0 || snapIdx >= ClothCount || !cloths[snapIdx] || !dst)
        {
            snapping = false;
            snapIdx = idx = -1;
            return;
        }
        var tr = cloths[snapIdx];
        float st = 1f / settle;
        DampTo(tr, dst.position, dst.rotation, st);
        if ((tr.position - dst.position).sqrMagnitude > 1e-4f || Quaternion.Angle(tr.rotation, dst.rotation) > 0.5f) return;
        tr.SetPositionAndRotation(dst.position, dst.rotation);
        tr.SetParent(dst, true);
        done++;
        if (done >= ClothCount) step = Step.CloseDoor;
        idx = snapIdx = -1;
        posVel = Vector3.zero;
        snapping = false;
    }

    Vector3 SamplePos(float u)
    {
        float d = u * totalLen;
        for (int i = 0; i < segLen.Length; i++)
        {
            if (d > segLen[i]) { d -= segLen[i]; continue; }
            return Vector3.Lerp(track[i].position, track[i + 1].position, segLen[i] > 1e-6f ? d / segLen[i] : 0f);
        }
        return track[track.Length - 1].position;
    }

    Quaternion SampleRot(float u)
    {
        float d = u * totalLen;
        for (int i = 0; i < segLen.Length; i++)
        {
            if (d > segLen[i]) { d -= segLen[i]; continue; }
            return Quaternion.Slerp(track[i].rotation, track[i + 1].rotation, segLen[i] > 1e-6f ? d / segLen[i] : 0f);
        }
        return track[track.Length - 1].rotation;
    }

    public bool TryGetCurrentStepHintId(out string stepId)
    {
        stepId = null;

        if (completing || step == Step.Done || snapping || idx >= 0)
            return false;

        if (minigameActivator != null && !minigameActivator.IsActivated)
            return false;

        switch (step)
        {
            case Step.OpenDoor:
                stepId = HintOpenDoor;
                return true;
            case Step.Clothes:
                stepId = HintClothes;
                return true;
            case Step.CloseDoor:
                stepId = HintCloseDoor;
                return true;
            case Step.Detergent:
                stepId = HintDetergent;
                return true;
            case Step.Token:
                stepId = HintToken;
                return true;
            case Step.Start:
                stepId = HintStart;
                return true;
            default:
                return false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (track == null || track.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < track.Length - 1; i++)
            if (track[i] && track[i + 1])
                Gizmos.DrawLine(track[i].position, track[i + 1].position);
    }
}
