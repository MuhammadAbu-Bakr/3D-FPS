using UnityEngine;
using UnityEngine.InputSystem;

public class playerlook : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private Vector2 mouseInput;
    
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerCamera;
    
    private float xRotation = 0f;
    private float yRotation = 0f;
    
    void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse input
        mouseInput = inputActions.Player.Look.ReadValue<Vector2>();
        
        // Calculate rotation
        xRotation -= mouseInput.y * mouseSensitivity;
        yRotation += mouseInput.x * mouseSensitivity;
        
        // Clamp vertical rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // Apply rotation to camera
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }
}
