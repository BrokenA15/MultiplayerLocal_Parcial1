using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{

    private Vector2 move;
    private Vector2 look;
    public float speed = 2;
    
    public float rotationSpeed = 5f;
    public float verticalLookSpeed = 80f;
    public float minAngle = 20f;
    public float maxAngle = 80f;

    [SerializeField]
    private float currentAngle = 45f;
    
    public CinemachineCamera virtualCamera;
    private CinemachineThirdPersonFollow follow;
    
    public float closeDistance = 6f;
    public float farDistance = 10f;
    
    public Transform cam;
    
    public bool gameOver = false;
    public bool gameWin = false;
    
   
    PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }
    
    void Start()
    {
        follow = virtualCamera.GetComponent<CinemachineThirdPersonFollow>();
        
        GameManager.instance.RegisterPlayer(this);
    }

    public void OnMove(InputValue action)
    {
        move = action.Get<Vector2>();
    }

    public void OnLook(InputValue action)
    {
        look = action.Get<Vector2>();
    }
    
    public void OnInteract(InputValue value)
    {
        
        transform.rotation = Quaternion.identity;
        
    }

    void Update()
    {
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        
        camForward.y = 0;
        camRight.y = 0;
        
        camForward.Normalize();
        camRight.Normalize();
        
        Vector3 moveDir = camForward * move.y + camRight * move.x;
        
        transform.position += moveDir * (speed * Time.deltaTime);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        currentAngle -= look.y * verticalLookSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        cam.localRotation = Quaternion.Euler(currentAngle, 0, 0);

        float t = Mathf.InverseLerp(maxAngle, minAngle, currentAngle);
        float distance = Mathf.Lerp(closeDistance, farDistance, t);

        if (follow != null)
        {
            follow.CameraDistance = distance;
        }

        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Dead"))
        {
            gameOver = true;
            GameManager.instance.CheckWinCondition();
        }
    }
}


