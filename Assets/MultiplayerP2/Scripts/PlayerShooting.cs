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
    private bool eraMiTurno = false;

    void Update()
    {
        if (!IsOwner) return;

        if (TurnManager1.Instance == null) return;

        bool esMiTurno = TurnManager1.Instance.IsMyTurn(OwnerClientId);
        bool esFaseDisparo = TurnManager1.Instance.currentPhase.Value == TurnManager1.GamePhase.Disparo;
        
        if (esMiTurno && !eraMiTurno)
        {
            yaDisparoEnEsteTurno = false;
        }
      
        eraMiTurno = esMiTurno;

        if (!esMiTurno) return;

        if (!esFaseDisparo) return;

        if (yaDisparoEnEsteTurno) return;

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
        if (TurnManager1.Instance.currentPhase.Value != TurnManager1.GamePhase.Disparo)
            return;
        
        if (Input.GetKeyDown(KeyCode.Return) && !yaDisparoEnEsteTurno)
        {
            yaDisparoEnEsteTurno = true;

            ShootServerRpc(shootPoint.position, shootPoint.right, force);

            
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
        // Buscamos el objeto en la red usando su ID �nico
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