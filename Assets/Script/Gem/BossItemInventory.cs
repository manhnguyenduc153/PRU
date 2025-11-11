using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossItemType
{
    Boss1Item,
    Boss2Item,
    Boss3Item,
    Boss4Item
    // Thêm các boss item khác tùy theo game của bạn
}

public class BossItemInventory : MonoBehaviour
{
    public static BossItemInventory Instance { get; private set; }

    // Lưu trữ các boss item đã nhặt
    private Dictionary<BossItemType, bool> collectedItems = new Dictionary<BossItemType, bool>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        // Khởi tạo tất cả boss items là chưa nhặt
        foreach (BossItemType itemType in System.Enum.GetValues(typeof(BossItemType)))
        {
            collectedItems[itemType] = false;
        }
    }

    // Thêm boss item vào inventory
    public void AddBossItem(BossItemType itemType)
    {
        if (!collectedItems.ContainsKey(itemType))
        {
            collectedItems[itemType] = false;
        }

        if (!collectedItems[itemType])
        {
            collectedItems[itemType] = true;
            Debug.Log($"Collected boss item: {itemType}");

            // Trigger event để UI update
            OnBossItemCollected?.Invoke(itemType);
        }
    }

    // Event để thông báo khi nhặt được boss item mới
    public System.Action<BossItemType> OnBossItemCollected;

    // Kiểm tra đã nhặt boss item chưa
    public bool HasBossItem(BossItemType itemType)
    {
        return collectedItems.ContainsKey(itemType) && collectedItems[itemType];
    }

    // Lấy số lượng boss items đã nhặt
    public int GetCollectedItemCount()
    {
        int count = 0;
        foreach (var item in collectedItems)
        {
            if (item.Value) count++;
        }
        return count;
    }

    // Lấy tổng số boss items trong game
    public int GetTotalItemCount()
    {
        return collectedItems.Count;
    }

    // Kiểm tra đã nhặt đủ tất cả boss items chưa (dùng để mở cổng)
    public bool HasAllBossItems()
    {
        foreach (var item in collectedItems)
        {
            if (!item.Value) return false;
        }
        return true;
    }

    // Kiểm tra đã nhặt đủ một số lượng boss items nhất định chưa
    public bool HasMinimumBossItems(int requiredCount)
    {
        return GetCollectedItemCount() >= requiredCount;
    }

    // Lấy danh sách các boss items đã nhặt
    public List<BossItemType> GetCollectedItems()
    {
        List<BossItemType> collected = new List<BossItemType>();
        foreach (var item in collectedItems)
        {
            if (item.Value)
            {
                collected.Add(item.Key);
            }
        }
        return collected;
    }

    public void DebugInventory()
    {
        foreach (var kv in collectedItems)
        {
            Debug.Log($"[BossItemInventory] {kv.Key}: {kv.Value}");
        }
    }

    // Reset inventory (dùng khi restart game hoặc new game)
    public void ResetInventory()
    {
        foreach (BossItemType itemType in System.Enum.GetValues(typeof(BossItemType)))
        {
            collectedItems[itemType] = false;
        }
        Debug.Log("Boss item inventory reset!");
    }

    // Set trạng thái của một boss item (dùng cho SaveSystem)
    public void SetBossItem(BossItemType itemType, bool collected)
    {
        if (!collectedItems.ContainsKey(itemType))
        {
            collectedItems[itemType] = false;
        }
        collectedItems[itemType] = collected;
        Debug.Log($"[BossItemInventory] Set {itemType} to {collected}");
    }

    // Lấy dictionary để lưu (cho SaveSystem)
    public Dictionary<BossItemType, bool> GetAllItems()
    {
        return new Dictionary<BossItemType, bool>(collectedItems);
    }
}