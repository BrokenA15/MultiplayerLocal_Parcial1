using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    private Rigidbody rb;
    private ulong ownerId;

    private float explosionMultiplier = 1f;

    [Header("Configuración de Bala")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float radioExplosion = 3.0f; // Tamaño del cráter/empuje
    [SerializeField] private float fuerzaExplosion = 15f; // Qué tan fuerte vuelan

    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float force, ulong shooterId)
    {
        ownerId = shooterId;
      /*                                          Nuevo                                            */
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(shooterId, out var client))
        {
            PlayerController player = client.PlayerObject.GetComponent<PlayerController>();

            if (player != null)
            {
                explosionMultiplier = player.explosionMultiplier.Value;
            }
        }
      /*                                                                                          */
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.Impulse);

        // ⏱️ destrucción automática si no golpea nada
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

        // Punto exacto donde la bala tocó algo
        Vector3 puntoImpacto = collision.contacts[0].point;

        Debug.Log($"Impacto en: {puntoImpacto}. Procesando explosión tipo Worms...");

        // 💥 EJECUTAR ONDA DE CHOQUE Y DESTRUCCIÓN
        ProcesarExplosionMecanica(puntoImpacto);

        // ⏱️ Esperar un poco antes de cambiar el turno (para ver el caos)
        FinalizarConDelay();
    }

    void ProcesarExplosionMecanica(Vector3 origen)
    {
        // Detectar todo en el radio de la explosión
        // Collider[] objetosCercanos = Physics.OverlapSphere(origen, radioExplosion); /*                Comentado                 */
        float finalRadius = radioExplosion * explosionMultiplier;

        Collider[] objetosCercanos =
            Physics.OverlapSphere(origen, finalRadius);/*                  Nuevo                                 */

        foreach (Collider col in objetosCercanos)
        {
            // 🧱 1. DESTRUIR SUELO Y BARRERAS
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
                {
                    barrier.RecibirImpacto();
                }
            
                continue;
            }

            // 🧍 2. DAÑAR Y EMPUJAR JUGADORES
            if (col.CompareTag("Player"))
            {
                // Aplicar Daño
                if (col.TryGetComponent(out PlayerController victim))
                {
                    // Si tiene escudo, ignoramos daño
                    if (!victim.shieldActive.Value)                          /*    Nuevo IF para el pwrup del escudo    */
                    {
                        victim.TakeDamage(20);
                    }
                }

                // Aplicar Empuje (Knockback)
                Rigidbody victimRb = col.GetComponent<Rigidbody>();
                if (victimRb != null)
                {
                    Vector3 direccion = (col.transform.position - origen).normalized;
                    direccion.y += 0.5f; // Impulso extra hacia arriba para que "vuelen"

                    victimRb.AddForce(direccion * fuerzaExplosion, ForceMode.Impulse);
                }
            }
        }
    }

    void FinalizarConDelay()
    {
        // Aumenté el delay a 1.2s para que la cámara pueda ver el empuje antes de cambiar
        Invoke(nameof(HandleEnd), 1.2f);
    }

    void HandleEnd()
    {
        if (!IsServer) return;

        // 🔄 cambiar turno
        TurnManager1.Instance.EndTurnServerRpc();

        // 🎥 mover cámara al nuevo jugador
        MoveCameraClientRpc(TurnManager1.Instance.currentTurn.Value);

        // 💣 destruir bala
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void MoveCameraClientRpc(ulong newTurn)
    {
        if (CameraManager.Instance != null)
            CameraManager.Instance.MoveToPlayerByTurn(newTurn);
    }

    // Visualizar el radio de la explosión en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}