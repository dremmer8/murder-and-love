using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    
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
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        crouchAction.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;
        
        characterController = GetComponent<CharacterController>();

        if (playerCamera != null)
        {
            rotationX = playerCamera.transform.localEulerAngles.x;
        }
    }

    void Update()
    {
        if (canMove && playerCamera != null)
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

        if (crouchAction.IsPressed() && canMove)
        {
            characterController.height = crouchHeight;
            currentSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
        }

        if (canMove)
        {
            float movementDirectionY = moveDirection.y;
            Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
            
            moveDirection = (forward * inputDirection.z * currentSpeed) + (right * inputDirection.x * currentSpeed);
            moveDirection.y = movementDirectionY;
        }

        if (characterController.isGrounded)
        {
            numberOfJumps = 0;

            if (jumpAction.WasPressedThisFrame() && canMove)
            {
                moveDirection.y = jumpPower;
                numberOfJumps++;
            }
        }
        else
        {
            if (jumpAction.WasPressedThisFrame() && canMove && numberOfJumps < 2)
            {
                moveDirection.y = jumpPower;
                numberOfJumps++;
            }

            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }
}