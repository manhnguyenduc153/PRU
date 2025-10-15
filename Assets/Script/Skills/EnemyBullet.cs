using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;

    [Header("Rotation Settings")]
    public bool autoRotate = true; // Tự động xoay theo hướng bay

    [Header("Hit Behavior")]
    public bool destroyOnHitPlayer = true; // Có biến mất khi va chạm Player hay không

    private Rigidbody2D rb;

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
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);

            PlayerKnockback playerKnockback = collision.GetComponent<PlayerKnockback>();
            if (playerKnockback != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                playerKnockback.ApplyKnockback(knockbackDirection, knockbackForce);
            }

            // Chỉ phá hủy nếu được bật trong Inspector
            if (destroyOnHitPlayer)
            {
                Destroy(gameObject);
            }

            return;
        }

        // Nếu chạm tường, luôn phá hủy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
