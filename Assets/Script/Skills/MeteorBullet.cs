using UnityEngine;

public class MeteorBullet : MonoBehaviour
{
    private Vector3 targetPosition;
    private bool hasTarget = false;
    private bool hasLanded = false; // Đã chạm đất chưa

    public float impactRadius = 1f; // Bán kính gây damage khi chạm đất
    public int damage = 15; // Sát thương gây ra

    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
        hasTarget = true;
    }

    private void Update()
    {
        if (hasTarget && !hasLanded)
        {
            // Kiểm tra xem đã đến gần vị trí target chưa
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (distance < 0.5f) // Đã chạm đất tại vị trí target
            {
                hasLanded = true;
                OnImpact();
            }
        }
    }

    void OnImpact()
    {
        // Tìm tất cả collider trong bán kính impact
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);

        foreach (Collider2D hit in hits)
        {
            // Kiểm tra xem có phải player không
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                // Gây damage cho player
                // player.TakeDamage(damage); // Uncomment khi có system HP
                Debug.Log("Player hit by meteor!");
            }
        }

        // Destroy meteor sau khi impact
        Destroy(gameObject);
    }

    // Vô hiệu hóa collision trong lúc rơi
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Không làm gì khi đang rơi, chỉ gây damage khi landed
        if (!hasLanded)
        {
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Không làm gì khi đang rơi, chỉ gây damage khi landed
        if (!hasLanded)
        {
            return;
        }
    }

    // Hiển thị bán kính impact trong Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}