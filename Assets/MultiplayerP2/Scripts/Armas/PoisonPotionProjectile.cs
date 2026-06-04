using UnityEngine;
using Unity.Netcode;

public class PoisonPotionProjectile : NetworkBehaviour
{
    [Header("Veneno")]
    [SerializeField] private float poisonRadius = 3f;

    [SerializeField] private float poisonDuration = 5f;

    [SerializeField] private int poisonDamagePerSecond = 4;

    [Header("Proyectil")]
    [SerializeField] private float lifeTime = 10f;

    private Rigidbody rb;

    private bool hasExploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float force)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction * force, ForceMode.Impulse);

        Invoke(nameof(Timeout), lifeTime);
    }

    void Timeout()
    {
        if (!IsServer) return;

        if (hasExploded) return;

        Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (hasExploded) return;

        Explode();
    }

    private void Explode()
    {
        hasExploded = true;

        Collider[] targets =
            Physics.OverlapSphere(
                transform.position,
                poisonRadius);

        foreach (Collider col in targets)
        {
            if (col.TryGetComponent(
                out PlayerController player))
            {
                player.ApplyPoison(
                    poisonDuration,
                    poisonDamagePerSecond);
            }
        }

        Invoke(nameof(EndTurnAndDestroy), 0.5f);
    }

    void EndTurnAndDestroy()
    {
        if (!IsServer) return;

        TurnManager1.Instance.EndTurnServerRpc();

        MoveCameraClientRpc(
            TurnManager1.Instance.currentTurn.Value);

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void MoveCameraClientRpc(ulong newTurn)
    {
        if (CameraManager.Instance == null || TurnManager1.Instance == null) return;

        // Buscamos el personaje activo y le damos su Transform a la cámara
        ulong activoId = TurnManager1.Instance.activeCharacterNetworkId.Value;
        PlayerAction[] personajes = FindObjectsByType<PlayerAction>(FindObjectsSortMode.None);

        foreach (PlayerAction p in personajes)
        {
            if (p.NetworkObject.NetworkObjectId == activoId)
            {
                CameraManager.Instance.FollowTarget(p.transform);
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            poisonRadius);
    }
}