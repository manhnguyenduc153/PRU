using System.Collections;
using UnityEngine;

public class EnemyRangedSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint; // Điểm bắn đạn
    public float bulletSpeed = 10f;
    public float skillCooldown = 3f;
    public float skillRange = 15f; // Khoảng cách để kích hoạt skill

    [Header("Burst Shot Settings")]
    public bool useBurstShot = false; // Bắn nhiều phát liên tiếp
    public int burstCount = 3; // Số viên đạn trong burst
    public float burstDelay = 0.2f; // Delay giữa mỗi viên

    [Header("Spread Settings")]
    public bool useSpread = false; // Bắn theo hình quạt
    public int spreadCount = 3; // Số đạn trong spread
    public float spreadAngle = 30f; // Góc giữa các đạn

    [Header("Audio")]
    public AudioClip shootSound;

    private Transform player;
    private float cooldownTimer;
    private bool isUsingSkill = false;
    private Animator animator;
    private AudioSource audioSource;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>()?.transform;
        cooldownTimer = 0f;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Thử sử dụng skill bắn đạn
    /// </summary>
    /// <returns>True nếu skill được kích hoạt</returns>
    public bool TryUseSkill()
    {
        if (isUsingSkill || cooldownTimer > 0 || player == null)
            return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > skillRange)
            return false;

        isUsingSkill = true;
        cooldownTimer = skillCooldown;

        // Trigger animation nếu có
        if (animator != null)
        {
            animator.SetTrigger("RangedAttack");
        }

        // Chọn kiểu bắn
        if (useBurstShot)
        {
            StartCoroutine(BurstShot());
        }
        else if (useSpread)
        {
            SpreadShot();
            StartCoroutine(ResetSkillState(0.5f));
        }
        else
        {
            SingleShot();
            StartCoroutine(ResetSkillState(0.3f));
        }

        return true;
    }

    /// <summary>
    /// Bắn 1 viên đạn đơn
    /// </summary>
    private void SingleShot()
    {
        if (bulletPrefab == null || player == null) return;

        Vector2 direction = (player.position - GetFirePosition()).normalized;
        ShootBullet(direction);
        PlayShootSound();
    }

    /// <summary>
    /// Bắn nhiều viên liên tiếp (burst)
    /// </summary>
    private IEnumerator BurstShot()
    {
        for (int i = 0; i < burstCount; i++)
        {
            if (player != null)
            {
                Vector2 direction = (player.position - GetFirePosition()).normalized;
                ShootBullet(direction);
                PlayShootSound();
            }

            if (i < burstCount - 1) // Không delay ở viên cuối
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }

        yield return new WaitForSeconds(0.3f);
        isUsingSkill = false;
    }

    /// <summary>
    /// Bắn nhiều viên theo hình quạt (spread)
    /// </summary>
    private void SpreadShot()
    {
        if (bulletPrefab == null || player == null) return;

        Vector2 baseDirection = (player.position - GetFirePosition()).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        // Tính góc bắt đầu
        float startAngle = baseAngle - (spreadAngle * (spreadCount - 1) / 2f);

        for (int i = 0; i < spreadCount; i++)
        {
            float currentAngle = startAngle + (spreadAngle * i);
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

            ShootBullet(direction);
        }

        PlayShootSound();
    }

    /// <summary>
    /// Tạo và bắn 1 viên đạn
    /// </summary>
    private void ShootBullet(Vector2 direction)
    {
        Vector3 spawnPos = GetFirePosition();
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = direction * bulletSpeed;
        }

        // Set rotation ban đầu nếu bullet có autoRotate
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Lấy vị trí bắn đạn
    /// </summary>
    private Vector3 GetFirePosition()
    {
        if (firePoint != null)
            return firePoint.position;

        // Nếu không có firePoint, bắn từ trước mặt enemy
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        return transform.position + (Vector3)directionToPlayer * 0.5f;
    }

    /// <summary>
    /// Reset trạng thái skill
    /// </summary>
    private IEnumerator ResetSkillState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isUsingSkill = false;
    }

    /// <summary>
    /// Phát âm thanh bắn
    /// </summary>
    private void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    /// <summary>
    /// Kiểm tra xem có thể dùng skill không
    /// </summary>
    public bool CanUseSkill()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        return !isUsingSkill && cooldownTimer <= 0 && distance <= skillRange;
    }

    /// <summary>
    /// Lấy thời gian cooldown còn lại
    /// </summary>
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0, cooldownTimer);
    }

    /// <summary>
    /// Animation Event - gọi từ Animation
    /// </summary>
    public void OnShootAnimationEvent()
    {
        // Có thể gọi hàm này từ Animation Event để đồng bộ thời điểm bắn với animation
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ skill range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, skillRange);

        // Vẽ fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}