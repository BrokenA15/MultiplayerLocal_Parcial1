using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    private Rigidbody rb;
    private ulong ownerId;

    [SerializeField] private float lifeTime = 5f;

    private bool hasHit = false;

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
        if (!IsServer) return;

        // ⏱️ si no golpeó nada, igual regresamos cámara
        ReturnCameraClientRpc(0.6f);

        Invoke(nameof(HandleEnd), 0.6f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (hasHit) return;

        hasHit = true;

        Debug.Log($"Bala golpeó a: {collision.gameObject.name} con Tag: {collision.gameObject.tag}");

        // 🧱 BARRERA
        if (collision.gameObject.CompareTag("Barrera"))
        {
            Debug.Log("BARRERA DETECTADA");

            if (collision.gameObject.TryGetComponent(out NetworkObject netObj))
            {
                if (netObj.IsSpawned) netObj.Despawn();
            }
            else
            {
                Destroy(collision.gameObject);
            }

            Impacto();
            return;
        }

        // 🧍 JUGADOR
        if (collision.gameObject.TryGetComponent(out PlayerController victim))
        {
            if (victim.OwnerClientId != ownerId)
            {
                victim.TakeDamage(20);
                Impacto();
            }
        }
    }

    // 💥 manejo central del impacto
    void Impacto()
    {
        // 🎥 delay para sentir el golpe
        ReturnCameraClientRpc(0.6f);

        Invoke(nameof(HandleEnd), 0.6f);
    }

    void HandleEnd()
    {
        if (!IsServer) return;

        // 🔄 cambiar turno
        TurnManager.Instance.EndTurnServerRpc();

        // 💣 destruir bala
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    // 📡 avisar a TODOS los clientes que regresen cámara
    [Rpc(SendTo.ClientsAndHost)]
    void ReturnCameraClientRpc(float delay)
    {
        CameraManager.Instance.ReturnToCenterWithDelay(delay);
    }
}