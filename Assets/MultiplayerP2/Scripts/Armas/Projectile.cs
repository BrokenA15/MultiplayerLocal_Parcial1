using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    private Rigidbody rb;
    private ulong ownerId;

    private float explosionMultiplier = 1f;

    [Header("Configuración de Bala")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float radioExplosion = 3.0f;
    [SerializeField] private float fuerzaExplosion = 15f;

    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float force, ulong shooterId)
    {
        ownerId = shooterId;

        // 🔑 FIX: SpawnWithOwnership no asigna PlayerObject — buscamos por OwnerClientId
        PlayerController[] todosLosPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController pc in todosLosPlayers)
        {
            if (pc.OwnerClientId == shooterId)
            {
                explosionMultiplier = pc.explosionMultiplier.Value;
                break;
            }
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        float mass = rb.mass > 0f ? rb.mass : 1f;
        rb.AddForce(direction * (force / mass), ForceMode.VelocityChange);

        // 🔑 FIX IMPULSO: Ignorar colisión física entre el proyectil y su dueño
        // para que no colisione con el personaje al salir del shootPoint
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Collider proyectilCol = GetComponent<Collider>();
        foreach (PlayerController pc in players)
        {
            if (pc.OwnerClientId == shooterId)
            {
                Collider[] ownerCols = pc.GetComponentsInChildren<Collider>();
                foreach (Collider c in ownerCols)
                    Physics.IgnoreCollision(proyectilCol, c, true);
            }
        }

        Invoke(nameof(Timeout), lifeTime);
    }

    void Timeout()
    {
        if (!IsServer || hasHit) return;
        FinalizarConDelay();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || hasHit) return;

        hasHit = true;

        Vector3 puntoImpacto = collision.contacts[0].point;
        Debug.Log($"Impacto en: {puntoImpacto}. Procesando explosión tipo Worms...");

        ProcesarExplosionMecanica(puntoImpacto);
        FinalizarConDelay();
    }

    void ProcesarExplosionMecanica(Vector3 origen)
    {
        float finalRadius = radioExplosion * explosionMultiplier;
        Collider[] objetosCercanos = Physics.OverlapSphere(origen, finalRadius);

        foreach (Collider col in objetosCercanos)
        {
            if (col.CompareTag("Suelo"))
            {
                if (col.TryGetComponent(out NetworkObject netObj))
                {
                    if (netObj.IsSpawned) netObj.Despawn();
                }
                else
                {
                    Destroy(col.gameObject);
                }
                continue;
            }

            if (col.CompareTag("Barrera"))
            {
                if (col.TryGetComponent(out BarrierHealth barrier))
                    barrier.RecibirImpacto();
                continue;
            }

            if (col.CompareTag("Player"))
            {
                // 🔑 FIX: Ignorar completamente al jugador que disparó — ni daño ni knockback
                PlayerController victim = col.GetComponent<PlayerController>();
                if (victim == null) continue;
                if (victim.OwnerClientId == ownerId) continue;

                // Daño
                if (!victim.shieldActive.Value)
                    victim.TakeDamage(20);

                // Knockback
                Rigidbody victimRb = col.GetComponent<Rigidbody>();
                if (victimRb != null)
                {
                    Vector3 direccion = (col.transform.position - origen).normalized;
                    direccion.y += 0.5f;
                    victimRb.AddForce(direccion * fuerzaExplosion, ForceMode.Impulse);
                }
            }
        }
    }

    void FinalizarConDelay()
    {
        Invoke(nameof(HandleEnd), 1.2f);
    }

    void HandleEnd()
    {
        if (!IsServer) return;

        TurnManager1.Instance.EndTurnServerRpc();

        MoveCameraClientRpc(TurnManager1.Instance.currentTurn.Value);

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void MoveCameraClientRpc(ulong newTurn)
    {
        if (CameraManager.Instance == null || TurnManager1.Instance == null) return;

        // 🔑 FIX: Buscar el personaje activo por NetworkObjectId en lugar de MoveToPlayerByTurn
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}