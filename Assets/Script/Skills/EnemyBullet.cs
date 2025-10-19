using UnityEngine;
using System.Collections.Generic;

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
    public bool pierceEnemies = true; // Có xuyên qua nhiều enemy hay không
    public int maxEnemyHits = -1; // Số enemy tối đa có thể hit (-1 = không giới hạn)

    [Header("Explosion Effect")]
    public GameObject explosionEffectPrefab; // Prefab hiệu ứng nổ

    private Rigidbody2D rb;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>(); // Track các enemy đã hit
    private int currentEnemyHits = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
        // Xử lý va chạm với Player
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Gây sát thương
            playerHealth.TakeDamage(damage);

            // Knockback nếu có
            PlayerKnockback playerKnockback = collision.GetComponent<PlayerKnockback>();
            if (playerKnockback != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                playerKnockback.ApplyKnockback(knockbackDirection, knockbackForce);
            }

            // Hiệu ứng phát nổ (tuỳ loại bullet)
            if (explodeOnHitPlayer && explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            // Tuỳ chọn: có biến mất sau khi chạm hay không
            if (destroyOnHitPlayer)
            {
                Destroy(gameObject);
            }
            return;
        }

        // Xử lý va chạm với Enemy
        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // Kiểm tra xem đã hit enemy này chưa (tránh hit 2 lần)
            if (!hitEnemies.Contains(collision.gameObject))
            {
                hitEnemies.Add(collision.gameObject);
                currentEnemyHits++;

                // Gây sát thương cho enemy
                enemyHealth.TakeDamage(damage, transform);

                // Kiểm tra có phá hủy bullet sau khi hit đủ số enemy hay không
                if (!pierceEnemies || (maxEnemyHits > 0 && currentEnemyHits >= maxEnemyHits))
                {
                    if (explosionEffectPrefab != null)
                    {
                        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
                    }
                    Destroy(gameObject);
                }
            }
            return;
        }

        // Nếu chạm tường thì luôn phá hủy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}