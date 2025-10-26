using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class Chest : MonoBehaviour
{
    [System.Serializable]
    public struct ChestItem
    {
        public GameObject prefab;
        [Range(0f, 100f)] public float dropChance; // Xác suất rơi của item
    }

    [Header("Items and Drop Rates")]
    [SerializeField] private ChestItem[] items; // danh sách item + tỉ lệ drop

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
        if (items.Length == 0)
        {
            Debug.LogWarning("Chest không có item!");
            return;
        }

        isOpened = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        if (animator != null)
            animator.SetTrigger("Open");

        // Danh sách item sẽ drop
        List<GameObject> itemsToDrop = new List<GameObject>();

        // Luôn có ít nhất 1 item được chọn theo tỉ lệ
        GameObject firstItem = GetRandomItemByWeight();
        if (firstItem != null)
            itemsToDrop.Add(firstItem);

        // Nếu trúng tỉ lệ multi-item, thêm nhiều item khác (không trùng)
        float roll = Random.Range(0f, 100f);
        if (roll <= multiItemChance)
        {
            int extraCount = Random.Range(1, maxExtraItems + 1);

            // Clone danh sách item để chọn thêm mà không lặp
            List<GameObject> available = new List<GameObject>();
            foreach (var chestItem in items)
                if (chestItem.prefab != firstItem)
                    available.Add(chestItem.prefab);

            for (int i = 0; i < extraCount && available.Count > 0; i++)
            {
                int randIndex = Random.Range(0, available.Count);
                GameObject selected = available[randIndex];
                itemsToDrop.Add(selected);
                available.RemoveAt(randIndex);
            }
        }

        // Spawn toàn bộ item trong danh sách
        foreach (var item in itemsToDrop)
            SpawnItem(item);
    }

    void SpawnItem(GameObject itemToSpawn)
    {
        if (itemToSpawn == null) return;

        // Vị trí ngẫu nhiên xung quanh rương
        float radius = 1f;
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        Vector3 spawnPos = spawnPoint.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        GameObject spawnedItem = Instantiate(itemToSpawn, spawnPos, Quaternion.identity);

        Rigidbody2D rb = spawnedItem.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomDirection = new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized;
            rb.AddForce(randomDirection * spawnForce, ForceMode2D.Impulse);
        }
    }

    GameObject GetRandomItemByWeight()
    {
        float totalWeight = 0f;
        foreach (var item in items)
            totalWeight += item.dropChance;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var item in items)
        {
            cumulative += item.dropChance;
            if (randomValue <= cumulative)
                return item.prefab;
        }

        return items[items.Length - 1].prefab;
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
