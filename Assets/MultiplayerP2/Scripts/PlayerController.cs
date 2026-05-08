using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    public TMP_Text textoDelbug;

    private int score = 0;
    private TurnManager1 turnManager1;
    public Transform cameraPivot;
    public bool gameEnded = false;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Cantidad total de metros para TODA la partida")]
    [SerializeField] private float maxMovementDistance = 20f;

    private NetworkVariable<float> remainingMovement =
        new NetworkVariable<float>(
            20f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Vector3 previousPosition;

    [Header("UI Movimiento")]
    [SerializeField] private Slider movementSlider;

    [Header("Ajustes de Vida")]
    public NetworkVariable<int> health = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChanged;
        remainingMovement.OnValueChanged += OnMovementChanged;

        UpdateHealthUI(health.Value);
        UpdateMovementUI(remainingMovement.Value);

        if (IsServer)
        {
            remainingMovement.Value = maxMovementDistance;
        }

        previousPosition = transform.position;

        if (turnManager1 == null)
            turnManager1 = FindFirstObjectByType<TurnManager1>();

        GameObject debugObj = GameObject.Find("Textito");

        if (debugObj != null)
            textoDelbug = debugObj.GetComponent<TMP_Text>();
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        Debug.Log($"BARRA ACTUALIZADA: {previousValue} -> {newValue}");
        UpdateHealthUI(newValue);
    }

    private void UpdateHealthUI(int currentHealth)
    {
        if (healthBar != null)
        {
            float currentFill = currentHealth / 100f;
            healthBar.value = currentFill;
        }
    }

    private void OnMovementChanged(float previousValue, float newValue)
    {
        UpdateMovementUI(newValue);
    }

    private void UpdateMovementUI(float currentMovement)
    {
        if (movementSlider != null)
        {
            float fill = currentMovement / maxMovementDistance;
            movementSlider.value = fill;
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

        if (gameEnded) return;

        if (textoDelbug != null && TurnManager1.Instance != null)
        {
            textoDelbug.text =
                TurnManager1.Instance.currentTurn.Value == 0
                ? "Turno: Host (Jugador 1)"
                : "Turno: Cliente (Jugador 2)";
        }

 

        if (transform.position.y <= -5f)
        {
            Debug.Log("MURIO");
            health.Value = 0;
            
                gameEnded = true;

                Debug.Log("Jugador muerto");

                ulong loserId = OwnerClientId;

                ulong winnerId = NetworkManager.Singleton.ConnectedClientsIds
                    .First(id => id != loserId);

                ShowResultClientRpc(winnerId);
            
        }

        if (!IsOwner) return;

        if (!TurnManager1.Instance.IsMyTurn(OwnerClientId)) return;

        HandleMovement();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TurnManager1.Instance.EndTurnServerRpc();
        }
    }

    private void HandleMovement()
    {
        if (remainingMovement.Value <= 0f)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");

        Vector3 movement = new Vector3(horizontal, 0f, 0f);

        transform.position += movement * moveSpeed * Time.deltaTime;

        float movedDistance = Vector3.Distance(transform.position, previousPosition);

        if (movedDistance > 0f)
        {
            SpendMovementServerRpc(movedDistance);
        }

        previousPosition = transform.position;
    }

    [ServerRpc]
    private void SpendMovementServerRpc(float distanceMoved)
    {
        remainingMovement.Value -= distanceMoved;

        if (remainingMovement.Value < 0f)
        {
            remainingMovement.Value = 0f;
        }
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
        remainingMovement.OnValueChanged -= OnMovementChanged;
    }
}