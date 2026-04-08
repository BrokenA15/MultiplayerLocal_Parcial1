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

    [Header("Ajustes de Vida")]
    public NetworkVariable<int> health = new NetworkVariable<int>(100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private Slider healthBar;

    public override void OnNetworkSpawn()
    {
      
        health.OnValueChanged += OnHealthChanged;
   
        UpdateHealthUI(health.Value);
     
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();

        GameObject debugObj = GameObject.Find("Textito");
        if (debugObj != null) textoDelbug = debugObj.GetComponent<TMP_Text>();
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        // Corregido: Debug.Log con 'L' mayúscula
        Debug.Log($"BARRA ACTUALIZADA: {previousValue} -> {newValue}");
        UpdateHealthUI(newValue);
    }

    private void UpdateHealthUI(int currentHealth)
    {
        if (healthBar != null)
        {
            // Forzamos que la división sea decimal usando 100f
            float currentFill = currentHealth / 100f;
            healthBar.value = currentFill;

            // Si esto imprime 0.8, 0.6, etc., el código está PERFECTO 
            // y el problema es el componente Slider en Unity.
            Debug.Log("Valor enviado al Slider: " + currentFill);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return; // Solo el servidor tiene permiso de modificar la NetworkVariable

        health.Value -= damage;
        Debug.Log($"Vida restante del jugador {OwnerClientId}: {health.Value}");

        if (health.Value <= 0)
        {
            Debug.Log("Jugador muerto");
            // Aquí podrías añadir: Despawn o lógica de reinicio
        }
    }

    void Update()
    {
      
        if (textoDelbug != null && TurnManager.Instance != null)
        {
            textoDelbug.text = TurnManager.Instance.currentTurn.Value == 0
                ? "Turno: Host (Jugador 1)"
                : "Turno: Cliente (Jugador 2)";
        }

      
        if (!IsOwner) return;

        if (!TurnManager.Instance.IsMyTurn(OwnerClientId)) return;

        // Movimiento (Input)
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float speed = 10 * Time.deltaTime;
        transform.Translate(new Vector3(x * speed, y * speed));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TurnManager.Instance.EndTurnServerRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
    }
}