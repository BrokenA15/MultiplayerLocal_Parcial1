using UnityEngine;
using Unity.Netcode;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 2, -10);
    public Vector3 projectileOffset = new Vector3(-4, 3, -6);

    private Transform currentTarget;
    private bool followingProjectile = false;

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 desiredPosition = followingProjectile
            ? currentTarget.position + projectileOffset
            : currentTarget.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    // 🎯 Seguir proyectil
    public void FollowProjectile(Transform projectile)
    {
        currentTarget = projectile;
        followingProjectile = true;
    }

    // 🎬 Seguir un Transform específico (el personaje activo)
    public void FollowTarget(Transform target)
    {
        if (target == null) return;
        currentTarget = target;
        followingProjectile = false;
    }
}