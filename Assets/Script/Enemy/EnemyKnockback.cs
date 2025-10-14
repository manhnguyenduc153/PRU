using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D cannot found on " + gameObject.name);
        }
    }

    public void ApplyKnockback(Vector2 direction)
    {
        if (!isKnockedBack && rb != null)
        {
            StartCoroutine(KnockbackCoroutine(direction));
        }
    }

    public void ApplyKnockback(Transform attacker)
    {
        Vector2 direction = (transform.position - attacker.position).normalized;
        ApplyKnockback(direction);
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction)
    {
        isKnockedBack = true;
        float elapsed = 0f;

        var enemyMovement = GetComponent<MonoBehaviour>();
        bool wasEnabled = false;
        if (enemyMovement != null)
        {
            wasEnabled = enemyMovement.enabled;
            enemyMovement.enabled = false;
        }

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float strength = knockbackCurve.Evaluate(elapsed / knockbackDuration);
            rb.velocity = direction * knockbackForce * strength;
            yield return null;
        }

        rb.velocity = Vector2.zero;

        if (enemyMovement != null && wasEnabled)
        {
            enemyMovement.enabled = true;
        }

        isKnockedBack = false;
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }
}