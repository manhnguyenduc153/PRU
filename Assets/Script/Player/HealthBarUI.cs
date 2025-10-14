using UnityEngine;
using UnityEngine.UI;
using TMPro; // Nếu dùng TextMeshPro

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Để trống, script sẽ tự động tìm PlayerHealth")]
    public PlayerHealth playerHealth; // Tham chiếu đến script PlayerHealth
    public Slider healthSlider; // Thanh máu (UI Slider)
    public TextMeshProUGUI healthText; // Text hiển thị số (optional)

    [Header("Visual Settings")]
    public Image fillImage; // Image bên trong Slider (để đổi màu)
    public Color healthyColor = Color.green;
    public Color damagedColor = Color.yellow;
    public Color criticalColor = Color.red;

    [Header("Animation")]
    public bool useSmooth = true; // Thanh máu giảm mượt
    public float smoothSpeed = 5f;

    private float targetFillAmount;

    void Start()
    {
        // Tự động tìm PlayerHealth
        if (playerHealth == null)
        {
            // Thử tìm trên cùng GameObject trước
            playerHealth = GetComponent<PlayerHealth>();

            // Nếu không có, tìm trong toàn bộ scene
            if (playerHealth == null)
            {
                playerHealth = FindObjectOfType<PlayerHealth>();
            }

            // Kiểm tra có tìm thấy không
            if (playerHealth == null)
            {
                Debug.LogError("Không tìm thấy PlayerHealth trong scene!");
                return;
            }
            else
            {
                Debug.Log($"Đã tìm thấy PlayerHealth trên: {playerHealth.gameObject.name}");
            }
        }

        // Khởi tạo thanh máu đầy
        UpdateHealthBar();
    }

    void Update()
    {
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (playerHealth == null) return;

        // Tính tỷ lệ máu hiện tại
        float currentHealth = playerHealth.GetCurrentHealth();
        float maxHealth = playerHealth.GetMaxHealth();
        targetFillAmount = currentHealth / maxHealth;

        // Cập nhật Slider
        if (healthSlider != null)
        {
            if (useSmooth)
            {
                // Giảm máu mượt
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetFillAmount, Time.deltaTime * smoothSpeed);
            }
            else
            {
                // Giảm máu tức thì
                healthSlider.value = targetFillAmount;
            }
        }

        // Cập nhật Text (nếu có)
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        // Đổi màu thanh máu theo % HP
        if (fillImage != null)
        {
            if (targetFillAmount > 0.5f)
            {
                fillImage.color = healthyColor;
            }
            else if (targetFillAmount > 0.25f)
            {
                fillImage.color = damagedColor;
            }
            else
            {
                fillImage.color = criticalColor;
            }
        }
    }
}