using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 3;
    private int currentHealth;
    private EnemyKnockback knockbackScript;
    private Animator animator;

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

        if (knockbackScript != null && attacker != null)
        {
            animator.SetTrigger("Attacked");
            knockbackScript.ApplyKnockback(attacker);
        }

        DetectDeath();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);

        if (knockbackScript != null)
        {
            knockbackScript.ApplyKnockback(knockbackDirection);
        }

        DetectDeath();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);
        DetectDeath();
    }

    private void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            //animator.SetTrigger("Death");
            //StartCoroutine(Die());
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