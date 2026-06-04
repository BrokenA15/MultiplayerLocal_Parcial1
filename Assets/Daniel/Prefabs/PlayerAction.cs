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

            // 📷 Al cambiar de fase, si volvemos a Construccion le devolvemos la cámara al personaje
            TurnManager1.Instance.currentPhase.OnValueChanged += OnPhaseChanged;
        }
    }

    private void OnTurnChanged(ulong previousTurn, ulong newTurn)
    {
        yaConstruyoEnEstaFase = false;
    }

    private void OnPhaseChanged(TurnManager1.GamePhase previousPhase, TurnManager1.GamePhase newPhase)
    {
        // 📷 Cuando termina el disparo y volvemos a Construccion, la cámara regresa al personaje activo
        if (newPhase == TurnManager1.GamePhase.Construccion)
        {
            if (TurnManager1.Instance != null &&
                NetworkObject.NetworkObjectId == TurnManager1.Instance.activeCharacterNetworkId.Value)
            {
                if (CameraManager.Instance != null)
                    CameraManager.Instance.FollowTarget(transform);
            }
        }

        // 🔑 FIX DISPARO PLAYER 2: Cuando la fase cambia a Disparo, encendemos
        // PlayerShooting directamente aquí — no dependemos de Update() para esto
        // porque Update() solo corre si el script estaba activo antes del cambio.
        if (newPhase == TurnManager1.GamePhase.Disparo)
        {
            bool somoElActivo = TurnManager1.Instance != null &&
                NetworkObject.NetworkObjectId == TurnManager1.Instance.activeCharacterNetworkId.Value;

            if (shootingScript != null)
                shootingScript.enabled = somoElActivo;
        }

        // Al volver a Construccion, apagar disparo y resetear construcción
        if (newPhase == TurnManager1.GamePhase.Construccion)
        {
            if (shootingScript != null) shootingScript.enabled = false;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

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
            // Encendemos el disparo solo si este script está activo (personaje activo)
            if (shootingScript != null && this.enabled)
                shootingScript.enabled = true;
        }
    }

    void HandleBuilding()
    {
        if (yaConstruyoEnEstaFase)
        {
            LimpiarGhost();
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
                    Collider playerCollider = GetComponent<Collider>();
                    Collider ghostCollider = ghostInstance.GetComponent<Collider>();
                    if (playerCollider != null && ghostCollider != null)
                        Physics.IgnoreCollision(playerCollider, ghostCollider);
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

        if (Input.GetKeyDown(KeyCode.Space)) TurnManager1.Instance.EndTurnServerRpc();
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
        {
            indicadorSeleccionado.SetActive(active);
        }
    }
}