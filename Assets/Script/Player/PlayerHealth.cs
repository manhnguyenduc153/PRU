using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 1000 ;
    private int currentHealth;

    [Header("Invincibility")]
    public float invincibilityDuration = 1f; // Thời gian bất tử sau khi bị damage
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"Player Health Initialized: {currentHealth}/{maxHealth}");
    }

    private void Update()
    {
        // Update invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Nếu đang bất tử thì không nhận damage
        if (isInvincible)
        {
            return;
        }

        currentHealth -= damage;

        // Log ra console
        Debug.Log($"Player took {damage} damage! Current HP: {currentHealth}/{maxHealth}");

        // Kích hoạt invincibility
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Kiểm tra chết
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // Thêm logic chết ở đây (animation, game over, respawn, etc.)

        // Ví dụ: Destroy player
        // Destroy(gameObject);

        // Hoặc respawn
        // Respawn();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log($"Player healed {amount} HP! Current HP: {currentHealth}/{maxHealth}");
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
}