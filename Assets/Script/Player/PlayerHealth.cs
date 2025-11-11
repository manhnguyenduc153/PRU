using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("Invincibility")]
    public float invincibilityDuration = 1f;
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    [Header("Death Settings")]
    public string gameOverSceneName = "GameOver";
    public float deathDelay = 1f; // Delay trước khi fade (cho animation chết)
    public bool useTransition = true; // Bật/tắt transition

    [SerializeField] private HealthBarUI healthBarUI;

    [Header("VFX")]
    [SerializeField] private GameObject deathVFXPrefab;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"Player Health Initialized: {currentHealth}/{maxHealth}");
    }

    private void Update()
    {
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
        if (isInvincible || isDead)
        {
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage! Current HP: {currentHealth}/{maxHealth}");

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player Died!");

        if (healthBarUI != null)
        {
            healthBarUI.SetHealthImmediate(0, GetMaxHealth());
        }


        // Vô hiệu hóa input
        //GetComponent<PlayerController>()?.enabled = false;

        //if (GameManager.Instance != null)
        //{
        //    GameManager.Instance.CleanupForGameOver();
        //}

        if (BossItemInventory.Instance != null)
            BossItemInventory.Instance.ResetInventory();

        // VFX
        if (deathVFXPrefab != null)
        {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        }

        if (useTransition && CanvasFadeTransition.Instance != null)
        {
            CanvasFadeTransition.Instance.LoadSceneWithDelay(gameOverSceneName, deathDelay);
        }
        else
        {
            Invoke(nameof(LoadGameOverScene), deathDelay);
        }

        Destroy(gameObject);
    }

    void LoadGameOverScene()
    {
        if (Application.CanStreamedLevelBeLoaded(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{gameOverSceneName}' không tồn tại trong Build Settings!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log($"Player healed {amount} HP! Current HP: {currentHealth}/{maxHealth}");
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsInvincible() => isInvincible;
    public bool IsDead() => isDead;

    // Phương thức cho SaveSystem
    public void SetHealth(int health, int max)
    {
        currentHealth = health;
        maxHealth = max;
        if (healthBarUI != null)
        {
            healthBarUI.SetHealthImmediate(currentHealth, maxHealth);
        }
        Debug.Log($"Health loaded: {currentHealth}/{maxHealth}");
    }

    [ContextMenu("Test Death")]
    void TestDeath()
    {
        TakeDamage(currentHealth);
    }
}