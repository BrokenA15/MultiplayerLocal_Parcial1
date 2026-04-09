using System;
using System.Collections;
using UnityEngine;

public class ForceRotation : MonoBehaviour
{
    public Vector3 targetRotation;
    public float interval = 5f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            transform.rotation = Quaternion.Euler(targetRotation);
            timer = 0f;
        }
    }
}
