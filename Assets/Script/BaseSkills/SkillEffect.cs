using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    [Header("Speed Settings")]
    public float moveSpeed = 5f; // Tốc độ bình thường
    public float animationSpeed = 1f; // Tốc độ animation bình thường

    [Header("Stats")]
    public float damage = 10f;

    private Vector2 moveDirection;
    private Animator animator;
    private float lifetime = 5f; // Thời gian tồn tại hợp lý

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = animationSpeed;
        }

        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    // QUAN TRỌNG: Phát hiện va chạm với Enemy
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu đối tượng có tag "Enemy"
        if (collision.CompareTag("Enemy"))
        {
            // Gây damage cho enemy
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)damage); // Chuyển float sang int
            }

            // Hủy skill sau khi trúng
            Destroy(gameObject);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void SetAnimationSpeed(float newAnimSpeed)
    {
        if (animator != null)
        {
            animator.speed = newAnimSpeed;
        }
    }
}