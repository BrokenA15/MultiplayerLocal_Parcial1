using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float knockback = 10f;

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
        PlayerStats stats = collision.collider.GetComponent<PlayerStats>();

        if (rb != null)
        {
            float resistance = 1f;

            if (stats != null)
                resistance = stats.knockbackResistance;

            Vector3 dir = (collision.transform.position - transform.position).normalized;

            rb.AddForce(dir * knockback / resistance, ForceMode.Impulse);
        }

        Destroy(gameObject, 1.5f);
    }
}