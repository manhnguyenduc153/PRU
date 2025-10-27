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
    public float detectionRange = 15f;
    public float minAttackRange = 3f;
    public float maxAttackRange = 12f;

    [Header("Range Attack Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 8f;
    public float attackCooldown = 2f;
    public float attackChance = 100f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip attackClip;
    [Range(0f, 1f)] public float moveVolume = 0.5f;
    [Range(0f, 1f)] public float attackVolume = 0.7f;
    [Range(0f, 1f)] public float fadeSmoothness = 0.2f; // độ mượt khi fade

    // Private variables
    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Animator animator;
    private Coroutine moveCoroutine;
    private float attackTimer;
    private Vector3 originalScale;
    private bool isMovingSoundPlaying = false;

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

        if (characterSR != null)
        {
            originalScale = characterSR.transform.localScale;
        }

        // Kiểm tra AudioSource
        if (moveAudioSource == null)
        {
            moveAudioSource = gameObject.AddComponent<AudioSource>();
            moveAudioSource.loop = true;
        }

        if (attackAudioSource == null)
        {
            attackAudioSource = gameObject.AddComponent<AudioSource>();
        }

        InvokeRepeating("CalculatePath", 0f, repeatTimeUpdatePath);
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        FacePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, GetPlayerPosition());

        // Điều khiển âm thanh di chuyển
        HandleMoveSound(distanceToPlayer);

        if (distanceToPlayer <= detectionRange &&
            distanceToPlayer >= minAttackRange &&
            distanceToPlayer <= maxAttackRange &&
            attackTimer <= 0)
        {
            TryAttack();
        }
    }

    void HandleMoveSound(float distanceToPlayer)
    {
        // Enemy chỉ phát tiếng bước khi roaming hoặc đang di chuyển
        bool shouldPlayMoveSound = roaming && distanceToPlayer > minAttackRange;

        if (shouldPlayMoveSound && !isMovingSoundPlaying && moveClip != null)
        {
            moveAudioSource.clip = moveClip;
            moveAudioSource.volume = 0;
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
        {
            source.Stop();
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
        float randomValue = Random.Range(0f, 100f);
        if (randomValue < attackChance)
        {
            UseNormalAttack();
            attackTimer = attackCooldown;
        }
    }

    void UseNormalAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Phát âm thanh tấn công
        if (attackClip != null)
        {
            attackAudioSource.PlayOneShot(attackClip, attackVolume);
        }

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
            return (Vector2)playerPos + (Random.Range(5f, 10f) * new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
        }
        else
        {
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minAttackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxAttackRange);
    }
}
