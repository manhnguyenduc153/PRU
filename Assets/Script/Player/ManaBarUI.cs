using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    [Header("References")]
    public Image manaBarFill; // ManaBar_Fill
    public TextMeshProUGUI manaText; // Text hiển thị mana (Current/Max)
    private PlayerMana playerMana; // Reference tới PlayerMana script

    [Header("Animation Settings")]
    public float smoothSpeed = 5f; // Tốc độ giảm dần
    public bool useAnimation = true; // Bật/tắt hiệu ứng

    private float targetFillAmount = 1f; // Giá trị mục tiêu
    private float currentFillAmount = 1f; // Giá trị hiện tại

    private void Start()
    {
        // Tìm PlayerMana component
        playerMana = FindObjectOfType<PlayerMana>();

        if (playerMana == null)
        {
            Debug.LogError("PlayerMana script not found in scene!");
            return;
        }

        if (manaBarFill == null)
        {
            manaBarFill = GetComponent<Image>();
        }

        // Khởi tạo mana bar
        UpdateManaBar();
    }

    private void Update()
    {
        // Cập nhật mana bar từ PlayerMana
        UpdateManaBar();

        // Nếu bật animation thì dùng Lerp để giảm dần mượt mà
        if (useAnimation)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            manaBarFill.fillAmount = currentFillAmount;
        }
        else
        {
            // Nếu không dùng animation thì cập nhật trực tiếp
            manaBarFill.fillAmount = targetFillAmount;
        }
    }

    private void UpdateManaBar()
    {
        if (playerMana == null) return;

        // Tính toán tỷ lệ Mana hiện tại
        float currentMana = playerMana.GetCurrentMana();
        float maxMana = playerMana.GetMaxMana();

        targetFillAmount = currentMana / maxMana;
        targetFillAmount = Mathf.Clamp01(targetFillAmount); // Đảm bảo giá trị từ 0 đến 1

        // Cập nhật text hiển thị mana
        if (manaText != null)
        {
            manaText.text = $"{(int)currentMana}/{(int)maxMana}";
        }
    }

    // Hàm công khai để có thể gọi từ nơi khác nếu cần
    public void RefreshManaBar()
    {
        UpdateManaBar();
    }
}