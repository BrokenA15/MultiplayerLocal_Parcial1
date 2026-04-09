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

        if (TurnManager.Instance != null && !TurnManager.Instance.IsMyTurn(OwnerClientId)) return;

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
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ShootServerRpc(shootPoint.position, shootPoint.right, force);
        }
    }

    // 🔥 SERVER: crea la bala
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

        // 📡 avisar a TODOS los clientes que sigan la bala
        FollowProjectileClientRpc(netObj.NetworkObjectId);
    }

    // 🎥 CLIENTES: siguen la bala
    [Rpc(SendTo.ClientsAndHost)]
    void FollowProjectileClientRpc(ulong projectileId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(projectileId, out NetworkObject netObj))
            return;

        CameraManager.Instance.FollowProjectile(netObj.transform);
    }
}