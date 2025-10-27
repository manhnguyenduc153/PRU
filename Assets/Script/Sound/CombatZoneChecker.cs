using UnityEngine;
using System.Collections;

public class CombatZoneChecker : MonoBehaviour
{
    [Header("Combat Zone Settings")]
    [SerializeField] private float combatRadius = 10f;

    [Header("Combat Exit Delay")]
    [SerializeField] private float exitCombatDelay = 10f;

    private bool isInCombat = false;
    private float lastEnemyDetectedTime = 0f;
    private Coroutine combatCheckCoroutine;

    void Start()
    {
        // Bắt đầu kiểm tra combat zone
        combatCheckCoroutine = StartCoroutine(CheckCombatZone());
    }

    private IEnumerator CheckCombatZone()
    {
        while (true)
        {
            // Tìm tất cả collider trong vùng combat
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, combatRadius);

            // Đếm số enemy có tag "Enemy"
            int enemyCount = 0;
            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    enemyCount++;
                }
            }

            if (enemyCount > 0)
            {
                // Có enemy trong vùng
                lastEnemyDetectedTime = Time.time;

                if (!isInCombat)
                {
                    // Chuyển sang chế độ combat
                    EnterCombat();
                }
            }
            else
            {
                // Không có enemy
                if (isInCombat && Time.time - lastEnemyDetectedTime >= exitCombatDelay)
                {
                    // Đã hết 10 giây không có enemy, thoát combat
                    ExitCombat();
                }
            }

            yield return new WaitForSeconds(0.2f); // Kiểm tra mỗi 0.2 giây
        }
    }

    private void EnterCombat()
    {
        isInCombat = true;
        Debug.Log("Entered Combat!");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatMusic();
        }
    }

    private void ExitCombat()
    {
        isInCombat = false;
        Debug.Log("Exited Combat!");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCombatMusic();
        }
    }

    // Vẽ combat zone trong Scene view để dễ debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isInCombat ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, combatRadius);
    }

    void OnDestroy()
    {
        if (combatCheckCoroutine != null)
        {
            StopCoroutine(combatCheckCoroutine);
        }
    }
}