using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<EnemyHealth>())
        {
            EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();

            enemyHealth.TakeDamage(damageAmount, transform);

            // THÊM DÒNG NÀY: Trigger buff effects khi hit enemy
            if (BuffManager.Instance != null)
            {
                BuffManager.Instance.OnEnemyHit(other.transform, transform);
            }
        }
    }
}
