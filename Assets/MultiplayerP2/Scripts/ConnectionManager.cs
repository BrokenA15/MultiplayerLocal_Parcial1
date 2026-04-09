using UnityEngine;
using static Unity.Netcode.NetworkManager;
using Unity.Netcode;

public class ConnectionManager : MonoBehaviour
{

    public NetworkManager networkManager;
    public Transform[] leftSpawnPoints;
    public Transform[] rightSpawnPoints;
    
    public GameObject[] playerPrefabs;
    
    private int playerIndex = 0;
    
    public void Start_Host()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Start_Client()
    {
        NetworkManager.Singleton.StartClient();

    }

    public void Start_Server()
    {
        NetworkManager.Singleton.StartServer();

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        GameObject prefabToSpawn = playerPrefabs[playerIndex];

        Transform spawnPoint;

        if (playerIndex == 0)
        {
            spawnPoint = leftSpawnPoints[Random.Range(0, leftSpawnPoints.Length)];
        }
        else
        {
            spawnPoint = rightSpawnPoints[Random.Range(0, rightSpawnPoints.Length)];
        }

        GameObject player = Instantiate(
            prefabToSpawn,
            spawnPoint.position,
            spawnPoint.rotation
        );

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        playerIndex++;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
