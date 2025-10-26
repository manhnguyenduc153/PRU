using UnityEngine;

[System.Serializable]
public class DropItem
{
    public GameObject itemPrefab;       // Prefab vật phẩm
    [Range(0f, 1f)] public float dropChance = 0.5f; // Tỉ lệ rơi (0.0 – 1.0)
}

public class EnemyDrop : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private DropItem[] possibleDrops;   // Danh sách các vật phẩm có thể rơi
    [SerializeField] private int minDropCount = 1;       // Số lượng vật phẩm rơi ít nhất
    [SerializeField] private int maxDropCount = 3;       // Số lượng vật phẩm rơi nhiều nhất
    [SerializeField] private float dropRadius = 0.5f;    // Bán kính ngẫu nhiên khi vật phẩm spawn ra xung quanh enemy
    [SerializeField] private bool allowDuplicateDrops = true; // Cho phép rơi trùng loại item không?

    /// <summary>
    /// Gọi hàm này khi enemy chết.
    /// </summary>
    public void DropLoot()
    {
        if (possibleDrops == null || possibleDrops.Length == 0) return;

        int dropCount = Random.Range(minDropCount, maxDropCount + 1);
        int attempts = 0;
        int dropped = 0;

        while (dropped < dropCount && attempts < possibleDrops.Length * 3)
        {
            attempts++;

            // Random chọn 1 loại item trong danh sách
            DropItem itemData = possibleDrops[Random.Range(0, possibleDrops.Length)];

            // Kiểm tra tỉ lệ rơi
            if (Random.value <= itemData.dropChance)
            {
                // Nếu không cho phép trùng loại, thì loại bỏ item đã rơi
                if (!allowDuplicateDrops)
                {
                    System.Collections.Generic.List<DropItem> list = new System.Collections.Generic.List<DropItem>(possibleDrops);
                    list.Remove(itemData);
                    possibleDrops = list.ToArray();
                }

                // Random vị trí xung quanh enemy
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * dropRadius;
                Instantiate(itemData.itemPrefab, spawnPos, Quaternion.identity);

                dropped++;
            }
        }
    }
}
