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

    [Header("PowerUps")]
    public NetworkVariable<bool> shieldActive =
        new NetworkVariable<bool>(false);

    public NetworkVariable<float> explosionMultiplier =
        new NetworkVariable<float>(1f);

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

    // 🔒 NUEVO: Rastrea si este personaje ya movió ESTE turno (local, no necesita ser NetworkVariable)
    private bool yaMoveEnEsteTurno = false;

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

        // 🔒 NUEVO: Escuchamos cambios de turno para resetear la bandera local
        if (TurnManager1.Instance != null)
        {
            TurnManager1.Instance.currentTurn.OnValueChanged += OnTurnChanged;
        }
    }

    // 🔒 NUEVO: Al cambiar el turno, cualquier personaje resetea su bandera de movimiento
    private void OnTurnChanged(ulong previousTurn, ulong newTurn)
    {
        yaMoveEnEsteTurno = false;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        Debug.Log($"BARRA ACTUALIZADA: {previousValue} -> {newValue}");
        UpdateHealthUI(newValue);
    }

    private void UpdateHealthUI(int currentHealth)
    {
        if (healthBar != null)
            healthBar.value = currentHealth / 100f;
    }

    private void OnMovementChanged(float previousValue, float newValue)
    {
        UpdateMovementUI(newValue);
    }

    private void UpdateMovementUI(float currentMovement)
    {
        if (movementSlider != null)
            movementSlider.value = currentMovement / maxMovementDistance;
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        if (gameEnded) return;

        health.Value -= damage;

        if (health.Value <= 0)
        {
            Debug.Log($"Clon {OwnerClientId} eliminado");

            // 🔑 FIX: Solo termina si TODO el equipo está muerto
            PlayerController[] todosLosJugadores = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            bool equipoCompleto = true;

            foreach (PlayerController pc in todosLosJugadores)
            {
                if (pc == this) continue;
                if (pc.OwnerClientId == OwnerClientId && pc.health.Value > 0)
                {
                    equipoCompleto = false;
                    break;
                }
            }

            if (!equipoCompleto) return;

            gameEnded = true;
            Debug.Log("Equipo eliminado — fin del juego");

            ulong loserId = OwnerClientId;
            ulong winnerId = NetworkManager.Singleton.ConnectedClientsIds
                .First(id => id != loserId);

            ShowResultClientRpc(winnerId);
        }
    }

    public void AddHealth(int amount)
    {
        if (!IsServer) return;
        health.Value = Mathf.Min(health.Value + amount, 100);
    }

    public void AddStamina(float amount)
    {
        if (!IsServer) return;
        remainingMovement.Value = Mathf.Min(remainingMovement.Value + amount, maxMovementDistance);
    }

    public void ActivateShield()
    {
        if (!IsServer) return;
        shieldActive.Value = true;
    }

    public void IncreaseExplosionRadius(float multiplier)
    {
        if (!IsServer) return;
        explosionMultiplier.Value = multiplier;
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

        // Muerte por caída
        if (transform.position.y <= -5f)
        {
            if (IsServer)
            {
                health.Value = 0;
                gameEnded = true;
                ulong loserId = OwnerClientId;
                ulong winnerId = NetworkManager.Singleton.ConnectedClientsIds.First(id => id != loserId);
                ShowResultClientRpc(winnerId);
            }
            return;
        }

        if (!IsOwner) return;

        HandleMovement();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TurnManager1.Instance.EndTurnServerRpc();
        }
    }

    private void HandleMovement()
    {
        if (remainingMovement.Value <= 0f) return;

        float horizontal = Input.GetAxisRaw("Horizontal");

        // 🔒 NUEVO: Si intenta moverse y aún no comprometió, avisar al servidor
        if (horizontal != 0f && !yaMoveEnEsteTurno)
        {
            yaMoveEnEsteTurno = true;
            TurnManager1.Instance.ComprometерPersonajeServerRpc(OwnerClientId);
        }

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
        remainingMovement.Value = Mathf.Max(remainingMovement.Value - distanceMoved, 0f);
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
        remainingMovement.OnValueChanged -= OnMovementChanged;

        if (TurnManager1.Instance != null)
            TurnManager1.Instance.currentTurn.OnValueChanged -= OnTurnChanged;
    }
}