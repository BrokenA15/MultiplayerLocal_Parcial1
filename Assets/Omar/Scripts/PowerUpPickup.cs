using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{

    public PowerUpType type;
    public float duration = 10f;
    public float value = 0.5f;

    void OnTriggerEnter(Collider other)
    {

        PlayerStats stats = other.GetComponent<PlayerStats>();

        if (stats != null)
        {
            stats.AddPowerup(type, duration, value);
            Destroy(gameObject);
        }

    }

}