using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    public NetworkVariable<ulong> currentTurn = new NetworkVariable<ulong>();

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;

        if (IsServer)
        {
            currentTurn.Value = NetworkManager.Singleton.ConnectedClientsIds.First();
        }
        Debug.Log("Turno actual: " + currentTurn.Value);    
    }

    public bool IsMyTurn(ulong clientId)
    {
        return currentTurn.Value == clientId;
    }

    [Rpc(SendTo.Server)]
    public void EndTurnServerRpc()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        int currentIndex = clients.IndexOf(currentTurn.Value);
        int nextIndex = (currentIndex + 1) % clients.Count;

        currentTurn.Value = clients[nextIndex];
    }
}
