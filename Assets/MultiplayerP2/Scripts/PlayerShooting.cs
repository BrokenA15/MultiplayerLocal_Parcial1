using UnityEngine;
using Unity.Netcode;

public class PlayerShooting : NetworkBehaviour
{
    public Transform shootPoint;
    [SerializeField] private GameObject dynamitePrefab;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject poisonPotionPrefab;

    public float minForce = 5f;
    public float maxForce = 20f;
    public float force = 10f;
    public float angle = 45f;

    private bool yaDisparoEnEsteTurno = false;

    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 50;
    [SerializeField] private float timeStep = 0.1f;

    [SerializeField] private float dynamiteMass = 1.5f;
    [SerializeField] private float arrowMass = 0.15f;
    [SerializeField] private float poisonMass = 2.5f;

    public enum WeaponType { Dynamite, Bow, PoisonPotion }
    public WeaponType currentWeapon = WeaponType.Dynamite;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Resetear el seguro al cambiar de turno
        if (TurnManager1.Instance != null)
            TurnManager1.Instance.currentTurn.OnValueChanged += OnTurnChanged;
    }

    private void OnTurnChanged(ulong prev, ulong next)
    {
        yaDisparoEnEsteTurno = false;

        // Ocultar línea de trayectoria al cambiar turno
        if (trajectoryLine != null)
            trajectoryLine.positionCount = 0;
    }

    void Update()
    {
        // 🔑 FIX PLAYER 2: Ya no dependemos de IsOwner ni de IsMyTurn aquí.
        // PlayerAction.Update() se encarga de encender/apagar este script según fase y turno.
        // Si este script está enabled, significa que TurnManager nos dio permiso.
        // Solo bloqueamos si no somos el dueño del objeto.
        if (!IsOwner) return;

        if (TurnManager1.Instance == null) return;

        bool esFaseDisparo = TurnManager1.Instance.currentPhase.Value == TurnManager1.GamePhase.Disparo;

        if (!esFaseDisparo)
        {
            if (trajectoryLine != null) trajectoryLine.positionCount = 0;
            return;
        }

        if (yaDisparoEnEsteTurno) return;

        HandleWeaponSelection();
        HandleAim();
        HandleShoot();
        DrawTrajectory();
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
        shootPoint.localPosition = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);
        shootPoint.right = (shootPoint.position - transform.position).normalized;
    }

    void HandleShoot()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            yaDisparoEnEsteTurno = true;
            ShootServerRpc(shootPoint.position, shootPoint.right, force, (int)currentWeapon);
        }
    }

    float GetCurrentMass()
    {
        switch (currentWeapon)
        {
            case WeaponType.Bow: return arrowMass;
            case WeaponType.PoisonPotion: return poisonMass;
            default: return dynamiteMass;
        }
    }

    void DrawTrajectory()
    {
        if (trajectoryLine == null) return;

        trajectoryLine.positionCount = trajectoryPoints;
        Vector3 startPosition = shootPoint.position;
        float mass = GetCurrentMass();
        Vector3 velocity = shootPoint.right * (force / mass);

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeStep;
            Vector3 point = startPosition + velocity * time + 0.5f * Physics.gravity * time * time;
            trajectoryLine.SetPosition(i, point);

            if (i > 0)
            {
                Vector3 previous = trajectoryLine.GetPosition(i - 1);
                if (Physics.Linecast(previous, point))
                {
                    trajectoryLine.positionCount = i + 1;
                    break;
                }
            }
        }
    }

    void HandleWeaponSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentWeapon = WeaponType.Dynamite; Debug.Log("Dinamita"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentWeapon = WeaponType.Bow; Debug.Log("Arco"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { currentWeapon = WeaponType.PoisonPotion; Debug.Log("Poción"); }
    }

    [Rpc(SendTo.Server)]
    void ShootServerRpc(Vector3 pos, Vector3 dir, float force, int weaponType)
    {
        GameObject selectedPrefab = dynamitePrefab;

        switch ((WeaponType)weaponType)
        {
            case WeaponType.Bow: selectedPrefab = arrowPrefab; break;
            case WeaponType.PoisonPotion: selectedPrefab = poisonPotionPrefab; break;
            default: selectedPrefab = dynamitePrefab; break;
        }

        GameObject proj = Instantiate(selectedPrefab, pos, Quaternion.identity);
        NetworkObject netObj = proj.GetComponent<NetworkObject>();
        netObj.Spawn();

      /*Projectile projectileScript = proj.GetComponent<Projectile>();
        if (projectileScript != null)
            projectileScript.Launch(dir, force, OwnerClientId);*/
        if (proj.TryGetComponent(out Projectile dynamite))
        {
            dynamite.Launch(dir, force, OwnerClientId);
        }
        else if (proj.TryGetComponent(out ArrowProjectile arrow))
        {
            arrow.Launch(
                dir,
                force);
        }
        else if (proj.TryGetComponent(
                     out PoisonPotionProjectile poison))
        {
            poison.Launch(
                dir,
                force);
        }

        FollowProjectileClientRpc(netObj.NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void FollowProjectileClientRpc(ulong projectileId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(projectileId, out NetworkObject netObj))
        {
            if (CameraManager.Instance != null)
                CameraManager.Instance.FollowProjectile(netObj.transform);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (TurnManager1.Instance != null)
            TurnManager1.Instance.currentTurn.OnValueChanged -= OnTurnChanged;
    }
}