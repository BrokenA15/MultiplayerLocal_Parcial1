using UnityEngine;
using Unity.Netcode;

public class PlayerShooting : NetworkBehaviour
{
    public Transform shootPoint;
    //public GameObject projectilePrefab;
    [SerializeField] private GameObject dynamitePrefab;     //
    [SerializeField] private GameObject arrowPrefab;        //
    [SerializeField] private GameObject poisonPotionPrefab; //

    public float minForce = 5f;
    public float maxForce = 20f;
    public float force = 10f;
    public float angle = 45f;

    // --- SEGURO ANTI-SPAM ---
    private bool yaDisparoEnEsteTurno = false;
    private bool eraMiTurno = false;

    [SerializeField] private LineRenderer trajectoryLine; /*        TrajectoryLine      */

    [SerializeField] private int trajectoryPoints = 50;
    [SerializeField] private float timeStep = 0.1f;

    [SerializeField] private float dynamiteMass = 1.5f;
    [SerializeField] private float arrowMass = 0.15f;
    [SerializeField] private float poisonMass = 2.5f;   /*                              */



    public enum WeaponType
    {
        Dynamite,
        Bow,
        PoisonPotion
    }

    public WeaponType currentWeapon = WeaponType.Dynamite;

    void Update()
    {
        if (!IsOwner) return;

        if (TurnManager1.Instance == null) return;

        bool esMiTurno = TurnManager1.Instance.IsMyTurn(OwnerClientId);
        bool esFaseDisparo = TurnManager1.Instance.currentPhase.Value == TurnManager1.GamePhase.Disparo;
        
        if (esMiTurno && !eraMiTurno)
        {
            yaDisparoEnEsteTurno = false;
        }
      
        eraMiTurno = esMiTurno;

        if (!esMiTurno) return;

        if (!esFaseDisparo) return;

        if (yaDisparoEnEsteTurno) return;

        HandleWeaponSelection(); //
        HandleAim();
        HandleShoot();
        DrawTrajectory(); //
    }

    void HandleAim()
    {
        if (Input.GetKey(KeyCode.Q)) angle += 80f * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) angle -= 80f * Time.deltaTime;
        if (Input.GetKey(KeyCode.Z)) force += 10f * Time.deltaTime;
        if (Input.GetKey(KeyCode.C)) force -= 10f * Time.deltaTime;

        angle = Mathf.Clamp(angle, 0f, 180f);
        force = Mathf.Clamp(force, minForce, maxForce);

        UpdateShootPointPosition();
    }

    void UpdateShootPointPosition()
    {
        float radius = 1.5f;
        float rad = angle * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad) * radius;
        float y = Mathf.Sin(rad) * radius;

        shootPoint.localPosition = new Vector3(x, y, 0);
        shootPoint.right = (shootPoint.position - transform.position).normalized;
    }


    void HandleShoot()
    {
        if (TurnManager1.Instance.currentPhase.Value != TurnManager1.GamePhase.Disparo)
            return;
        
        if (Input.GetKeyDown(KeyCode.Return) && !yaDisparoEnEsteTurno)
        {
            yaDisparoEnEsteTurno = true;

            //ShootServerRpc(shootPoint.position, shootPoint.right, force);
            ShootServerRpc(shootPoint.position, shootPoint.right, force,(int)currentWeapon); // 


        }
    }

    float GetCurrentMass()                  /*          TrajectoryLine          */
    {
        switch (currentWeapon)
        {
            case WeaponType.Bow:
                return arrowMass;

            case WeaponType.PoisonPotion:
                return poisonMass;

            default:
                return dynamiteMass;
        }
    }

    void DrawTrajectory()
    {
        trajectoryLine.positionCount = trajectoryPoints;

        Vector3 startPosition = shootPoint.position;

        float mass = GetCurrentMass();

        Vector3 velocity =
            shootPoint.right * (force / mass);

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeStep;

            Vector3 point =
                startPosition +
                velocity * time +
                0.5f * Physics.gravity * time * time;

            trajectoryLine.SetPosition(i, point);

            if (i > 0)
            {
                Vector3 previous =
                    trajectoryLine.GetPosition(i - 1);

                if (Physics.Linecast(previous, point))
                {
                    trajectoryLine.positionCount = i + 1;
                    break;
                }
            }
        }
    }                                           /*---------------------------------------*/

    void HandleWeaponSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeapon = WeaponType.Dynamite;
            Debug.Log("Dinamita seleccionada");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon = WeaponType.Bow;
            Debug.Log("Arco seleccionado");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentWeapon = WeaponType.PoisonPotion;
            Debug.Log("Poción seleccionada");
        }
    }
    [Rpc(SendTo.Server)]
    //void ShootServerRpc(Vector3 pos, Vector3 dir, float force)
      void ShootServerRpc(Vector3 pos, Vector3 dir, float force, int weaponType)  // Se le agrego weaponType para la seleccion de armas
    {
        //GameObject proj = Instantiate(projectilePrefab, pos, Quaternion.identity);
        GameObject selectedPrefab = dynamitePrefab;

        WeaponType weapon =
            (WeaponType)weaponType;

        switch (weapon)
        {
            case WeaponType.Bow:
                selectedPrefab = arrowPrefab;
                break;

            case WeaponType.PoisonPotion:
                selectedPrefab = poisonPotionPrefab;
                break;

            case WeaponType.Dynamite:
            default:
                selectedPrefab = dynamitePrefab;
                break;
        }

        GameObject proj = Instantiate(selectedPrefab, pos, Quaternion.identity);

        NetworkObject netObj = proj.GetComponent<NetworkObject>();
        netObj.Spawn();

        Projectile projectileScript = proj.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Launch(dir, force, OwnerClientId);
        }

        // Notificamos a todos los clientes que sigan esta nueva bala
        FollowProjectileClientRpc(netObj.NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void FollowProjectileClientRpc(ulong projectileId)
    {
        // Buscamos el objeto en la red usando su ID �nico
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(projectileId, out NetworkObject netObj))
        {
            // Verificamos que el CameraManager exista antes de llamarlo
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.FollowProjectile(netObj.transform);
            }
        }
    }
}

