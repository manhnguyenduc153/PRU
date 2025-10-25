using TMPro;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Header("Coin Cost")]
    [SerializeField] private int coinCost = 10;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnForce = 5f;

    [Header("Multiple Items Chance")]
    [Tooltip("Xác suất (%) rơi ra nhiều item khi mở chest")]
    [Range(0f, 100f)][SerializeField] private float multiItemChance = 25f;
    [Tooltip("Số item tối đa có thể rơi ra khi chest mở nhiều item")]
    [SerializeField] private int maxExtraItems = 3;

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("UI - Hiển thị trên đầu Chest")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private TextMeshProUGUI interactText;

    private bool isPlayerNearby = false;
    private bool isOpened = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnObj.transform.parent = transform;
            spawnObj.transform.localPosition = new Vector3(0, 1f, 0);
            spawnPoint = spawnObj.transform;
        }

        if (interactUI != null)
            interactUI.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenChest();
        }

        if (isPlayerNearby && !isOpened)
        {
            UpdateInteractText();
        }
    }

    void TryOpenChest()
    {
        if (CoinManager.Instance.GetCurrentCoins() >= coinCost)
        {
            CoinManager.Instance.SpendCoins(coinCost);
            OpenChest();
        }
        else
        {
            Debug.Log($"Need {coinCost} coins! You have {CoinManager.Instance.GetCurrentCoins()}");
        }
    }

    void OpenChest()
    {
        if (itemPrefabs.Length == 0)
        {
            Debug.LogWarning("Chest không có item!");
            return;
        }

        isOpened = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        if (animator != null)
            animator.SetTrigger("Open");

        // Spawn ít nhất 1 item
        SpawnRandomItem();

        // Kiểm tra khả năng spawn thêm item
        float roll = Random.Range(0f, 100f);
        if (roll <= multiItemChance)
        {
            int extraItems = Random.Range(1, maxExtraItems + 1);
            for (int i = 0; i < extraItems; i++)
            {
                SpawnRandomItem();
            }
        }
    }

    void SpawnRandomItem()
    {
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        GameObject itemToSpawn = itemPrefabs[randomIndex];

        // Vị trí ngẫu nhiên xung quanh rương
        float radius = 1f; // Bán kính rơi ra, có thể tùy chỉnh
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        Vector3 spawnPos = spawnPoint.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        GameObject spawnedItem = Instantiate(itemToSpawn, spawnPos, Quaternion.identity);

        Rigidbody2D rb = spawnedItem.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Force vẫn hướng lên trên nhưng có thể hơi lệch trái/phải
            Vector2 randomDirection = new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized;
            rb.AddForce(randomDirection * spawnForce, ForceMode2D.Impulse);
        }
    }


    void UpdateInteractText()
    {
        if (interactText != null)
        {
            int currentCoins = CoinManager.Instance.GetCurrentCoins();
            interactText.text = $"({currentCoins}/{coinCost})";
            interactText.color = (currentCoins >= coinCost) ? Color.blue : Color.red;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            isPlayerNearby = true;

            if (interactUI != null)
                interactUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }
}
