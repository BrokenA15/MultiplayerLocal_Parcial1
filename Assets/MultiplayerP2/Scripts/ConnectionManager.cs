using System.Collections.Generic;
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
        List<GameObject> clonesSpawneados = new List<GameObject>();

        for (int i = 0; i < 3; i++)
        {
            Transform puntoEspecifico = grupoSpawn[i % grupoSpawn.Length];

            // Offset aumentado a 2.5f — el CapsuleCollider tiene radio 0.5 (diámetro 1.0)
            // así que 2.5f garantiza que no se solapen ni se empujen al spawnear
            float offsetPorClon = 2.5f;
            Vector3 posicionDesfasada = puntoEspecifico.position + new Vector3(i * offsetPorClon, 0f, 0f);

            GameObject player = Instantiate(
                prefabToSpawn,
                posicionDesfasada,
                puntoEspecifico.rotation
            );

            player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
            clonesSpawneados.Add(player);
        }

        // 🔑 Compañeros de equipo se ignoran entre sí — no se empujan ni bloquean
        for (int a = 0; a < clonesSpawneados.Count; a++)
        {
            for (int b = a + 1; b < clonesSpawneados.Count; b++)
            {
                Collider colA = clonesSpawneados[a].GetComponent<Collider>();
                Collider colB = clonesSpawneados[b].GetComponent<Collider>();
                if (colA != null && colB != null)
                    Physics.IgnoreCollision(colA, colB);
            }
        }

        playerIndex++;
    }
}