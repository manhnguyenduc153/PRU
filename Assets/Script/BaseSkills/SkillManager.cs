using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public List<SkillData> skills = new List<SkillData>();

    [Header("Range Settings")]
    public float maxSkillRange = 5f;
    public Color inRangeColor = Color.green;
    public Color outRangeColor = Color.red;
    public float indicatorSize = 0.5f;

    [Header("Player Reference")]
    public Transform player;
    public PlayerMana playerMana; // THÊM: Tham chiếu đến PlayerMana

    private Dictionary<KeyCode, float> cooldowns = new Dictionary<KeyCode, float>();
    private Vector2 mouseWorldPos;
    private bool isInRange;

    // THÊM: Biến kiểm soát cast time
    private bool isCasting = false;
    private float castTimeRemaining = 0f;

    //void Start()
    //{
    //    if (player == null)
    //    {
    //        player = transform;
    //    }

    //    // THÊM: Tự động lấy PlayerMana nếu chưa được gán
    //    if (playerMana == null)
    //    {
    //        playerMana = GetComponent<PlayerMana>();
    //    }

    //    foreach (var skill in skills)
    //    {
    //        cooldowns[skill.keyBinding] = 0f;
    //    }
    //}

    void Start()
    {
        if (player == null)
            player = transform;

        if (playerMana == null)
            playerMana = GetComponent<PlayerMana>();

        // Khởi tạo cooldowns
        foreach (var skill in skills)
        {
            cooldowns[skill.keyBinding] = 0f;

            // Gán requiredLevel dựa theo skillName
            switch (skill.skillName)
            {
                case "Molten_Spear":
                    skill.requiredLevel = 3;
                    break;
                case "Water_Geyser":
                    skill.requiredLevel = 5;
                    break;
                case "Portal":
                    skill.requiredLevel = 10;
                    break;
                case "Tornado":
                    skill.requiredLevel = 15;
                    break;
                case "Earth_Spike":
                    skill.requiredLevel = 20;
                    break;
                default:
                    skill.requiredLevel = 1;
                    break;
            }
        }
    }

    void Update()
    {
        UpdateMousePosition();
        UpdateCooldowns();
        UpdateCastTime();
        CheckSkillInput();
    }

    void UpdateMousePosition()
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(player.position, mouseWorldPos);
        isInRange = distance <= maxSkillRange;
    }

    void CheckSkillInput()
    {
        // Không cho nhấn skill khi đang cast
        if (isCasting)
        {
            return;
        }

        foreach (var skill in skills)
        {
            if (Input.GetKeyDown(skill.keyBinding))
            {
                TryCastSkill(skill);
            }
        }

        if (Input.GetMouseButtonDown(1) && skills.Count > 0)
        {
            TryCastSkill(skills[0]);
        }
    }

    void TryCastSkill(SkillData skill)
    {
        if (PlayerExperience.Instance != null &&
        PlayerExperience.Instance.GetCurrentLevel() < skill.requiredLevel)
        {
            Debug.Log($"❌ {skill.skillName} cần Level {skill.requiredLevel} mới dùng được! Hiện tại: Level {PlayerExperience.Instance.GetCurrentLevel()}");
            return;
        }

        // Kiểm tra đang cast
        if (isCasting)
        {
            Debug.Log("⏳ Đang cast skill khác!");
            return;
        }

        // Kiểm tra cooldown
        if (cooldowns[skill.keyBinding] > 0)
        {
            Debug.Log($"{skill.skillName} đang cooldown! Còn {cooldowns[skill.keyBinding]:F1}s");
            return;
        }

        // THÊM: Kiểm tra mana
        if (playerMana != null && !playerMana.HasEnoughMana(skill.manaCost))
        {
            Debug.Log($"❌ Không đủ mana! Cần {skill.manaCost}, hiện có {playerMana.GetCurrentMana()}");
            return;
        }

        // Kiểm tra trong tầm
        if (!isInRange)
        {
            Debug.Log($"{skill.skillName} ngoài tầm! Khoảng cách tối đa: {maxSkillRange}m");
            ShowOutOfRangeEffect();
            return;
        }

        // Bắt đầu cast skill
        StartCoroutine(CastSkillWithDelay(skill));
    }

    IEnumerator CastSkillWithDelay(SkillData skill)
    {
        // Khóa cast (không thể dùng skill khác)
        isCasting = true;
        castTimeRemaining = skill.castTime;

        Debug.Log($"⏳ Bắt đầu cast {skill.skillName}...");

        // Chờ castDelay trước khi spawn skill
        yield return new WaitForSeconds(skill.castDelay);

        // Spawn skill
        CastSkill(skill);

        // Chờ hết castTime
        yield return new WaitForSeconds(skill.castTime - skill.castDelay);

        // Mở khóa
        isCasting = false;
        castTimeRemaining = 0f;

        Debug.Log($"✅ Hoàn thành cast {skill.skillName}!");
    }

    void CastSkill(SkillData skill)
    {
        // THÊM: Trừ mana khi cast skill
        if (playerMana != null)
        {
            playerMana.UseMana(skill.manaCost);
        }

        GameObject skillObj = Instantiate(skill.skillPrefab, mouseWorldPos, Quaternion.identity);
        Vector2 direction = (mouseWorldPos - (Vector2)player.position).normalized;

        SkillEffect skillEffect = skillObj.GetComponent<SkillEffect>();
        if (skillEffect != null)
        {
            skillEffect.SetDirection(direction);
            skillEffect.damage = skill.damage;
            skillEffect.SetSpeed(skill.skillSpeed);
            skillEffect.SetAnimationSpeed(skill.animationSpeed);
        }

        // Bắt đầu cooldown
        cooldowns[skill.keyBinding] = skill.cooldownTime;

        Debug.Log($"🔥 Cast {skill.skillName} tại vị trí chuột!");
    }

    void UpdateCooldowns()
    {
        List<KeyCode> keys = new List<KeyCode>(cooldowns.Keys);
        foreach (var key in keys)
        {
            if (cooldowns[key] > 0)
            {
                cooldowns[key] -= Time.deltaTime;
                if (cooldowns[key] < 0) cooldowns[key] = 0;
            }
        }
    }

    void UpdateCastTime()
    {
        if (castTimeRemaining > 0)
        {
            castTimeRemaining -= Time.deltaTime;
            if (castTimeRemaining < 0) castTimeRemaining = 0;
        }
    }

    void ShowOutOfRangeEffect()
    {
        Debug.Log("⚠️ Ngoài tầm cast skill!");
    }

    void OnDrawGizmos()
    {
        if (player == null) return;

        // Vẽ vòng tròn phạm vi
        Gizmos.color = Color.cyan;
        DrawCircle(player.position, maxSkillRange, 50);

        if (Application.isPlaying)
        {
            // Vẽ màu đỏ khi ngoài tầm
            Gizmos.color = isInRange ? inRangeColor : outRangeColor;
            Gizmos.DrawWireSphere(mouseWorldPos, indicatorSize);
            Gizmos.DrawLine(player.position, mouseWorldPos);
        }
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(radius, 0, 0);

        for (int i = 0; i <= segments; i++)
        {
            angle += 2 * Mathf.PI / segments;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }

    // API cho UI
    public float GetCooldown(KeyCode key)
    {
        return cooldowns.ContainsKey(key) ? cooldowns[key] : 0f;
    }

    public bool IsSkillReady(KeyCode key)
    {
        return cooldowns.ContainsKey(key) && cooldowns[key] <= 0 && !isCasting;
    }

    public bool IsCasting()
    {
        return isCasting;
    }

    public float GetCastTimeRemaining()
    {
        return castTimeRemaining;
    }
}