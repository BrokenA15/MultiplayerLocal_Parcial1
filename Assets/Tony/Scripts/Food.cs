using System;
using UnityEngine;

public class Food : MonoBehaviour
{
    public SphereCollider foodCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foodCollider = GetComponent<SphereCollider>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Piso"))
        {
            Destroy (this.gameObject, 6f);
        }
    }
}
