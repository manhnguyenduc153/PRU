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
    Coroutine moveCoroutine;

    // Skill System
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

    [Header("Ultimate - Meteor Rain")]
    public GameObject meteorBullet;
    public float meteorFallSpeed = 10f;
    public float ultimateCooldown = 15f;
    public float ultimateChance = 10f; // 10%
    public int meteorCount = 12; // Số meteor rơi
    public float meteorSpawnHeight = 8f; // Độ cao spawn meteor
    public float meteorSpawnRadius = 5f; // Bán kính spawn xung quanh player

    private float normalAttackTimer;
    private float waveAttackTimer;
    private float ultimateTimer;

    // Freeze
    public float freezeDurationTime;
    float freezeDuration;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        freezeDuration = 0;

        // Khởi tạo cooldown timer
        normalAttackTimer = normalAttackCooldown;
        waveAttackTimer = waveAttackCooldown;
        ultimateTimer = ultimateCooldown;

        InvokeRepeating("CalculatePath", 0f, repeatTimeUpdatePath);
    }

    private void Update()
    {
        // Update cooldown timers
        normalAttackTimer -= Time.deltaTime;
        waveAttackTimer -= Time.deltaTime;
        ultimateTimer -= Time.deltaTime;

        // Chọn skill để sử dụng dựa trên priority và cooldown
        TryUseSkill();
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

    // Chiêu 3: Ultimate - Meteor Rain
    void UseUltimateSkill()
    {
        if (meteorBullet == null) return;
        StartCoroutine(MeteorRainCoroutine());
    }

    IEnumerator MeteorRainCoroutine()
    {
        Vector3 playerPos = GetPlayerPosition();

        for (int i = 0; i < meteorCount; i++)
        {
            // Vị trí ngẫu nhiên xung quanh player (target)
            Vector2 randomOffset = Random.insideUnitCircle * meteorSpawnRadius;
            Vector3 targetPos = new Vector3(
                playerPos.x + randomOffset.x,
                playerPos.y + randomOffset.y,
                playerPos.z
            );

            // Spawn meteor ở trên cao
            Vector3 spawnPos = new Vector3(
                targetPos.x,
                targetPos.y + meteorSpawnHeight,
                targetPos.z
            );

            // Spawn meteor
            var meteorTmp = Instantiate(meteorBullet, spawnPos, Quaternion.identity);
            Rigidbody2D meteorRb = meteorTmp.GetComponent<Rigidbody2D>();

            // Set target position cho meteor
            MeteorBullet meteorScript = meteorTmp.GetComponent<MeteorBullet>();
            if (meteorScript != null)
            {
                meteorScript.SetTargetPosition(targetPos);
            }

            // Rơi thẳng xuống
            meteorRb.velocity = Vector2.down * meteorFallSpeed;

            // Delay nhỏ giữa các meteor
            yield return new WaitForSeconds(0.1f);
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

            if (force.x != 0)
            {
                if (force.x < 0)
                    characterSR.transform.localScale = new Vector3(-1, 1, 1);
                else
                    characterSR.transform.localScale = new Vector3(1, 1, 1);
            }

            yield return null;
        }
    }
}