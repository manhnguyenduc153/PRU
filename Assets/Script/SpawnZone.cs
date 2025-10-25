using UnityEngine;
using System.Collections.Generic;

public class SpawnZone : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        [Range(0f, 100f)]
        public float spawnChance = 50f;
    }

    [Header("Enemy Settings")]
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] private int minEnemyCount = 3;
    [SerializeField] private int maxEnemyCount = 6;

    [Header("Spawn Area (2D)")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 5f);

    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnce = true;
    [SerializeField] private float minDistanceBetweenEnemies = 1f;

    public float radiusDetectColliderSpawn = 1.5f;

    [SerializeField] private bool hasSpawned = false;
    [SerializeField] private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Kiểm tra với GameManager xem đã spawn chưa
        if (spawnOnce && GameManager.Instance != null && GameManager.Instance.IsSpawned(this))
            return;

        if (!spawnOnce || !hasSpawned)
        {
            SpawnEnemies();
            hasSpawned = true;

            // Cập nhật trạng thái vào GameManager
            if (GameManager.Instance != null)
            {
                string key = GameManager.Instance.GetSpawnZoneKey(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, gameObject);
                GameManager.Instance.SetSpawned(key, true, spawnedEnemies);
            }
        }
    }

    private void SpawnEnemies()
    {
        if (enemyTypes.Count == 0)
        {
            Debug.LogError("Chưa thêm Enemy Prefab nào!");
            return;
        }

        int enemyCount = Random.Range(minEnemyCount, maxEnemyCount + 1);
        List<Vector2> spawnedPositions = new List<Vector2>();

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject selectedEnemy = GetRandomEnemyPrefab();
            if (selectedEnemy == null) continue;

            Vector2 spawnPos = GetRandomSpawnPosition(spawnedPositions);
            if (spawnPos != Vector2.zero)
            {
                GameObject enemy = Instantiate(selectedEnemy, spawnPos, Quaternion.identity);
                spawnedEnemies.Add(enemy);
                spawnedPositions.Add(spawnPos);

                if (Random.value > 0.5f)
                {
                    Vector3 scale = enemy.transform.localScale;
                    scale.x *= -1;
                    enemy.transform.localScale = scale;
                }

                var enemyAI = enemy.GetComponent<MeleeEnemyAINormal>();
                if (enemyAI != null)
                {
                    enemyAI.enabled = false;
                    enemyAI.enabled = true;
                }

                Collider2D enemyCol = enemy.GetComponent<Collider2D>();
                Collider2D zoneCol = GetComponent<Collider2D>();
                if (enemyCol != null && zoneCol != null)
                {
                    Physics2D.IgnoreCollision(zoneCol, enemyCol, true);
                }
            }
        }

        Debug.Log($"Đã spawn {spawnedEnemies.Count} enemies trong khu vực {gameObject.name}");
    }

    private GameObject GetRandomEnemyPrefab()
    {
        List<GameObject> weightedList = new List<GameObject>();
        foreach (var enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null)
            {
                int weight = Mathf.RoundToInt(enemyData.spawnChance);
                for (int i = 0; i < weight; i++) weightedList.Add(enemyData.enemyPrefab);
            }
        }
        if (weightedList.Count == 0)
        {
            Debug.LogError("Không có enemy prefab hợp lệ!");
            return null;
        }
        return weightedList[Random.Range(0, weightedList.Count)];
    }

    private Vector2 GetRandomSpawnPosition(List<Vector2> existingPositions)
    {
        int attempts = 0;
        int maxAttempts = 30;

        while (attempts < maxAttempts)
        {
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomY = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
            Vector2 randomPos = (Vector2)transform.position + new Vector2(randomX, randomY);

            bool validPosition = true;
            foreach (var pos in existingPositions)
            {
                if (Vector2.Distance(randomPos, pos) < minDistanceBetweenEnemies)
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
            {
                float checkRadius = radiusDetectColliderSpawn;
                Collider2D hit = Physics2D.OverlapCircle(randomPos, checkRadius, LayerMask.GetMask("Obstacle", "Ground"));
                if (hit == null) return randomPos;
            }

            attempts++;
        }

        Debug.LogWarning("Không tìm được vị trí spawn phù hợp (vướng collider)!");
        return Vector2.zero;
    }

    public void ClearSpawnedEnemies()
    {
        foreach (var enemy in spawnedEnemies)
            if (enemy != null) Destroy(enemy);
        spawnedEnemies.Clear();
        hasSpawned = false;
    }

    public void ResetZone()
    {
        hasSpawned = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Vector3 size = new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f);
        Gizmos.DrawCube(transform.position, size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
