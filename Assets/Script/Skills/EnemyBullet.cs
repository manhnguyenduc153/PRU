using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f; // Có thể điều chỉnh trong Inspector

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có phải player không
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Gây damage
            playerHealth.TakeDamage(damage);

            // Gây knockback
            PlayerKnockback playerKnockback = collision.GetComponent<PlayerKnockback>();
            if (playerKnockback != null)
            {
                // Tính hướng từ bullet đến player
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                playerKnockback.ApplyKnockback(knockbackDirection, knockbackForce);
            }

            Destroy(gameObject);
            return;
        }

        // Nếu chạm tường thì destroy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}