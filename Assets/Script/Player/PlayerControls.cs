using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    public PlayerInputActions controls;

    private InputAction move;
    private InputAction jump;
    private InputAction look;
    [SerializeField] private float moveSpeed = 5;
    private Rigidbody rb;

    //camera vars
    private Camera mainCamera;
    public float mouseSensitivity = 0.2f;
    public float cameraVerticalRotation = 0f;

    void Awake()
    {
        controls = new PlayerInputActions();
        mainCamera = Camera.main;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        move = controls.Player.Move; //player action map inside input actions
        move.Enable();

        look = controls.Player.Look;
        look.Enable();

        jump = controls.Player.Jump;
        jump.Enable();
        jump.performed += Jump;
    }

    private void OnDisable()
    {
        move.Disable();
        look.Disable();
        jump.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        //camera
        Vector2 lookInput = look.ReadValue<Vector2>() * mouseSensitivity;

        cameraVerticalRotation -= lookInput.y;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);
        mainCamera.transform.localEulerAngles = Vector3.right * cameraVerticalRotation;

        this.gameObject.transform.Rotate(Vector3.up * lookInput.x);

        //movement
        Vector2 moveInput = move.ReadValue<Vector2>();
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        forward.y = 0; // Ignore vertical component for forward movement
        right.y = 0; // Ignore vertical component for right movement

        //vector maths, adding vectors = new destination, multiplying a vector gives you a bigger arrow
        Vector3 desiredMoveDirection = (forward * moveInput.y + right * moveInput.x) * moveSpeed * Time.deltaTime;

        // Apply movement
        transform.position += desiredMoveDirection;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        rb.AddForce(new Vector3(0, 3, 0), ForceMode.Impulse);
    }
}
