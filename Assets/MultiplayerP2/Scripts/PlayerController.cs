using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    public TMP_Text textoDelbug;
    private int score = 0;
    private TurnManager turnManager;
    public Transform cameraPivot;
    private bool gameEnded = false;

    [Header("Barrier")]
    public GameObject barrierPrefab;
    public Transform barrierPoint;

    [Header("Barrier Placement")]
    public GameObject barrierPreviewPrefab;
    private Transform barrierPreviewInstance; 
    public float moveSpeed = 5f;


    [Header("Ajustes de Vida")]
    public NetworkVariable<int> health = new NetworkVariable<int>(100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private Slider healthBar;

    public override void OnNetworkSpawn()
    {
        TurnManager.Instance.currentTurn.OnValueChanged += OnTurnChanged;

        health.OnValueChanged += OnHealthChanged;
   
        UpdateHealthUI(health.Value);
     
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();

        GameObject debugObj = GameObject.Find("Textito");
        if (debugObj != null) textoDelbug = debugObj.GetComponent<TMP_Text>();
    }

    void OnTurnChanged(ulong previous, ulong current)
    {
       
        CameraManager.Instance.MoveToPlayerByTurn(current);

       
        if (!IsOwner) return;

        if (current == OwnerClientId)
        {
            StartTurn();
        }
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        // Corregido: Debug.Log con 'L' may�scula
        Debug.Log($"BARRA ACTUALIZADA: {previousValue} -> {newValue}");
        UpdateHealthUI(newValue);
    }

    private void UpdateHealthUI(int currentHealth)
    {
        if (healthBar != null)
        {
            // Forzamos que la divisi�n sea decimal usando 100f
            float currentFill = currentHealth / 100f;
            healthBar.value = currentFill;

            // Si esto imprime 0.8, 0.6, etc., el c�digo est� PERFECTO 
            // y el problema es el componente Slider en Unity.
            Debug.Log("Valor enviado al Slider: " + currentFill);
        }
    }


    public void TakeDamage(int damage)
    {
        if (!IsServer) return;

        if (gameEnded) return;

        health.Value -= damage;

        if (health.Value <= 0)
        {
            gameEnded = true;

            Debug.Log("Jugador muerto");

            ulong loserId = OwnerClientId;

            ulong winnerId = NetworkManager.Singleton.ConnectedClientsIds
                .First(id => id != loserId);

            ShowResultClientRpc(winnerId);
        }
    }

    void StartTurn()
    {
        var phase = TurnManager.Instance.currentPhase.Value;

        switch (phase)
        {
            case TurnManager.GamePhase.PlacingBarriers:

                if (barrierPreviewInstance == null)
                {
                    GameObject preview = Instantiate(barrierPreviewPrefab, transform.position + Vector3.forward * 2f, Quaternion.identity);
                    barrierPreviewInstance = preview.transform;
                }

                break;

            case TurnManager.GamePhase.Shooting:

                if (barrierPreviewInstance != null)
                {
                    Destroy(barrierPreviewInstance.gameObject);
                    barrierPreviewInstance = null;
                }

                CameraManager.Instance.MoveToPlayerByTurn(OwnerClientId);

                break;
        }
    }
    
    [ClientRpc]
    void ShowResultClientRpc(ulong winnerId)
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;

        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager no encontrado");
            return;
        }

        if (localId == winnerId)
        {
            Debug.Log("VICTORIA");
            UIManager.Instance.ShowVictory();
        }
        else
        {
            Debug.Log("DERROTA");
            UIManager.Instance.ShowDefeat();
        }
    }


    void Update()
    {
      
        if (textoDelbug != null && TurnManager.Instance != null)
        {
            textoDelbug.text =
                $"Turno: {(TurnManager.Instance.currentTurn.Value == 0 ? "Jugador 1" : "Jugador 2")}\n" +
                $"Fase: {TurnManager.Instance.currentPhase.Value}\n" +
                $"Ronda: {TurnManager.Instance.currentRound.Value}";
        }


        if (!IsOwner) return;
        if (!TurnManager.Instance.IsMyTurn(OwnerClientId)) return;

        var phase = TurnManager.Instance.currentPhase.Value;

        switch (phase)
        {
            case TurnManager.GamePhase.PlacingBarriers:
                HandleBarrierPlacement();
                break;

            case TurnManager.GamePhase.Shooting:
                HandleShootingTurn();
                break;
        }
    }
    
    void HandleBarrierPlacement()
    {
        HandleBarrierMovement();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (barrierPreviewInstance == null) return;

            PlaceBarrierServerRpc(barrierPreviewInstance.position);

            Destroy(barrierPreviewInstance.gameObject);
            barrierPreviewInstance = null;

            TurnManager.Instance.EndTurnServerRpc();
        }
    }
    
    void HandleBarrierMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(x, 0, z) * moveSpeed * Time.deltaTime;

        if (barrierPreviewInstance != null)
        {
            barrierPreviewInstance.position += move;
        }
    }
    
    void HandleShootingTurn()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // aquí puedes llamar tu sistema de disparo si quieres
            Debug.Log("Disparo");

            TurnManager.Instance.EndTurnServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    void PlaceBarrierServerRpc(Vector3 position)
    {
        GameObject barrier = Instantiate(barrierPrefab, position, Quaternion.identity);
        NetworkObject netObj = barrier.GetComponent<NetworkObject>();

        netObj.Spawn();

        FollowBarrierClientRpc(netObj.NetworkObjectId);
    }

    [ClientRpc]
    void FollowBarrierClientRpc(ulong networkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(networkObjectId))
            return;

        var barrier = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];

        CameraManager.Instance.MoveToBarrier(barrier.transform);
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
        TurnManager.Instance.currentTurn.OnValueChanged -= OnTurnChanged;
    }
}