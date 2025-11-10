using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public Image healthBarFill; // HealthBar_Fill
    public TextMeshProUGUI healthText; // Text hiển thị máu (Current/Max)
    private PlayerHealth playerHealth; // Reference tới PlayerHealth script

    [Header("Animation Settings")]
    public float smoothSpeed = 5f; // Tốc độ giảm dần (càng cao càng nhanh)
    public bool useAnimation = true; // Bật/tắt hiệu ứng

    private float targetFillAmount = 1f; // Giá trị mục tiêu
    private float currentFillAmount = 1f; // Giá trị hiện tại

    private void Start()
    {
        // Tìm PlayerHealth component
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth script not found in scene!");
            return;
        }

        if (healthBarFill == null)
        {
            healthBarFill = GetComponent<Image>();
        }

        // Khởi tạo health bar
        UpdateHealthBar();
    }

    private void Update()
    {
        // Cập nhật health bar từ PlayerHealth
        UpdateHealthBar();

        // Nếu bật animation thì dùng Lerp để giảm dần mượt mà
        if (useAnimation)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            healthBarFill.fillAmount = currentFillAmount;
        }
        else
        {
            // Nếu không dùng animation thì cập nhật trực tiếp
            healthBarFill.fillAmount = targetFillAmount;
        }
    }

    private void UpdateHealthBar()
    {
        if (playerHealth == null) return;

        // Tính toán tỷ lệ HP hiện tại
        float currentHealth = playerHealth.GetCurrentHealth();
        float maxHealth = playerHealth.GetMaxHealth();

        targetFillAmount = currentHealth / maxHealth;
        targetFillAmount = Mathf.Clamp01(targetFillAmount); // Đảm bảo giá trị từ 0 đến 1

        // Cập nhật text hiển thị máu
        if (healthText != null)
        {
            healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";
        }
    }

    // Hàm công khai để có thể gọi từ nơi khác nếu cần
    public void RefreshHealthBar()
    {
        UpdateHealthBar();
    }

    public void SetHealthImmediate(float current, float max)
    {
        targetFillAmount = Mathf.Clamp01(current / max);
        currentFillAmount = targetFillAmount;

        if (healthBarFill != null)
            healthBarFill.fillAmount = currentFillAmount;

        if (healthText != null)
            healthText.text = $"{(int)current}/{(int)max}";
    }

}