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
        Transform spawn = spawnpos[spawnCount];
        playerInput.transform.SetPositionAndRotation(
            spawn.position,
            Quaternion.identity);
        spawnCount++;
    }
}
