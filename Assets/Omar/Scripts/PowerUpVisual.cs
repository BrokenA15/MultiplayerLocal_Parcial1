using UnityEngine;

public class PowerUpVisual : MonoBehaviour
{

    public float rotationSpeed = 90f;
    public float floatHeight = 0.25f;
    public float floatSpeed = 2f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {

        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position = startPos + new Vector3(0, y, 0);

    }

}