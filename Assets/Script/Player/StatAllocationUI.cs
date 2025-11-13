using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StatAllocationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject statPanel; // Panel chính
    [SerializeField] private TextMeshProUGUI availablePointsText; // "Available Points: 5"
    
    [Header("Stat Rows")]
    [SerializeField] private TextMeshProUGUI healthText; // "Health: 100 (+0)"
    [SerializeField] private Button healthButton;
    
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Button manaButton;
    
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private Button attackButton;
    
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private Button defenseButton;
    
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Button speedButton;

    [Header("Animation")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private bool useScaleAnimation = true;

    private StatPointManager statPointManager;
    private PlayerStats playerStats;
    private CanvasGroup canvasGroup;
    private bool isOpen = false;

    private void Start()
    {
        statPointManager = StatPointManager.Instance;
        playerStats = PlayerStats.Instance;

        // Setup Canvas Group cho animation
        canvasGroup = statPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = statPanel.AddComponent<CanvasGroup>();
        }

        // Đăng ký button events
        if (healthButton != null) healthButton.onClick.AddListener(() => OnStatButtonClicked("Health"));
        if (manaButton != null) manaButton.onClick.AddListener(() => OnStatButtonClicked("Mana"));
        if (attackButton != null) attackButton.onClick.AddListener(() => OnStatButtonClicked("Attack"));
        if (defenseButton != null) defenseButton.onClick.AddListener(() => OnStatButtonClicked("Defense"));
        if (speedButton != null) speedButton.onClick.AddListener(() => OnStatButtonClicked("Speed"));

        // Đăng ký events
        if (statPointManager != null)
        {
            statPointManager.OnStatPointsChanged += UpdateUI;
        }

        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateUI;
        }

        // Ẩn panel ban đầu
        statPanel.SetActive(false);
        canvasGroup.alpha = 0;

        // Cập nhật UI lần đầu
        UpdateUI();
    }

    private void Update()
    {
        // Nhấn Tab để mở/đóng
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePanel();
        }

        // Nhấn Escape để đóng
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            ClosePanel();
        }
    }

    private void TogglePanel()
    {
        if (isOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    private void OpenPanel()
    {
        isOpen = true;
        statPanel.SetActive(true);
        StartCoroutine(FadeIn());
        UpdateUI();
        
        // Pause game (optional)
        // Time.timeScale = 0;
        
        Debug.Log("Stat Panel Opened");
    }

    private void ClosePanel()
    {
        isOpen = false;
        StartCoroutine(FadeOut());
        
        // Unpause game
        // Time.timeScale = 1;
        
        Debug.Log("Stat Panel Closed");
    }

    private IEnumerator FadeIn()
    {
        float targetAlpha = 1f;
        Vector3 originalScale = statPanel.transform.localScale;
        
        if (useScaleAnimation)
        {
            statPanel.transform.localScale = originalScale * 0.8f;
        }

        while (canvasGroup.alpha < targetAlpha - 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            
            if (useScaleAnimation)
            {
                statPanel.transform.localScale = Vector3.Lerp(statPanel.transform.localScale, originalScale, Time.unscaledDeltaTime * fadeSpeed);
            }
            
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        statPanel.transform.localScale = originalScale;
    }

    private IEnumerator FadeOut()
    {
        float targetAlpha = 0f;

        while (canvasGroup.alpha > targetAlpha + 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        statPanel.SetActive(false);
    }

    private void OnStatButtonClicked(string statName)
    {
        if (statPointManager == null)
        {
            Debug.LogError("StatPointManager not found!");
            return;
        }

        bool success = statPointManager.SpendStatPoint(statName);
        
        if (success)
        {
            // Animation feedback (optional)
            StartCoroutine(ButtonClickFeedback(GetButtonForStat(statName)));
        }
        else
        {
            Debug.LogWarning("Cannot allocate stat point!");
        }
    }

    private Button GetButtonForStat(string statName)
    {
        switch (statName.ToLower())
        {
            case "health": return healthButton;
            case "mana": return manaButton;
            case "attack": return attackButton;
            case "defense": return defenseButton;
            case "speed": return speedButton;
            default: return null;
        }
    }

    private IEnumerator ButtonClickFeedback(Button button)
    {
        if (button == null) yield break;

        Vector3 originalScale = button.transform.localScale;
        float duration = 0.1f;
        float elapsed = 0f;

        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(1f, 0.9f, elapsed / duration);
            button.transform.localScale = originalScale * scale;
            yield return null;
        }

        elapsed = 0f;

        // Scale back
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(0.9f, 1f, elapsed / duration);
            button.transform.localScale = originalScale * scale;
            yield return null;
        }

        button.transform.localScale = originalScale;
    }

    private void UpdateUI()
    {
        UpdateUI(0); // Overload để match event signature
    }

    private void UpdateUI(int _)
    {
        if (statPointManager == null || playerStats == null) return;

        // Available points
        int availablePoints = statPointManager.GetAvailablePoints();
        if (availablePointsText != null)
        {
            availablePointsText.text = $"Available Points: {availablePoints}";
        }

        // Health
        if (healthText != null)
        {
            int allocated = playerStats.GetAllocatedPoints("Health");
            int currentMax = playerStats.GetMaxHealth();
            healthText.text = $"Health: {currentMax} (+{allocated})";
        }

        // Mana
        if (manaText != null)
        {
            int allocated = playerStats.GetAllocatedPoints("Mana");
            int currentMax = playerStats.GetMaxMana();
            manaText.text = $"Mana: {currentMax} (+{allocated})";
        }

        // Attack
        if (attackText != null)
        {
            int allocated = playerStats.GetAllocatedPoints("Attack");
            int current = playerStats.GetAttack();
            attackText.text = $"Attack: {current} (+{allocated})";
        }

        // Defense
        if (defenseText != null)
        {
            int allocated = playerStats.GetAllocatedPoints("Defense");
            int current = playerStats.GetDefense();
            defenseText.text = $"Defense: {current} (+{allocated})";
        }

        // Speed
        if (speedText != null)
        {
            int allocated = playerStats.GetAllocatedPoints("Speed");
            float current = playerStats.GetSpeed();
            speedText.text = $"Speed: {current:F1} (+{allocated})";
        }

        // Enable/Disable buttons based on available points
        bool hasPoints = availablePoints > 0;
        if (healthButton != null) healthButton.interactable = hasPoints;
        if (manaButton != null) manaButton.interactable = hasPoints;
        if (attackButton != null) attackButton.interactable = hasPoints;
        if (defenseButton != null) defenseButton.interactable = hasPoints;
        if (speedButton != null) speedButton.interactable = hasPoints;
    }

    private void OnDestroy()
    {
        if (statPointManager != null)
        {
            statPointManager.OnStatPointsChanged -= UpdateUI;
        }

        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateUI;
        }
    }
}

