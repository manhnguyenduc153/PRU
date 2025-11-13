using UnityEngine;
using System;

public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int baseExperienceRequired = 100; // XP cần cho level 2
    [SerializeField] private float experienceMultiplier = 1.5f; // Tăng XP mỗi level

    [Header("Level Up Rewards")]
    [SerializeField] private int healthIncreasePerLevel = 50;
    [SerializeField] private int manaIncreasePerLevel = 20;

    [Header("Audio")]
    [SerializeField] private AudioClip levelUpSound;

    // Events
    public event Action<int> OnLevelUp; // Event khi lên cấp
    public event Action<int, int> OnExperienceChanged; // Event khi XP thay đổi (current, required)

    private PlayerHealth playerHealth;
    private PlayerMana playerMana;
    private AudioSource audioSource;
    private bool isFirstLoad = true; // Flag để tránh trigger effect lần đầu

    private void Awake()
    {
        // Singleton
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Trigger initial event để UI cập nhật
        OnExperienceChanged?.Invoke(currentExperience, GetExperienceRequired());
        
        // Sau khi khởi tạo xong, cho phép level up effect
        Invoke(nameof(EnableLevelUpEffects), 0.1f);
    }
    
    private void EnableLevelUpEffects()
    {
        isFirstLoad = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // Thêm kinh nghiệm
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        currentExperience += amount;
        Debug.Log($"Gained {amount} XP! Current: {currentExperience}/{GetExperienceRequired()}");

        // Trigger event
        OnExperienceChanged?.Invoke(currentExperience, GetExperienceRequired());

        // Kiểm tra lên cấp
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        int requiredXP = GetExperienceRequired();

        while (currentExperience >= requiredXP)
        {
            LevelUp();
            requiredXP = GetExperienceRequired();
        }
    }

    private void LevelUp()
    {
        // Trừ XP thừa
        currentExperience -= GetExperienceRequired();
        currentLevel++;

        Debug.Log($"🎉 LEVEL UP! New Level: {currentLevel}");

        // Tăng stats (CHỈ TĂNG MAX, KHÔNG HỒI ĐẦY)
        if (playerHealth != null)
        {
            int currentHealth = playerHealth.GetCurrentHealth();
            int newMaxHealth = playerHealth.GetMaxHealth() + healthIncreasePerLevel;
            playerHealth.SetHealth(currentHealth, newMaxHealth); // Giữ nguyên máu hiện tại
            Debug.Log($"Max Health increased to: {newMaxHealth}");
        }

        if (playerMana != null)
        {
            int currentMana = playerMana.GetCurrentMana();
            int newMaxMana = playerMana.GetMaxMana() + manaIncreasePerLevel;
            playerMana.SetMana(currentMana, newMaxMana); // Giữ nguyên mana hiện tại
            Debug.Log($"Max Mana increased to: {newMaxMana}");
        }

        // Play sound
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        // Trigger event (CHỈ KHI KHÔNG PHẢI LOAD LẦN ĐẦU)
        if (!isFirstLoad)
        {
            OnLevelUp?.Invoke(currentLevel);
        }
        OnExperienceChanged?.Invoke(currentExperience, GetExperienceRequired());

        // Spawn VFX (sẽ được LevelUpEffect handle)
    }

    // Tính XP cần thiết cho level tiếp theo
    public int GetExperienceRequired()
    {
        return Mathf.RoundToInt(baseExperienceRequired * Mathf.Pow(experienceMultiplier, currentLevel - 1));
    }

    // Getters
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentExperience() => currentExperience;
    public float GetExperienceProgress() => (float)currentExperience / GetExperienceRequired();

    // Setter cho Save System
    public void SetLevelAndExperience(int level, int experience)
    {
        currentLevel = level;
        currentExperience = experience;
        OnExperienceChanged?.Invoke(currentExperience, GetExperienceRequired());
        Debug.Log($"Level & XP loaded: Level {currentLevel}, XP {currentExperience}/{GetExperienceRequired()}");
    }

    // Debug: Test thêm XP
    [ContextMenu("Add 50 XP")]
    private void TestAddXP()
    {
        AddExperience(50);
    }

    [ContextMenu("Add 500 XP")]
    private void TestAddLargeXP()
    {
        AddExperience(500);
    }
}

