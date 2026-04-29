using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    private Rigidbody rb;
    private ulong ownerId;
    private bool ended = false; 
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

        // :stopwatch: destrucción automática si no golpea nada
        Invoke(nameof(Timeout), lifeTime);
    }

    // :stopwatch: caso: NO golpeó nada
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

        CancelInvoke(nameof(Timeout));


        /*
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
        */
        
        // :person_standing: JUGADOR
        if (collision.gameObject.TryGetComponent(out PlayerController victim))
        {
            if (victim.OwnerClientId != ownerId)
            {
                victim.TakeDamage(20);

                // :fire: NUEVO
                TurnManager.Instance.RegisterHit(ownerId);

               
            }
        }

       
        FinalizarConDelay();
    }

    // :boom: punto único de salida
    void FinalizarConDelay()
    {
        Invoke(nameof(HandleEnd), 0.6f);
    }

    void HandleEnd()
    {
        if (!IsServer) return;
        if (ended) return;
        
        ended = true;

        TurnManager.Instance.RegisterShot();
        TurnManager.Instance.EndTurnServerRpc();

        MoveCameraClientRpc(TurnManager.Instance.currentTurn.Value);

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void MoveCameraClientRpc(ulong newTurn)
    {
        CameraManager.Instance.MoveToPlayerByTurn(newTurn);
    }
}