using UnityEngine;

public class PotionItem : MonoBehaviour
{
    public enum PotionType
    {
        HealthPotion,
        ManaPotion
    }

    [Header("Potion Settings")]
    public PotionType potionType = PotionType.HealthPotion;
    public int quantity = 1;

    private ItemInventory itemInventory;
    private bool hasBeenPickedUp = false;

    private void Start()
    {
        // Tìm ItemInventory khi game start
        itemInventory = FindObjectOfType<ItemInventory>();

        if (itemInventory == null)
        {
            Debug.LogError("ItemInventory not found in scene!");
        }
    }

    // Cho 3D
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player") && !hasBeenPickedUp)
        {
            PickupPotion();
        }
    }

    // Cho 2D
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasBeenPickedUp)
        {
            PickupPotion();
        }
    }

    private void PickupPotion()
    {
        if (itemInventory == null) return;

        // Nhặt potion dựa trên loại
        if (potionType == PotionType.HealthPotion)
        {
            itemInventory.PickupHealthPotion(quantity);
            Debug.Log($"Picked up {quantity} Health Potion!");
        }
        else if (potionType == PotionType.ManaPotion)
        {
            itemInventory.PickupManaPotion(quantity);
            Debug.Log($"Picked up {quantity} Mana Potion!");
        }

        hasBeenPickedUp = true;

        // Xóa item khỏi scene
        Destroy(gameObject);
    }
}