using UnityEngine;
using Unity.Netcode;

public class PowerUp : NetworkBehaviour
{
    public enum PowerUpType
    {
        ExplosionRadius,
        Shield,
        Health,
        Stamina
    }

    public PowerUpType type;

    [SerializeField] private int healthAmount = 25;
    [SerializeField] private float staminaAmount = 5f;
    [SerializeField] private float explosionMultiplier = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerController player =
            other.GetComponent<PlayerController>();

        if (player == null) return;

        switch (type)
        {
            case PowerUpType.ExplosionRadius:
                player.IncreaseExplosionRadius(explosionMultiplier);
                break;

            case PowerUpType.Shield:
                player.ActivateShield();
                break;

            case PowerUpType.Health:
                player.AddHealth(healthAmount);
                break;

            case PowerUpType.Stamina:
                player.AddStamina(staminaAmount);
                break;
        }

        //GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}