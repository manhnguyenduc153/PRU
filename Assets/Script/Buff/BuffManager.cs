using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    private const int MAX_BUFF_LEVEL = 5;
    private const float BASE_PROC_INCREASE = 0.05f; // Tăng 5% proc chance mỗi level

    // Buff 1: Slash on hit
    [SerializeField] private int slashLevel = 0;
    [SerializeField] private float slashBaseProcChance = 0.3f;
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private float slashSpeed = 10f;

    // Buff 2: Lightning strike
    [SerializeField] private int lightningLevel = 0;
    [SerializeField] private float lightningBaseProcChance = 0.25f;
    [SerializeField] private GameObject lightningWarningPrefab;
    [SerializeField] private GameObject lightningStrikePrefab;
    [SerializeField] private float lightningRadius = 3f;
    [SerializeField] private float warningDuration = 0.5f;
    [SerializeField] private int lightningBaseDamage = 2;
    [SerializeField] private float lightningSpawnHeight = 3f;

    // Buff 3: Triple shot
    [SerializeField] private int tripleShotLevel = 0;
    [SerializeField] private float tripleShotBaseProcChance = 0.3f;
    [SerializeField] private GameObject tripleShotPrefab;
    [SerializeField] private float tripleShotSpeed = 8f;
    [SerializeField] private float spreadAngle = 20f;

    private void OnValidate()
    {
        // Clamp các giá trị level trong Unity Editor
        slashLevel = Mathf.Clamp(slashLevel, 0, MAX_BUFF_LEVEL);
        lightningLevel = Mathf.Clamp(lightningLevel, 0, MAX_BUFF_LEVEL);
        tripleShotLevel = Mathf.Clamp(tripleShotLevel, 0, MAX_BUFF_LEVEL);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Gọi method này khi player tấn công trúng enemy
    public void OnEnemyHit(Transform enemyTransform, Transform attackSource)
    {
        List<System.Action> availableBuffs = new List<System.Action>();

        if (slashLevel > 0 && Random.value <= GetSlashProcChance())
        {
            availableBuffs.Add(() => TriggerSlashEffect(enemyTransform, attackSource));
        }

        if (lightningLevel > 0 && Random.value <= GetLightningProcChance())
        {
            availableBuffs.Add(() => TriggerLightningEffect(enemyTransform));
        }

        if (tripleShotLevel > 0 && Random.value <= GetTripleShotProcChance())
        {
            availableBuffs.Add(() => TriggerTripleShotEffect(attackSource));
        }

        if (availableBuffs.Count > 0)
        {
            int randomIndex = Random.Range(0, availableBuffs.Count);
            availableBuffs[randomIndex].Invoke();
        }
    }

    #region Proc Chance Calculations
    private float GetSlashProcChance()
    {
        return Mathf.Min(slashBaseProcChance + (slashLevel - 1) * BASE_PROC_INCREASE, 0.99f);
    }

    private float GetLightningProcChance()
    {
        return Mathf.Min(lightningBaseProcChance + (lightningLevel - 1) * BASE_PROC_INCREASE, 0.99f);
    }

    private float GetTripleShotProcChance()
    {
        return Mathf.Min(tripleShotBaseProcChance + (tripleShotLevel - 1) * BASE_PROC_INCREASE, 0.99f);
    }

    private int GetLightningDamage()
    {
        return lightningBaseDamage + (lightningLevel - 1);
    }
    #endregion

    #region Slash Effect
    private void TriggerSlashEffect(Transform enemyTransform, Transform attackSource)
    {
        if (slashPrefab == null) return;

        Vector2 direction = (enemyTransform.position - attackSource.position).normalized;
        int slashCount = slashLevel; // Level 1 = 1 slash, Level 5 = 5 slashes

        for (int i = 0; i < slashCount; i++)
        {
            // Tạo một chút độ trễ giữa các slash
            StartCoroutine(SpawnSlashDelayed(direction, attackSource, i * 0.05f));
        }
    }

    private IEnumerator SpawnSlashDelayed(Vector2 direction, Transform attackSource, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject slash = Instantiate(slashPrefab, attackSource.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        slash.transform.rotation = Quaternion.Euler(0, 0, angle);

        Rigidbody2D slashRb = slash.GetComponent<Rigidbody2D>();
        if (slashRb != null)
        {
            slashRb.velocity = direction * slashSpeed;
        }

        Destroy(slash, 2f);
    }
    #endregion

    #region Lightning Effect
    private void TriggerLightningEffect(Transform enemyTransform)
    {
        if (lightningWarningPrefab == null || lightningStrikePrefab == null) return;

        int strikeCount = lightningLevel; // Level 1 = 1 strike, Level 5 = 5 strikes

        for (int i = 0; i < strikeCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * lightningRadius;
            Vector3 strikePosition = enemyTransform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            StartCoroutine(LightningStrikeSequence(strikePosition));
        }
    }

    private IEnumerator LightningStrikeSequence(Vector3 position)
    {
        GameObject warning = Instantiate(lightningWarningPrefab, position, Quaternion.identity);
        yield return new WaitForSeconds(warningDuration);
        Destroy(warning);

        Vector3 lightningSpawnPos = position + new Vector3(0, lightningSpawnHeight, 0);
        GameObject lightning = Instantiate(lightningStrikePrefab, lightningSpawnPos, Quaternion.identity);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, 1f);
        foreach (Collider2D col in hitEnemies)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(GetLightningDamage(), lightning.transform);
            }
        }

        Destroy(lightning, 1f);
    }
    #endregion

    #region Triple Shot Effect
    private void TriggerTripleShotEffect(Transform attackSource)
    {
        if (tripleShotPrefab == null) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(attackSource.position);
        Vector2 direction = new Vector2(mousePos.x - playerScreenPoint.x, mousePos.y - playerScreenPoint.y).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Level 1 = 3 projectiles
        // Level 2 = 4 projectiles, Level 3 = 5 projectiles, etc.
        int projectileCount = tripleShotLevel + 2;

        for (int i = 0; i < projectileCount; i++)
        {
            int offset = i - tripleShotLevel;
            float currentAngle = baseAngle + (offset * spreadAngle);
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 shootDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            GameObject projectile = Instantiate(tripleShotPrefab, attackSource.position, Quaternion.Euler(0, 0, currentAngle));
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = shootDirection * tripleShotSpeed;
            }

            Destroy(projectile, 3f);
        }
    }
    #endregion

    #region Buff Management
    public void AddSlashBuff()
    {
        if (slashLevel < MAX_BUFF_LEVEL)
        {
            slashLevel++;
            Debug.Log($"Slash Buff Level: {slashLevel} (Proc Chance: {GetSlashProcChance():P0})");
        }
        else
        {
            Debug.Log("Slash Buff is already at max level!");
        }
    }

    public void AddLightningBuff()
    {
        if (lightningLevel < MAX_BUFF_LEVEL)
        {
            lightningLevel++;
            Debug.Log($"Lightning Buff Level: {lightningLevel} (Proc Chance: {GetLightningProcChance():P0})");
        }
        else
        {
            Debug.Log("Lightning Buff is already at max level!");
        }
    }

    public void AddTripleShotBuff()
    {
        if (tripleShotLevel < MAX_BUFF_LEVEL)
        {
            tripleShotLevel++;
            Debug.Log($"Triple Shot Buff Level: {tripleShotLevel} (Proc Chance: {GetTripleShotProcChance():P0})");
        }
        else
        {
            Debug.Log("Triple Shot Buff is already at max level!");
        }
    }

    public int GetSlashLevel() => slashLevel;
    public int GetLightningLevel() => lightningLevel;
    public int GetTripleShotLevel() => tripleShotLevel;
    #endregion
}