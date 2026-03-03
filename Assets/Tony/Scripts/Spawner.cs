using UnityEngine;
using UnityEngine.InputSystem;

public class Spawner : MonoBehaviour
{

    public Transform[] spawnpos;
    int spawnCount = 0;
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.position = spawnpos[spawnCount].position;
        spawnCount++;
    }




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
