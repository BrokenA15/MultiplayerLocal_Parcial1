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

        // ⏱️ destrucción automática si no golpea nada
        Invoke(nameof(Timeout), lifeTime);
    }

    // ⏱️ caso: NO golpeó nada
    void Timeout()
    {
        if (!IsServer) return;
        if (hasHit) return;

        FinalizarConDelay();
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
            if (collision.gameObject.TryGetComponent(out NetworkObject netObj))
            {
                if (netObj.IsSpawned) netObj.Despawn();
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }

        // 🧍 JUGADOR
        if (collision.gameObject.TryGetComponent(out PlayerController victim))
        {
            if (victim.OwnerClientId != ownerId)
            {
                victim.TakeDamage(20);
            }
        }

        // 💥 impacto o cualquier colisión válida
        FinalizarConDelay();
    }

    // 💥 punto único de salida
    void FinalizarConDelay()
    {
        Invoke(nameof(HandleEnd), 0.6f);
    }

    void HandleEnd()
    {
        if (!IsServer) return;

        // 🔄 cambiar turno
        TurnManager.Instance.EndTurnServerRpc();

        // 🎥 mover cámara
        MoveCameraClientRpc(TurnManager.Instance.currentTurn.Value);

        // 💣 destruir bala
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void MoveCameraClientRpc(ulong newTurn)
    {
        CameraManager.Instance.MoveToPlayerByTurn(newTurn);
    }
}