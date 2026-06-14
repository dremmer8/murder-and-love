using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    [Header("Camera Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 85f;
    public Vector3 cameraOffset = new Vector3(0, 1.5f, 0);

    [Header("Crouch Settings")]
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private int numberOfJumps;
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
        {
            rotationX = playerCamera.transform.localEulerAngles.x;
        }
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (Input.GetKey(KeyCode.R) && canMove)
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
            
            moveDirection = (forward * inputDirection.z) + (right * inputDirection.x);
            moveDirection *= currentSpeed;
            moveDirection.y = movementDirectionY;
        }

        if (characterController.isGrounded)
        {
            numberOfJumps = 0;

            if (Input.GetButtonDown("Jump") && canMove)
            {
                moveDirection.y = jumpPower;
                numberOfJumps++;
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && canMove && numberOfJumps < 2)
            {
                moveDirection.y = jumpPower;
                numberOfJumps++;
            }

            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    void LateUpdate()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.position = transform.position + cameraOffset;
        }
    }
}