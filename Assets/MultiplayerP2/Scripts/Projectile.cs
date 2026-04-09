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

        if (NetworkObject.IsSpawned)
        {
            TurnManager.Instance.EndTurnServerRpc();
            NetworkObject.Despawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (hasHit) return;

        hasHit = true;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(20);
        }

        // 💥 delay antes de regresar cámara
        ReturnCameraClientRpc(0.6f);

        Invoke(nameof(HandleImpact), 0.6f);
    }
    
    void HandleImpact()
    {
        if (!IsServer) return;

        TurnManager.Instance.EndTurnServerRpc();

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    void ReturnCameraClientRpc(float delay)
    {
        CameraManager.Instance.ReturnToCenterWithDelay(delay);
    }
}