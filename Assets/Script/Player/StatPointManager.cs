using UnityEngine;
using System;

public class StatPointManager : MonoBehaviour
{
    public static StatPointManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int statPointsPerLevel = 1; // Mỗi level nhận bao nhiêu điểm

    private int availableStatPoints = 0;

    // Events
    public event Action<int> OnStatPointsChanged; // (new amount)

    private PlayerExperience playerExperience;

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
        playerExperience = GetComponent<PlayerExperience>();

        if (playerExperience != null)
        {
            // Đăng ký event lên cấp
            playerExperience.OnLevelUp += OnPlayerLevelUp;
        }
    }

    private void OnDestroy()
    {
        if (playerExperience != null)
        {
            playerExperience.OnLevelUp -= OnPlayerLevelUp;
        }
    }

    private void OnPlayerLevelUp(int newLevel)
    {
        // Thêm stat points khi lên cấp
        AddStatPoints(statPointsPerLevel);
        Debug.Log($"Level Up! Received {statPointsPerLevel} stat point(s). Total: {availableStatPoints}");
    }

    public void AddStatPoints(int amount)
    {
        availableStatPoints += amount;
        OnStatPointsChanged?.Invoke(availableStatPoints);
    }

    public bool SpendStatPoint(string statName)
    {
        if (availableStatPoints <= 0)
        {
            Debug.LogWarning("No stat points available!");
            return false;
        }

        // Trừ điểm
        availableStatPoints--;
        
        // Cộng vào stat
        PlayerStats stats = PlayerStats.Instance;
        if (stats != null)
        {
            stats.AllocateStatPoint(statName);
        }

        OnStatPointsChanged?.Invoke(availableStatPoints);
        Debug.Log($"Spent 1 point on {statName}. Remaining: {availableStatPoints}");
        
        return true;
    }

    public int GetAvailablePoints() => availableStatPoints;

    public bool HasAvailablePoints() => availableStatPoints > 0;

    // Save/Load support
    public void SetStatPoints(int points)
    {
        availableStatPoints = points;
        OnStatPointsChanged?.Invoke(availableStatPoints);
    }

    // Debug: Test thêm điểm
    [ContextMenu("Add 5 Stat Points")]
    private void TestAddPoints()
    {
        AddStatPoints(5);
    }
}

