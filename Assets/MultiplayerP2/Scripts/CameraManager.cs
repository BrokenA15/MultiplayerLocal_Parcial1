using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Transform currentTarget;
    public Transform defaultPivot;

    public float smoothSpeed = 5f;

    public Vector3 offset = new Vector3(0, 2, -10);
    public Vector3 projectileOffset = new Vector3(-4, 3, -6);

    private bool followingProjectile = false;

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 desiredPosition;

        if (followingProjectile)
            desiredPosition = currentTarget.position + projectileOffset;
        else
            desiredPosition = currentTarget.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    // 🎯 seguir bala
    public void FollowProjectile(Transform projectile)
    {
        currentTarget = projectile;
        followingProjectile = true;
    }

    // 🎬 regresar al centro con delay
    public void ReturnToCenterWithDelay(float delay)
    {
        Invoke(nameof(ReturnToCenter), delay);
    }

    void ReturnToCenter()
    {
        currentTarget = defaultPivot;
        followingProjectile = false;
    }
}
