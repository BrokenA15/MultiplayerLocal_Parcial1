using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{

    private Vector2 move;
    public float speed = 2;

    public void OnMove(InputValue action)
    {
        move = action.Get<Vector2>();
    }
 
    void Update()
    {
        transform.Translate(move.x * Time.deltaTime * speed, 0, move.y * Time.deltaTime * speed);
    }
}


