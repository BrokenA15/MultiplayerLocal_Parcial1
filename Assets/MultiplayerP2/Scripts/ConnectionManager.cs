using UnityEngine;
using static Unity.Netcode.NetworkManager;
using Unity.Netcode;

public class ConnectionManager : MonoBehaviour
{

    public void Start_Host()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Start_Client()
    {
        NetworkManager.Singleton.StartClient();

    }

    public void Start_Server()
    {
        NetworkManager.Singleton.StartServer();

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
