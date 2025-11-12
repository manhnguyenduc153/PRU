using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "RPG/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public float cooldownTime = 5f;
    public GameObject skillPrefab; // Prefab của skill effect
    public float damage = 10f;
    public float range = 5f;
    public KeyCode keyBinding;
}