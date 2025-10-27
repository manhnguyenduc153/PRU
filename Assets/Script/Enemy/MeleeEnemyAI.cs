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
    public float projectileRange = 6f;
    public float projectileMaxRange = 12f;
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

    // ------------------ 🎧 AUDIO SETTINGS ------------------
    [Header("Audio Settings")]
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip normalAttackClip;
    [SerializeField] private AudioClip groundSlamClip;
    [Range(0f, 1f)] public float moveVolume = 0.4f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    [Range(0.05f, 1f)] public float fadeSmoothness = 0.2f;
    private bool isMovingSoundPlaying = false;
    // -------------------------------------------------------

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

        // 🔊 Tạo AudioSource nếu chưa có
        if (moveAudioSource == null)
        {
            moveAudioSource = gameObject.AddComponent<AudioSource>();
            moveAudioSource.loop = true;
            moveAudioSource.playOnAwake = false;
        }
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.loop = false;
            sfxAudioSource.playOnAwake = false;
        }

        StartCoroutine(InitializePathfinding());
    }

    private IEnumerator InitializePathfinding()
    {
        if (waitForGraphScan && AstarPath.active != null)
        {
            while (AstarPath.active.isScanning)
                yield return null;

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
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (jumpAttackTimer > 0) jumpAttackTimer -= Time.deltaTime;
        if (projectileTimer > 0) projectileTimer -= Time.deltaTime;

        FacePlayer();

        HandleMoveSound(); // 🔊 xử lý tiếng bước chân
    }

    // 🔊 Chạy loop tiếng bước chân khi enemy di chuyển
    void HandleMoveSound()
    {
        bool shouldPlayMoveSound = !isAttacking && !isJumping && !isShooting && rb.velocity.magnitude > 0.1f;

        if (shouldPlayMoveSound && !isMovingSoundPlaying && moveClip != null)
        {
            moveAudioSource.clip = moveClip;
            moveAudioSource.volume = 0f;
            moveAudioSource.loop = true;
            moveAudioSource.Play();
            StartCoroutine(FadeAudio(moveAudioSource, moveVolume, fadeSmoothness));
            isMovingSoundPlaying = true;
        }
        else if (!shouldPlayMoveSound && isMovingSoundPlaying)
        {
            StartCoroutine(FadeAudio(moveAudioSource, 0f, fadeSmoothness, stopAfterFade: true));
            isMovingSoundPlaying = false;
        }
    }

    IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration, bool stopAfterFade = false)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;

        if (stopAfterFade && targetVolume <= 0f)
            source.Stop();
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
            seeker.StartPath(rb.position, player.position, OnPathCompleted);
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

            // Jump attack
            if (enableJumpAttack && CanUseJumpAttack(distanceToPlayer))
            {
                rb.velocity = Vector2.zero;
                StartCoroutine(PerformJumpAttack());
                yield break;
            }

            // Projectile attack
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

            // Normal attack
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
            && !isAttacking && !isJumping && !isShooting
            && distanceToPlayer >= jumpAttackRange && distanceToPlayer <= jumpAttackMaxRange
            && warningPrefab != null;
    }

    bool CanUseProjectileAttack(float distanceToPlayer)
    {
        return projectileTimer <= 0
            && !isAttacking && !isJumping && !isShooting
            && distanceToPlayer >= projectileRange && distanceToPlayer <= projectileMaxRange
            && projectilePrefab != null;
    }

    IEnumerator PerformProjectileAttack()
    {
        isShooting = true;
        projectileTimer = projectileCooldown;

        if (animator != null)
            animator.SetTrigger("Punch");

        yield return new WaitForSeconds(0.3f);
        ShootProjectile();
        yield return new WaitForSeconds(0.3f);

        isShooting = false;
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null || player == null) return;
        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 spawnPos = projectileSpawnPoint ? projectileSpawnPoint.position : transform.position + (Vector3)direction * 1f;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null) projRb.velocity = direction * projectileSpeed;
    }

    IEnumerator PerformJumpAttack()
    {
        isJumping = true;
        jumpAttackTimer = jumpAttackCooldown;
        Vector3 targetPosition = player.position;

        GameObject warning = null;
        if (warningPrefab != null)
            warning = Instantiate(warningPrefab, targetPosition, Quaternion.identity);

        animator.SetTrigger("Jump");
        yield return new WaitForSeconds(warningDuration);
        if (warning != null) Destroy(warning);
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
        // 🔊 Âm thanh ground slam
        if (groundSlamClip != null)
            sfxAudioSource.PlayOneShot(groundSlamClip, sfxVolume);

        if (groundSlamPrefab != null)
        {
            GameObject slam = Instantiate(groundSlamPrefab, position, Quaternion.identity);
            Destroy(slam, groundSlamDuration);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, jumpDamageRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(jumpDamage);
                PlayerKnockback pk = hit.GetComponent<PlayerKnockback>();
                if (pk != null)
                {
                    Vector2 dir = (hit.transform.position - position).normalized;
                    pk.ApplyKnockback(dir, jumpKnockbackForce);
                }
            }
        }
    }

    void TryAttack()
    {
        if (attackTimer > 0 || isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("Attack");

        // 🔊 Phát âm thanh tấn công cận chiến
        if (normalAttackClip != null)
            sfxAudioSource.PlayOneShot(normalAttackClip, sfxVolume);

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
        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 spawnPos = slashSpawnPoint ? slashSpawnPoint.position : transform.position + (Vector3)direction * slashOffsetDistance;
        GameObject slash = Instantiate(slashPrefab, spawnPos, Quaternion.identity);
        EnemySlashEffect slashEffect = slash.GetComponent<EnemySlashEffect>();
        if (slashEffect != null)
            slashEffect.Initialize(direction, characterSR.transform.localScale.x < 0, knockbackForce);
    }

    public void FreezeEnemy() => freezeDuration = freezeDurationTime;
    public void PlayHurtAnimation() => animator.SetTrigger("Attacked");
    public void MeleeAttackComplete() => isAttacking = false;
    public void ShootAnimationEvent() => ShootProjectile();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        if (slashSpawnPoint != null) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(slashSpawnPoint.position, 0.3f); }
        if (enableJumpAttack) { Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, jumpAttackRange); Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, jumpAttackMaxRange); }
        if (enableProjectileAttack) { Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, projectileRange); Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, projectileMaxRange); }
    }
}
