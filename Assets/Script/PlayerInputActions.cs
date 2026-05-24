using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-100)]
public class PlayerInputActions : MonoBehaviour
{
    const string DefaultInputActionsPath = "Assets/InputSystem_Actions.inputactions";

    [SerializeField] InputActionAsset inputActions;

    public InputActionAsset Asset => inputActions;

    void Awake()
    {
        if (inputActions != null)
            return;

#if UNITY_EDITOR
        inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(DefaultInputActionsPath);
#endif
    }

    public InputActionMap PlayerMap => inputActions != null ? inputActions.FindActionMap("Player") : null;

    void OnEnable()
    {
        inputActions?.Enable();
    }

    void OnDisable()
    {
        inputActions?.Disable();
    }
}
