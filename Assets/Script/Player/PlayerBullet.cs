using UnityEngine;
using System.Collections.Generic;

public class PlayerBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public int damage = 10;
    public float lifeTime = 5f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;

    [Header("Rotation Settings")]
    public bool autoRotate = true; // Tự động xoay theo hướng bay

    [Header("Hit Behavior")]
    public bool destroyOnHitEnemy = true; // Đạn có biến mất khi chạm enemy hay không
    public bool explodeOnHitEnemy = true; // Có phát nổ khi chạm enemy hay không
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
        // ❌ Không gây sát thương cho Player — bỏ qua Player
        if (collision.GetComponent<PlayerHealth>() != null)
            return;

        // ✅ Xử lý va chạm với Enemy
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

                // Knockback cho enemy nếu có
                Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }

                // Kiểm tra có phá hủy bullet sau khi hit đủ số enemy hay không
                if (!pierceEnemies || (maxEnemyHits > 0 && currentEnemyHits >= maxEnemyHits))
                {
                    if (explodeOnHitEnemy && explosionEffectPrefab != null)
                    {
                        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
                    }
                    if (destroyOnHitEnemy)
                    {
                        Destroy(gameObject);
                    }
                }
            }
            return;
        }

        // ✅ Nếu chạm tường thì luôn phá hủy
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
