using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float damage = 10f;

    void Start()
    {
        // Auto destroy after 2 seconds
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        // Move forward
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Deal damage to enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Destroy skill
            Destroy(gameObject);
        }
    }
}