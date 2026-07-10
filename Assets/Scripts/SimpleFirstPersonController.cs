using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;

    private CharacterController characterController;
    private Camera playerCamera;
    private float verticalVelocity;
    private float cameraPitch;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (cameraRoot == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
            {
                cameraRoot = childCamera.transform;
            }
        }

        playerCamera = cameraRoot != null ? cameraRoot.GetComponent<Camera>() : GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();

        if (InteractPressed())
        {
            TryInteract();
        }

        if (EscapePressed())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Look()
    {
        Vector2 look = GetLookInput() * mouseSensitivity;

        transform.Rotate(Vector3.up * look.x);

        cameraPitch = Mathf.Clamp(cameraPitch - look.y, -maxLookAngle, maxLookAngle);
        if (cameraRoot != null)
        {
            cameraRoot.localEulerAngles = Vector3.right * cameraPitch;
        }
    }

    private void Move()
    {
        Vector2 moveInput = GetMoveInput();
        Vector3 input = Vector3.ClampMagnitude(new Vector3(moveInput.x, 0f, moveInput.y), 1f);
        Vector3 movement = transform.TransformDirection(input);
        float speed = SprintHeld() ? sprintSpeed : moveSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (characterController.isGrounded && JumpPressed())
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        movement = movement * speed + Vector3.up * verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    private void TryInteract()
    {
        Camera rayCamera = playerCamera != null ? playerCamera : Camera.main;
        if (rayCamera == null)
        {
            return;
        }

        Ray ray = new Ray(rayCamera.transform.position, rayCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            interactable?.Interact();
        }
    }

    private Vector2 GetMoveInput()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        Vector2 move = Vector2.zero;
        move.x += keyboard.dKey.isPressed ? 1f : 0f;
        move.x -= keyboard.aKey.isPressed ? 1f : 0f;
        move.y += keyboard.wKey.isPressed ? 1f : 0f;
        move.y -= keyboard.sKey.isPressed ? 1f : 0f;
        return Vector2.ClampMagnitude(move, 1f);
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    private Vector2 GetLookInput()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Mouse mouse = Mouse.current;
        return mouse != null ? mouse.delta.ReadValue() * 0.05f : Vector2.zero;
#else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
    }

    private bool JumpPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetButtonDown("Jump");
#endif
    }

    private bool SprintHeld()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftShift);
#endif
    }

    private bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool keyboardPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return keyboardPressed || mousePressed;
#else
        return Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
#endif
    }

    private bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
