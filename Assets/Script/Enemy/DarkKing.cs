using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class DarkKing : MonoBehaviour
{
    public bool roaming = true;
    public float moveSpeed = 2f;
    public float nextWayPointDistance = 2f;
    public float repeatTimeUpdatePath = 0.5f;
    public SpriteRenderer characterSR;

    Path path;
    Seeker seeker;
    Rigidbody2D rb;
    Animator animator;
    Coroutine moveCoroutine;

    [Header("Melee Attack")]
    public bool hasMeleeAttack = true;
    public float meleeAttackRange = 2f;
    public float meleeAttackCooldown = 1.2f;
    public int meleeDamage = 20;
    public float knockbackForce = 8f;
    public float meleeAttackChance = 100f;
    private float meleeAttackTimer;
    private bool isAttacking = false;

    [Header("Detection Range")]
    public float detectionRange = 15f;
    public float closeRangeThreshold = 7f; // khoảng cách để ưu tiên cận chiến

    [Header("Wave Bullet Attack")]
    public GameObject waveBullet;
    public float waveBulletSpeed = 6f;
    public float waveAttackCooldown = 5f;
    public float waveAttackChance = 70f;
    public int wavesCount = 3;
    public float delayBetweenWaves = 0.3f;

    [Header("Ultimate - Lightning Strike")]
    public GameObject lightningBolt;
    public GameObject warningIndicator;
    public float ultimateCooldown = 10f;
    public float ultimateChance = 40f;
    public int lightningCount = 8;
    public float lightningSpawnRadius = 5f;
    public float warningDuration = 0.5f;
    public float delayBetweenLightning = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioClip meleeAttackSFX;
    [SerializeField] private AudioClip moveSFX;
    [SerializeField, Range(0f, 1f)] private float moveVolume = 0.3f;
    [SerializeField, Range(0f, 1f)] private float meleeVolume = 1f;
    [SerializeField, Range(0f, 2f)] private float fadeSpeed = 1f;

    private float waveAttackTimer;
    private float ultimateTimer;
    private bool isMovingSound = false;
    private float moveTargetVolume = 0f;

    public float freezeDurationTime;
    float freezeDuration;
    private Vector3 originalScale;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        freezeDuration = 0;
        if (characterSR != null)
            originalScale = characterSR.transform.localScale;

        waveAttackTimer = waveAttackCooldown;
        ultimateTimer = ultimateCooldown;
        meleeAttackTimer = 0f;

        InvokeRepeating("CalculatePath", 0f, repeatTimeUpdatePath);

        if (moveAudioSource == null)
            moveAudioSource = gameObject.AddComponent<AudioSource>();
        if (attackAudioSource == null)
            attackAudioSource = gameObject.AddComponent<AudioSource>();

        if (moveSFX != null)
        {
            moveAudioSource.clip = moveSFX;
            moveAudioSource.loop = true;
            moveAudioSource.volume = 0f;
            moveAudioSource.playOnAwake = false;
        }

        attackAudioSource.loop = false;
        attackAudioSource.playOnAwake = false;
    }

    private void Update()
    {
        waveAttackTimer -= Time.deltaTime;
        ultimateTimer -= Time.deltaTime;
        meleeAttackTimer -= Time.deltaTime;

        FacePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, GetPlayerPosition());

        // Quản lý âm thanh di chuyển
        bool shouldMoveSound = !isAttacking && distanceToPlayer <= detectionRange;
        moveTargetVolume = shouldMoveSound ? moveVolume : 0f;

        if (moveAudioSource != null && moveSFX != null)
        {
            if (!moveAudioSource.isPlaying)
                moveAudioSource.Play();

            moveAudioSource.volume = Mathf.MoveTowards(
                moveAudioSource.volume,
                moveTargetVolume,
                fadeSpeed * Time.deltaTime
            );
        }

        if (distanceToPlayer > detectionRange)
            return;

        // --- Ưu tiên chọn hành động theo khoảng cách ---
        if (distanceToPlayer <= closeRangeThreshold && hasMeleeAttack && !isAttacking)
        {
            // Ưu tiên melee khi gần
            TryUseMeleeAttack();
        }
        else if (!isAttacking)
        {
            // Ưu tiên chiêu tầm xa khi xa
            TryUseSkill(distanceToPlayer);
        }
    }

    void FacePlayer()
    {
        if (characterSR == null) return;

        Vector3 playerPos = GetPlayerPosition();
        float dirX = playerPos.x - transform.position.x;

        if (Mathf.Abs(dirX) > 0.05f)
        {
            float newScaleX = dirX < 0 ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
            characterSR.transform.localScale = new Vector3(newScaleX, originalScale.y, originalScale.z);
        }
    }

    void TryUseMeleeAttack()
    {
        if (meleeAttackTimer > 0 || isAttacking) return;
        UseMeleeAttack();
        meleeAttackTimer = meleeAttackCooldown;
    }

    void UseMeleeAttack()
    {
        isAttacking = true;
        if (animator != null)
            animator.SetTrigger("Attack");

        if (attackAudioSource != null && meleeAttackSFX != null)
            attackAudioSource.PlayOneShot(meleeAttackSFX, meleeVolume);

        StartCoroutine(MeleeAttackCoroutine());
    }

    IEnumerator MeleeAttackCoroutine()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        yield return new WaitForSeconds(0.3f);
        DealMeleeDamageToPlayer();
        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
        if (path != null)
            MoveToTarget();
    }

    void DealMeleeDamageToPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= meleeAttackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(meleeDamage);

            Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    void TryUseSkill(float distanceToPlayer)
    {
        // Ưu tiên Ultimate khi đủ điều kiện
        if (ultimateTimer <= 0)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue < ultimateChance)
            {
                UseUltimateSkill();
                ultimateTimer = ultimateCooldown;
                return;
            }
        }

        // Nếu player ở xa, tăng xác suất dùng Wave Attack
        if (waveAttackTimer <= 0)
        {
            float adjustedChance = waveAttackChance;
            if (distanceToPlayer > closeRangeThreshold)
                adjustedChance += 20f; // tăng thêm 20% khi player ở xa

            float randomValue = Random.Range(0f, 100f);
            if (randomValue < adjustedChance)
            {
                UseWaveSkill();
                waveAttackTimer = waveAttackCooldown;
                return;
            }
        }
    }

    void UseWaveSkill()
    {
        if (waveBullet == null) return;

        if (animator != null)
            animator.SetTrigger("Bullet");

        StartCoroutine(WaveAttackCoroutine());
    }

    IEnumerator WaveAttackCoroutine()
    {
        for (int wave = 0; wave < wavesCount; wave++)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );

                var bulletTmp = Instantiate(waveBullet, transform.position, Quaternion.identity);
                Rigidbody2D bulletRb = bulletTmp.GetComponent<Rigidbody2D>();
                bulletRb.AddForce(direction * waveBulletSpeed, ForceMode2D.Impulse);
            }

            if (wave < wavesCount - 1)
                yield return new WaitForSeconds(delayBetweenWaves);
        }
    }

    void UseUltimateSkill()
    {
        if (lightningBolt == null) return;

        if (animator != null)
            animator.SetTrigger("Cast");

        StartCoroutine(LightningStrikeCoroutine());
    }

    [SerializeField] private float spawnHeightOffset = 2f;

    IEnumerator LightningStrikeCoroutine()
    {
        Vector3 playerPos = GetPlayerPosition();
        List<Vector3> strikePositions = new List<Vector3>();
        List<GameObject> warnings = new List<GameObject>();

        for (int i = 0; i < lightningCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * lightningSpawnRadius;
            Vector3 strikePos = new Vector3(
                playerPos.x + randomOffset.x,
                playerPos.y + randomOffset.y,
                playerPos.z
            );
            strikePositions.Add(strikePos);

            if (warningIndicator != null)
            {
                GameObject warning = Instantiate(warningIndicator, strikePos, Quaternion.identity);
                warnings.Add(warning);
            }

            yield return new WaitForSeconds(delayBetweenLightning);
        }

        yield return new WaitForSeconds(warningDuration);

        for (int i = 0; i < strikePositions.Count; i++)
        {
            if (i < warnings.Count && warnings[i] != null)
                Destroy(warnings[i]);

            Vector3 lightningPos = strikePositions[i];
            lightningPos.y += spawnHeightOffset;
            Instantiate(lightningBolt, lightningPos, Quaternion.identity);

            yield return new WaitForSeconds(0.05f);
        }
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
            while (isAttacking)
                yield return null;

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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, closeRangeThreshold);
    }
}
