using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;

public class PlayerController : NetworkBehaviour
{

    public TMP_Text textoDelbug;
    private int score = 0;

    private TurnManager turnManager;

    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();        
        textoDelbug = GameObject.Find("Textito").GetComponent<TMP_Text>();
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("sos el host");
        }
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("sos el server");
        }
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("sos el cliente");
        }
    }
   
    void Update()
    {
        textoDelbug.text = 
            TurnManager.Instance.currentTurn.Value == 0 
                ? "Turno: Host (Jugador 1)" 
                : "Turno: Cliente (Jugador 2)";
        
        if (!IsOwner) return;   

        if (!TurnManager.Instance.IsMyTurn(OwnerClientId)) return;
            
        if (IsClient)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float speed = 10 * Time.deltaTime;
            transform.Translate(new Vector3(x * speed, y * speed));

            if(Input.GetKeyDown(KeyCode.Space))
            {
                TurnManager.Instance.EndTurnServerRpc();
            }
        }   
    }
}
