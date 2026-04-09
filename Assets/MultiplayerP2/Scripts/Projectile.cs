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
        if (IsServer)
            NetworkObject.Despawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();

        if (netObj != null && netObj.OwnerClientId == ownerId)
            return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(20); 
        }
        

        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}