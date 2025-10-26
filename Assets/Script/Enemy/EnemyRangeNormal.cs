using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyRangeNormal : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool roaming = true;
    public float moveSpeed = 2f;
    public float nextWayPointDistance = 2f;
    public float repeatTimeUpdatePath = 0.5f;
    public SpriteRenderer characterSR;

    [Header("Attack Range")]
    public float detectionRange = 15f; // Phạm vi phát hiện player
    public float minAttackRange = 3f; // Khoảng cách tối thiểu để tấn công
    public float maxAttackRange = 12f; // Khoảng cách tối đa để tấn công

    [Header("Range Attack Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 8f;
    public float attackCooldown = 2f;
    public float attackChance = 100f; // Tỉ lệ tấn công khi trong tầm (100% = luôn tấn công)

    // Private variables
    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Animator animator;
    private Coroutine moveCoroutine;
    private float attackTimer;
    private Vector3 originalScale;

    // Freeze
    public float freezeDurationTime;
    private float freezeDuration;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        freezeDuration = 0;
        attackTimer = attackCooldown;

        // Lưu scale ban đầu
        if (characterSR != null)
        {
            originalScale = characterSR.transform.localScale;
        }

        InvokeRepeating("CalculatePath", 0f, repeatTimeUpdatePath);
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        // Flip theo hướng player
        FacePlayer();

        // Kiểm tra khoảng cách để tấn công
        float distanceToPlayer = Vector2.Distance(transform.position, GetPlayerPosition());

        // Chỉ tấn công khi player trong vùng tấn công hợp lệ
        if (distanceToPlayer <= detectionRange &&
            distanceToPlayer >= minAttackRange &&
            distanceToPlayer <= maxAttackRange &&
            attackTimer <= 0)
        {
            TryAttack();
        }
    }

    void FacePlayer()
    {
        if (characterSR == null) return;

        Vector3 playerPos = GetPlayerPosition();
        float dirX = playerPos.x - transform.position.x;

        if (Mathf.Abs(dirX) > 0.05f)
        {
            float newScaleX = dirX < 0 ? -Mathf.Abs(originalScale.x) : Mathf.Abs(originalScale.x);
            characterSR.transform.localScale = new Vector3(newScaleX, originalScale.y, originalScale.z);
        }
    }

    void TryAttack()
    {
        // Random để quyết định có tấn công không
        float randomValue = Random.Range(0f, 100f);
        if (randomValue < attackChance)
        {
            UseNormalAttack();
            attackTimer = attackCooldown;
        }
    }

    // Tấn công bắn đạn - giống Normal Attack từ script gốc
    void UseNormalAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Gọi coroutine để delay việc bắn đạn 0.5s
        StartCoroutine(DelayedShoot(0.5f));
    }

    IEnumerator DelayedShoot(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bulletPrefab == null) yield break;

        var bulletTmp = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bulletTmp.GetComponent<Rigidbody2D>();

        Vector3 playerPos = GetPlayerPosition();
        Vector3 direction = (playerPos - transform.position).normalized;

        bulletRb.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
    }


    Vector3 GetPlayerPosition()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            return player.transform.position;
        return transform.position;
    }

    void CalculatePath()
    {
        Vector2 target = FindTarget();
        if (seeker.IsDone())
            seeker.StartPath(rb.position, target, OnPathCompleted);
    }

    Vector2 FindTarget()
    {
        Vector3 playerPos = GetPlayerPosition();
        if (roaming)
        {
            // Roaming: di chuyển xung quanh player nhưng giữ khoảng cách
            return (Vector2)playerPos + (Random.Range(5f, 10f) * new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
        }
        else
        {
            // Đuổi theo player
            return playerPos;
        }
    }

    void OnPathCompleted(Path p)
    {
        if (!p.error)
        {
            path = p;
            MoveToTarget();
        }
    }

    void MoveToTarget()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToTargetCoroutine());
    }

    public void FreezeEnemy()
    {
        freezeDuration = freezeDurationTime;
    }

    IEnumerator MoveToTargetCoroutine()
    {
        int currentWP = 0;
        while (currentWP < path.vectorPath.Count)
        {
            // Xử lý freeze
            while (freezeDuration > 0)
            {
                freezeDuration -= Time.deltaTime;
                yield return null;
            }

            Vector2 direction = ((Vector2)path.vectorPath[currentWP] - rb.position).normalized;
            Vector2 force = direction * moveSpeed * Time.deltaTime;
            transform.position += (Vector3)force;

            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWP]);
            if (distance < nextWayPointDistance)
                currentWP++;

            yield return null;
        }
    }

    // Gizmos để debug
    private void OnDrawGizmosSelected()
    {
        // Vẽ detection range (màu xanh dương)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ min attack range (màu vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minAttackRange);

        // Vẽ max attack range (màu đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxAttackRange);
    }
}