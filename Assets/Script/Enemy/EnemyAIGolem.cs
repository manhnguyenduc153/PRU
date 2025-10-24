using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAIGolem : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float nextWaypointDistance = 2f;
    public float repeatTimeUpdatePath = 0.5f;
    public SpriteRenderer characterSR;
    public float stopDistance = 8f;

    [Header("Prefab Projectile Attack")]
    public bool usePrefabAttack = true;
    public GameObject projectilePrefab;
    public Transform prefabSpawnPoint;
    public float prefabAttackRange = 6f;
    public float prefabAttackMaxRange = 15f;
    public float prefabAttackCooldown = 3f;
    public float prefabProjectileSpeed = 10f;
    private float prefabAttackTimer;
    private bool isFiringPrefab = false;

    [Header("Extra Skills")]
    public GameObject warningPrefab; // Prefab cảnh báo
    public GameObject extraPrefab;   // Prefab spawn quanh player
    public int extraProjectileCount = 12; // Số lượng projectile 360 độ
    public int extraPrefabCount = 5;
    public float extraProjectileSpeed = 8f;
    public float extraPrefabRange = 5f;
    public float extraPrefabCooldown = 5f;
    public float extraProjectileCooldown = 5f;
    private float extraPrefabTimer;
    private float extraProjectileTimer;
    private bool isUsingExtraPrefab = false;
    private bool isUsingExtraProjectile = false;

    [Header("Pathfinding Settings")]
    public float maxPathDistance = 100f;
    public bool waitForGraphScan = true;
    private bool isInitialized = false;

    [Header("Freeze Settings")]
    public float freezeDurationTime = 1f;
    private float freezeDuration;

    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Animator animator;
    private Coroutine moveCoroutine;
    private Transform player;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        player = FindObjectOfType<PlayerController>()?.transform;
        freezeDuration = 0;
        prefabAttackTimer = 0f;
        extraPrefabTimer = 0f;
        extraProjectileTimer = 0f;

        StartCoroutine(InitializePathfinding());
    }

    private IEnumerator InitializePathfinding()
    {
        if (waitForGraphScan && AstarPath.active != null)
        {
            while (AstarPath.active.isScanning)
            {
                yield return null;
            }
            yield return new WaitForEndOfFrame();
        }

        if (AstarPath.active != null)
        {
            NNInfo nearestNode = AstarPath.active.GetNearest(transform.position);
            if (nearestNode.node != null)
            {
                Vector3 nearestPoint = (Vector3)nearestNode.position;
                float distanceToNode = Vector3.Distance(transform.position, nearestPoint);

                if (distanceToNode > 2f)
                {
                    Debug.LogWarning($"Golem spawned too far from navmesh. Moving from {transform.position} to {nearestPoint}");
                    transform.position = nearestPoint;
                }
            }
            else
            {
                Debug.LogError("No valid pathfinding node found near Golem spawn position!");
                yield break;
            }
        }

        isInitialized = true;
        InvokeRepeating(nameof(CalculatePath), 0f, repeatTimeUpdatePath);
    }

    private void Update()
    {
        if (prefabAttackTimer > 0)
            prefabAttackTimer -= Time.deltaTime;

        if (extraPrefabTimer > 0)
            extraPrefabTimer -= Time.deltaTime;

        if (extraProjectileTimer > 0)
            extraProjectileTimer -= Time.deltaTime;

        FacePlayer();
    }

    private void FacePlayer()
    {
        if (player == null || characterSR == null) return;

        float dirX = player.position.x - transform.position.x;
        if (Mathf.Abs(dirX) > 0.05f)
        {
            characterSR.transform.localScale = new Vector3(
                dirX < 0 ? -1 : 1,
                1,
                1
            );
        }
    }

    private void CalculatePath()
    {
        if (!isInitialized || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > maxPathDistance) return;

        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, player.position, OnPathCompleted);
        }
    }

    private void OnPathCompleted(Path p)
    {
        if (p.error) return;

        path = p;
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToPlayerCoroutine());
    }

    IEnumerator MoveToPlayerCoroutine()
    {
        int currentWP = 0;

        while (path != null && currentWP < path.vectorPath.Count)
        {
            while (freezeDuration > 0)
            {
                freezeDuration -= Time.deltaTime;
                rb.velocity = Vector2.zero;
                yield return null;
            }

            if (player == null)
                yield break;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Kiểm tra chiêu
            bool canPrefab = usePrefabAttack && CanUsePrefabAttack(distanceToPlayer);
            bool canExtraPrefab = !isUsingExtraPrefab && extraPrefabTimer <= 0;
            bool canExtraProjectile = !isUsingExtraProjectile && extraProjectileTimer <= 0 && projectilePrefab != null;

            if (canPrefab)
            {
                rb.velocity = Vector2.zero;
                yield return StartCoroutine(PerformPrefabAttack());
                continue;
            }
            else if (canExtraPrefab)
            {
                rb.velocity = Vector2.zero;
                yield return StartCoroutine(PerformExtraPrefab());
                continue;
            }
            else if (canExtraProjectile)
            {
                rb.velocity = Vector2.zero;
                yield return StartCoroutine(PerformExtraProjectile());
                continue;
            }

            if (distanceToPlayer <= stopDistance)
            {
                rb.velocity = Vector2.zero;
                yield return null;
                continue;
            }

            Vector2 targetPos = path.vectorPath[currentWP];
            Vector2 direction = (targetPos - rb.position).normalized;

            Vector2 movement = direction * moveSpeed * Time.deltaTime;
            transform.position += (Vector3)movement;

            float distance = Vector2.Distance(rb.position, targetPos);
            if (distance < nextWaypointDistance)
                currentWP++;

            yield return null;
        }
    }

    bool CanUsePrefabAttack(float distanceToPlayer)
    {
        return prefabAttackTimer <= 0
            && !isFiringPrefab
            && distanceToPlayer >= prefabAttackRange
            && distanceToPlayer <= prefabAttackMaxRange
            && projectilePrefab != null
            && prefabSpawnPoint != null;
    }

    IEnumerator PerformPrefabAttack()
    {
        isFiringPrefab = true;
        prefabAttackTimer = prefabAttackCooldown;

        if (animator != null)
            animator.SetTrigger("Lazer");

        yield return new WaitForSeconds(0.3f);

        ShootPrefabProjectile();

        yield return new WaitForSeconds(0.3f);
        isFiringPrefab = false;
    }

    void ShootPrefabProjectile()
    {
        if (projectilePrefab == null || player == null || prefabSpawnPoint == null) return;

        Vector2 direction = (player.position - prefabSpawnPoint.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, prefabSpawnPoint.position, Quaternion.identity);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
            projRb.velocity = direction * prefabProjectileSpeed;
    }

    // Chiêu 1: Spawn prefab quanh player với cảnh báo
    // Chiêu 1: Spawn prefab quanh player với cảnh báo
    IEnumerator PerformExtraPrefab()
    {
        isUsingExtraPrefab = true;
        extraPrefabTimer = extraPrefabCooldown;

        if (animator != null)
            animator.SetTrigger("Lazer");

        List<Vector2> spawnPositions = new List<Vector2>();
        List<GameObject> warnings = new List<GameObject>();

        // Tạo vị trí spawn ngẫu nhiên 1 lần và spawn warning
        for (int i = 0; i < extraPrefabCount; i++)
        {
            Vector2 randomPos = (Vector2)player.position + Random.insideUnitCircle * extraPrefabRange;
            spawnPositions.Add(randomPos);

            if (warningPrefab != null)
            {
                GameObject warning = Instantiate(warningPrefab, randomPos, Quaternion.identity);
                warnings.Add(warning);
            }
            else
            {
                warnings.Add(null); // để giữ index đồng bộ
            }
        }

        yield return new WaitForSeconds(0.2f);

        // Spawn prefab ở cùng vị trí với warning và destroy warning
        for (int i = 0; i < extraPrefabCount; i++)
        {
            if (extraPrefab != null)
                Instantiate(extraPrefab, spawnPositions[i], Quaternion.identity);

            if (warnings[i] != null)
                Destroy(warnings[i]); // xóa warning khi prefab xuất hiện
        }

        yield return new WaitForSeconds(0.3f);
        isUsingExtraPrefab = false;
    }




    // Chiêu 2: Bắn projectile 360 độ
    IEnumerator PerformExtraProjectile()
    {
        isUsingExtraProjectile = true;
        extraProjectileTimer = extraProjectileCooldown;

        if (animator != null)
            animator.SetTrigger("Lazer");

        int count = extraProjectileCount;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject proj = Instantiate(projectilePrefab, prefabSpawnPoint.position, Quaternion.identity);
            proj.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

            Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
            if (rbProj != null)
                rbProj.velocity = dir * extraProjectileSpeed;
        }

        yield return new WaitForSeconds(0.3f);
        isUsingExtraProjectile = false;
    }

    public void FreezeEnemy()
    {
        freezeDuration = freezeDurationTime;
    }

    public void PlayHurtAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    public void ForceRecalculatePath()
    {
        if (isInitialized)
        {
            CancelInvoke(nameof(CalculatePath));
            InvokeRepeating(nameof(CalculatePath), 0f, repeatTimeUpdatePath);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (usePrefabAttack)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, prefabAttackRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, prefabAttackMaxRange);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maxPathDistance);

        if (prefabSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(prefabSpawnPoint.position, 0.3f);
        }
    }
}
