using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f; // Tự động biến mất sau 5 giây

    private void Start()
    {
        // Tự động destroy sau một khoảng thời gian
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có phải player không
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject); // Destroy bullet sau khi hit
            return;
        }

        // Nếu chạm tường hoặc obstacle thì cũng destroy
        //if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        //{
        //    Destroy(gameObject);
        //}

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}