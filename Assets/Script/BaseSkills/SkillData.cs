using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "RPG/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public Sprite skillIcon;

    [Header("Combat Settings")]
    public float damage = 10f;
    public float range = 5f;
    public int manaCost = 20;
    public KeyCode keyBinding = KeyCode.Q;

    [Header("Timing")]
    public float cooldownTime = 5f;      // Thời gian chờ giữa các lần dùng
    public float castTime = 0.5f;        // Thời gian khóa không dùng skill khác
    public float castDelay = 0.2f;       // Delay trước khi skill spawn ra

    [Header("Movement")]
    public GameObject skillPrefab;       // Prefab của skill effect
    public float skillSpeed = 5f;        // Tốc độ di chuyển của skill
    public float animationSpeed = 1f;    // Tốc độ animation
}