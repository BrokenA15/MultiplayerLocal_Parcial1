using UnityEngine;
using Unity.Netcode;
using System.Linq;

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

    enum CameraState
    {
        Player,
        Projectile,
        Barrier
    }
     
    private CameraState currentState = CameraState.Player;

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

        switch (currentState)
        {
            case CameraState.Player:
                desiredPosition = currentTarget.position + offset;
                break;

            case CameraState.Projectile:
                desiredPosition = currentTarget.position + projectileOffset;
                break;

            case CameraState.Barrier:
                desiredPosition = currentTarget.position + offset;
                break;

            default:
                desiredPosition = currentTarget.position + offset;
                break;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    public void FollowProjectile(Transform projectile)
    {
        currentTarget = projectile;
        currentState = CameraState.Projectile;
    }

    public void MoveToPlayerByTurn(ulong turnClientId)
    {
        currentState = CameraState.Player;

        if (turnClientId == NetworkManager.Singleton.ConnectedClientsIds.First())
            currentTarget = player1Pivot;
        else
            currentTarget = player2Pivot;
    }

    public void MoveToBarrier(Transform barrierPoint)
    {
        currentTarget = barrierPoint;
        currentState = CameraState.Barrier;
    }
}