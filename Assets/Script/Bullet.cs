using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 25f;
    [SerializeField] float lifetime = 3f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }
}
