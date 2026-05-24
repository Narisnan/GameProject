using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputActions))]
public class PlayerLook : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] float lookSensitivity = 0.15f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;
    [SerializeField] Vector3 cameraLocalPosition = new(0f, 1.6f, 0f);

    PlayerInputActions inputProvider;
    InputAction lookAction;
    float pitch;

    void Awake()
    {
        inputProvider = GetComponent<PlayerInputActions>();

        if (cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraTransform = cam.transform;
        }

        if (cameraTransform != null)
            cameraTransform.localPosition = cameraLocalPosition;

        var map = inputProvider.PlayerMap;
        if (map != null)
            lookAction = map.FindAction("Look");
        else
            lookAction = CreateFallbackLookAction();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        lookAction?.Enable();
    }

    void OnDisable()
    {
        lookAction?.Disable();
    }

    void Update()
    {
        Vector2 delta = lookAction.ReadValue<Vector2>();

        float yaw = delta.x * lookSensitivity;
        float pitchDelta = -delta.y * lookSensitivity;

        transform.Rotate(0f, yaw, 0f, Space.Self);

        pitch = Mathf.Clamp(pitch + pitchDelta, minPitch, maxPitch);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    static InputAction CreateFallbackLookAction()
    {
        var look = new InputAction("Look", InputActionType.Value);
        look.AddBinding("<Mouse>/delta");
        return look;
    }
}
