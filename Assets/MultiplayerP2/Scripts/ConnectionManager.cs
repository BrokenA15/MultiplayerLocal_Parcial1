using UnityEngine;
using static Unity.Netcode.NetworkManager;
using Unity.Netcode;

public class ConnectionManager : MonoBehaviour
{
    public NetworkManager networkManager;

    // IMPORTANTE: Asegúrate en Unity de que estas listas tengan al menos 3 Transforms cada una
    public Transform[] leftSpawnPoints;
    public Transform[] rightSpawnPoints;

    public GameObject[] playerPrefabs;

    private int playerIndex = 0; // 0 para Host/Jugador 1, 1 para Cliente/Jugador 2

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

    void Start()
    {
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Evitamos desbordamiento de índice si se conectan más de 2 jugadores por error
        if (playerIndex >= playerPrefabs.Length) return;

        GameObject prefabToSpawn = playerPrefabs[playerIndex];

        // Decidimos qué grupo de puntos usar según el jugador que entra
        Transform[] grupoSpawn = (playerIndex == 0) ? leftSpawnPoints : rightSpawnPoints;

        // 🐛 BUCLE ESTILO WORMS: Creamos 3 personajes para este equipo
        for (int i = 0; i < 3; i++)
        {
            Transform puntoEspecifico = grupoSpawn[i % grupoSpawn.Length];

            // 🌟 NUEVO: Añadir un offset en X basado en el índice 'i' para que no compartan la misma posición
            float offsetPorClon = 1.5f; // Distancia en metros entre cada clon
            Vector3 posicionDesfasada = puntoEspecifico.position + new Vector3(i * offsetPorClon, 0f, 0f);

            GameObject player = Instantiate(
                prefabToSpawn,
                posicionDesfasada, // Usamos la nueva posición con offset
                puntoEspecifico.rotation
            );

            player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
        }

        playerIndex++;
    }
}