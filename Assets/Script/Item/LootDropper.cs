using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== LOOT ITEM CLASS ====================
[System.Serializable]
public class LootItem
{
    public GameObject itemPrefab;
    [Range(0f, 100f)]
    public float dropChance = 50f; // % chance drop item này
    public int minAmount = 1;
    public int maxAmount = 1;

    [Header("Display Info")]
    public string itemName = "Item";
}

// ==================== LOOT DROP COMPONENT ====================
public class LootDropper : MonoBehaviour
{
    [Header("Loot Table")]
    [SerializeField] private List<LootItem> lootTable = new List<LootItem>();

    [Header("Global Settings")]
    [SerializeField] private bool guaranteedDrop = false; // Đảm bảo ít nhất 1 item

    [Header("Spawn Settings")]
    [SerializeField] private float dropForce = 3f;
    [SerializeField] private float dropRadius = 0.5f;
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private float spawnDelay = 0.1f; // Delay giữa các item spawn

    // Gọi method này khi enemy chết
    public void DropLoot()
    {
        if (lootTable.Count == 0)
        {
            Debug.LogWarning("Loot table is empty!");
            return;
        }

        StartCoroutine(DropLootCoroutine());
    }

    private IEnumerator DropLootCoroutine()
    {
        bool hasDroppedAnything = false;

        // Duyệt qua từng item trong loot table
        foreach (LootItem loot in lootTable)
        {
            if (loot.itemPrefab == null) continue;

            // Random xem có drop item này không
            float roll = Random.Range(0f, 100f);

            if (roll <= loot.dropChance)
            {
                // Random số lượng
                int amount = Random.Range(loot.minAmount, loot.maxAmount + 1);

                // Spawn items
                for (int i = 0; i < amount; i++)
                {
                    SpawnItem(loot.itemPrefab);
                    hasDroppedAnything = true;

                    // Delay nhỏ giữa các lần spawn
                    if (spawnDelay > 0)
                    {
                        yield return new WaitForSeconds(spawnDelay);
                    }
                }

                Debug.Log($"Dropped {amount}x {loot.itemName}");
            }
        }

        // Nếu guaranteed drop và chưa drop gì, drop item đầu tiên
        if (guaranteedDrop && !hasDroppedAnything && lootTable.Count > 0)
        {
            LootItem fallbackItem = lootTable[0];
            if (fallbackItem.itemPrefab != null)
            {
                SpawnItem(fallbackItem.itemPrefab);
                Debug.Log($"Guaranteed drop: {fallbackItem.itemName}");
            }
        }
    }

    void SpawnItem(GameObject itemPrefab)
    {
        // Vị trí spawn ngẫu nhiên xung quanh
        Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        // Rotation
        Quaternion spawnRotation = randomRotation ?
            Quaternion.Euler(0, 0, Random.Range(0f, 360f)) :
            Quaternion.identity;

        // Spawn
        GameObject droppedItem = Instantiate(itemPrefab, spawnPosition, spawnRotation);

        // Thêm lực
        Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * dropForce, ForceMode2D.Impulse);
        }
    }

    // Optional: Xem thông tin drop rates trong editor
    [ContextMenu("Show Drop Rates Info")]
    void ShowDropRatesInfo()
    {
        Debug.Log("=== LOOT TABLE ===");
        foreach (LootItem loot in lootTable)
        {
            Debug.Log($"{loot.itemName}: {loot.dropChance}% chance, Amount: {loot.minAmount}-{loot.maxAmount}");
        }
    }
}