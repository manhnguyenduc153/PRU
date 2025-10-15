using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private GameObject deathVFXPrefab;

    private int currentHealth;
    private EnemyKnockback knockbackScript;
    private Animator animator;
    private Flash flash;


    private void Awake()
    {
        flash = GetComponent<Flash>();
    }

    private void Start()
    {
        currentHealth = startingHealth;
        knockbackScript = GetComponent<EnemyKnockback>();
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);

        // Thêm dòng này để kích hoạt flash
        if (flash != null)
        {
            StartCoroutine(flash.FlashRoutine());
        }

        if (knockbackScript != null && attacker != null)
        {
            animator.SetTrigger("Attacked");
            knockbackScript.ApplyKnockback(attacker);
        }

        // Xóa DetectDeath() ở đây vì nó đã được gọi trong FlashRoutine()
        // DetectDeath();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);

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

        if (flash != null)
        {
            StartCoroutine(flash.FlashRoutine());
        }
    }

    public void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            //animator.SetTrigger("Death");
            //StartCoroutine(Die());
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private IEnumerator Die()
    {
        // Vô hiệu hóa movement và collider
        GetComponent<Collider2D>().enabled = false;

        // Tắt script movement nếu có
        var movement = GetComponent<MonoBehaviour>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // Đợi animation Death chạy xong
        yield return new WaitForSeconds(1f); // Điều chỉnh theo độ dài animation Death

        Destroy(gameObject);
    }
}