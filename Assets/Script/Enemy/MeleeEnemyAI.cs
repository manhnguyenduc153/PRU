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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (slashSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(slashSpawnPoint.position, 0.3f);
        }
    }
}