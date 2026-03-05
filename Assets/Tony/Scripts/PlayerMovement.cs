using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    private Vector2 move;
    private Vector2 look;
    public float speed = 2;
    public float lookSpeed = 100f;
    public float rotationSpeed = 10f;
    public Transform cam;
    PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    public void OnMove(InputValue action)
    {
        move = action.Get<Vector2>();
    }

    /*public void OnLook(InputValue action)
    {
        look = action.Get<Vector2>();
    }*/
    
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
        if (cam != null)    
        {
            cam.RotateAround(transform.position, Vector3.up, look.x * lookSpeed  * Time.deltaTime);
        }
    }
}


