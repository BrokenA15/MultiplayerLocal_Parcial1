using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayConnection : MonoBehaviour
{
    [SerializeField] private TMP_Text code;
    [SerializeField] private TMP_InputField codeInput;  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.Instance.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public async void StartRelay()
    {
       string joinCode = await StartHost_(2);
        code.text = joinCode;
    }
    
    public async void JoinRelay()
    {
        await StartClient_(codeInput.text);
    }

    
    async Task<string> StartHost_(int MaxConnections)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        string joincCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        
        Debug.Log(joincCode);
        
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
        NetworkManager.Singleton.StartHost();
        
        return joincCode;   
        
    }
    
    async Task<bool> StartClient_(string joinCode)
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
        
        NetworkManager.Singleton.StartClient();
        
        return NetworkManager.Singleton.StartClient();   
        
    }
}
