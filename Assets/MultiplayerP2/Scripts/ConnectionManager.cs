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
            // Usamos el índice del bucle para que cada personaje vaya a un punto diferente
            // Si el arreglo tiene menos de 3 puntos, usamos % para evitar errores
            Transform puntoEspecifico = grupoSpawn[i % grupoSpawn.Length];

            GameObject player = Instantiate(
                prefabToSpawn,
                puntoEspecifico.position,
                puntoEspecifico.rotation
            );

            // CLAVE: Usamos SpawnWithOwnership en lugar de SpawnAsPlayerObject.
            // Esto le da el control de red al cliente sobre sus 3 personajes de forma individual.
            player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
        }

        playerIndex++;
    }
}