using System;
using UnityEngine;
using System.Collections;


public class BulletDestroy : MonoBehaviour
{
   public SphereCollider bulletCollider;

   void Start()
   {
      bulletCollider = GetComponent<SphereCollider>();
   }

   private void OnCollisionEnter(Collision other)
   {
      if (other.gameObject.CompareTag("Player")  || other.gameObject.CompareTag("Bullet"))
      {
         StartCoroutine(DestroyBullet());
      }
   }

   void Update()
   {
      Destroy(gameObject, 6f);
   }

   private IEnumerator DestroyBullet()
   {
      yield return new WaitForSeconds(1f);
      Destroy(this.gameObject);
   }
}
