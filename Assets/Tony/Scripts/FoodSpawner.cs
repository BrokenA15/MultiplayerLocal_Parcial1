using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;

    [Header("Spawn Area")]
    public Vector2 areaSize = new Vector2(10f, 10f);

    [Header("Spawn Timing")]
    public float startSpawnTime = 3f;
    public float minSpawnTime = 0.5f;
    public float difficultyTime = 30f;

    [Header("Multiple Spawn Chance")]
    [Range(0,1)]
    public float doubleSpawnChance = 0.3f;

    private float timer;
    private float gameTimer;

    void Update()
    {
        gameTimer += Time.deltaTime;

        float difficulty = Mathf.Clamp01(gameTimer / difficultyTime);
        float currentSpawnTime = Mathf.Lerp(startSpawnTime, minSpawnTime, difficulty);

        timer += Time.deltaTime;

        if (timer >= currentSpawnTime)
        {
            SpawnFood();

            if (Random.value < doubleSpawnChance)
            {
                SpawnFood();
            }

            timer = 0f;
        }
    }

    void SpawnFood()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-areaSize.x / 2, areaSize.x / 2),
            0,
            Random.Range(-areaSize.y / 2, areaSize.y / 2)
        );

        Vector3 spawnPos = transform.position + randomPos;

        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0.1f, areaSize.y));
    }
}
