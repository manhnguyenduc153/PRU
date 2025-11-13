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

    [Header("Cutscene Backstory")]
    [SerializeField] private GameObject cutsceneManagerGO;
    private CutsceneBackstory cutsceneBackstory;

    [Header("Final Boss Settings")]
    [SerializeField] private bool isFinalBoss = false;
    [SerializeField] private string finalPortalTag = "FinalPortal";

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

        if (cutsceneManagerGO != null)
            cutsceneBackstory = cutsceneManagerGO.GetComponent<CutsceneBackstory>();
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
                healthUI.HideBossUI();

            // Drop loot
            if (lootDropper != null)
                lootDropper.DropLoot();

            EnemyDrop dropper = GetComponent<EnemyDrop>();
            if (dropper != null)
                dropper.DropLoot();

            // VFX
            if (deathVFXPrefab != null)
                Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);

            if (isFinalBoss)
            {
                HandleFinalBossDeath(); // Chỉ active portal
            }


            if (isBoss)
            {
                // ✅ NGAY LẬP TỨC tắt hoạt động của boss
                DisableBossActivity();

                // Delay 1 giây rồi chạy cutscene
                if (cutsceneBackstory != null)
                    StartCoroutine(DelayedCutscene());
                else
                    Destroy(gameObject); // Nếu không có cutscene thì destroy luôn
            }
            else
            {
                // Enemy thường thì destroy ngay
                Destroy(gameObject);
            }
        }
    }

    // ✅ Phương thức mới: Tắt hoạt động của boss
    private void DisableBossActivity()
    {
        // Tắt collider để không còn trigger combat
        var colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
            col.enabled = false;

        // Dừng di chuyển
        var rigidbody = GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            rigidbody.velocity = Vector2.zero;
            rigidbody.isKinematic = true; // Không bị ảnh hưởng bởi vật lý
        }

        // Tắt animator nếu có
        if (animator != null)
            animator.enabled = false;

        // Tắt tất cả script di chuyển và AI
        var movementScripts = GetComponents<MonoBehaviour>();
        foreach (var script in movementScripts)
        {
            // Không disable EnemyHealth (script này) và CutsceneBackstory
            if (script != this && script.GetType() != typeof(CutsceneBackstory))
                script.enabled = false;
        }
    }

    private IEnumerator DelayedCutscene()
    {
        yield return new WaitForSecondsRealtime(1f); // delay 1 giây bất chấp Time.timeScale

        // Gán enemy này cho cutscene biết để destroy
        cutsceneBackstory.targetEnemy = gameObject;

        // Chạy cutscene
        cutsceneBackstory.PlayCutscene();
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

    private void HandleFinalBossDeath()
    {
        // Tìm parent object bằng tag
        GameObject portalParent = GameObject.FindWithTag("FinalPortal");

        if (portalParent != null)
        {
            // Active tất cả child (hoặc portal chính)
            foreach (Transform child in portalParent.transform)
            {
                child.gameObject.SetActive(true);
            }

            Debug.Log("[EnemyHealth] Final portal activated.");
        }
        else
        {
            Debug.LogWarning("[EnemyHealth] No GameObject found with tag 'FinalPortal'");
        }
    }
}