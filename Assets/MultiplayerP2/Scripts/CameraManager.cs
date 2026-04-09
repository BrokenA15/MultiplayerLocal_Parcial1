using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Transform player1Pivot;
    public Transform player2Pivot;

    private Transform currentTarget;

    public float smoothSpeed = 5f;

    public Vector3 offset = new Vector3(0, 2, -10);
    public Vector3 projectileOffset = new Vector3(-4, 3, -6);

    private bool followingProjectile = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Empieza en jugador 1
        currentTarget = player1Pivot;
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

    // 🎬 ir al jugador según turno
    public void MoveToPlayerByTurn(ulong turnClientId)
    {
        followingProjectile = false;

        // 👇 asumiendo 2 jugadores
        if (turnClientId == 0)
            currentTarget = player1Pivot;
        else
            currentTarget = player2Pivot;
    }
}