using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private int baseMaxMana = 100;
    [SerializeField] private int baseAttack = 10;
    [SerializeField] private int baseDefense = 5;
    [SerializeField] private float baseSpeed = 5f;

    [Header("Stat Points Per Level")]
    [SerializeField] private int healthPerPoint = 20; // +20 HP mỗi điểm
    [SerializeField] private int manaPerPoint = 15;   // +15 Mana mỗi điểm
    [SerializeField] private int attackPerPoint = 5;  // +5 Attack mỗi điểm
    [SerializeField] private int defensePerPoint = 3; // +3 Defense mỗi điểm
    [SerializeField] private float speedPerPoint = 0.2f; // +0.2 Speed mỗi điểm

    // Allocated points
    private int allocatedHealthPoints = 0;
    private int allocatedManaPoints = 0;
    private int allocatedAttackPoints = 0;
    private int allocatedDefensePoints = 0;
    private int allocatedSpeedPoints = 0;

    // Events
    public event Action OnStatsChanged;

    private PlayerHealth playerHealth;
    private PlayerMana playerMana;
    private PlayerController playerController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMana = GetComponent<PlayerMana>();
        playerController = GetComponent<PlayerController>();

        // Apply initial stats
        ApplyStats();
    }

    public void AllocateStatPoint(string statName)
    {
        switch (statName.ToLower())
        {
            case "health":
                allocatedHealthPoints++;
                break;
            case "mana":
                allocatedManaPoints++;
                break;
            case "attack":
                allocatedAttackPoints++;
                break;
            case "defense":
                allocatedDefensePoints++;
                break;
            case "speed":
                allocatedSpeedPoints++;
                break;
            default:
                Debug.LogError($"Unknown stat: {statName}");
                return;
        }

        ApplyStats();
        OnStatsChanged?.Invoke();
        
        Debug.Log($"Allocated point to {statName}! Total allocated: {GetTotalAllocatedPoints()}");
    }

    private void ApplyStats()
    {
        // Apply Health
        if (playerHealth != null)
        {
            int newMaxHealth = baseMaxHealth + (allocatedHealthPoints * healthPerPoint);
            int currentHealth = playerHealth.GetCurrentHealth();
            playerHealth.SetHealth(currentHealth, newMaxHealth);
        }

        // Apply Mana
        if (playerMana != null)
        {
            int newMaxMana = baseMaxMana + (allocatedManaPoints * manaPerPoint);
            int currentMana = playerMana.GetCurrentMana();
            playerMana.SetMana(currentMana, newMaxMana);
        }

        // Apply Speed
        if (playerController != null)
        {
            float newSpeed = baseSpeed + (allocatedSpeedPoints * speedPerPoint);
            playerController.SetMoveSpeed(newSpeed);
        }

        // Attack và Defense sẽ được dùng trong combat system
    }

    // Getters
    public int GetMaxHealth() => baseMaxHealth + (allocatedHealthPoints * healthPerPoint);
    public int GetMaxMana() => baseMaxMana + (allocatedManaPoints * manaPerPoint);
    public int GetAttack() => baseAttack + (allocatedAttackPoints * attackPerPoint);
    public int GetDefense() => baseDefense + (allocatedDefensePoints * defensePerPoint);
    public float GetSpeed() => baseSpeed + (allocatedSpeedPoints * speedPerPoint);

    public int GetAllocatedPoints(string statName)
    {
        switch (statName.ToLower())
        {
            case "health": return allocatedHealthPoints;
            case "mana": return allocatedManaPoints;
            case "attack": return allocatedAttackPoints;
            case "defense": return allocatedDefensePoints;
            case "speed": return allocatedSpeedPoints;
            default: return 0;
        }
    }

    public int GetTotalAllocatedPoints()
    {
        return allocatedHealthPoints + allocatedManaPoints + allocatedAttackPoints + 
               allocatedDefensePoints + allocatedSpeedPoints;
    }

    // Save/Load support
    public void SetAllocatedPoints(int health, int mana, int attack, int defense, int speed)
    {
        allocatedHealthPoints = health;
        allocatedManaPoints = mana;
        allocatedAttackPoints = attack;
        allocatedDefensePoints = defense;
        allocatedSpeedPoints = speed;
        ApplyStats();
        OnStatsChanged?.Invoke();
    }
}

