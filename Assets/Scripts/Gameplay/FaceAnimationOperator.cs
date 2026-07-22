using UnityEngine;

/// <summary>
/// Procedural face life for an animated character. Runs in LateUpdate on top of the
/// Animator pose and drives three independent systems:
///
/// 1. Blinking — rotates the eyelid bone(s) on their local X axis from 0 (open) to
///    <see cref="blinkClosedAngle"/> (default 60, fully closed). Two triggers:
///      • Idle: a random blink from time to time.
///      • Reaction: a single blink the moment the head starts moving in world space.
///
/// 2. Neck + head look — when the player camera is near, the character naturally turns
///    to look at the player. It occasionally loses interest and lets the animation play
///    through, then re-engages. During dialogue the head + neck are hard-locked to the
///    player camera and never lose interest.
///
/// 3. Eyes — when the player is close the eyes track the camera, with a small amount of
///    gaze wander around the camera so the stare feels alive rather than robotic.
///
/// Head / neck bones keep following the Animator when the look weight is 0, so
/// "losing interest" blends back into the baked animation rather than snapping to rest.
/// </summary>
[DisallowMultipleComponent]
public class FaceAnimationOperator : MonoBehaviour
{
    [Header("Look Target")]
    [Tooltip("Player camera to look at. Auto-resolved from PlayerController / Camera.main if empty.")]
    [SerializeField] Transform lookTarget;

    [Header("Bones")]
    [Tooltip("Neck bone (outer). Takes a fraction of the head turn so the look reads through the shoulders.")]
    [SerializeField] Transform neckBone;

    [Tooltip("Head bone (inner). Also watched to fire the reaction blink when it starts moving.")]
    [SerializeField] Transform headBone;

    [Tooltip("Eye bones (usually two). Track the player with a little wander when close.")]
    [SerializeField] Transform[] eyeBones;

    [Tooltip("Local forward axis of the head / neck / eye bones (the axis that should point at the target).")]
    [SerializeField] Vector3 boneForwardAxis = Vector3.forward;

    [Header("Blink — Eyelids")]
    [Tooltip("Eyelid bones rotated to close the eyes. Assumed to be driven only by this script.")]
    [SerializeField] Transform[] eyelidBones;

    [Tooltip("Local axis the eyelids rotate around to close (X by default).")]
    [SerializeField] Vector3 blinkAxis = Vector3.right;

    [Tooltip("Angle (degrees) at which the eyelids count as fully closed. 0 = open.")]
    [Range(0f, 120f)]
    [SerializeField] float blinkClosedAngle = 60f;

    [Tooltip("Seconds for the lids to snap shut.")]
    [SerializeField] float blinkCloseTime = 0.06f;

    [Tooltip("Seconds the eyes stay shut at the bottom of a blink.")]
    [SerializeField] float blinkHoldTime = 0.02f;

    [Tooltip("Seconds for the lids to re-open.")]
    [SerializeField] float blinkOpenTime = 0.11f;

    [Header("Blink — Idle timing")]
    [Tooltip("Random seconds between idle blinks (min, max).")]
    [SerializeField] Vector2 blinkInterval = new(2.5f, 6f);

    [Tooltip("Chance (0..1) that an idle blink is immediately followed by a second quick blink.")]
    [Range(0f, 1f)]
    [SerializeField] float doubleBlinkChance = 0.15f;

    [Header("Blink — Head-movement reaction")]
    [Tooltip("Blink once when the (animation-driven) head starts rotating in world space.")]
    [SerializeField] bool blinkOnHeadMovement = true;

    [Tooltip("Head angular speed (deg/sec) above which the head counts as 'moving'.")]
    [SerializeField] float headMoveStartSpeed = 45f;

    [Tooltip("Head must be quieter than this (deg/sec) before another reaction blink can arm.")]
    [SerializeField] float headMoveStillSpeed = 12f;

    [Header("Head + Neck Look")]
    [Tooltip("Player must be within this distance for the character to look at them (outside dialogue).")]
    [SerializeField] float lookRange = 4.5f;

    [Tooltip("How strongly the head turns toward the target (0..1).")]
    [Range(0f, 1f)]
    [SerializeField] float headWeight = 0.85f;

    [Tooltip("How strongly the neck turns toward the target (0..1). Keep low for a subtle turn.")]
    [Range(0f, 1f)]
    [SerializeField] float neckWeight = 0.35f;

    [Tooltip("Max head yaw / pitch away from the animation pose (degrees).")]
    [SerializeField] Vector2 headYawPitchLimit = new(70f, 45f);

    [Tooltip("Max neck yaw / pitch away from the animation pose (degrees).")]
    [SerializeField] Vector2 neckYawPitchLimit = new(35f, 20f);

