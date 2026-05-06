using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class TurnManager1 : NetworkBehaviour
{
    public static TurnManager1 Instance;

    public enum GamePhase { Construccion, Disparo }
    public NetworkVariable<ulong> currentTurn = new NetworkVariable<ulong>();
    public NetworkVariable<GamePhase> currentPhase = new NetworkVariable<GamePhase>(GamePhase.Construccion);

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;

        if (IsServer)
        {
            var clients = NetworkManager.Singleton.ConnectedClientsIds.ToList();
            if (clients.Count > 0) currentTurn.Value = clients[0];
        }
    }

    public bool IsMyTurn(ulong clientId) => currentTurn.Value == clientId;

    [Rpc(SendTo.Server)]
    public void EndTurnServerRpc()
    {
        // Si terminamos de disparar, pasamos al siguiente jugador y volvemos a construcción
        if (currentPhase.Value == GamePhase.Disparo)
        {
            PasarSiguienteJugador();
            currentPhase.Value = GamePhase.Construccion;
        }
        else
        {
            // Si estábamos en construcción, pasamos a disparo del mismo jugador
            currentPhase.Value = GamePhase.Disparo;
        }
    }

    private void PasarSiguienteJugador()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsIds.ToList();
        int currentIndex = clients.IndexOf(currentTurn.Value);
        int nextIndex = (currentIndex + 1) % clients.Count;
        currentTurn.Value = clients[nextIndex];
    }
}