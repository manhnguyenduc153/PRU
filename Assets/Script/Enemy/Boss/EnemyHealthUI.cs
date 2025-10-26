using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bossHealthPanel;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image hudImage;
    [SerializeField] private TextMeshProUGUI bossNameText;

    [Header("Animation Settings")]
    [SerializeField] private float fillSpeed = 2f;
    [SerializeField] private Color healthColor = Color.red;

    private int maxHealth;
    private int currentHealth;
    private float targetFillAmount;

    private void Start()
    {
        if (healthBarFill != null)
        {
            healthBarFill.color = healthColor;
        }

        // Ẩn UI lúc bắt đầu
        //if (bossHealthPanel != null)
        //{
        //    bossHealthPanel.SetActive(false);
        //}
    }

    private void Update()
    {
        // Smooth fill animation
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount,
                targetFillAmount,
                Time.deltaTime * fillSpeed
            );
        }
    }

    public void SetupBossHealth(string name, int current, int max)
    {
        maxHealth = max;
        currentHealth = current;
        targetFillAmount = 1f;

        if (bossNameText != null)
        {
            bossNameText.text = name;
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }
    }

    public void UpdateHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        targetFillAmount = (float)currentHealth / maxHealth;
    }

    public void ShowBossUI()
    {
        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(true);
        }
    }

    public void HideBossUI()
    {
        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(false);
        }
    }
}