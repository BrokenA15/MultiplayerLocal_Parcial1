using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{

    public GameObject[] powerUpPrefabs;

    public Transform[] spawnPoints;

    public float spawnDelay = 5f;

    public int maxPowerups = 3;

    private List<GameObject> spawnedPowerups = new List<GameObject>();


    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {

        while (true)
        {

            yield return new WaitForSeconds(spawnDelay);

            spawnedPowerups.RemoveAll(item => item == null);

            if (spawnedPowerups.Count >= maxPowerups)
                continue;

            SpawnPowerup();
        }

    }


    void SpawnPowerup()
    {

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

        GameObject powerup = Instantiate(prefab, point.position, Quaternion.identity);

        spawnedPowerups.Add(powerup);

    }

}