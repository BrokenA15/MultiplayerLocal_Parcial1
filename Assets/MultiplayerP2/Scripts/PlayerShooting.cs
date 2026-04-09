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

    void Update()
    {
        if (!IsOwner) return;
        if (!TurnManager.Instance.IsMyTurn(OwnerClientId)) return;

        HandleAim();
        HandleShoot();
    }

    void HandleAim()
    {
        if (Input.GetKey(KeyCode.Q))
            angle += 80f * Time.deltaTime;

        if (Input.GetKey(KeyCode.E))
            angle -= 80f * Time.deltaTime;

        if (Input.GetKey(KeyCode.Z))
            force += 10f * Time.deltaTime;

        if (Input.GetKey(KeyCode.C))
            force -= 10f * Time.deltaTime;

        angle = Mathf.Clamp(angle, 0f, 180f);
        force = Mathf.Clamp(force, minForce, maxForce);

        UpdateShootPointPosition();
    }

    void UpdateShootPointPosition() // Hace que se mueva la ubicacion de donde sale el proyectil, si es mucho pedo para animar pues se 
                                    // chingan putos                    

                                    // No se crean los amo, si necesitan creo que con que quiten esta funcion deja de pasar eso, sino es modificar poquito la de arriba
    {
        float radius = 1.5f; // distancia de la boquilla del arma del jugadorsini

        float rad = angle * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad) * radius;
        float y = Mathf.Sin(rad) * radius;

        shootPoint.localPosition = new Vector3(x, y, 0);

        shootPoint.right = (shootPoint.position - transform.position).normalized;
    }

    void HandleShoot()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ShootServerRpc(shootPoint.position, shootPoint.right, force);
        }
    }

    [Rpc(SendTo.Server)]
    void ShootServerRpc(Vector3 pos, Vector3 dir, float force)
    {
        GameObject proj = Instantiate(projectilePrefab, pos, Quaternion.identity);

        proj.GetComponent<NetworkObject>().Spawn();

        proj.GetComponent<Projectile>().Launch(dir, force, OwnerClientId);
        FollowProjectileClientRpc(proj.GetComponent<NetworkObject>().NetworkObjectId);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    void FollowProjectileClientRpc(ulong projectileId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(projectileId, out NetworkObject netObj))
            return;

        CameraManager.Instance.FollowProjectile(netObj.transform);
    }
}