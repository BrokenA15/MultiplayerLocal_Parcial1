using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    public enum GamePhase
    {
        PlacingBarriers,
        Movement,////
        Shooting,
        RoundEnd
    }

    public NetworkVariable<GamePhase> currentPhase = new NetworkVariable<GamePhase>();
    
    public NetworkVariable<int> currentRound = new NetworkVariable<int>(1);
    public NetworkVariable<ulong> currentTurn = new NetworkVariable<ulong>();
    
    public NetworkVariable<int> player1Hits = new NetworkVariable<int>(0);
    public NetworkVariable<int> player2Hits = new NetworkVariable<int>(0);

    private int shotsThisRound = 0;
    private int movementsThisRound = 0;////
    private int barriersPlacedThisRound = 0;
    private int shotsFiredThisRound = 0;
    
    [SerializeField]
    private int maxRounds = 5;
    
    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;
        
        if (IsServer)
        {
            var clients = NetworkManager.Singleton.ConnectedClientsIds.ToList();
            if (clients.Count > 0)
            {
                currentTurn.Value = clients[0];
                currentPhase.Value = GamePhase.PlacingBarriers;
                currentRound.Value = 1;
            }
        }
    }

    public bool IsMyTurn(ulong clientId)
    {
        return currentTurn.Value == clientId;
    }

    [Rpc(SendTo.Server)]
    public void EndTurnServerRpc()
    {
        if (!IsServer) return;
        
        var clients = NetworkManager.Singleton.ConnectedClientsIds.ToList();

         if (currentPhase.Value == GamePhase.PlacingBarriers)
         {
             barriersPlacedThisRound++;
         }
        else if (currentPhase.Value == GamePhase.Movement)////
        {
            movementsThisRound++;
        }                                                 ////
        else if (currentPhase.Value == GamePhase.Shooting)
         {
             shotsFiredThisRound++;
         }
         
        int currentIndex = clients.IndexOf(currentTurn.Value);
        int nextIndex = (currentIndex + 1) % clients.Count;
        currentTurn.Value = clients[nextIndex];

        CheckPhaseProgress();
    }

    public void RegisterHit(ulong attackerId)
    {
        if (!IsServer) return;

        if (attackerId == 0)
            player1Hits.Value++;
        else
            player2Hits.Value++;
    }
    
    public void RegisterShot()
    {
        if (!IsServer) return;

        shotsThisRound++;

        // 2 jugadores = 2 disparos
        if (shotsThisRound >= 2)
        {
            EndRound();
        }
    }
    
    void EndRound()
    {
        Debug.Log($"Ronda {currentRound.Value} terminada");

        if (player1Hits.Value > player2Hits.Value)
            Debug.Log("Jugador 1 gana la ronda");
        else if (player2Hits.Value > player1Hits.Value)
            Debug.Log("Jugador 2 gana la ronda");
        else
            Debug.Log("Empate");

        // Reset para siguiente ronda
        player1Hits.Value = 0;
        player2Hits.Value = 0;
        shotsThisRound = 0;

        currentRound.Value++;

        // 👉 aumentar barreras aquí después
        currentPhase.Value = GamePhase.PlacingBarriers;
    }
    
    void CheckPhaseProgress()
    {
        int playersCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        
        if (currentPhase.Value == GamePhase.PlacingBarriers)
        {
            int totalNeeded = playersCount * currentRound.Value;

            if (barriersPlacedThisRound >= totalNeeded)
            {
                Debug.Log("➡️ Cambiando a fase de MOVIMIENTO");//

                currentPhase.Value = GamePhase.Movement;//
                movementsThisRound = 0;//

                ResetTurnToFirstPlayer();
            }
        }

        else if (currentPhase.Value == GamePhase.Movement)////
        {
            int totalMovements = playersCount;

            if (movementsThisRound >= totalMovements)
            {
                Debug.Log("➡ Cambiando a fase de DISPARO");

                currentPhase.Value = GamePhase.Shooting;
                shotsFiredThisRound = 0;

                ResetTurnToFirstPlayer();
            }
        }                                               ////

        else if (currentPhase.Value == GamePhase.Shooting)
        {
            int totalShots = playersCount;

            if (shotsFiredThisRound >= totalShots)
            {
                Debug.Log("➡️ Fin de ronda");

                currentPhase.Value = GamePhase.RoundEnd;
                Invoke(nameof(NextRound), 2f);
            }
        }
    }
    
    void NextRound()
    {
        if (!IsServer) return;

        currentRound.Value++;

        if (currentRound.Value > maxRounds)
        {
            Debug.Log("🏁 FIN DEL JUEGO");
            
            return;
        }

        Debug.Log("🔁 Nueva ronda: " + currentRound.Value);

        barriersPlacedThisRound = 0;
        movementsThisRound = 0;////
        shotsFiredThisRound = 0;

        currentPhase.Value = GamePhase.PlacingBarriers;

        ResetTurnToFirstPlayer();

       
        CleanupBarriersClientRpc();
    }
    
    void ResetTurnToFirstPlayer()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        if (clients.Count > 0)
            currentTurn.Value = clients[0];
    }
    
    [ClientRpc]
    void CleanupBarriersClientRpc()
    {
        var barriers = GameObject.FindGameObjectsWithTag("Barrera");

        foreach (var b in barriers)
        {
            if (b.TryGetComponent(out NetworkObject netObj))
            {
                if (netObj.IsSpawned)
                    netObj.Despawn();
            }
            else
            {
                Destroy(b);
            }
        }
    }

}
