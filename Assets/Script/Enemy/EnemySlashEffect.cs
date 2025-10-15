using System.Collections;
using UnityEngine;

public class EnemySlashEffect : MonoBehaviour
{
    [Header("Slash Settings")]
    public int damage = 10;
    public float lifetime = 0.5f;

    private float knockbackForce;
    private Vector2 attackDirection;
    private bool hasHit = false;

    public void Initialize(Vector2 direction, bool flipX, float knockback)
    {
        attackDirection = direction;
        knockbackForce = knockback;

        // Flip slash sprite theo hướng enemy
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = flipX;
        }

        // Tự động xóa sau lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ hit player 1 lần
        if (hasHit) return;

        if (collision.CompareTag("Player"))
        {
            hasHit = true;

            // Gây damage
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Knockback
            PlayerKnockback playerKnockback = collision.GetComponent<PlayerKnockback>();
            if (playerKnockback != null)
            {
                playerKnockback.ApplyKnockback(attackDirection, knockbackForce);
            }

            // Có thể destroy ngay sau khi hit hoặc để animation chạy hết
            // Destroy(gameObject);
        }
    }
}