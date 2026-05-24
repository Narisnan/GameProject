using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputActions))]
public class PlayeMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float groundedRememberTime = 0.15f;
    [SerializeField] LayerMask groundMask = ~0;
    [SerializeField] bool cameraRelative = true;

    Rigidbody rb;
    PlayerInputActions inputProvider;
    InputAction moveAction;
    InputAction jumpAction;
    bool usesSharedInputAsset;
    bool jumpQueued;
    float lastGroundedTime = float.NegativeInfinity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints |= RigidbodyConstraints.FreezeRotation;

        inputProvider = GetComponent<PlayerInputActions>();
        var map = inputProvider.PlayerMap;

        if (map != null)
        {
            usesSharedInputAsset = true;
            moveAction = map.FindAction("Move");
            jumpAction = map.FindAction("Jump");
        }
        else
        {
            usesSharedInputAsset = false;
            moveAction = CreateFallbackMoveAction();
            jumpAction = CreateFallbackJumpAction();
        }
    }

    void OnEnable()
    {
        if (!usesSharedInputAsset)
        {
            moveAction?.Enable();
            jumpAction?.Enable();
        }
    }

    void OnDisable()
    {
        if (!usesSharedInputAsset)
        {
            moveAction?.Disable();
            jumpAction?.Disable();
        }
    }

    void Update()
    {
        if (WasJumpPressed())
            jumpQueued = true;
    }

    bool WasJumpPressed()
    {
        if (jumpAction != null && jumpAction.WasPressedThisFrame())
            return true;

        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
    }

    void OnCollisionStay(Collision collision)
    {
        if (!IsValidGroundCollision(collision))
            return;

        lastGroundedTime = Time.time;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsValidGroundCollision(collision))
            return;

        lastGroundedTime = Time.time;
    }

    bool IsValidGroundCollision(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundMask) == 0)
            return false;

        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
                return true;
        }

        return false;
    }

    void FixedUpdate()
    {
        if (jumpQueued)
        {
            jumpQueued = false;
            TryJump();
        }

        Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
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

    void TryJump()
    {
        if (!IsGrounded())
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    bool IsGrounded()
    {
        return Time.time - lastGroundedTime <= groundedRememberTime;
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
