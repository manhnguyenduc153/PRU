using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    // Buff 1: Slash on hit
    public bool hasSlashBuff = false;
    [SerializeField] private float slashProcChance = 0.3f; // 30% chance
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private float slashSpeed = 10f;

    // Buff 2: Lightning strike
    public bool hasLightningBuff = false;
    [SerializeField] private float lightningProcChance = 0.25f; // 25% chance
    [SerializeField] private GameObject lightningWarningPrefab;
    [SerializeField] private GameObject lightningStrikePrefab;
    [SerializeField] private float lightningRadius = 3f;
    [SerializeField] private float warningDuration = 0.5f;
    [SerializeField] private int lightningDamage = 2;
    [SerializeField] private float lightningSpawnHeight = 3f;

    // Buff 3: Triple shot
    public bool hasTripleShotBuff = false;
    [SerializeField] private float tripleShotProcChance = 0.3f; // 30% chance
    [SerializeField] private GameObject tripleShotPrefab;
    [SerializeField] private float tripleShotSpeed = 8f;
    [SerializeField] private float spreadAngle = 20f; // Góc giữa các projectile

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
        // Tạo danh sách các buff có thể kích hoạt
        List<System.Action> availableBuffs = new List<System.Action>();

        // Thêm các buff đã unlock vào danh sách
        if (hasSlashBuff && Random.value <= slashProcChance)
        {
            availableBuffs.Add(() => TriggerSlashEffect(enemyTransform, attackSource));
        }

        if (hasLightningBuff && Random.value <= lightningProcChance)
        {
            availableBuffs.Add(() => TriggerLightningEffect(enemyTransform));
        }

        if (hasTripleShotBuff && Random.value <= tripleShotProcChance)
        {
            availableBuffs.Add(() => TriggerTripleShotEffect(attackSource));
        }

        // Nếu có ít nhất 1 buff proc, chọn ngẫu nhiên 1 buff để kích hoạt
        if (availableBuffs.Count > 0)
        {
            int randomIndex = Random.Range(0, availableBuffs.Count);
            availableBuffs[randomIndex].Invoke();
        }
    }

    private void TriggerSlashEffect(Transform enemyTransform, Transform attackSource)
    {
        if (slashPrefab == null) return;

        // Tính direction từ player đến enemy
        Vector2 direction = (enemyTransform.position - attackSource.position).normalized;

        // Spawn slash tại vị trí player
        GameObject slash = Instantiate(slashPrefab, attackSource.position, Quaternion.identity);

        // Tính góc rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        slash.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Add velocity nếu slash có Rigidbody2D
        Rigidbody2D slashRb = slash.GetComponent<Rigidbody2D>();
        if (slashRb != null)
        {
            slashRb.velocity = direction * slashSpeed;
        }

        // Destroy sau 2 giây
        Destroy(slash, 2f);
    }

    private void TriggerLightningEffect(Transform enemyTransform)
    {
        if (lightningWarningPrefab == null || lightningStrikePrefab == null) return;

        // Random vị trí xung quanh enemy
        Vector2 randomOffset = Random.insideUnitCircle * lightningRadius;
        Vector3 strikePosition = enemyTransform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        StartCoroutine(LightningStrikeSequence(strikePosition));
    }

    private IEnumerator LightningStrikeSequence(Vector3 position)
    {
        // Hiện warning ở vị trí đất
        GameObject warning = Instantiate(lightningWarningPrefab, position, Quaternion.identity);

        // Đợi warning duration
        yield return new WaitForSeconds(warningDuration);

        // Destroy warning
        Destroy(warning);

        // Tính vị trí spawn lightning cao hơn
        Vector3 lightningSpawnPos = position + new Vector3(0, lightningSpawnHeight, 0);

        // Spawn lightning strike ở vị trí cao hơn
        GameObject lightning = Instantiate(lightningStrikePrefab, lightningSpawnPos, Quaternion.identity);

        // Check va chạm với enemies trong vùng warning (vị trí đất)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, 1f);
        foreach (Collider2D col in hitEnemies)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(lightningDamage, lightning.transform);
            }
        }

        // Destroy lightning sau 1 giây
        Destroy(lightning, 1f);
    }

    private void TriggerTripleShotEffect(Transform attackSource)
    {
        if (tripleShotPrefab == null) return;

        // Lấy hướng player đang nhìn (từ mouse position hoặc facing direction)
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(attackSource.position);
        Vector2 direction = new Vector2(mousePos.x - playerScreenPoint.x, mousePos.y - playerScreenPoint.y).normalized;

        // Tính góc chính
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Bắn 3 projectile với góc khác nhau
        for (int i = -1; i <= 1; i++)
        {
            float currentAngle = baseAngle + (i * spreadAngle);
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector2 shootDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // Spawn projectile
            GameObject projectile = Instantiate(tripleShotPrefab, attackSource.position, Quaternion.Euler(0, 0, currentAngle));

            // Add velocity
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = shootDirection * tripleShotSpeed;
            }

            // Destroy sau 3 giây
            Destroy(projectile, 3f);
        }
    }

    // Methods để thêm buff
    public void AddSlashBuff()
    {
        hasSlashBuff = true;
        Debug.Log("Slash Buff activated!");
    }

    public void AddLightningBuff()
    {
        hasLightningBuff = true;
        Debug.Log("Lightning Buff activated!");
    }

    public void AddTripleShotBuff()
    {
        hasTripleShotBuff = true;
        Debug.Log("Triple Shot Buff activated!");
    }
}