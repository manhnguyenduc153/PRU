using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    private const int MAX_BUFF_LEVEL = 5;
    private const float BASE_PROC_INCREASE = 0.05f; // Tăng 5% proc chance mỗi level

    #region Buff Levels
    [Header("Buff Levels")]
    [SerializeField] private int slashLevel = 0;
    [SerializeField] private int lightningLevel = 0;
    [SerializeField] private int tripleShotLevel = 0;
    [SerializeField] private int attackLevel = 0;
    [SerializeField] private int manaLevel = 0;
    [SerializeField] private int hpLevel = 0;
    #endregion

    #region Buff Settings
    [Header("Slash Buff")]
    [SerializeField] private float slashBaseProcChance = 0.3f;
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private float slashSpeed = 10f;

    [Header("Lightning Buff")]
    [SerializeField] private float lightningBaseProcChance = 0.25f;
    [SerializeField] private GameObject lightningWarningPrefab;
    [SerializeField] private GameObject lightningStrikePrefab;
    [SerializeField] private float lightningRadius = 3f;
    [SerializeField] private float warningDuration = 0.5f;
    [SerializeField] private int lightningBaseDamage = 2;
    [SerializeField] private float lightningSpawnHeight = 3f;

    [Header("Triple Shot Buff")]
    [SerializeField] private float tripleShotBaseProcChance = 0.3f;
    [SerializeField] private GameObject tripleShotPrefab;
    [SerializeField] private float tripleShotSpeed = 8f;
    [SerializeField] private float spreadAngle = 20f;

    [Header("Attack / Mana / HP Buff")]
    [SerializeField] private int attackPerLevel = 5;
    [SerializeField] private int manaPerLevel = 50;
    [SerializeField] private int hpPerLevel = 50;
    #endregion

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnValidate()
    {
        // Clamp levels
        slashLevel = Mathf.Clamp(slashLevel, 0, MAX_BUFF_LEVEL);
        lightningLevel = Mathf.Clamp(lightningLevel, 0, MAX_BUFF_LEVEL);
        tripleShotLevel = Mathf.Clamp(tripleShotLevel, 0, MAX_BUFF_LEVEL);
        attackLevel = Mathf.Clamp(attackLevel, 0, MAX_BUFF_LEVEL);
        manaLevel = Mathf.Clamp(manaLevel, 0, MAX_BUFF_LEVEL);
        hpLevel = Mathf.Clamp(hpLevel, 0, MAX_BUFF_LEVEL);
    }

    #region Proc Chance Calculations
    private float GetSlashProcChance() => Mathf.Min(slashBaseProcChance + (slashLevel - 1) * BASE_PROC_INCREASE, 0.99f);
    private float GetLightningProcChance() => Mathf.Min(lightningBaseProcChance + (lightningLevel - 1) * BASE_PROC_INCREASE, 0.99f);
    private float GetTripleShotProcChance() => Mathf.Min(tripleShotBaseProcChance + (tripleShotLevel - 1) * BASE_PROC_INCREASE, 0.99f);
    private int GetLightningDamage() => lightningBaseDamage + (lightningLevel - 1);
    #endregion

    #region Buff Triggers
    public void OnEnemyHit(Transform enemyTransform, Transform attackSource)
    {
        List<System.Action> availableBuffs = new List<System.Action>();

        if (slashLevel > 0 && Random.value <= GetSlashProcChance())
            availableBuffs.Add(() => TriggerSlashEffect(enemyTransform, attackSource));

        if (lightningLevel > 0 && Random.value <= GetLightningProcChance())
            availableBuffs.Add(() => TriggerLightningEffect(enemyTransform));

        if (tripleShotLevel > 0 && Random.value <= GetTripleShotProcChance())
            availableBuffs.Add(() => TriggerTripleShotEffect(attackSource));

        if (availableBuffs.Count > 0)
        {
            int randomIndex = Random.Range(0, availableBuffs.Count);
            availableBuffs[randomIndex].Invoke();
        }
    }
    #endregion

    #region Slash Effect
    private void TriggerSlashEffect(Transform enemyTransform, Transform attackSource)
    {
        if (slashPrefab == null) return;

        Vector2 direction = (enemyTransform.position - attackSource.position).normalized;
        int slashCount = slashLevel;

        for (int i = 0; i < slashCount; i++)
            StartCoroutine(SpawnSlashDelayed(direction, attackSource, i * 0.05f));
    }

    private IEnumerator SpawnSlashDelayed(Vector2 direction, Transform attackSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject slash = Instantiate(slashPrefab, attackSource.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        slash.transform.rotation = Quaternion.Euler(0, 0, angle);
        Rigidbody2D slashRb = slash.GetComponent<Rigidbody2D>();
        if (slashRb != null)
            slashRb.velocity = direction * slashSpeed;
        Destroy(slash, 2f);
    }
    #endregion

    #region Lightning Effect
    private void TriggerLightningEffect(Transform enemyTransform)
    {
        if (lightningWarningPrefab == null || lightningStrikePrefab == null) return;

        int strikeCount = lightningLevel;
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
                enemyHealth.TakeDamage(GetLightningDamage(), lightning.transform);
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
        Vector2 direction = (mousePos - playerScreenPoint).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        int projectileCount = tripleShotLevel + 2;
        for (int i = 0; i < projectileCount; i++)
        {
            int offset = i - tripleShotLevel;
            float currentAngle = baseAngle + (offset * spreadAngle);
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 shootDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            GameObject projectile = Instantiate(tripleShotPrefab, attackSource.position, Quaternion.Euler(0, 0, currentAngle));
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = shootDirection * tripleShotSpeed;

            Destroy(projectile, 3f);
        }
    }
    #endregion

    #region Buff Management
    public void AddSlashBuff() { if (slashLevel < MAX_BUFF_LEVEL) slashLevel++; }
    public void AddLightningBuff() { if (lightningLevel < MAX_BUFF_LEVEL) lightningLevel++; }
    public void AddTripleShotBuff() { if (tripleShotLevel < MAX_BUFF_LEVEL) tripleShotLevel++; }

    public void AddAttackBuff() { if (attackLevel < MAX_BUFF_LEVEL) attackLevel++; }
    public void AddManaBuff()
    {
        if (manaLevel < MAX_BUFF_LEVEL)
        {
            manaLevel++;
            PlayerMana playerMana = FindObjectOfType<PlayerMana>();
            if (playerMana != null)
            {
                playerMana.maxMana += manaPerLevel;
                playerMana.RegenerateMana(manaPerLevel);
            }
        }
    }
    public void AddHpBuff()
    {
        if (hpLevel < MAX_BUFF_LEVEL)
        {
            hpLevel++;
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.maxHealth += hpPerLevel;
                playerHealth.Heal(hpPerLevel);
            }
        }
    }

    public int GetSlashLevel() => slashLevel;
    public int GetLightningLevel() => lightningLevel;
    public int GetTripleShotLevel() => tripleShotLevel;
    public int GetAttackLevel() => attackLevel;
    public int GetManaLevel() => manaLevel;
    public int GetHpLevel() => hpLevel;

    // Phương thức cho SaveSystem - Set tất cả buff levels
    public void SetBuffLevels(int slash, int lightning, int tripleShot, int attack, int mana, int hp)
    {
        slashLevel = Mathf.Clamp(slash, 0, MAX_BUFF_LEVEL);
        lightningLevel = Mathf.Clamp(lightning, 0, MAX_BUFF_LEVEL);
        tripleShotLevel = Mathf.Clamp(tripleShot, 0, MAX_BUFF_LEVEL);
        attackLevel = Mathf.Clamp(attack, 0, MAX_BUFF_LEVEL);
        manaLevel = Mathf.Clamp(mana, 0, MAX_BUFF_LEVEL);
        hpLevel = Mathf.Clamp(hp, 0, MAX_BUFF_LEVEL);

        Debug.Log($"[BuffManager] Buff levels set: Slash={slashLevel}, Lightning={lightningLevel}, TripleShot={tripleShotLevel}, Atk={attackLevel}, Mana={manaLevel}, HP={hpLevel}");
    }

    // Reset tất cả buffs (cho new game)
    public void ResetAllBuffs()
    {
        slashLevel = 0;
        lightningLevel = 0;
        tripleShotLevel = 0;
        attackLevel = 0;
        manaLevel = 0;
        hpLevel = 0;
        Debug.Log("[BuffManager] All buffs reset!");
    }
    #endregion
}
