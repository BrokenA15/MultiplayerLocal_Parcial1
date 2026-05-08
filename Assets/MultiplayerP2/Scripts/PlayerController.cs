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
    public static bool gameEnded = false;



    [Header("Ajustes de Vida")]
    public NetworkVariable<int> health = new NetworkVariable<int>(100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private Slider healthBar;

    public override void OnNetworkSpawn()
    {
      
        health.OnValueChanged += OnHealthChanged;
   
        UpdateHealthUI(health.Value);
     
        if (turnManager1 == null) turnManager1 = FindFirstObjectByType<TurnManager1>();

        GameObject debugObj = GameObject.Find("Textito");
        if (debugObj != null) textoDelbug = debugObj.GetComponent<TMP_Text>();

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
            float currentFill = currentHealth / 100f;
            healthBar.value = currentFill;

         
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
      
        if (textoDelbug != null && TurnManager1.Instance != null)
        {
            textoDelbug.text = TurnManager1.Instance.currentTurn.Value == 0
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

        // Movimiento (Input)
       

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TurnManager1.Instance.EndTurnServerRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
    }
}