using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputActions))]
public class PlayeMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float groundCheckDistance = 0.25f;
    [SerializeField] LayerMask groundMask = ~0;
    [SerializeField] bool cameraRelative = true;

    Rigidbody rb;
    PlayerInputActions inputProvider;
    InputAction moveAction;
    InputAction jumpAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints |= RigidbodyConstraints.FreezeRotation;

        inputProvider = GetComponent<PlayerInputActions>();
        var map = inputProvider.PlayerMap;

        if (map != null)
        {
            moveAction = map.FindAction("Move");
            jumpAction = map.FindAction("Jump");
        }
        else
        {
            moveAction = CreateFallbackMoveAction();
            jumpAction = CreateFallbackJumpAction();
        }

        if (jumpAction != null)
            jumpAction.performed += OnJump;
    }

    void OnDestroy()
    {
        if (jumpAction != null)
            jumpAction.performed -= OnJump;
    }

    void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }

    void OnJump(InputAction.CallbackContext _)
    {
        if (!IsGrounded())
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundMask, QueryTriggerInteraction.Ignore);
    }

    void FixedUpdate()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Vector3 velocity = rb.linearVelocity;

        if (input.sqrMagnitude < 0.01f)
        {
            velocity.x = 0f;
            velocity.z = 0f;
            rb.linearVelocity = velocity;
            return;
        }

        Vector3 direction = GetMoveDirection(input);
        velocity.x = direction.x * moveSpeed;
        velocity.z = direction.z * moveSpeed;
        rb.linearVelocity = velocity;
    }

    Vector3 GetMoveDirection(Vector2 input)
    {
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraRelative && Camera.main != null)
        {
            forward = Camera.main.transform.forward;
            right = Camera.main.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        return (forward * input.y + right * input.x).normalized;
    }

    static InputAction CreateFallbackMoveAction()
    {
        var move = new InputAction("Move", InputActionType.Value);

        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        move.AddBinding("<Gamepad>/leftStick");
        return move;
    }

    static InputAction CreateFallbackJumpAction()
    {
        var jump = new InputAction("Jump", InputActionType.Button);
        jump.AddBinding("<Keyboard>/space");
        return jump;
    }
}
