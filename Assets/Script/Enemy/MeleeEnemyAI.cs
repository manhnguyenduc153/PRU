using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MeleeEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float nextWaypointDistance = 2f;
    public float repeatTimeUpdatePath = 0.5f;
    public SpriteRenderer characterSR;
    public float attackRange = 2f;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    public GameObject slashPrefab;
    public Transform slashSpawnPoint;
    public float slashOffsetDistance = 1.5f;
    private float attackTimer;
    private bool isAttacking = false;

    [Header("Projectile Attack Settings")]
    public bool enableProjectileAttack = true;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileRange = 6f; // Khoảng cách tối thiểu để bắn
    public float projectileMaxRange = 12f; // Khoảng cách tối đa
    public float projectileCooldown = 4f;
    public float projectileSpeed = 8f;
    public int projectileDamage = 15;
    public float projectileKnockbackForce = 5f;
    private float projectileTimer;
    private bool isShooting = false;

    [Header("Jump Attack Settings")]
    public bool enableJumpAttack = true;
    public float jumpAttackRange = 8f;
    public float jumpAttackMaxRange = 15f;
    public float jumpAttackCooldown = 8f;
    public GameObject warningPrefab;
    public float warningDuration = 1f;
    public float jumpDuration = 0.5f;
    public float jumpHeight = 3f;
    public float jumpDamageRadius = 2f;
    public int jumpDamage = 30;
    public float jumpKnockbackForce = 12f;
    private float jumpAttackTimer;
    private bool isJumping = false;

    [Header("Ground Slam Effect")]
    public GameObject groundSlamPrefab;
    public float groundSlamDuration = 0.5f;

    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Animator animator;

    private Coroutine moveCoroutine;
    private Transform player;

    [Header("Freeze Settings")]
    public float freezeDurationTime;
    private float freezeDuration;

    public float knockbackForce = 8f;

    [Header("Pathfinding Fix")]
    public float maxPathDistance = 100f;
    public bool waitForGraphScan = true;
    private bool isInitialized = false;


    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        player = FindObjectOfType<PlayerController>()?.transform;
        freezeDuration = 0;
        attackTimer = 0f;
        jumpAttackTimer = 0f;
        projectileTimer = 0f;

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

        if (jumpAttackTimer > 0)
        {
            jumpAttackTimer -= Time.deltaTime;
        }

        if (projectileTimer > 0)
        {
            projectileTimer -= Time.deltaTime;
        }

        FacePlayer();
    }

    private void FacePlayer()
    {
        if (player == null || characterSR == null || isJumping) return;

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

            // Kiểm tra jump attack trước (ưu tiên cao nhất)
            if (enableJumpAttack && CanUseJumpAttack(distanceToPlayer))
            {
                rb.velocity = Vector2.zero;
                StartCoroutine(PerformJumpAttack());
                yield break;
            }

            // Kiểm tra projectile attack (ưu tiên thứ 2)
            if (enableProjectileAttack && CanUseProjectileAttack(distanceToPlayer))
            {
                rb.velocity = Vector2.zero;
                yield return StartCoroutine(PerformProjectileAttack());
                continue;
            }

            if (isAttacking || isJumping || isShooting)
            {
                rb.velocity = Vector2.zero;
                yield return null;
                continue;
            }

            // Tấn công cận chiến
            if (distanceToPlayer <= attackRange)
            {
                rb.velocity = Vector2.zero;
                TryAttack();

                while (isAttacking || (distanceToPlayer <= attackRange && attackTimer > 0))
                {
                    distanceToPlayer = Vector2.Distance(transform.position, player.position);
                    rb.velocity = Vector2.zero;
                    yield return null;
                }

                continue;
            }

            Vector2 targetPos = path.vectorPath[currentWP];
            Vector2 direction = (targetPos - rb.position).normalized;

            float speedMultiplier = distanceToPlayer < attackRange * 1.5f ? 0.7f : 1f;
            Vector2 movement = direction * moveSpeed * speedMultiplier * Time.deltaTime;

            transform.position += (Vector3)movement;

            float distance = Vector2.Distance(rb.position, targetPos);
            if (distance < nextWaypointDistance)
                currentWP++;

            yield return null;
        }
    }

    bool CanUseJumpAttack(float distanceToPlayer)
    {
        return jumpAttackTimer <= 0
            && !isAttacking
            && !isJumping
            && !isShooting
            && distanceToPlayer >= jumpAttackRange
            && distanceToPlayer <= jumpAttackMaxRange
            && warningPrefab != null;
    }

    bool CanUseProjectileAttack(float distanceToPlayer)
    {
        return projectileTimer <= 0
            && !isAttacking
            && !isJumping
            && !isShooting
            && distanceToPlayer >= projectileRange
            && distanceToPlayer <= projectileMaxRange
            && projectilePrefab != null;
    }

    IEnumerator PerformProjectileAttack()
    {
        isShooting = true;
        projectileTimer = projectileCooldown;

        // Trigger animation bắn (nếu có)
        if (animator != null)
        {
            animator.SetTrigger("Punch");
        }

        // Đợi animation chuẩn bị
        yield return new WaitForSeconds(0.3f);

        // Bắn đạn
        ShootProjectile();

        // Đợi animation kết thúc
        yield return new WaitForSeconds(0.3f);

        isShooting = false;
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        // Tính hướng bắn
        Vector2 direction = (player.position - transform.position).normalized;

        // Vị trí spawn
        Vector3 spawnPos;
        if (projectileSpawnPoint != null)
        {
            spawnPos = projectileSpawnPoint.position;
        }
        else
        {
            spawnPos = transform.position + (Vector3)direction * 1f;
        }

        // Tạo projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Set velocity cho Rigidbody2D
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.velocity = direction * projectileSpeed;
        }

        // Script EnemyBullet sẽ tự động xử lý:
        // - Xoay theo hướng bay (autoRotate)
        // - Damage và knockback khi chạm Player
        // - Tự hủy sau lifeTime hoặc khi chạm tường
    }

    IEnumerator PerformJumpAttack()
    {
        isJumping = true;
        jumpAttackTimer = jumpAttackCooldown;

        Vector3 targetPosition = player.position;

        GameObject warning = null;
        if (warningPrefab != null)
        {
            warning = Instantiate(warningPrefab, targetPosition, Quaternion.identity);
        }

        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(warningDuration);

        if (warning != null)
        {
            Destroy(warning);
        }

        yield return StartCoroutine(JumpToPosition(targetPosition));

        PerformGroundSlam(targetPosition);

        yield return new WaitForSeconds(0.3f);

        isJumping = false;

        CalculatePath();
    }

    IEnumerator JumpToPosition(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        targetPos.y += 1.7f;
        float elapsed = 0f;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            float height = jumpHeight * Mathf.Sin(t * Mathf.PI);
            currentPos.y += height;

            transform.position = currentPos;

            yield return null;
        }

        transform.position = targetPos;

        if (col != null) col.enabled = true;
    }

    void PerformGroundSlam(Vector3 position)
    {
        if (groundSlamPrefab != null)
        {
            GameObject slam = Instantiate(groundSlamPrefab, position, Quaternion.identity);
            Destroy(slam, groundSlamDuration);
        }
        else
        {
            StartCoroutine(CreateSimpleGroundSlamEffect(position));
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, jumpDamageRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(jumpDamage);
                }

                PlayerKnockback playerKnockback = hit.GetComponent<PlayerKnockback>();
                if (playerKnockback != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - position).normalized;
                    playerKnockback.ApplyKnockback(knockbackDir, jumpKnockbackForce);
                }
            }
        }
    }

    IEnumerator CreateSimpleGroundSlamEffect(Vector3 position)
    {
        int ringCount = 3;
        for (int i = 0; i < ringCount; i++)
        {
            GameObject ring = new GameObject("SlamRing");
            ring.transform.position = position;

            LineRenderer lr = ring.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(1, 0.5f, 0, 0.8f);
            lr.endColor = new Color(1, 0.5f, 0, 0.8f);
            lr.startWidth = 0.2f;
            lr.endWidth = 0.2f;
            lr.positionCount = 50;
            lr.useWorldSpace = false;

            float angle = 0f;
            for (int j = 0; j < 50; j++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle);
                float y = Mathf.Cos(Mathf.Deg2Rad * angle);
                lr.SetPosition(j, new Vector3(x, y, 0) * 0.5f);
                angle += 360f / 50f;
            }

            StartCoroutine(ExpandAndFadeRing(ring, i * 0.1f));
        }

        yield return null;
    }

    IEnumerator ExpandAndFadeRing(GameObject ring, float delay)
    {
        yield return new WaitForSeconds(delay);

        LineRenderer lr = ring.GetComponent<LineRenderer>();
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * jumpDamageRadius;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            ring.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            Color col = lr.startColor;
            col.a = 1 - t;
            lr.startColor = col;
            lr.endColor = col;

            yield return null;
        }

        Destroy(ring);
    }

    void TryAttack()
    {
        if (attackTimer > 0 || isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("Attack");

        attackTimer = attackCooldown;
        StartCoroutine(ResetAttackState());
    }

    IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(0.3f);
        SpawnSlashEffect();
        yield return new WaitForSeconds(0.3f);
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

    public void ShootAnimationEvent()
    {
        ShootProjectile();
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (slashSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(slashSpawnPoint.position, 0.3f);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxPathDistance);

        if (enableJumpAttack)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, jumpAttackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, jumpAttackMaxRange);
        }

        if (enableProjectileAttack)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, projectileRange);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, projectileMaxRange);
        }

        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.3f);
        }
    }
}