using UnityEngine;
using static Unity.Netcode.NetworkManager;
using Unity.Netcode;
using UnityEngine.SceneManagement; // Requerido para la gestión de escenas normales si se necesita

public class ConnectionManager : MonoBehaviour
{
    public NetworkManager networkManager;

          // El nuevo panel de controles

    [Header("Configuración de Niveles")]
    [Tooltip("Escribe los nombres exactos de tus 3 escenas de juego aquí")]
    [SerializeField] private string[] nombresDeEscenas = { "Nivel1", "Nivel2", "Nivel3" };

    // IMPORTANTE: Asegúrate en Unity de que estas listas tengan al menos 3 Transforms cada una
    public Transform[] leftSpawnPoints;
    public Transform[] rightSpawnPoints;

    public GameObject[] playerPrefabs;

    private int playerIndex = 0; // 0 para Host/Jugador 1, 1 para Cliente/Jugador 2

    void Start()
    {
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    // --- SECCIÓN 1: CONTROL DEL PANEL DE CONTROLES ---

    


    // --- SECCIÓN 2: INICIO DE PARTIDA CON ESCENA ALEATORIA ---

    public void Start_Host()
    {
        // 1. Iniciamos el Host de Netcode normalmente
        NetworkManager.Singleton.StartHost();

        // 2. Elegimos una escena de juego de forma aleatoria
        if (nombresDeEscenas.Length > 0)
        {
            int indiceAleatorio = Random.Range(0, nombresDeEscenas.Length);
            string escenaSeleccionada = nombresDeEscenas[indiceAleatorio];

            Debug.Log($"[SERVER] Cargando escenario aleatorio: {escenaSeleccionada}");

            // 3. Cargamos la escena usando el NetworkSceneManager. 
            // Esto obligará a cualquier cliente que se conecte a sincronizarse y cargar este mismo mapa.
            NetworkManager.Singleton.SceneManager.LoadScene(escenaSeleccionada, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("No has configurado nombres de escenas en el array 'nombresDeEscenas'.");
        }
    }

    public void Start_Client()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void Start_Server()
    {
        NetworkManager.Singleton.StartServer();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (playerIndex >= playerPrefabs.Length) return;

        GameObject prefabToSpawn = playerPrefabs[playerIndex];
        Transform[] grupoSpawn = (playerIndex == 0) ? leftSpawnPoints : rightSpawnPoints;

        for (int i = 0; i < 3; i++)
        {
            Transform puntoEspecifico = grupoSpawn[i % grupoSpawn.Length];

            GameObject player = Instantiate(
                prefabToSpawn,
                puntoEspecifico.position,
                puntoEspecifico.rotation
            );

            player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
        }

        playerIndex++;
    }
}