using System.Collections;
using UnityEngine;

// Simple obstacle spawner that instantiates a "Rock" prefab at random X positions
// in front of the player at regular intervals.
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject rockPrefab;

    [Header("Spawn Target")]
    [Tooltip("If set, rocks will spawn in front of this transform (usually the player). If null, spawner's position is used.")]
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnIntervalRandomness = 0.5f; // optional random variation

    [Header("Spawn Position")]
    [SerializeField] private float spawnDistance = 30f; // forward distance from player/spawner
    [SerializeField] private float spawnY = 1f; // ensure rocks spawn above ground and are visible
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;

    private Coroutine spawnCoroutine;

    void Start()
    {
        if (rockPrefab == null)
        {
            Debug.LogWarning("ObstacleSpawner: rockPrefab is not assigned.");
            enabled = false;
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnRock();
            float wait = spawnInterval;
            if (spawnIntervalRandomness > 0f)
            {
                wait += Random.Range(-spawnIntervalRandomness, spawnIntervalRandomness);
                wait = Mathf.Max(0.05f, wait);
            }
            yield return new WaitForSeconds(wait);
        }
    }

    private void SpawnRock()
    {
        float x = Random.Range(minX, maxX);

        float zBase = (playerTransform != null) ? playerTransform.position.z : transform.position.z;
        float z = zBase + spawnDistance;

        Vector3 spawnPos = new Vector3(x, spawnY, z);

        // Parent under spawner for hierarchy cleanliness
        Instantiate(rockPrefab, spawnPos, Quaternion.identity, transform);
    }

    void OnDisable()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }
}
