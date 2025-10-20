
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ==================== COIN MANAGER ====================
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI coinText;

    private int currentCoins = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinUI();
        Debug.Log($"Collected {amount} coins! Total: {currentCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            UpdateCoinUI();
            Debug.Log($"Spent {amount} coins! Remaining: {currentCoins}");
            return true;
        }
        else
        {
            Debug.Log("Not enough coins!");
            return false;
        }
    }

    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = currentCoins.ToString();
        }
    }
}