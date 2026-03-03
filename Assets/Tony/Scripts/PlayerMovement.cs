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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(move.x * Time.deltaTime * speed, move.y * Time.deltaTime * speed, 0);
    }
}
