using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAIMinotaur : MonoBehaviour
{
    [Header("Sprite Facing Settings")]
    [SerializeField] private bool defaultFacingRight = true;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float nextWaypointDistance = 2f;
    public float repeatTimeUpdatePath = 0.5f;
    public SpriteRenderer characterSR;
    public float attackRange = 2f;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float attackTimer;
    private bool isAttacking = false;
    public int normalAttackDamage = 15;

    [Header("Force Attack Settings (Skill 2)")]
    public bool enableForceAttack = true;
    public float forceDashDistance = 4f;
    public float forceDashTime = 0.2f;
    public float forceDashCooldown = 3f;
    private float forceDashTimer = 0f;

    [Header("Jump Attack / Ground Slam (Skill 3)")]
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
    public GameObject groundSlamPrefab;
    public float groundSlamDuration = 0.5f;
    private float jumpAttackTimer;
    private bool isJumping = false;

    [Header("Freeze Settings")]
    public float freezeDurationTime;
    private float freezeDuration;
    public float knockbackForce = 8f;

    [Header("Pathfinding Fix")]
    public float maxPathDistance = 100f;
    public bool waitForGraphScan = true;
    private bool isInitialized = false;

    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Animator animator;
    private Coroutine moveCoroutine;
    private Transform player;
    private Vector3 originalScale;

    // ======================================
    // 🎵 SOUND SETTINGS - IMPROVED
    // ======================================
    [Header("Sound Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip groundSlamSound;

    [Range(0f, 1f)] public float moveVolume = 0.7f;
    [Range(0f, 1f)] public float attackVolume = 1f;
    [Range(0f, 1f)] public float groundSlamVolume = 1f;

    [Tooltip("Độ mượt khi loop âm thanh di chuyển")]
    [Range(0f, 1f)] public float moveFadeSmoothness = 0.2f;

    private bool isPlayingMoveSound = false;
    private Coroutine moveSoundCoroutine;

    // ======================================

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = FindObjectOfType<PlayerController>()?.transform;

        SetupAudioSource();

        freezeDuration = 0;
        attackTimer = 0f;
        jumpAttackTimer = 0f;
        forceDashTimer = 0f;

        if (characterSR != null)
        {
            originalScale = characterSR.transform.localScale;

            // ✅ FIX: Khởi tạo hướng đúng ngay từ đầu
            characterSR.flipX = !defaultFacingRight;
        }

        StartCoroutine(InitializePathfinding());
    }

    // 🎵 Tự động tạo AudioSource nếu thiếu
    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("Auto-created AudioSource for " + gameObject.name);
            }
        }

        // Cấu hình AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.loop = false;

        // Kiểm tra AudioListener
        if (FindObjectOfType<AudioListener>() == null)
        {
            Debug.LogWarning("⚠️ No AudioListener found in scene! Add one to Main Camera.");
        }

        // Debug log để kiểm tra
        if (moveSound == null) Debug.LogWarning("⚠️ Move Sound not assigned!");
        if (attackSound == null) Debug.LogWarning("⚠️ Attack Sound not assigned!");
        if (groundSlamSound == null) Debug.LogWarning("⚠️ Ground Slam Sound not assigned!");
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
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (jumpAttackTimer > 0)
            jumpAttackTimer -= Time.deltaTime;

        if (forceDashTimer > 0)
            forceDashTimer -= Time.deltaTime;

        FacePlayer();
    }

    void FacePlayer()
    {
        if (characterSR == null || player == null) return; // ✅ FIX: Thêm check player null

        float dirX = player.position.x - transform.position.x;

        if (Mathf.Abs(dirX) > 0.05f)
        {
            if (defaultFacingRight)
                characterSR.flipX = dirX < 0;
            else
                characterSR.flipX = dirX > 0;
        }
    }

    Vector3 GetPlayerPosition()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            return player.transform.position;
        return transform.position;
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
        PlayMoveSound();

        while (path != null && currentWP < path.vectorPath.Count)
        {
            while (freezeDuration > 0)
            {
                freezeDuration -= Time.deltaTime;
                rb.velocity = Vector2.zero;
                yield return null;
            }

            if (player == null)
            {
                StopMoveSound();
                yield break;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (enableJumpAttack && CanUseJumpAttack(distanceToPlayer))
            {
                StopMoveSound();
                rb.velocity = Vector2.zero;
                StartCoroutine(PerformJumpAttack());
                yield break;
            }

            if (enableForceAttack && forceDashTimer <= 0f && distanceToPlayer > attackRange && distanceToPlayer <= jumpAttackRange)
            {
                StopMoveSound();
                rb.velocity = Vector2.zero;
                yield return StartCoroutine(PerformForceAttack());
                continue;
            }

            if (distanceToPlayer <= attackRange)
            {
                StopMoveSound();
                rb.velocity = Vector2.zero;
                TryAttack();

                while (isAttacking || (distanceToPlayer <= attackRange && attackTimer > 0))
                {
                    distanceToPlayer = Vector2.Distance(transform.position, player.position);
                    rb.velocity = Vector2.zero;
                    yield return null;
                }

                PlayMoveSound();
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

        StopMoveSound();
    }

    bool CanUseJumpAttack(float distanceToPlayer)
    {
        return jumpAttackTimer <= 0 && !isAttacking && !isJumping &&
               distanceToPlayer >= jumpAttackRange && distanceToPlayer <= jumpAttackMaxRange &&
               warningPrefab != null;
    }

    void TryAttack()
    {
        if (attackTimer > 0 || isAttacking) return;

        isAttacking = true;
        attackTimer = attackCooldown;
        animator.SetTrigger("Attack");

        // 🎵 Phát 2 âm Attack liên tiếp
        StartCoroutine(PlayAttackSoundTwice());

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(normalAttackDamage);

                PlayerKnockback playerKnockback = hit.GetComponent<PlayerKnockback>();
                if (playerKnockback != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    playerKnockback.ApplyKnockback(knockbackDir, knockbackForce);
                }
            }
        }

        StartCoroutine(ResetAttackState());
    }

    IEnumerator PlayAttackSoundTwice()
    {
        if (attackSound == null || audioSource == null)
        {
            Debug.LogWarning("⚠️ Cannot play attack sound - AudioSource or AudioClip missing!");
            yield break;
        }

        // Debug để kiểm tra
        Debug.Log("🎵 Playing attack sound x2");

        audioSource.PlayOneShot(attackSound, attackVolume);
        yield return new WaitForSeconds(0.1f);
        audioSource.PlayOneShot(attackSound, attackVolume);
    }

    IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    IEnumerator PerformForceAttack()
    {
        isAttacking = true;
        forceDashTimer = forceDashCooldown;
        FacePlayer();

        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (Vector3)(direction * forceDashDistance);

        float elapsed = 0f;
        while (elapsed < forceDashTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / forceDashTime;

            FacePlayer();
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        TryAttack();
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
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

        if (warning != null)
            Destroy(warning);

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

        // 🎵 Âm thanh Ground Slam
        PlaySoundEffect(groundSlamSound, groundSlamVolume, "Ground Slam");

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, jumpDamageRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(jumpDamage);

                PlayerKnockback playerKnockback = hit.GetComponent<PlayerKnockback>();
                if (playerKnockback != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - position).normalized;
                    playerKnockback.ApplyKnockback(knockbackDir, jumpKnockbackForce);
                }
            }
        }
    }

    // ======================================
    // 🎵 SOUND HELPERS - IMPROVED
    // ======================================

    void PlayMoveSound()
    {
        if (audioSource == null || moveSound == null)
        {
            Debug.LogWarning("⚠️ Cannot play move sound - AudioSource or AudioClip missing!");
            return;
        }

        if (isPlayingMoveSound) return;

        if (moveSoundCoroutine != null)
            StopCoroutine(moveSoundCoroutine);

        moveSoundCoroutine = StartCoroutine(MoveSoundLoop());
    }

    IEnumerator MoveSoundLoop()
    {
        isPlayingMoveSound = true;
        Debug.Log("🎵 Started move sound loop");

        while (isPlayingMoveSound)
        {
            audioSource.PlayOneShot(moveSound, moveVolume);
            yield return new WaitForSeconds(moveSound.length * (1f - moveFadeSmoothness));
        }

        Debug.Log("🎵 Stopped move sound loop");
    }

    void StopMoveSound()
    {
        isPlayingMoveSound = false;

        if (moveSoundCoroutine != null)
        {
            StopCoroutine(moveSoundCoroutine);
            moveSoundCoroutine = null;
        }
    }

    // 🎵 Helper method để play sound với error handling
    void PlaySoundEffect(AudioClip clip, float volume, string soundName)
    {
        if (audioSource == null)
        {
            Debug.LogWarning($"⚠️ Cannot play {soundName} - AudioSource missing!");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning($"⚠️ Cannot play {soundName} - AudioClip not assigned!");
            return;
        }

        Debug.Log($"🎵 Playing {soundName}");
        audioSource.PlayOneShot(clip, volume);
    }

    // ======================================

    public void FreezeEnemy()
    {
        freezeDuration = freezeDurationTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxPathDistance);

        if (enableJumpAttack)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, jumpAttackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, jumpAttackMaxRange);
        }
    }

    private void OnDestroy()
    {
        // Clean up coroutines
        StopMoveSound();
    }
}