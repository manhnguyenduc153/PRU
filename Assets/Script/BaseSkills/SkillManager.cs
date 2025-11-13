//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class SkillManager : MonoBehaviour
//{
//    [Header("Skill Settings")]
//    public List<SkillData> skills = new List<SkillData>();

//    [Header("Range Settings")]
//    public float maxSkillRange = 5f;
//    public Color inRangeColor = Color.green;
//    public Color outRangeColor = Color.red;
//    public float indicatorSize = 0.5f;

//    [Header("Spawn Settings")]
//    public float spawnDistance = 1.5f;

//    [Header("Player Reference")]
//    public Transform player;

//    private Dictionary<KeyCode, float> cooldowns = new Dictionary<KeyCode, float>();
//    private Vector2 mouseWorldPos;
//    private bool isInRange;
//    private Camera mainCamera;
//    private bool isAnyCasting = false;
//    private PlayerMana playerMana;

//    void Start()
//    {
//        if (player == null)
//        {
//            player = transform;
//        }

//        mainCamera = Camera.main;
//        playerMana = FindObjectOfType<PlayerMana>();

//        foreach (var skill in skills)
//        {
//            cooldowns[skill.keyBinding] = 0f;
//        }
//    }

//    void Update()
//    {
//        UpdateMousePosition();
//        UpdateCooldowns();
//        CheckSkillInput();
//    }

//    void UpdateMousePosition()
//    {
//        Vector3 screenPos = Input.mousePosition;
//        screenPos.z = 10f;

//        mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos);

//        float distance = Vector2.Distance((Vector2)player.position, mouseWorldPos);
//        isInRange = distance <= maxSkillRange;
//    }

//    void CheckSkillInput()
//    {
//        // Check từng skill bằng key binding
//        foreach (var skill in skills)
//        {
//            if (Input.GetKeyDown(skill.keyBinding) && !isAnyCasting)
//            {
//                TryCastSkill(skill);
//            }
//        }

//        // Right click để cast skill đầu tiên
//        if (Input.GetMouseButtonDown(1) && skills.Count > 0 && !isAnyCasting)
//        {
//            TryCastSkill(skills[0]);
//        }
//    }

//    void TryCastSkill(SkillData skill)
//    {
//        // Check cooldown
//        if (cooldowns[skill.keyBinding] > 0)
//        {
//            Debug.Log($"❌ {skill.skillName} đang cooldown! Còn {cooldowns[skill.keyBinding]:F1}s");
//            return;
//        }

//        // Check range
//        if (!isInRange)
//        {
//            float dist = Vector2.Distance((Vector2)player.position, mouseWorldPos);
//            Debug.Log($"❌ {skill.skillName} ngoài tầm! Khoảng cách: {dist:F1}m (Max: {maxSkillRange}m)");
//            return;
//        }

//        // Check mana
//        if (playerMana != null && !playerMana.HasEnoughMana(skill.manaCost))
//        {
//            Debug.Log($"❌ Không đủ mana! Cần {skill.manaCost}, hiện có {playerMana.currentMana}");
//            return;
//        }

//        // Start casting
//        StartCoroutine(CastSkillCoroutine(skill));
//    }

//    IEnumerator CastSkillCoroutine(SkillData skill)
//    {
//        isAnyCasting = true;

//        Debug.Log($"🔷 Bắt đầu cast {skill.skillName}...");

//        // Trừ mana ngay khi bắt đầu cast
//        if (playerMana != null)
//        {
//            playerMana.UseMana(skill.manaCost);
//        }

//        // Delay trước khi skill spawn
//        yield return new WaitForSeconds(skill.castDelay);

//        // Spawn skill
//        SpawnSkill(skill);

//        // Start cooldown
//        cooldowns[skill.keyBinding] = skill.cooldownTime;

//        Debug.Log($"✅ {skill.skillName} được bắn! Cooldown: {skill.cooldownTime}s");

//        // Cho phép dùng skill khác sau cast time
//        yield return new WaitForSeconds(skill.castTime - skill.castDelay);
//        isAnyCasting = false;
//    }

//    void SpawnSkill(SkillData skill)
//    {
//        // Tính hướng từ player tới chuột
//        Vector2 direction = (mouseWorldPos - (Vector2)player.position).normalized;

//        // Tính vị trí spawn (cách xa player theo hướng)
//        Vector3 spawnPos = (Vector2)player.position + direction * spawnDistance;

//        // Spawn skill prefab
//        GameObject skillObj = Instantiate(skill.skillPrefab, spawnPos, Quaternion.identity);

//        // Set thông tin cho SkillEffect
//        SkillEffect skillEffect = skillObj.GetComponent<SkillEffect>();
//        if (skillEffect != null)
//        {
//            skillEffect.SetDirection(direction);
//            skillEffect.damage = skill.damage;
//            skillEffect.SetSpeed(skill.skillSpeed);
//            skillEffect.SetAnimationSpeed(skill.animationSpeed);
//        }
//        else
//        {
//            Debug.LogWarning($"⚠️ {skill.skillPrefab.name} không có SkillEffect component!");
//        }
//    }

//    void UpdateCooldowns()
//    {
//        List<KeyCode> keys = new List<KeyCode>(cooldowns.Keys);
//        foreach (var key in keys)
//        {
//            if (cooldowns[key] > 0)
//            {
//                cooldowns[key] -= Time.deltaTime;
//                if (cooldowns[key] < 0) cooldowns[key] = 0;
//            }
//        }
//    }

//    void OnDrawGizmos()
//    {
//        if (player == null) return;

//        // Vẽ vòng tròn phạm vi
//        Gizmos.color = Color.cyan;
//        DrawCircle(player.position, maxSkillRange, 50);

//        if (Application.isPlaying)
//        {
//            // Vẽ indicator tại vị trí chuột
//            Gizmos.color = isInRange ? inRangeColor : outRangeColor;
//            Gizmos.DrawWireSphere(mouseWorldPos, indicatorSize);
//            Gizmos.DrawLine(player.position, mouseWorldPos);

//            // Vẽ vị trí spawn
//            Vector2 direction = (mouseWorldPos - (Vector2)player.position).normalized;
//            Vector3 spawnPos = (Vector2)player.position + direction * spawnDistance;
//            Gizmos.color = Color.yellow;
//            Gizmos.DrawWireSphere(spawnPos, 0.3f);
//        }
//    }

//    void DrawCircle(Vector3 center, float radius, int segments)
//    {
//        float angle = 0f;
//        Vector3 lastPoint = center + new Vector3(radius, 0, 0);

//        for (int i = 0; i <= segments; i++)
//        {
//            angle = (2 * Mathf.PI / segments) * i;
//            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
//            Gizmos.DrawLine(lastPoint, newPoint);
//            lastPoint = newPoint;
//        }
//    }

//    public float GetCooldown(KeyCode key)
//    {
//        return cooldowns.ContainsKey(key) ? cooldowns[key] : 0f;
//    }

//    public bool IsSkillReady(KeyCode key)
//    {
//        return cooldowns.ContainsKey(key) && cooldowns[key] <= 0;
//    }
//}