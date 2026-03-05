using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletForce = 20f;

    public int shootsAvailable;

    private float shotTimer;
    PlayerStats stats; 
    

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        shootsAvailable = 0;
        shotTimer = 0f;
    }

    private void Update()
    {
       if(shotTimer > 0) 
       { 
           shotTimer -= Time.deltaTime;
       }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && shootsAvailable > 0)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        shootsAvailable--;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
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
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            shootsAvailable++;
            Destroy(other.gameObject);
        }
    }
}
