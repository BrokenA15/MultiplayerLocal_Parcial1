using UnityEngine;
using Unity.Netcode;

public class ArrowProjectile : NetworkBehaviour
{
    [SerializeField] private int damage = 35;
    [SerializeField] private float lifeTime = 10f;

    private Rigidbody rb;
    private bool hitSomething;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float force)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction * force, ForceMode.Impulse);

        Invoke(nameof(DestroyArrow), lifeTime);
    }

    private void Update()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.right = rb.linearVelocity.normalized;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (hitSomething) return;

        hitSomething = true;

        if (collision.collider.TryGetComponent(
            out PlayerController player))
        {
            player.TakeDamage(damage);
        }

        Invoke(nameof(EndTurnAndDestroy), 0.5f);
    }

    void DestroyArrow()
    {
        if (!IsServer) return;

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    void EndTurnAndDestroy()
    {
        if (!IsServer) return;

        TurnManager1.Instance.EndTurnServerRpc();

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}