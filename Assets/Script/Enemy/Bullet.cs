using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 5f; // tự xóa sau 5 giây nếu không trúng gì
    public int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu va chạm với player
        if (collision.CompareTag("Player"))
        {
            // Gây damage cho player (nếu có script HP)
            var player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // Gọi hàm trừ máu
                // player.TakeDamage(damage);
            }

            Destroy(gameObject); // Xóa đạn
        }
        else if (collision.CompareTag("Wall")) // Va tường cũng xóa
        {
            Destroy(gameObject);
        }
    }
}
