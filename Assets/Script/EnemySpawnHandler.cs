using UnityEngine;
using System.Collections;

public class EnemySpawnHandler : MonoBehaviour
{
    public MonoBehaviour enemyAI; // tham chiếu đến script EnemyAI
    public float delay = 2f;

    void OnEnable() // gọi khi object được bật (spawn ra)
    {
        if (enemyAI == null)
            enemyAI = GetComponent<MonoBehaviour>();

        StartCoroutine(ActivateAfterDelay());
    }

    IEnumerator ActivateAfterDelay()
    {
        // Tắt script AI
        if (enemyAI != null)
            enemyAI.enabled = false;

        yield return new WaitForSeconds(delay);

        // Bật lại sau delay
        if (enemyAI != null)
            enemyAI.enabled = true;
    }
}
