using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayerInputActions))]
public class PlayerGun : MonoBehaviour
{
    const string DefaultBulletPrefabPath = "Assets/Prefabs/Bullet.prefab";

    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireRate = 0.15f;

    PlayerInputActions inputProvider;
    InputAction attackAction;
    float nextFireTime;

    void Awake()
    {
#if UNITY_EDITOR
        if (bulletPrefab == null)
            bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultBulletPrefabPath);
#endif

        if (firePoint == null)
        {
            var gun = transform.Find("Main Camera/Gun");
            if (gun != null)
                firePoint = gun.Find("FirePoint");
        }

        inputProvider = GetComponent<PlayerInputActions>();
        var map = inputProvider.PlayerMap;
        if (map != null)
            attackAction = map.FindAction("Attack");
        else
            attackAction = CreateFallbackAttackAction();
    }

    void OnEnable()
    {
        attackAction?.Enable();
    }

    void OnDisable()
    {
        attackAction?.Disable();
    }

    void Update()
    {
        if (Time.time < nextFireTime)
            return;

        if (attackAction == null || !attackAction.WasPressedThisFrame())
            return;

        if (firePoint == null || bulletPrefab == null)
            return;

        nextFireTime = Time.time + fireRate;
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    static InputAction CreateFallbackAttackAction()
    {
        var attack = new InputAction("Attack", InputActionType.Button);
        attack.AddBinding("<Mouse>/leftButton");
        return attack;
    }
}
