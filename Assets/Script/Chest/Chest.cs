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

        // Ẩn UI lúc bắt đầu
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenChest();
        }

        // Cập nhật text mỗi frame khi player ở gần
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

        // Ẩn UI
        if (interactUI != null)
            interactUI.SetActive(false);

        // Animation
        if (animator != null)
            animator.SetTrigger("Open");

        // Spawn item
        SpawnRandomItem();
    }

    void SpawnRandomItem()
    {
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        GameObject itemToSpawn = itemPrefabs[randomIndex];

        GameObject spawnedItem = Instantiate(itemToSpawn, spawnPoint.position, Quaternion.identity);

        Rigidbody2D rb = spawnedItem.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomDirection = new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized;
            rb.AddForce(randomDirection * spawnForce, ForceMode2D.Impulse);
        }
    }

    void UpdateInteractText()
    {
        if (interactText != null)
        {
            int currentCoins = CoinManager.Instance.GetCurrentCoins();
            if (currentCoins >= coinCost)
            {
                interactText.text = $"({currentCoins}/{coinCost})";
                interactText.color = Color.blue;
            }
            else
            {
                interactText.text = $"({currentCoins}/{coinCost})";
                interactText.color = Color.red;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            isPlayerNearby = true;

            // Hiện UI
            if (interactUI != null)
                interactUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // Ẩn UI
            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }
}