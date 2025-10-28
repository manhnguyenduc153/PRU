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
    private int originalLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D cannot found on " + gameObject.name);
        }

        // Lưu layer gốc (phải là Enemy layer)
        originalLayer = gameObject.layer;

        // Kiểm tra xem có đúng layer không
        if (LayerMask.LayerToName(originalLayer) != "Enemy")
        {
            Debug.LogWarning(gameObject.name + " không ở layer 'Enemy'. Vui lòng gán layer 'Enemy' cho object này!");
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

        // Chuyển sang layer EnemyKnockback
        int knockbackLayer = LayerMask.NameToLayer("EnemyKnockback");

        if (knockbackLayer == -1)
        {
            Debug.LogError("Layer 'EnemyKnockback' không tồn tại! Vui lòng tạo layer này.");
            yield break;
        }

        SetLayerRecursively(gameObject, knockbackLayer);

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float strength = knockbackCurve.Evaluate(elapsed / knockbackDuration);
            rb.velocity = direction * knockbackForce * strength;
            yield return null;
        }

        rb.velocity = Vector2.zero;

        // Chuyển về layer Enemy gốc
        SetLayerRecursively(gameObject, originalLayer);

        if (enemyMovement != null && wasEnabled)
        {
            enemyMovement.enabled = true;
        }

        isKnockedBack = false;
    }

    // Hàm đệ quy để set layer cho cả object và children
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }
}