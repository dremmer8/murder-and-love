using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private const float CrouchDropFactor = 0.85f;
    private const float CrouchLeanZ = 0.12f;

    [Header("References")]
    public Camera playerCamera;
    [Tooltip("Objects that follow player movement, rotation, and crouch (same height/lean as the camera).")]
    public Transform[] followMovement;
    
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 20f;

    [Header("Camera Settings")]
    public float lookSpeed = 0.2f; 
    public float lookXLimit = 85f;

    [Header("Crouch Settings")]
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    [Tooltip("How quickly the character drops into a crouch.")]
    public float crouchDownTime = 0.18f;
    [Tooltip("How quickly the character stands back up.")]
    public float standUpTime = 0.28f;
    [Tooltip("Extra clearance checked above the head before standing.")]
    public float standUpCheckPadding = 0.08f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private int numberOfJumps;
    private bool canMove = true;
    private bool poseDriven;
    private bool wantsCrouch;
    private float currentHeight;
    private float heightVelocity;
    private float cameraHeightVelocity;
    private float currentCrouchBlend;

    private float defaultRadius;
    private Vector3 defaultCenter;
    private float defaultBottom;
    private Vector3 defaultCameraPosition;
    private Vector3[] followDefaultLocalPositions;
    private float[] followHeightVelocities;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private void Awake()
    {
        moveAction = new InputAction("Move", InputActionType.Value, "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        lookAction = new InputAction("Look", InputActionType.Value, "Vector2");
        lookAction.AddBinding("<Pointer>/delta");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");

        sprintAction = new InputAction("Sprint", InputActionType.Button);
        sprintAction.AddBinding("<Keyboard>/leftShift");

        crouchAction = new InputAction("Crouch", InputActionType.Button);
        crouchAction.AddBinding("<Keyboard>/leftCtrl");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        crouchAction.Enable();
        
        GameStateManager.OnGameStateChanged += HandleStateChange;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        crouchAction.Disable();
        
        GameStateManager.OnGameStateChanged -= HandleStateChange;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        characterController = GetComponent<CharacterController>();
        
        defaultRadius = characterController.radius;
        defaultCenter = characterController.center;
        defaultBottom = defaultCenter.y - defaultHeight / 2f;
        currentHeight = characterController.height;

        if (playerCamera != null)
        {
            rotationX = playerCamera.transform.localEulerAngles.x;
            defaultCameraPosition = playerCamera.transform.localPosition;
        }

        CacheFollowDefaults();
    }

    private void CacheFollowDefaults()
    {
        if (followMovement == null || followMovement.Length == 0)
        {
            followDefaultLocalPositions = System.Array.Empty<Vector3>();
            followHeightVelocities = System.Array.Empty<float>();
            return;
        }

        followDefaultLocalPositions = new Vector3[followMovement.Length];
        followHeightVelocities = new float[followMovement.Length];

        for (int i = 0; i < followMovement.Length; i++)
        {
            Transform target = followMovement[i];
            if (target == null)
                continue;

            followDefaultLocalPositions[i] = transform.InverseTransformPoint(target.position);
        }
    }

    /// <summary>
    /// When true, movement / look / gravity are skipped so an external system
    /// (e.g. CameraManager player pose tween) can drive the transform.
    /// </summary>
    public bool PoseDriven
    {
        get => poseDriven;
        set => poseDriven = value;
    }

    /// <summary>
    /// Places the character at a world pose. Yaw drives the body; pitch drives the camera look.
    /// Safe to call while a CharacterController is present.
    /// </summary>
    public void ApplyWorldPose(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        float yaw = worldRotation.eulerAngles.y;
        float pitch = worldRotation.eulerAngles.x;
        if (pitch > 180f)
            pitch -= 360f;

        rotationX = Mathf.Clamp(pitch, -lookXLimit, lookXLimit);

        bool wasEnabled = characterController != null && characterController.enabled;
        if (wasEnabled)
            characterController.enabled = false;

        transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(0f, yaw, 0f));

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        if (wasEnabled)
            characterController.enabled = true;

        moveDirection = Vector3.zero;
        SyncFollowTargets();
    }

    private void HandleStateChange(GameState newState)
    {
        // Pager locks movement but keeps look free so the player can aim at the screen.
        canMove = newState == GameState.Gameplay;
    }

    void Update()
    {
        Debug.Log(GameStateManager.CurrentState);

        if (poseDriven)
            return;

        bool canLook = canMove || GameStateManager.CurrentState == GameState.Pager;

        if (canLook && playerCamera != null)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            rotationX += -lookInput.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            transform.Rotate(0, lookInput.x * lookSpeed, 0);
        }

        if (!canMove)
        {
            ApplyGravityOnly();
            return;
        }

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float horizontal = moveInput.x;
        float vertical = moveInput.y;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = sprintAction.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        wantsCrouch = crouchAction.IsPressed();
        UpdateCrouch(ref currentSpeed);

        float movementDirectionY = moveDirection.y;
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
            
        moveDirection = (forward * inputDirection.z * currentSpeed) + (right * inputDirection.x * currentSpeed);
        moveDirection.y = movementDirectionY;

        if (characterController.isGrounded)
        {
            numberOfJumps = 0;

            if (jumpAction.WasPressedThisFrame())
            {
                moveDirection.y = jumpPower;
                numberOfJumps++;
            }
        }
        else
        {
            if (jumpAction.WasPressedThisFrame() && numberOfJumps < 2)
            {
                moveDirection.y = jumpPower;
                numberOfJumps++;
            }

            moveDirection.y -= gravity * Time.deltaTime;
        }

        MoveAndSyncFollowers(moveDirection * Time.deltaTime);
    }

    private void UpdateCrouch(ref float currentSpeed)
    {
        float targetHeight = wantsCrouch ? crouchHeight : defaultHeight;

        // Stay crouched while something is overhead.
        if (!wantsCrouch && !CanStandUp())
            targetHeight = crouchHeight;

        float transitionTime = targetHeight < currentHeight ? crouchDownTime : standUpTime;
        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, transitionTime);
        currentHeight = Mathf.Clamp(currentHeight, crouchHeight, defaultHeight);

        ApplyCapsuleHeight(currentHeight);

        currentCrouchBlend = Mathf.InverseLerp(defaultHeight, crouchHeight, currentHeight);
        currentSpeed = Mathf.Lerp(currentSpeed, crouchSpeed, currentCrouchBlend);

        UpdateCrouchCamera(currentCrouchBlend);
    }

    private void ApplyCapsuleHeight(float height)
    {
        float minHeight = defaultRadius * 2f;
        height = Mathf.Max(height, minHeight);

        characterController.height = height;
        characterController.radius = Mathf.Min(defaultRadius, height / 2f);
        characterController.center = new Vector3(defaultCenter.x, defaultBottom + height / 2f, defaultCenter.z);
    }

    private bool CanStandUp()
    {
        float standTop = defaultBottom + defaultHeight;
        float currentTop = defaultBottom + currentHeight;
        float castDistance = standTop - currentTop + standUpCheckPadding;

        if (castDistance <= 0f)
            return true;

        float radius = Mathf.Max(0.01f, characterController.radius - characterController.skinWidth);
        Vector3 origin = transform.TransformPoint(new Vector3(defaultCenter.x, currentTop - radius, defaultCenter.z));

        return !Physics.SphereCast(
            origin,
            radius,
            transform.up,
            out _,
            castDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetCrouchedLocalPosition(Vector3 standingLocal, float crouchBlend, float currentLocalY, ref float yVelocity)
    {
        float crouchedY = standingLocal.y - (defaultHeight - crouchHeight) * CrouchDropFactor * crouchBlend;
        float transitionTime = wantsCrouch ? crouchDownTime : standUpTime;
        float smoothedY = Mathf.SmoothDamp(currentLocalY, crouchedY, ref yVelocity, transitionTime);
        float leanZ = Mathf.Lerp(standingLocal.z, standingLocal.z + CrouchLeanZ, crouchBlend);

        return new Vector3(standingLocal.x, smoothedY, leanZ);
    }

    private void UpdateCrouchCamera(float crouchBlend)
    {
        if (playerCamera == null)
            return;

        Vector3 crouchedLocal = GetCrouchedLocalPosition(
            defaultCameraPosition,
            crouchBlend,
            playerCamera.transform.localPosition.y,
            ref cameraHeightVelocity);

        playerCamera.transform.localPosition = crouchedLocal;
    }

    private void ApplyGravityOnly()
    {
        if (!characterController.isGrounded)
        {
            moveDirection.x = 0;
            moveDirection.z = 0;
            moveDirection.y -= gravity * Time.deltaTime;
            MoveAndSyncFollowers(moveDirection * Time.deltaTime);
        }
    }

    private void MoveAndSyncFollowers(Vector3 motion)
    {
        characterController.Move(motion);
        SyncFollowTargets();
    }

    private void SyncFollowTargets()
    {
        if (followMovement == null || followDefaultLocalPositions == null)
            return;

        for (int i = 0; i < followMovement.Length; i++)
        {
            Transform target = followMovement[i];
            if (target == null || target == transform)
                continue;

            if (playerCamera != null && target == playerCamera.transform)
                continue;

            Vector3 currentLocal = transform.InverseTransformPoint(target.position);
            Vector3 crouchedLocal = GetCrouchedLocalPosition(
                followDefaultLocalPositions[i],
                currentCrouchBlend,
                currentLocal.y,
                ref followHeightVelocities[i]);

            target.position = transform.TransformPoint(crouchedLocal);
            target.rotation = transform.rotation;
        }
    }
}
