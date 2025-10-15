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

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        player = FindObjectOfType<PlayerController>()?.transform;
        freezeDuration = 0;
        attackTimer = 0f;

        InvokeRepeating(nameof(CalculatePath), 0f, repeatTimeUpdatePath);
    }

    private void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    private void CalculatePath()
    {
        if (player == null) return;
        if (seeker.IsDone())
            seeker.StartPath(rb.position, player.position, OnPathCompleted);
    }

    private void OnPathCompleted(Path p)
    {
        if (!p.error)
        {
            path = p;
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveToPlayerCoroutine());
        }
    }

    IEnumerator MoveToPlayerCoroutine()
    {
        int currentWP = 0;

        while (path != null && currentWP < path.vectorPath.Count)
        {
            // Nếu bị đóng băng, tạm dừng
            while (freezeDuration > 0)
            {
                freezeDuration -= Time.deltaTime;
                rb.velocity = Vector2.zero;
                yield return null;
            }

            if (player == null)
                yield break;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Nếu đang attack, đợi xong mới tiếp tục
            if (isAttacking)
            {
                rb.velocity = Vector2.zero;
                yield return null;
                continue;
            }

            // KIỂM TRA ATTACK RANGE - NẾU Ở TRONG PHẠM VI THÌ CHỈ TẤN CÔNG, KHÔNG DI CHUYỂN
            if (distanceToPlayer <= attackRange)
            {
                rb.velocity = Vector2.zero;
                TryAttack();

                // Đợi cooldown hoặc đang attack
                while (isAttacking || (distanceToPlayer <= attackRange && attackTimer > 0))
                {
                    distanceToPlayer = Vector2.Distance(transform.position, player.position);
                    rb.velocity = Vector2.zero;
                    yield return null;
                }

                // Sau khi attack xong, tiếp tục kiểm tra vị trí
                continue;
            }

            // Di chuyển theo đường đi của path
            Vector2 targetPos = path.vectorPath[currentWP];
            Vector2 direction = (targetPos - rb.position).normalized;

            // Giảm tốc độ khi gần player
            float speedMultiplier = distanceToPlayer < attackRange * 1.5f ? 0.7f : 1f;
            Vector2 movement = direction * moveSpeed * speedMultiplier * Time.deltaTime;

            transform.position += (Vector3)movement;

            float distance = Vector2.Distance(rb.position, targetPos);
            if (distance < nextWaypointDistance)
                currentWP++;

            // Flip hướng
            if (direction.x != 0)
            {
                characterSR.transform.localScale = new Vector3(
                    direction.x < 0 ? -1 : 1,
                    1,
                    1
                );
            }

            yield return null;
        }
    }

    void TryAttack()
    {
        if (attackTimer > 0 || isAttacking) return;

        Debug.Log("Enemy trying to attack!");

        isAttacking = true;
        animator.SetTrigger("Attack");

        Debug.Log("Attack trigger set!");

        attackTimer = attackCooldown;
        StartCoroutine(ResetAttackState());
    }

    IEnumerator ResetAttackState()
    {
        // Đợi animation bắt đầu
        yield return new WaitForSeconds(0.3f);
        MeleeAttackHit(); // Gọi damage

        // Đợi animation attack chạy xong
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
        Debug.Log("Attack state reset");
    }

    public void FreezeEnemy()
    {
        freezeDuration = freezeDurationTime;
    }

    public void PlayHurtAnimation()
    {
        animator.SetTrigger("Attacked");
    }

    public void TestAnimationEvent()
    {
        Debug.LogWarning("=== ANIMATION EVENT WORKING! ===");
    }

    public void MeleeAttackHit()
    {
        Debug.LogWarning("=== MeleeAttackHit CALLED! ===");

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Debug.Log($"Distance to player: {distance}, Attack range: {attackRange}");

        if (distance <= attackRange)
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
                Debug.Log("Damage dealt to player!");
            }
            else
            {
                Debug.LogError("PlayerHealth component not found!");
            }
        }
        else
        {
            Debug.Log("Player too far to deal damage!");
        }
    }

    public void MeleeAttackComplete()
    {
        isAttacking = false;
        Debug.Log("MeleeAttackComplete called from animation event");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}