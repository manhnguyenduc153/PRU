using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image expBarFill; // Image Fill cho thanh XP
    [SerializeField] private TextMeshProUGUI levelText; // Text hiển thị Level
    [SerializeField] private TextMeshProUGUI expText; // Text hiển thị XP (Current/Required)

    [Header("Animation Settings")]
    [SerializeField] private float smoothSpeed = 5f; // Tốc độ animation
    [SerializeField] private bool useAnimation = true;

    [Header("Colors (Optional)")]
    [SerializeField] private bool useColorGradient = false;
    [SerializeField] private Color lowExpColor = Color.red;
    [SerializeField] private Color highExpColor = Color.green;

    private PlayerExperience playerExperience;
    private float targetFillAmount = 0f;
    private float currentFillAmount = 0f;

    private void Start()
    {
        // Tìm PlayerExperience
        playerExperience = FindObjectOfType<PlayerExperience>();

        if (playerExperience == null)
        {
            Debug.LogError("PlayerExperience script not found in scene!");
            return;
        }

        // Đăng ký events
        playerExperience.OnExperienceChanged += UpdateExperienceBar;
        playerExperience.OnLevelUp += OnPlayerLevelUp;

        // Khởi tạo UI
        UpdateExperienceBar(playerExperience.GetCurrentExperience(), playerExperience.GetExperienceRequired());
    }

    private void OnDestroy()
    {
        // Hủy đăng ký events
        if (playerExperience != null)
        {
            playerExperience.OnExperienceChanged -= UpdateExperienceBar;
            playerExperience.OnLevelUp -= OnPlayerLevelUp;
        }
    }

    private void Update()
    {
        if (expBarFill == null) return;

        // Animation mượt mà
        if (useAnimation)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            expBarFill.fillAmount = currentFillAmount;
        }
        else
        {
            expBarFill.fillAmount = targetFillAmount;
        }

        // Gradient color (optional)
        if (useColorGradient && expBarFill != null)
        {
            expBarFill.color = Color.Lerp(lowExpColor, highExpColor, currentFillAmount);
        }
    }

    private void UpdateExperienceBar(int currentExp, int requiredExp)
    {
        // Cập nhật fill amount
        targetFillAmount = (float)currentExp / requiredExp;
        targetFillAmount = Mathf.Clamp01(targetFillAmount);

        // Cập nhật text
        if (expText != null)
        {
            expText.text = $"{currentExp}/{requiredExp}";
        }

        if (levelText != null)
        {
            levelText.text = $"Level {playerExperience.GetCurrentLevel()}";
        }
    }

    private void OnPlayerLevelUp(int newLevel)
    {
        // Reset fill về 0 khi lên cấp (XP thừa sẽ tự động fill lại)
        currentFillAmount = 0f;
        
        Debug.Log($"UI: Player reached level {newLevel}!");
    }

    // Public method để force update
    public void RefreshExperienceBar()
    {
        if (playerExperience != null)
        {
            UpdateExperienceBar(playerExperience.GetCurrentExperience(), playerExperience.GetExperienceRequired());
        }
    }

    public void SetExperienceImmediate(int current, int required, int level)
    {
        targetFillAmount = Mathf.Clamp01((float)current / required);
        currentFillAmount = targetFillAmount;

        if (expBarFill != null)
            expBarFill.fillAmount = currentFillAmount;

        if (expText != null)
            expText.text = $"{current}/{required}";

        if (levelText != null)
            levelText.text = $"Level {level}";
    }
}

