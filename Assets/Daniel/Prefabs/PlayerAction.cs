using UnityEngine;
using Unity.Netcode;

public class PlayerAction : NetworkBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject barreraPrefab;
    [SerializeField] private GameObject barreraGhostPrefab;
    [SerializeField] private float rangoMaximo = 10f;

    private GameObject ghostInstance;
    private bool yaConstruyoEnEstaFase = false;
    private PlayerShooting shootingScript;

    [Header("Ajustes de Eje")]
    [SerializeField] private float profundidadZFija = 0f;

    [Header("Visual Resalte")]
    [SerializeField] private GameObject indicadorSeleccionado;

    void Awake()
    {
        shootingScript = GetComponent<PlayerShooting>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer && TurnManager1.Instance != null)
            TurnManager1.Instance.RegistrarPersonaje(this);

        if (TurnManager1.Instance != null)
        {
            TurnManager1.Instance.ActualizarControlesLocalesRpc(TurnManager1.Instance.activeCharacterNetworkId.Value);
            TurnManager1.Instance.currentTurn.OnValueChanged += OnTurnChanged;
            TurnManager1.Instance.currentPhase.OnValueChanged += OnPhaseChanged;
        }
    }

    private void OnTurnChanged(ulong previousTurn, ulong newTurn)
    {
        yaConstruyoEnEstaFase = false;
    }

    private void OnPhaseChanged(TurnManager1.GamePhase previousPhase, TurnManager1.GamePhase newPhase)
    {
        bool somoElActivo = TurnManager1.Instance != null &&
            NetworkObject.NetworkObjectId == TurnManager1.Instance.activeCharacterNetworkId.Value;

        if (newPhase == TurnManager1.GamePhase.Disparo)
        {
            // Encender disparo solo en el personaje activo
            if (shootingScript != null)
                shootingScript.enabled = somoElActivo;

            // Limpiar ghost al entrar en fase de disparo
            LimpiarGhost();
        }

        if (newPhase == TurnManager1.GamePhase.Construccion)
        {
            // Apagar disparo siempre al volver a construcción
            if (shootingScript != null) shootingScript.enabled = false;

            // Devolver cámara al personaje activo
            if (somoElActivo && CameraManager.Instance != null)
                CameraManager.Instance.FollowTarget(transform);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // Cambio de personaje — O o Tab
        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (TurnManager1.Instance.personajeComprometido.Value)
            {
                Debug.Log("[TURNO] Ya moviste o construiste — no puedes cambiar de personaje");
                return;
            }
            TurnManager1.Instance.CambiarPersonajePropioServerRpc(OwnerClientId);
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
            LimpiarGhost();
            if (shootingScript != null && this.enabled)
                shootingScript.enabled = true;
        }
    }

    void HandleBuilding()
    {
        bool sinBarreras = !TurnManager1.Instance.PuedeConstructor(OwnerClientId);

        // Si ya construyó o no tiene barreras disponibles — solo esperar Space para pasar a disparo
        if (yaConstruyoEnEstaFase || sinBarreras)
        {
            LimpiarGhost();
            if (Input.GetKeyDown(KeyCode.Space))
                TurnManager1.Instance.EndTurnServerRpc();
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

                    int ghostLayer = LayerMask.NameToLayer("Ghost");
                    if (ghostLayer != -1)
                    {
                        ghostInstance.layer = ghostLayer;
                        foreach (Transform hijo in ghostInstance.GetComponentsInChildren<Transform>())
                            hijo.gameObject.layer = ghostLayer;
                    }
                    else
                    {
                        Collider ghostCollider = ghostInstance.GetComponent<Collider>();
                        if (ghostCollider != null) ghostCollider.enabled = false;
                    }
                }

                ghostInstance.SetActive(true);
                ghostInstance.transform.position = new Vector3(puntoEnPlano.x, puntoEnPlano.y, profundidadZFija);

                if (Input.GetMouseButtonDown(0))
                {
                    SpawnBarreraServerRpc(ghostInstance.transform.position);
                    yaConstruyoEnEstaFase = true;
                    LimpiarGhost();
                    TurnManager1.Instance.ComprometерPersonajeServerRpc(OwnerClientId);
                }
            }
            else
            {
                if (ghostInstance != null) ghostInstance.SetActive(false);
            }
        }

        // Space: pasar de Construccion a Disparo sin construir
        if (Input.GetKeyDown(KeyCode.Space))
            TurnManager1.Instance.EndTurnServerRpc();
    }

    public void LimpiarGhost()
    {
        if (ghostInstance != null)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
        }
    }

    [Rpc(SendTo.Server)]
    void SpawnBarreraServerRpc(Vector3 pos)
    {
        if (!TurnManager1.Instance.PuedeConstructor(OwnerClientId))
        {
            Debug.Log($"[BARRERAS] Jugador {OwnerClientId} ya usó las {TurnManager1.MAX_BARRERAS} barreras permitidas");
            return;
        }

        TurnManager1.Instance.RegistrarBarrera(OwnerClientId);

        GameObject nueva = Instantiate(barreraPrefab, pos, Quaternion.identity);
        nueva.GetComponent<NetworkObject>().Spawn();
    }

    public override void OnNetworkDespawn()
    {
        if (TurnManager1.Instance != null)
        {
            TurnManager1.Instance.currentTurn.OnValueChanged -= OnTurnChanged;
            TurnManager1.Instance.currentPhase.OnValueChanged -= OnPhaseChanged;
        }
    }

    public void SetHighlight(bool active)
    {
        if (indicadorSeleccionado != null)
            indicadorSeleccionado.SetActive(active);
    }
}