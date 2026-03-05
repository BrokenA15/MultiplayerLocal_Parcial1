using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletForce = 20f;


    //-------------------------------------------------------//
    //------------------------Start--------------------------//
    //-------------------------------------------------------//
    PlayerStats stats; 

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }
    //--------------------------------------------------------//
    //-------------------------End----------------------------//
    //--------------------------------------------------------//
    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        //-------------------------------------------------------//
        //------------------------Start--------------------------//
        //-------------------------------------------------------//

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.knockback *= stats.knockbackPower;
        }

        bullet.transform.localScale *= stats.projectileSizeMultiplier;

        if (rb != null)
        {
            rb.linearVelocity = bulletSpawn.forward * bulletForce * stats.projectileForceMultiplier;
        }

        //--------------------------------------------------------//
        //-------------------------End----------------------------//
        //--------------------------------------------------------//

        /*if (rb != null)
        {
            rb.linearVelocity = bulletSpawn.forward * bulletForce;
        }*/

    }
}
