using UnityEngine;
using Unity.Netcode;

public class PlayerController : MonoBehaviour
{

    void Start()
    {
        if(NetworkManager.Singleton.IsHost)
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
