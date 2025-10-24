using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
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

    // ✅ THÊM: Melee Attack Settings
    [Header("Melee Attack")]
    public bool hasMeleeAttack = true; // Bật/tắt melee
    public float meleeAttackRange = 2f;
    public float meleeAttackCooldown = 2f;
    public GameObject slashPrefab;
    public Transform slashSpawnPoint;
    public float slashOffsetDistance = 1.5f;
    public float knockbackForce = 8f;
    public float meleeAttackChance = 100f; // 100% khi đủ gần
    private float meleeAttackTimer;
    private bool isAttacking = false;

    // Skill System
    [Header("Detection Range")]
    public float detectionRange = 15f; // Phạm vi phát hiện player để tấn công

    [Header("Normal Attack")]
    public GameObject normalBullet;
    public float normalBulletSpeed = 8f;
    public float normalAttackCooldown = 3f;
    public float normalAttackChance = 60f; // 60%

    [Header("Wave Bullet Attack")]
    public GameObject waveBullet;
    public float waveBulletSpeed = 6f;
    public float waveAttackCooldown = 8f;
    public float waveAttackChance = 30f; // 30%
    public int wavesCount = 3; // Số đợt sóng
    public float delayBetweenWaves = 0.3f; // Delay giữa các đợt

    [Header("Ultimate - Lightning Strike")]
    public GameObject lightningBolt; // Prefab cột sét
    public GameObject warningIndicator; // Prefab vùng cảnh báo màu đỏ
    public float ultimateCooldown = 15f;
    public float ultimateChance = 10f; // 10%
    public int lightningCount = 8; // Số cột sét
    public float lightningSpawnRadius = 5f; // Bán kính spawn xung quanh player
    public float warningDuration = 0.5f; // Thời gian hiển thị cảnh báo trước khi sét đánh
    public float delayBetweenLightning = 0.15f; // Delay giữa mỗi cột sét

    private float normalAttackTimer;
    private float waveAttackTimer;
    private float ultimateTimer;

    // Freeze
    public float freezeDurationTime;
    float freezeDuration;

    // ✅ Lưu scale ban đầu
    private Vector3 originalScale;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        freezeDuration = 0;

        // ✅ Lưu scale ban đầu của characterSR
        if (characterSR != null)
        {
            originalScale = characterSR.transform.localScale;
        }

        // Khởi tạo cooldown timer
        normalAttackTimer = normalAttackCooldown;
        waveAttackTimer = waveAttackCooldown;
        ultimateTimer = ultimateCooldown;
        meleeAttackTimer = 0f;

        InvokeRepeating("CalculatePath", 0f, repeatTimeUpdatePath);
    }

    private void Update()
    {
        // Update cooldown timers
        normalAttackTimer -= Time.deltaTime;
        waveAttackTimer -= Time.deltaTime;
        ultimateTimer -= Time.deltaTime;
        meleeAttackTimer -= Time.deltaTime;

        // ✅ Luôn flip theo hướng player
        FacePlayer();

        // ✅ Kiểm tra khoảng cách để dùng melee hoặc ranged
        float distanceToPlayer = Vector2.Distance(transform.position, GetPlayerPosition());

        // Chỉ tấn công khi player trong phạm vi phát hiện
        if (distanceToPlayer > detectionRange)
            return;

        // Nếu đủ gần thì ưu tiên melee, nếu không thì dùng skill tầm xa
        if (hasMeleeAttack && distanceToPlayer <= meleeAttackRange && !isAttacking)
        {
            TryUseMeleeAttack();
        }
        else if (!isAttacking) // Chỉ dùng skill tầm xa khi không đang melee
        {
            TryUseSkill();
        }
    }

    // ✅ Hàm flip hướng nhìn player
    void FacePlayer()
    {
        if (characterSR == null) return;

        Vector3 playerPos = GetPlayerPosition();
        float dirX = playerPos.x - transform.position.x;

        if (Mathf.Abs(dirX) > 0.05f)
        {
            // ✅ Chỉ flip trục X, giữ nguyên Y và Z
            float newScaleX = dirX < 0 ? -Mathf.Abs(originalScale.x) : Mathf.Abs(originalScale.x);
            characterSR.transform.localScale = new Vector3(newScaleX, originalScale.y, originalScale.z);
        }
    }

    // ✅ THÊM: Thử tấn công cận chiến
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

    // ✅ THÊM: Tấn công cận chiến
    void UseMeleeAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        StartCoroutine(MeleeAttackCoroutine());
    }

    // ✅ THÊM: Coroutine xử lý melee attack
    IEnumerator MeleeAttackCoroutine()
    {
        // Dừng di chuyển trong lúc tấn công
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        yield return new WaitForSeconds(0.3f); // Delay trước khi spawn slash
        SpawnSlashEffect();

        yield return new WaitForSeconds(0.3f); // Delay sau khi spawn slash
        isAttacking = false;

        // Tiếp tục di chuyển
        if (path != null)
        {
            MoveToTarget();
        }
    }

    // ✅ THÊM: Tạo hiệu ứng chém
    void SpawnSlashEffect()
    {
        if (slashPrefab == null) return;

        Vector3 playerPos = GetPlayerPosition();
        Vector2 directionToPlayer = (playerPos - transform.position).normalized;

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

        // Nếu slash effect có script EnemySlashEffect
        EnemySlashEffect slashEffect = slash.GetComponent<EnemySlashEffect>();
        if (slashEffect != null)
        {
            slashEffect.Initialize(directionToPlayer, characterSR.transform.localScale.x < 0, knockbackForce);
        }
    }

    // ✅ THÊM: Animation Event callback (nếu dùng animation event)
    public void MeleeAttackComplete()
    {
        isAttacking = false;
    }

    void TryUseSkill()
    {
        // Random để quyết định skill nào được dùng
        float randomValue = Random.Range(0f, 100f);

        // Ưu tiên Ultimate > Wave > Normal
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

        if (normalAttackTimer <= 0 && randomValue < (ultimateChance + waveAttackChance + normalAttackChance))
        {
            UseNormalAttack();
            normalAttackTimer = normalAttackCooldown;
            return;
        }
    }

    // Chiêu 1: Bắn thường
    void UseNormalAttack()
    {
        if (normalBullet == null) return;

        // ✅ Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        var bulletTmp = Instantiate(normalBullet, transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bulletTmp.GetComponent<Rigidbody2D>();

        Vector3 playerPos = GetPlayerPosition();
        Vector3 direction = (playerPos - transform.position).normalized;

        bulletRb.AddForce(direction * normalBulletSpeed, ForceMode2D.Impulse);
    }

    // Chiêu 2: Wave Bullet - Bắn 3 đợt sóng đạn
    void UseWaveSkill()
    {
        if (waveBullet == null) return;

        // ✅ Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        StartCoroutine(WaveAttackCoroutine());
    }

    IEnumerator WaveAttackCoroutine()
    {
        for (int wave = 0; wave < wavesCount; wave++)
        {
            // Bắn 8 hướng xung quanh
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f; // 360 / 8 = 45 độ
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );

                var bulletTmp = Instantiate(waveBullet, transform.position, Quaternion.identity);
                Rigidbody2D bulletRb = bulletTmp.GetComponent<Rigidbody2D>();
                bulletRb.AddForce(direction * waveBulletSpeed, ForceMode2D.Impulse);
            }

            // Delay trước khi bắn đợt tiếp theo
            if (wave < wavesCount - 1)
                yield return new WaitForSeconds(delayBetweenWaves);
        }
    }

    // Chiêu 3: Ultimate - Lightning Strike
    void UseUltimateSkill()
    {
        if (lightningBolt == null) return;

        // ✅ Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        StartCoroutine(LightningStrikeCoroutine());
    }

    [SerializeField] private float spawnHeightOffset = 2f; // độ cao spawn thêm

    IEnumerator LightningStrikeCoroutine()
    {
        Vector3 playerPos = GetPlayerPosition();
        List<Vector3> strikePositions = new List<Vector3>();
        List<GameObject> warnings = new List<GameObject>();

        // Tạo vị trí và hiển thị cảnh báo cho tất cả các cột sét
        for (int i = 0; i < lightningCount; i++)
        {
            // Vị trí ngẫu nhiên xung quanh player
            Vector2 randomOffset = Random.insideUnitCircle * lightningSpawnRadius;
            Vector3 strikePos = new Vector3(
                playerPos.x + randomOffset.x,
                playerPos.y + randomOffset.y, // ✅ thêm offset
                playerPos.z
            );
            strikePositions.Add(strikePos);

            // Tạo vùng cảnh báo màu đỏ nếu có prefab
            if (warningIndicator != null)
            {
                GameObject warning = Instantiate(warningIndicator, strikePos, Quaternion.identity);
                warnings.Add(warning);
            }

            yield return new WaitForSeconds(delayBetweenLightning);
        }

        // Chờ một chút để người chơi thấy cảnh báo
        yield return new WaitForSeconds(warningDuration);

        // Xóa tất cả cảnh báo và triệu hồi sét
        for (int i = 0; i < strikePositions.Count; i++)
        {
            // Xóa cảnh báo
            if (i < warnings.Count && warnings[i] != null)
            {
                Destroy(warnings[i]);
            }

            // Triệu hồi cột sét tại vị trí
            Vector3 lightningPos = strikePositions[i];
            lightningPos.y += spawnHeightOffset; // ✅ có thể thêm offset riêng cho tia sét
            Instantiate(lightningBolt, lightningPos, Quaternion.identity);

            // Delay nhỏ giữa các cột sét để tạo hiệu ứng
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
            // ✅ Dừng di chuyển khi đang tấn công
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

            // ✅ Không cần flip ở đây vì đã có FacePlayer() trong Update()

            yield return null;
        }
    }

    // ✅ Gizmos để debug
    private void OnDrawGizmosSelected()
    {
        // Vẽ detection range (màu xanh dương)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ melee attack range (màu đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

        // Vẽ slash spawn point (màu vàng)
        if (slashSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(slashSpawnPoint.position, 0.3f);
        }
    }
}