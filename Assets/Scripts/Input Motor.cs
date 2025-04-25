using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class InputMotor : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standardHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isCrouching;
    private bool isSprinting;
    private float targetHeight;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();
        targetHeight = standardHeight;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Jump.canceled += OnJump;
        inputActions.Player.Crouch.performed += OnCrouch;
        inputActions.Player.Crouch.canceled += OnCrouch;
        inputActions.Player.Sprint.performed += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Jump.canceled -= OnJump;
        inputActions.Player.Crouch.performed -= OnCrouch;
        inputActions.Player.Crouch.canceled -= OnCrouch;
        inputActions.Player.Sprint.performed -= OnSprint;
        inputActions.Player.Sprint.canceled -= OnSprint;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = context.performed;
        if (jumpPressed && isCrouching)
        {
            isCrouching = false;
            targetHeight = standardHeight;
        }
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = context.performed;
        targetHeight = isCrouching ? crouchHeight : standardHeight;
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed && !isCrouching;
    }

    private void Update()
    {
        UpdateCharacterHeight();
        HandleMovement();
    }

    private void UpdateCharacterHeight()
    {
        float currentHeight = controller.height;
        if (currentHeight != targetHeight)
        {
            Vector3 originalPos = transform.position;
            float newHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            
            if (newHeight > currentHeight)
            {
                float radiusOffset = controller.radius * 0.5f;
                Vector3 rayStart = transform.position + Vector3.up * (currentHeight - radiusOffset);
                float rayLength = newHeight - currentHeight + radiusOffset;
                
                if (Physics.Raycast(rayStart, Vector3.up, out RaycastHit hit, rayLength))
                {
                    newHeight = currentHeight;
                    targetHeight = currentHeight;
                    isCrouching = true;
                }
            }

            controller.height = newHeight;
            
            Vector3 center = controller.center;
            center.y = newHeight * 0.5f;
            controller.center = center;

            float heightDifference = newHeight - currentHeight;
            transform.position = new Vector3(originalPos.x, originalPos.y + heightDifference * 0.5f, originalPos.z);
        }
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        float currentSpeed = moveSpeed;
        if (isSprinting) currentSpeed *= sprintSpeedMultiplier;
        if (isCrouching) currentSpeed *= crouchSpeedMultiplier;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = transform.right * move.x + transform.forward * move.z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (jumpPressed && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
