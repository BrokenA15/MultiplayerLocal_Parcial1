using UnityEngine;
using Unity.Netcode;

public class DisableOnPlayerJoin : NetworkBehaviour
{
    [Header("Objetos a desactivar")]
    [SerializeField] private GameObject[] objetosADesactivar;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnJugadorConectado;
        }
    }

    private void OnJugadorConectado(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId) return;

        DesactivarObjetosClientRpc();
    }

    [ClientRpc]
    private void DesactivarObjetosClientRpc()
    {
        foreach (GameObject obj in objetosADesactivar)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnJugadorConectado;
        }
    }
}