    [Tooltip("Seconds for the head/neck look weight to fade in / out. Higher = lazier.")]
    [SerializeField] float lookWeightSmooth = 0.4f;

    [Tooltip("Seconds of lag as the gaze point chases the moving camera. Higher = more relaxed follow.")]
    [SerializeField] float gazeFollowSmooth = 0.18f;

    [Header("Interest (outside dialogue)")]
    [Tooltip("Random seconds the character stays engaged before it may lose interest (min, max).")]
    [SerializeField] Vector2 interestDuration = new(3.5f, 8f);

    [Tooltip("Random seconds the character ignores the player after losing interest (min, max).")]
    [SerializeField] Vector2 disinterestDuration = new(1.5f, 4f);

    [Tooltip("Chance (0..1), evaluated when an interest window ends, that the character disengages.")]
    [Range(0f, 1f)]
    [SerializeField] float loseInterestChance = 0.5f;

    [Header("Eyes")]
    [Tooltip("Player must be within this distance for the eyes to track them.")]
    [SerializeField] float eyeRange = 3f;

    [Tooltip("How strongly the eyes point at the (wandering) gaze target (0..1).")]
    [Range(0f, 1f)]
    [SerializeField] float eyeWeight = 1f;

    [Tooltip("Max eye yaw / pitch away from the rest pose (degrees).")]
    [SerializeField] Vector2 eyeYawPitchLimit = new(28f, 18f);

    [Tooltip("Seconds for the eye look weight to fade in / out.")]
    [SerializeField] float eyeWeightSmooth = 0.15f;

    [Tooltip("Radius (metres, around the camera) the gaze wanders to fake micro-saccades. Keep small.")]
    [SerializeField] float eyeWanderRadius = 0.12f;

    [Tooltip("How quickly the eye wander offset drifts.")]
    [SerializeField] float eyeWanderSpeed = 0.6f;

    // --- runtime ---
    Quaternion[] _eyelidRest;
    Quaternion[] _eyeRest;

    // Blink state.
    bool _isBlinking;
    float _blinkPhaseTime;
    float _blinkTimer;
    bool _queuedDoubleBlink;

    // Head-movement reaction state.
    Vector3 _prevHeadForward;
    bool _hasPrevHeadForward;
    bool _headReactionArmed = true;

    // Look state.
    float _lookWeight;      // smoothed head/neck weight
    float _lookWeightVel;
    float _eyeWeightCurrent; // smoothed eye weight
    float _eyeWeightVel;
    Vector3 _gazePoint;      // smoothed world point the head/eyes aim at
    Vector3 _gazePointVel;
    bool _hasGazePoint;

    // Interest state.
    bool _interested = true;
    float _interestTimer;

    // Eye wander noise seed.
    Vector2 _wanderSeed;

    void Awake()
    {
        _eyelidRest = CaptureLocalRotations(eyelidBones);
        _eyeRest = CaptureLocalRotations(eyeBones);
        ScheduleNextBlink();
        _interestTimer = RandomIn(interestDuration);
        _wanderSeed = new Vector2(Random.value * 100f, Random.value * 100f);
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        ResolveTarget();

        bool dialogue = IsDialogueActive();

        UpdateHeadMovementReaction(dt);
        UpdateBlink(dt);
        UpdateInterest(dt, dialogue);
        UpdateGaze(dt, dialogue);
        ApplyLook();
    }

    // ---------------------------------------------------------------- Blink

    void UpdateHeadMovementReaction(float dt)
    {
        if (headBone == null)
            return;

        // headBone.rotation here is still the pure Animator pose (we overwrite it later
        // in ApplyLook, and the Animator resets it before the next LateUpdate).
        Vector3 forward = headBone.rotation * boneForwardAxis;

        if (!_hasPrevHeadForward)
        {
            _prevHeadForward = forward;
            _hasPrevHeadForward = true;
            return;
        }

        float speed = Vector3.Angle(_prevHeadForward, forward) / dt;
        _prevHeadForward = forward;

        if (!blinkOnHeadMovement)
            return;

        if (_headReactionArmed && speed >= headMoveStartSpeed)
        {
            TriggerBlink();
            _headReactionArmed = false;
        }
        else if (!_headReactionArmed && speed <= headMoveStillSpeed)
        {
            _headReactionArmed = true;
        }
    }

