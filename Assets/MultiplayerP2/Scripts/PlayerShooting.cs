using UnityEngine;
using Unity.Netcode;

public class PlayerShooting : NetworkBehaviour
{
    public Transform shootPoint;
    public GameObject projectilePrefab;

    public float minForce = 5f;
    public float maxForce = 20f;
    public float force = 10f;
    public float angle = 45f;

    // --- SEGURO ANTI-SPAM ---
    private bool yaDisparoEnEsteTurno = false;

    void Update()
    {
        if (!IsOwner) return;

        // Si ya disparó, bloqueamos para evitar múltiples balas antes del cambio de turno
        if (yaDisparoEnEsteTurno) return;

        // Verificamos turno y reseteamos el seguro si ya no es nuestro turno
        if (TurnManager.Instance != null && !TurnManager.Instance.IsMyTurn(OwnerClientId))
        {
            yaDisparoEnEsteTurno = false;
            return;
        }

        HandleAim();
        HandleShoot();
    }

    void HandleAim()
    {
        if (Input.GetKey(KeyCode.Q)) angle += 80f * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) angle -= 80f * Time.deltaTime;
        if (Input.GetKey(KeyCode.Z)) force += 10f * Time.deltaTime;
        if (Input.GetKey(KeyCode.C)) force -= 10f * Time.deltaTime;

        angle = Mathf.Clamp(angle, 0f, 180f);
        force = Mathf.Clamp(force, minForce, maxForce);

        UpdateShootPointPosition();
    }

    void UpdateShootPointPosition()
    {
        float radius = 1.5f;
        float rad = angle * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad) * radius;
        float y = Mathf.Sin(rad) * radius;

        shootPoint.localPosition = new Vector3(x, y, 0);
        shootPoint.right = (shootPoint.position - transform.position).normalized;
    }

    void HandleShoot()
    {
        // Solo permitimos disparar si la tecla se presiona y el seguro está libre
        if (Input.GetKeyDown(KeyCode.Return) && !yaDisparoEnEsteTurno)
        {
            yaDisparoEnEsteTurno = true; // Bloqueo inmediato

            ShootServerRpc(shootPoint.position, shootPoint.right, force);

            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.EndTurnServerRpc();
            }
        }
    }

    [Rpc(SendTo.Server)]
    void ShootServerRpc(Vector3 pos, Vector3 dir, float force)
    {
        GameObject proj = Instantiate(projectilePrefab, pos, Quaternion.identity);

        NetworkObject netObj = proj.GetComponent<NetworkObject>();
        netObj.Spawn();

        Projectile projectileScript = proj.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Launch(dir, force, OwnerClientId);
        }

        // Notificamos a todos los clientes que sigan esta nueva bala
        FollowProjectileClientRpc(netObj.NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void FollowProjectileClientRpc(ulong projectileId)
    {
        // Buscamos el objeto en la red usando su ID único
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(projectileId, out NetworkObject netObj))
        {
            // Verificamos que el CameraManager exista antes de llamarlo
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.FollowProjectile(netObj.transform);
            }
        }
    }
}