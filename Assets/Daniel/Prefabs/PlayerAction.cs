using UnityEngine;
using Unity.Netcode;

public class PlayerAction : NetworkBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private GameObject barreraPrefab;
    [SerializeField] private GameObject barreraGhostPrefab;
    [SerializeField] private float rangoMaximo = 10f;

    private GameObject ghostInstance;
    private bool yaConstruyoEnEstaFase = false;
    private PlayerShooting shootingScript;

    [Header("Ajustes de Eje")]
    [SerializeField] private float profundidadZFija = 0f; // Corregido: sin espacio

    void Awake()
    {
        shootingScript = GetComponent<PlayerShooting>();
    }

    void Update()
    {
        if (!IsOwner) return;

        // Usamos TurnManager1 porque as� se llama tu clase
        if (TurnManager1.Instance == null || !TurnManager1.Instance.IsMyTurn(OwnerClientId))
        {
            if (ghostInstance != null) Destroy(ghostInstance);
            yaConstruyoEnEstaFase = false;
            return;
        }

        var faseActual = TurnManager1.Instance.currentPhase.Value;

        if (faseActual == TurnManager1.GamePhase.Construccion)
        {
            if (shootingScript != null) shootingScript.enabled = false;
            HandleBuilding();
        }
        else 
        {
            if (ghostInstance != null) Destroy(ghostInstance);
            if (shootingScript != null) shootingScript.enabled = true;
        }
    }

    void HandleBuilding()
    {
        if (yaConstruyoEnEstaFase)
        {
            if (ghostInstance != null) Destroy(ghostInstance);
            return;
        }

        Plane planoConstruccion = new Plane(Vector3.forward, new Vector3(0, 0, profundidadZFija));
        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (planoConstruccion.Raycast(rayo, out float distanciaAlPlano))
        {
            Vector3 puntoEnPlano = rayo.GetPoint(distanciaAlPlano);
            float distancia = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(puntoEnPlano.x, puntoEnPlano.y)
            );

            if (distancia <= rangoMaximo)
            {
                if (ghostInstance == null)
                {
                    ghostInstance = Instantiate(barreraGhostPrefab);

                    // --- SOLUCIÓN AL EMPUJÓN ---
                    // Ignoramos colisión entre el jugador y el ghost justo al nacer
                    Collider playerCollider = GetComponent<Collider>();
                    Collider ghostCollider = ghostInstance.GetComponent<Collider>();
                    if (playerCollider != null && ghostCollider != null)
                    {
                        Physics.IgnoreCollision(playerCollider, ghostCollider);
                    }
                }

                ghostInstance.SetActive(true);
                ghostInstance.transform.position = new Vector3(puntoEnPlano.x, puntoEnPlano.y, profundidadZFija);

                if (Input.GetMouseButtonDown(0))
                {
                    SpawnBarreraServerRpc(ghostInstance.transform.position);
                    yaConstruyoEnEstaFase = true;
                    Destroy(ghostInstance);
                }
            }
            else
            {
                if (ghostInstance != null) ghostInstance.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) TurnManager1.Instance.EndTurnServerRpc();
    }

    [Rpc(SendTo.Server)]
    void SpawnBarreraServerRpc(Vector3 pos)
    {
        GameObject nueva = Instantiate(barreraPrefab, pos, Quaternion.identity);
        nueva.GetComponent<NetworkObject>().Spawn();
    }
}