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

    [Header("Sound Settings")]
    [SerializeField] private AudioClip pickupSoundHealth;  // Âm thanh khi nhặt bình máu
    [SerializeField] private AudioClip pickupSoundMana;    // Âm thanh khi nhặt bình mana
    [SerializeField] private float soundVolume = 1f;       // Âm lượng

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

            // Phát âm thanh pickup health
            if (pickupSoundHealth != null)
                AudioSource.PlayClipAtPoint(pickupSoundHealth, transform.position, soundVolume);
        }
        else if (potionType == PotionType.ManaPotion)
        {
            itemInventory.PickupManaPotion(quantity);
            Debug.Log($"Picked up {quantity} Mana Potion!");

            // Phát âm thanh pickup mana
            if (pickupSoundMana != null)
                AudioSource.PlayClipAtPoint(pickupSoundMana, transform.position, soundVolume);
        }

        hasBeenPickedUp = true;

        // Xóa item khỏi scene sau khi phát âm thanh
        Destroy(gameObject);
    }
}
