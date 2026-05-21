using UnityEngine;
using Unity.Netcode;

public class BarrierHealth : NetworkBehaviour
{
    private int hitsRestantes;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            hitsRestantes = Random.Range(1, 4);
        }
    }

    public void RecibirImpacto()
    {
        hitsRestantes--;

        Debug.Log($"Hits restantes: {hitsRestantes}");

        if (hitsRestantes <= 0)
        {
            if (TryGetComponent(out NetworkObject netObj))
            {
                if (netObj.IsSpawned)
                    netObj.Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}