    void UpdateBlink(float dt)
    {
        if (!_isBlinking)
        {
            _blinkTimer -= dt;
            if (_blinkTimer <= 0f)
                TriggerBlink();
        }

        float blink01 = 0f;
        if (_isBlinking)
        {
            _blinkPhaseTime += dt;
            float total = blinkCloseTime + blinkHoldTime + blinkOpenTime;

            if (_blinkPhaseTime >= total)
            {
                _isBlinking = false;
                blink01 = 0f;

                if (_queuedDoubleBlink)
                {
                    _queuedDoubleBlink = false;
                    TriggerBlink();
                }
                else
                {
                    ScheduleNextBlink();
                }
            }
            else if (_blinkPhaseTime < blinkCloseTime)
            {
                blink01 = SafeDiv(_blinkPhaseTime, blinkCloseTime);
            }
            else if (_blinkPhaseTime < blinkCloseTime + blinkHoldTime)
            {
                blink01 = 1f;
            }
            else
            {
                float openT = _blinkPhaseTime - blinkCloseTime - blinkHoldTime;
                blink01 = 1f - SafeDiv(openT, blinkOpenTime);
            }

            blink01 = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(blink01));
        }

        ApplyBlink(blink01);
    }

    void TriggerBlink()
    {
        if (_isBlinking)
            return;

        _isBlinking = true;
        _blinkPhaseTime = 0f;

        // Only idle blinks may chain into a double blink (reaction blinks stay single).
        _queuedDoubleBlink = !_queuedDoubleBlink && Random.value < doubleBlinkChance;
    }

    void ScheduleNextBlink()
    {
        _blinkTimer = RandomIn(blinkInterval);
    }

    void ApplyBlink(float blink01)
    {
        if (eyelidBones == null)
            return;

        float angle = blinkClosedAngle * blink01;
        Quaternion offset = Quaternion.AngleAxis(angle, blinkAxis.sqrMagnitude > 1e-6f ? blinkAxis : Vector3.right);

        for (int i = 0; i < eyelidBones.Length; i++)
        {
            Transform lid = eyelidBones[i];
            if (lid == null || _eyelidRest == null || i >= _eyelidRest.Length)
                continue;

            lid.localRotation = _eyelidRest[i] * offset;
        }
    }

    // ---------------------------------------------------------- Interest

    void UpdateInterest(float dt, bool dialogue)
    {
        if (dialogue)
        {
            // Locked on during dialogue — always engaged, reset the interest cycle.
            _interested = true;
            _interestTimer = RandomIn(interestDuration);
            return;
        }

        _interestTimer -= dt;
        if (_interestTimer > 0f)
            return;

        if (_interested)
        {
            // Interest window ended — maybe drift off, otherwise stay engaged.
            if (Random.value < loseInterestChance)
            {
                _interested = false;
                _interestTimer = RandomIn(disinterestDuration);
            }
            else
            {
                _interestTimer = RandomIn(interestDuration);
            }
        }
        else
        {
            _interested = true;
            _interestTimer = RandomIn(interestDuration);
        }
    }

    // -------------------------------------------------------------- Gaze

    void UpdateGaze(float dt, bool dialogue)
    {
        float headTarget = 0f;
        float eyeTarget = 0f;

        if (lookTarget != null)
        {
            float distance = Vector3.Distance(GazeOrigin(), lookTarget.position);

            bool headEngaged = dialogue || (_interested && distance <= lookRange);
            bool eyeEngaged = dialogue || distance <= eyeRange;

            headTarget = headEngaged ? 1f : 0f;
            eyeTarget = eyeEngaged ? 1f : 0f;
        }

        _lookWeight = Mathf.SmoothDamp(_lookWeight, headTarget, ref _lookWeightVel, lookWeightSmooth);
        _eyeWeightCurrent = Mathf.SmoothDamp(_eyeWeightCurrent, eyeTarget, ref _eyeWeightVel, eyeWeightSmooth);

        if (lookTarget != null)
        {
            Vector3 desired = lookTarget.position;
            if (!_hasGazePoint)
            {
                _gazePoint = desired;
                _hasGazePoint = true;
            }
            else
            {
                _gazePoint = Vector3.SmoothDamp(_gazePoint, desired, ref _gazePointVel, gazeFollowSmooth);
            }
        }
    }

    Vector3 EyeGazePoint()
    {
        if (lookTarget == null)
            return _gazePoint;

        // Small drifting offset around the camera fakes idle micro-saccades.
        float t = Time.time * eyeWanderSpeed;
        float nx = (Mathf.PerlinNoise(_wanderSeed.x, t) - 0.5f) * 2f;
        float ny = (Mathf.PerlinNoise(_wanderSeed.y, t) - 0.5f) * 2f;

        Vector3 right = lookTarget.right;
        Vector3 up = lookTarget.up;
        return lookTarget.position + (right * nx + up * ny) * eyeWanderRadius;
    }

    // -------------------------------------------------------------- Apply

    void ApplyLook()
    {
        // Neck first (outer bone), then head (child inherits the neck turn), then eyes.
        if (neckBone != null && _lookWeight > 0.0001f)
            AimBoneFromAnimation(neckBone, _gazePoint, neckYawPitchLimit, _lookWeight * neckWeight);

        if (headBone != null && _lookWeight > 0.0001f)
            AimBoneFromAnimation(headBone, _gazePoint, headYawPitchLimit, _lookWeight * headWeight);

        ApplyEyes();
    }

    void ApplyEyes()
    {
        if (eyeBones == null)
            return;

        Vector3 gaze = EyeGazePoint();

        for (int i = 0; i < eyeBones.Length; i++)
        {
            Transform eye = eyeBones[i];
            if (eye == null || _eyeRest == null || i >= _eyeRest.Length)
                continue;

            // Eyes are driven only by us: rebuild a stable base from the parent + rest pose.
            Quaternion baseRot = (eye.parent != null ? eye.parent.rotation : Quaternion.identity) * _eyeRest[i];
            AimBone(eye, baseRot, gaze, eyeYawPitchLimit, _eyeWeightCurrent * eyeWeight);
        }
    }

    /// <summary>Aims a bone that is also animated, using its current Animator pose as the base.</summary>
    void AimBoneFromAnimation(Transform bone, Vector3 targetPoint, Vector2 yawPitchLimit, float weight)
    {
        AimBone(bone, bone.rotation, targetPoint, yawPitchLimit, weight);
    }

    /// <summary>
    /// Rotates <paramref name="bone"/> so <see cref="boneForwardAxis"/> points toward
    /// <paramref name="targetPoint"/>, clamped to a yaw/pitch cone around <paramref name="baseRot"/>,
    /// then blended in by <paramref name="weight"/>.
    /// </summary>
    void AimBone(Transform bone, Quaternion baseRot, Vector3 targetPoint, Vector2 yawPitchLimit, float weight)
    {
        weight = Mathf.Clamp01(weight);
        if (weight <= 0.0001f)
        {
            bone.rotation = baseRot;
            return;
        }

        Vector3 toTarget = targetPoint - bone.position;
        if (toTarget.sqrMagnitude < 1e-8f)
        {
            bone.rotation = baseRot;
            return;
        }

        Vector3 desiredWorld = toTarget.normalized;

        // Work in a space where the bone's forward axis is +Z so yaw/pitch are meaningful.
        Quaternion axisToZ = Quaternion.FromToRotation(
            boneForwardAxis.sqrMagnitude > 1e-6f ? boneForwardAxis : Vector3.forward,
            Vector3.forward);

        Vector3 localDir = axisToZ * (Quaternion.Inverse(baseRot) * desiredWorld);

        float yaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        float pitch = Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(yaw, -yawPitchLimit.x, yawPitchLimit.x);
        pitch = Mathf.Clamp(pitch, -yawPitchLimit.y, yawPitchLimit.y);

        Vector3 clampedZ = Quaternion.Euler(-pitch, yaw, 0f) * Vector3.forward;
        Vector3 clampedWorld = baseRot * (Quaternion.Inverse(axisToZ) * clampedZ);

        Quaternion delta = Quaternion.FromToRotation(baseRot * boneForwardAxis, clampedWorld);
        Quaternion aimed = delta * baseRot;

        bone.rotation = Quaternion.Slerp(baseRot, aimed, weight);
    }

    // ------------------------------------------------------------ Helpers

    Vector3 GazeOrigin() => headBone != null ? headBone.position : transform.position;

    void ResolveTarget()
    {
        if (lookTarget != null)
            return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && player.playerCamera != null)
        {
            lookTarget = player.playerCamera.transform;
            return;
        }

        if (Camera.main != null)
            lookTarget = Camera.main.transform;
    }

    static bool IsDialogueActive()
    {
        DialogueManager dialogue = DialogueManager.GetInstance();
        return dialogue != null && dialogue.dialogueIsPlaying;
    }

    static Quaternion[] CaptureLocalRotations(Transform[] bones)
    {
        if (bones == null)
            return System.Array.Empty<Quaternion>();

        var rots = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++)
            rots[i] = bones[i] != null ? bones[i].localRotation : Quaternion.identity;
        return rots;
    }

    static float RandomIn(Vector2 range) => Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));

    static float SafeDiv(float a, float b) => b <= 1e-6f ? 1f : a / b;

    void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? GazeOrigin()
            : (headBone != null ? headBone.position : transform.position);

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(origin, lookRange);

        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(origin, eyeRange);

        if (lookTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, lookTarget.position);
        }
    }
}
