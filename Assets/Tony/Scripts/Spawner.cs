using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


public class Spawner : MonoBehaviour
{
    public PlayerInputManager managerPlayerSkin;
    public Transform[] spawnpos;
    public GameObject[] prefabPlayer;

    private int prefabCount = 0;
    int spawnCount = 0;
    
    public GameObject joinText;
    public GameObject camInicial;

    void Start()
    {
       
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (joinText != null)
        {
            joinText.SetActive(false);
        }

        if (camInicial != null)
        {
            camInicial.SetActive(false);
        }
        managerPlayerSkin.playerPrefab = prefabPlayer[prefabCount];
        Transform spawn = spawnpos[spawnCount];
        playerInput.transform.SetPositionAndRotation(spawn.position, Quaternion.identity);
        HUDManager.instance.CreateHUD(playerInput.gameObject);
        
       
        
        spawnCount++;
        prefabCount++;
       
    }
}
