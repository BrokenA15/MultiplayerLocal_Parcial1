using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


public class Spawner : MonoBehaviour
{
    public Transform[] spawnpos;
    int spawnCount = 0;
    public Transform ringCenter;
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        StartCoroutine(SpawnRoutine(playerInput));
    }
    
    IEnumerator SpawnRoutine(PlayerInput playerInput)
    {
        yield return new WaitForEndOfFrame(); 

        Transform spawn = spawnpos[spawnCount];

        Vector3 dir = ringCenter.position - spawn.position;
        dir.y = 0;

        playerInput.transform.SetPositionAndRotation(
            spawn.position,
            Quaternion.LookRotation(dir)
        );

        spawnCount++;
    }
}
