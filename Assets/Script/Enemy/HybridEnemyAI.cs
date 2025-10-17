using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class HybridEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float nextWaypointDistance = 2f;
    public float repeatTimeUpdatePath = 0.5f;
    public SpriteRenderer characterSR;

    [Header("Combat Settings")]
    public float meleeAttackRange = 2f;
    public float rangedAttackRange = 10f;
    public float attackCooldown = 2f;

    [Header("Melee Attack")]
    public GameObject slashPrefab;
    public Transform slashSpawnPoint;
    public float slashOffsetDistance = 1.5f;
    public float knockbackForce = 8f;

    [Header("Ranged Skill")]
    public bool hasRangedSkill = true; // Toggle để bật/tắt skill bắn
    private EnemyRangedSkill rangedSkill;

    [Header("Combat Behavior")]
    [Tooltip("Ưu tiên tấn công tầm xa khi có thể")]
    public bool preferRanged = true;
    [Tooltip("Xác suất dùng skill tầm xa khi trong range (0-1)")]
    [Range(0f, 1f)]
    public float rangedSkillChance = 0.7f;

    private float attackTimer;
    private bool isAttacking = false;

    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Animator animator;

    private Coroutine moveCoroutine;
    private Transform player;

    [Header("Freeze Settings")]
    public float freezeDurationTime;
    private float freezeDuration;

    [Header("Pathfinding Fix")]
    public float maxPathDistance = 100f;
    public bool waitForGraphScan = true;
    private bool isInitialized = false;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rangedSkill = GetComponent<EnemyRangedSkill>();

        player = FindObjectOfType<PlayerController>()?.transform;
        freezeDuration = 0;
        attackTimer = 0f;

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
                    Debug.LogWarning($"Enemy spawned too far from navmesh. Moving from {transform.position} to {nearestPoint}");
                    transform.position = nearestPoint;
                }
            }
            else
            {
                Debug.LogError("No valid pathfinding node found near enemy spawn position!");
                yield break;
            }
        }

        isInitialized = true;
        InvokeRepeating(nameof(CalculatePath), 0f, repeatTimeUpdatePath);
    }

    private void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

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
        if (distanceToPlayer > maxPathDistance)
        {
            Debug.LogWarning($"Player too far away: {distanceToPlayer}. Max distance: {maxPathDistance}");
            return;
        }

        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, player.position, OnPathCompleted);
        }
    }

    private void OnPathCompleted(Path p)
    {
        if (p.error)
        {
            Debug.LogWarning($"Path error: {p.errorLog}");
            return;
        }

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

            if (isAttacking)
            {
                rb.velocity = Vector2.zero;
                yield return null;
                continue;
            }

            // ✅ Logic tấn công mới: ưu tiên ranged nếu được bật
            bool triedAttack = TryPerformAttack(distanceToPlayer);

            if (triedAttack)
            {
                rb.velocity = Vector2.zero;

                // Đợi cho đến khi tấn công xong
                while (isAttacking || attackTimer > 0)
                {
                    distanceToPlayer = Vector2.Distance(transform.position, player.position);
                    rb.velocity = Vector2.zero;
                    yield return null;
                }

                continue;
            }

            // Di chuyển về phía player
            Vector2 targetPos = path.vectorPath[currentWP];
            Vector2 direction = (targetPos - rb.position).normalized;

            float speedMultiplier = distanceToPlayer < meleeAttackRange * 1.5f ? 0.7f : 1f;
            Vector2 movement = direction * moveSpeed * speedMultiplier * Time.deltaTime;

            transform.position += (Vector3)movement;

            float distance = Vector2.Distance(rb.position, targetPos);
            if (distance < nextWaypointDistance)
                currentWP++;

            yield return null;
        }
    }

    /// <summary>
    /// Thử tấn công player (tự động chọn ranged hoặc melee)
    /// </summary>
    private bool TryPerformAttack(float distanceToPlayer)
    {
        if (attackTimer > 0 || isAttacking) return false;

        // Kiểm tra ranged attack trước nếu được ưu tiên
        if (hasRangedSkill && rangedSkill != null && preferRanged)
        {
            if (distanceToPlayer <= rangedAttackRange && distanceToPlayer > meleeAttackRange)
            {
                // Random xem có dùng ranged không
                if (Random.value <= rangedSkillChance)
                {
                    if (rangedSkill.TryUseSkill())
                    {
                        isAttacking = true;
                        attackTimer = attackCooldown;
                        StartCoroutine(ResetAttackStateForRanged());
                        return true;
                    }
                }
            }
        }

        // Melee attack khi đủ gần
        if (distanceToPlayer <= meleeAttackRange)
        {
            TryMeleeAttack();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tấn công cận chiến
    /// </summary>
    void TryMeleeAttack()
    {
        if (attackTimer > 0 || isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("Attack");

        attackTimer = attackCooldown;
        StartCoroutine(ResetMeleeAttackState());
    }

    IEnumerator ResetMeleeAttackState()
    {
        yield return new WaitForSeconds(0.3f);
        SpawnSlashEffect();
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    IEnumerator ResetAttackStateForRanged()
    {
        // Đợi skill animation hoàn thành
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    void SpawnSlashEffect()
    {
        if (slashPrefab == null || player == null) return;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        Vector3 spawnPosition;
        if (slashSpawnPoint != null)
        {
            spawnPosition = slashSpawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position + (Vector3)directionToPlayer * slashOffsetDistance;
        }

        GameObject slash = Instantiate(slashPrefab, spawnPosition, Quaternion.identity);

        EnemySlashEffect slashEffect = slash.GetComponent<EnemySlashEffect>();
        if (slashEffect != null)
        {
            slashEffect.Initialize(directionToPlayer, characterSR.transform.localScale.x < 0, knockbackForce);
        }
    }

    public void FreezeEnemy()
    {
        freezeDuration = freezeDurationTime;
    }

    public void PlayHurtAnimation()
    {
        animator.SetTrigger("Attacked");
    }

    public void TestAnimationEvent() { }

    public void MeleeAttackComplete()
    {
        isAttacking = false;
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
        // Melee range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

        // Ranged range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);

        if (slashSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(slashSpawnPoint.position, 0.3f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxPathDistance);
    }
}