using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public int damage = 10;
    public float lifeTime = 5f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;

    [Header("Rotation Settings")]
    public bool autoRotate = true; // Tự động xoay theo hướng bay

    [Header("Hit Behavior")]
    public bool destroyOnHitPlayer = true; // Đạn có biến mất khi chạm Player hay không
    public bool explodeOnHitPlayer = true; // Có phát nổ khi chạm Player hay không

    [Header("Explosion Effect")]
    public GameObject explosionEffectPrefab; // Prefab hiệu ứng nổ

    private Rigidbody2D rb;
    private BulletAudioManager audioManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioManager = GetComponent<BulletAudioManager>();

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Tự động xoay bullet theo hướng velocity
        if (autoRotate && rb != null && rb.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ✅ Chỉ xử lý khi va chạm với Player
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Gây sát thương cho Player
            playerHealth.TakeDamage(damage);

            // Knockback nếu có
            PlayerKnockback playerKnockback = collision.GetComponent<PlayerKnockback>();
            if (playerKnockback != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                playerKnockback.ApplyKnockback(knockbackDirection, knockbackForce);
            }

            // ⭐ Phát âm thanh va chạm với Player
            if (audioManager != null)
            {
                audioManager.PlayHitPlayerSound();
            }

            // Hiệu ứng nổ
            if (explodeOnHitPlayer && explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            // Biến mất sau khi chạm Player (nếu có)
            if (destroyOnHitPlayer)
            {
                Destroy(gameObject);
            }
            return;
        }

        // ✅ Nếu chạm tường thì luôn phá hủy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // ⭐ Phát âm thanh va chạm với tường
            if (audioManager != null)
            {
                audioManager.PlayHitWallSound();
            }

            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}