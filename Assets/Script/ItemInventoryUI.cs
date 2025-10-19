using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInventoryUI : MonoBehaviour
{
    [Header("Health Potion UI")]
    public Image healthPotionIcon;
    public TextMeshProUGUI healthPotionCount;

    [Header("Mana Potion UI")]
    public Image manaPotionIcon;
    public TextMeshProUGUI manaPotionCountText;

    private ItemInventory itemInventory;

    private void Start()
    {
        // Tìm ItemInventory component
        itemInventory = FindObjectOfType<ItemInventory>();

        if (itemInventory == null)
        {
            Debug.LogError("ItemInventory script not found in scene!");
            return;
        }

        // Khởi tạo UI
        UpdateInventoryUI();
    }

    private void Update()
    {
        // Cập nhật UI mỗi frame
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        if (itemInventory == null) return;

        // Cập nhật Health Potion Count
        if (healthPotionCount != null)
        {
            healthPotionCount.text = itemInventory.GetHealthPotionCount().ToString();
        }

        // Cập nhật Mana Potion Count
        if (manaPotionCountText != null)
        {
            manaPotionCountText.text = itemInventory.GetManaPotionCount().ToString();
        }
    }
}