using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnZone : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        [Range(0f, 100f)] public float spawnChance = 50f;
    }

    [Header("Enemy Settings")]
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] private int minEnemyCount = 3;
    [SerializeField] private int maxEnemyCount = 6;

    [Header("Spawn Area (2D)")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 5f);
    [SerializeField] private float minDistanceBetweenEnemies = 1f;
    [SerializeField] private float radiusDetectColliderSpawn = 1.5f;

    [Header("Spawn Behavior")]
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private bool spawnOnPlayerEnter = true;
    [SerializeField] private bool enableRespawn = false;
    [SerializeField] private bool requireTriggerToRespawn = false;
    [SerializeField] private float respawnDelay = 10f;

    // Internal state
    private bool hasTriggered = false;
    private bool isWaitingForRespawn = false; // Đang trong thời gian delay
    private bool canRespawn = false; // Đã hết delay, sẵn sàng spawn
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Coroutine respawnCoroutine;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (spawnOnStart)
        {
            TriggerSpawn();
        }
    }

    private void Update()
    {
        if (!hasTriggered || spawnedEnemies.Count == 0) return;

        // Cleanup null references và check nếu tất cả enemy đã chết
        CleanupDeadEnemies();

        // Bắt đầu respawn coroutine khi tất cả enemy chết
        if (spawnedEnemies.Count == 0 && enableRespawn && !isWaitingForRespawn && !canRespawn)
        {
            respawnCoroutine = StartCoroutine(RespawnCoroutine());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!spawnOnPlayerEnter) return;

        // Trường hợp 1: Lần đầu spawn
        if (!hasTriggered)
        {
            // Kiểm tra với GameManager nếu có
            if (GameManager.Instance != null && GameManager.Instance.IsSpawned(this))
            {
                return;
            }

            TriggerSpawn();
            return;
        }

        // Trường hợp 2: Respawn khi đã hết delay
        if (canRespawn && spawnedEnemies.Count == 0)
        {
            Debug.Log($"[{gameObject.name}] Player vào trigger sau {respawnDelay}s, respawn ngay!");
            canRespawn = false;
            SpawnEnemies();
        }
    }

    private void TriggerSpawn()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        SpawnEnemies();
        hasTriggered = true;
        isWaitingForRespawn = false;
        canRespawn = false;

        // Lưu vào GameManager nếu có
        if (GameManager.Instance != null)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string key = GameManager.Instance.GetSpawnZoneKey(sceneName, gameObject);
            GameManager.Instance.SetSpawned(key, true, spawnedEnemies);
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        isWaitingForRespawn = true;
        Debug.Log($"[{gameObject.name}] Tất cả enemy đã chết. Chờ {respawnDelay}s...");

        yield return new WaitForSeconds(respawnDelay);

        // Double check không có enemy nào còn sống
        CleanupDeadEnemies();

        if (spawnedEnemies.Count == 0)
        {
            if (requireTriggerToRespawn)
            {
                // Chờ player vào trigger
                canRespawn = true;
                Debug.Log($"[{gameObject.name}] Đã hết delay. Chờ player vào trigger để respawn...");
            }
            else
            {
                // Auto spawn ngay
                Debug.Log($"[{gameObject.name}] Auto respawn enemies");
                SpawnEnemies();
            }
        }

        isWaitingForRespawn = false;
        respawnCoroutine = null;
    }

    private void SpawnEnemies()
    {
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogError($"[{gameObject.name}] Chưa thêm Enemy Prefab nào!");
            return;
        }

        // Clear danh sách cũ
        ClearSpawnedEnemies();

        int enemyCount = Random.Range(minEnemyCount, maxEnemyCount + 1);
        List<Vector2> spawnedPositions = new List<Vector2>();

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject selectedPrefab = GetRandomEnemyPrefab();
            if (selectedPrefab == null) continue;

            Vector2 spawnPos = GetValidSpawnPosition(spawnedPositions);
            if (spawnPos == Vector2.zero) continue;

            GameObject enemy = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
            spawnedEnemies.Add(enemy);
            spawnedPositions.Add(spawnPos);

            // Random flip hướng
            if (Random.value > 0.5f)
            {
                Vector3 scale = enemy.transform.localScale;
                scale.x *= -1;
                enemy.transform.localScale = scale;
            }

            // Reset AI component nếu có
            var enemyAI = enemy.GetComponent<MeleeEnemyAINormal>();
            if (enemyAI != null)
            {
                enemyAI.enabled = false;
                enemyAI.enabled = true;
            }

            // Ignore collision với spawn zone
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            Collider2D zoneCol = GetComponent<Collider2D>();
            if (enemyCol != null && zoneCol != null)
            {
                Physics2D.IgnoreCollision(zoneCol, enemyCol, true);
            }
        }

        Debug.Log($"[{gameObject.name}] Đã spawn {spawnedEnemies.Count}/{enemyCount} enemies");
    }

    private GameObject GetRandomEnemyPrefab()
    {
        List<GameObject> weightedList = new List<GameObject>();

        foreach (var enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null)
            {
                int weight = Mathf.Max(1, Mathf.RoundToInt(enemyData.spawnChance));
                for (int i = 0; i < weight; i++)
                {
                    weightedList.Add(enemyData.enemyPrefab);
                }
            }
        }

        if (weightedList.Count == 0)
        {
            Debug.LogError($"[{gameObject.name}] Không có enemy prefab hợp lệ!");
            return null;
        }

        return weightedList[Random.Range(0, weightedList.Count)];
    }

    private Vector2 GetValidSpawnPosition(List<Vector2> existingPositions)
    {
        int maxAttempts = 30;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomY = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
            Vector2 candidatePos = (Vector2)transform.position + new Vector2(randomX, randomY);

            // Check khoảng cách với enemy khác
            bool tooClose = false;
            foreach (var pos in existingPositions)
            {
                if (Vector2.Distance(candidatePos, pos) < minDistanceBetweenEnemies)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue;

            // Check overlap với obstacle/ground
            Collider2D hit = Physics2D.OverlapCircle(
                candidatePos,
                radiusDetectColliderSpawn,
                LayerMask.GetMask("Obstacle", "Ground")
            );

            if (hit == null)
            {
                return candidatePos;
            }
        }

        Debug.LogWarning($"[{gameObject.name}] Không tìm được vị trí spawn hợp lệ sau {maxAttempts} lần thử!");
        return Vector2.zero;
    }

    private void CleanupDeadEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    public void ClearSpawnedEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    public void ResetZone()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        ClearSpawnedEnemies();
        hasTriggered = false;
        isWaitingForRespawn = false;
        canRespawn = false;
    }

    public void ForceSpawn()
    {
        TriggerSpawn();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Vector3 size = new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f);
        Gizmos.DrawCube(transform.position, size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, size);

        // Vẽ min distance circle
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistanceBetweenEnemies);
    }
}