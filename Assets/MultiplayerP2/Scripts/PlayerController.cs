using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerController : NetworkBehaviour
{

    public TMP_Text textoDelbug;
    private int score = 0;
    public NetworkVariable<int> hp = new NetworkVariable<int> (0);
    public GameObject bullet;

    [SerializeField]
    private float speedBullet = 100;

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

        hp.Value = score;
    }

   
    void Update()
    {
        textoDelbug.text = hp.Value.ToString();
        if (!IsOwner) return;

        if (IsClient)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float speed = 10 * Time.deltaTime;
            transform.Translate(new Vector3(x * speed, y * speed));

            if(Input.GetKeyDown(KeyCode.Space))
            {
                score++; 
                ShootClientRPC();// NIGGA
                
            }
        }
                hp.Value = score;
        
    }

    //New shit 2
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.tag == "Bullet");
        {
            hp.Value -= 20;
            if (hp.Value <= 0)
            {
                hp.Value = 0;
            }
        }
    }

    //[Rpc(SendTo.ClientsAndHost)]
    [Rpc(SendTo.Server)]
    public void ShootClientRPC()
    {
        textoDelbug.text = "Shoot Client";

        //Nuevo lol 
        GameObject b = Instantiate(bullet, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Quaternion.identity);
        b.GetComponent<Rigidbody>().AddForce(Vector3.right * speedBullet * Time.deltaTime);
    }

    [Rpc(SendTo.Server)]
    public void ShootServerRPC()
    {
        textoDelbug.text = "Shoot Server";

    }
}
