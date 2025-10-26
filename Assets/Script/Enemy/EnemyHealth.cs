using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int startingHealth = 3;
    
    [Header("Boss Settings (Optional)")]
    [SerializeField] private bool isBoss = false;
    [SerializeField] private string bossName = "Boss Name";
    
    [Header("VFX")]
    [SerializeField] private GameObject deathVFXPrefab;
    
    private int currentHealth;
    private EnemyKnockback knockbackScript;
    private Animator animator;
    private Flash flash;
    private LootDropper lootDropper;
    private EnemyHealthUI healthUI;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        lootDropper = GetComponent<LootDropper>();
    }

    private void Start()
    {
        currentHealth = startingHealth;
        knockbackScript = GetComponent<EnemyKnockback>();
        animator = GetComponent<Animator>();
        
        // Chỉ tìm và setup UI khi là boss
        if (isBoss)
        {
            healthUI = FindObjectOfType<EnemyHealthUI>();
            if (healthUI != null)
            {
                healthUI.SetupBossHealth(bossName, currentHealth, startingHealth);
                healthUI.ShowBossUI();
            }
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);
        
        // Chỉ update UI khi là boss
        if (isBoss && healthUI != null)
        {
            healthUI.UpdateHealth(currentHealth);
        }
        
        if (flash != null)
        {
            StartCoroutine(flash.FlashRoutine());
        }
        
        if (knockbackScript != null && attacker != null)
        {
            knockbackScript.ApplyKnockback(attacker);
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);
        
        // Chỉ update UI khi là boss
        if (isBoss && healthUI != null)
        {
            healthUI.UpdateHealth(currentHealth);
        }
        
        if (flash != null)
        {
            StartCoroutine(flash.FlashRoutine());
        }
        
        if (knockbackScript != null)
        {
            knockbackScript.ApplyKnockback(knockbackDirection);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);
        
        // Chỉ update UI khi là boss
        if (isBoss && healthUI != null)
        {
            healthUI.UpdateHealth(currentHealth);
        }
        
        if (flash != null)
        {
            StartCoroutine(flash.FlashRoutine());
        }
    }

    public void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            // Ẩn UI nếu là boss
            if (isBoss && healthUI != null)
            {
                healthUI.HideBossUI();
            }
            
            // Drop loot
            if (lootDropper != null)
            {
                lootDropper.DropLoot();
            }
            
            // VFX
            if (deathVFXPrefab != null)
            {
                Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }

    private IEnumerator Die()
    {
        GetComponent<Collider2D>().enabled = false;
        var movement = GetComponent<MonoBehaviour>();
        if (movement != null)
        {
            movement.enabled = false;
        }
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}