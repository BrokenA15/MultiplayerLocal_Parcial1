using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    private Rigidbody rb;
    private ulong ownerId;
    [SerializeField] private float lifeTime = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float force, ulong shooterId)
    {
        ownerId = shooterId;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.Impulse);

        Invoke(nameof(DestroyProjectile), lifeTime);
    }

    void DestroyProjectile()
    {
        if (IsServer && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. REGLA DE ORO: Solo el servidor manda
        if (!IsServer) return;

        // Log para saber qué tocamos exactamente según Unity
        Debug.Log($"Bala golpeó a: {collision.gameObject.name} con Tag: {collision.gameObject.tag}");

        // 2. ¿Es una barrera? (Asegúrate que el Tag en Unity sea exactamente 'Barrera')
        if (collision.gameObject.CompareTag("Barrera"))
        {
            Debug.Log("¡BARRERA DETECTADA! Despawneando...");

            if (collision.gameObject.TryGetComponent(out NetworkObject netObj))
            {
                if (netObj.IsSpawned) netObj.Despawn();
            }
            else
            {
                Destroy(collision.gameObject);
            }

            FinalizarBala();
            return;
        }

        // 3. ¿Es el jugador enemigo?
        if (collision.gameObject.TryGetComponent(out PlayerController victim))
        {
            if (victim.OwnerClientId != ownerId)
            {
                victim.TakeDamage(20);
                FinalizarBala();
            }
        }
    }

    void FinalizarBala()
    {
        if (NetworkObject.IsSpawned) NetworkObject.Despawn();
    }
}