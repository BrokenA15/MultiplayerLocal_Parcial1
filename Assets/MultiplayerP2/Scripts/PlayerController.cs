using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerController : NetworkBehaviour
{

    public TMP_Text textoDelbug;

    void Start()
    {
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
        ShootClientRPC();
        ShootServerRPC();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (IsClient)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float speed = 10 * Time.deltaTime;
            transform.Translate(new Vector3(x * speed, y * speed));

        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShootClientRPC()
    {
        textoDelbug.text = "Shoot Client";
    }

    [Rpc(SendTo.Server)]
    public void ShootServerRPC()
    {
        textoDelbug.text = "Shoot Server";

    }
}
