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

    [SerializeField]
    public bool canUseNormalAttack = true;

    Path path;
    Seeker seeker;
    Rigidbody2D rb;
    Animator animator;
    Coroutine moveCoroutine;

    [Header("Melee Attack")]
    public bool hasMeleeAttack = true;
    public float meleeAttackRange = 2f;
    public float meleeAttackCooldown = 2f;
    public int meleeDamage = 20;
    public float knockbackForce = 8f;
    public float meleeAttackChance = 100f;
    private float meleeAttackTimer;
    private bool isAttacking = false;

    [Header("Detection Range")]
    public float detectionRange = 15f;

    [Header("Normal Attack")]
    public GameObject normalBullet;
    public float normalBulletSpeed = 8f;
    public float normalAttackCooldown = 3f;
    public float normalAttackChance = 60f;

    [Header("Wave Bullet Attack")]
    public GameObject waveBullet;
    public float waveBulletSpeed = 6f;
    public float waveAttackCooldown = 8f;
    public float waveAttackChance = 30f;
    public int wavesCount = 3;
    public float delayBetweenWaves = 0.3f;

    [Header("Ultimate - Lightning Strike")]
    public GameObject lightningBolt;
    public GameObject warningIndicator;
    public float ultimateCooldown = 15f;
    public float ultimateChance = 10f;
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
    [SerializeField, Range(0f, 2f)] private float fadeSpeed = 1f; // Tốc độ fade âm

    private float normalAttackTimer;
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

        normalAttackTimer = normalAttackCooldown;
        waveAttackTimer = waveAttackCooldown;
        ultimateTimer = ultimateCooldown;
        meleeAttackTimer = 0f;

        InvokeRepeating("CalculatePath", 0f, repeatTimeUpdatePath);

        // --- Tạo AudioSource tự động nếu chưa gắn ---
        if (moveAudioSource == null)
        {
            moveAudioSource = gameObject.AddComponent<AudioSource>();
        }
        if (attackAudioSource == null)
        {
            attackAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // --- Cấu hình moveAudioSource ---
        if (moveSFX != null)
        {
            moveAudioSource.clip = moveSFX;
            moveAudioSource.loop = true;
            moveAudioSource.volume = 0f;
            moveAudioSource.playOnAwake = false;
        }

        // --- Cấu hình attackAudioSource ---
        attackAudioSource.loop = false;
        attackAudioSource.playOnAwake = false;
    }

    private void Update()
    {
        normalAttackTimer -= Time.deltaTime;
        waveAttackTimer -= Time.deltaTime;
        ultimateTimer -= Time.deltaTime;
        meleeAttackTimer -= Time.deltaTime;

        FacePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, GetPlayerPosition());

        // Quản lý phát âm thanh di chuyển
        bool shouldMoveSound = !isAttacking && distanceToPlayer <= detectionRange;

        if (shouldMoveSound)
            moveTargetVolume = moveVolume;
        else
            moveTargetVolume = 0f;

        if (moveAudioSource != null && moveSFX != null)
        {
            if (!moveAudioSource.isPlaying)
                moveAudioSource.Play();

            // Làm mượt (fade) âm di chuyển
            moveAudioSource.volume = Mathf.MoveTowards(
                moveAudioSource.volume,
                moveTargetVolume,
                fadeSpeed * Time.deltaTime
            );
        }

        if (distanceToPlayer > detectionRange)
            return;

        if (hasMeleeAttack && distanceToPlayer <= meleeAttackRange && !isAttacking)
        {
            TryUseMeleeAttack();
        }
        else if (!isAttacking)
        {
            TryUseSkill();
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

        float randomValue = Random.Range(0f, 100f);
        if (randomValue < meleeAttackChance)
        {
            UseMeleeAttack();
            meleeAttackTimer = meleeAttackCooldown;
        }
    }

    void UseMeleeAttack()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Attack");

        // Phát âm thanh tấn công
        if (attackAudioSource != null && meleeAttackSFX != null)
        {
            Debug.Log("🎵 DarkKing: Play melee attack sound!");
            attackAudioSource.PlayOneShot(meleeAttackSFX, meleeVolume);
        }

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

    void TryUseSkill()
    {
        float randomValue = Random.Range(0f, 100f);

        if (ultimateTimer <= 0 && randomValue < ultimateChance)
        {
            UseUltimateSkill();
            ultimateTimer = ultimateCooldown;
            return;
        }

        if (waveAttackTimer <= 0 && randomValue < (ultimateChance + waveAttackChance))
        {
            UseWaveSkill();
            waveAttackTimer = waveAttackCooldown;
            return;
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
            {
                yield return null;
            }

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
    }
}
