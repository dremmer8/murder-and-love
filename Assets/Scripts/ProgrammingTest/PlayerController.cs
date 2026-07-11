using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    [Tooltip("Transforms that should move and rotate with this character controller.")]
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

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private int numberOfJumps;
    private bool canMove = true;

    private float defaultRadius;
    private Vector3 defaultCenter;
    private Vector3 defaultCameraPosition;

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

        if (playerCamera != null)
        {
            rotationX = playerCamera.transform.localEulerAngles.x;
            defaultCameraPosition = playerCamera.transform.localPosition;
        }
    }

    private void HandleStateChange(GameState newState)
    {
        canMove = newState == GameState.Gameplay;
    }

    void Update()
    {
        Debug.Log(GameStateManager.CurrentState);
        
        if (!canMove)
        {
            ApplyGravityOnly();
            return;
        }

        if (playerCamera != null)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            rotationX += -lookInput.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            
            transform.Rotate(0, lookInput.x * lookSpeed, 0);
        }

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float horizontal = moveInput.x;
        float vertical = moveInput.y;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = sprintAction.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (crouchAction.IsPressed())
        {
            characterController.radius = Mathf.Min(defaultRadius, crouchHeight / 2f);
            characterController.height = crouchHeight;
            characterController.center = new Vector3(defaultCenter.x, crouchHeight / 2f, defaultCenter.z);
            currentSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            characterController.radius = defaultRadius;
            characterController.center = defaultCenter;
        }

        if (playerCamera != null)
        {
            float cameraY = characterController.height - (defaultHeight - defaultCameraPosition.y);
            playerCamera.transform.localPosition = new Vector3(defaultCameraPosition.x, cameraY, defaultCameraPosition.z);
        }

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
        Vector3 positionBefore = transform.position;
        characterController.Move(motion);
        SyncFollowMovement(transform.position - positionBefore);
    }

    private void SyncFollowMovement(Vector3 positionDelta)
    {
        if (followMovement == null)
            return;

        foreach (Transform target in followMovement)
        {
            if (target == null || target == transform)
                continue;

            if (positionDelta != Vector3.zero)
                target.position += positionDelta;

            target.rotation = transform.rotation;
        }
    }
}