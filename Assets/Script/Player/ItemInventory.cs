using UnityEngine;

public class ItemInventory : MonoBehaviour
{
    [Header("Item Quantities")]
    public int healthPotionCount = 0;
    public int manaPotionCount = 0;

    [Header("Potion Settings")]
    public int healAmount = 50;
    public int manaRestoreAmount = 50;

    private PlayerHealth playerHealth;
    private PlayerMana playerMana;

    private void Start()
    {
        // Tìm các script cần thiết
        playerHealth = FindObjectOfType<PlayerHealth>();
        playerMana = FindObjectOfType<PlayerMana>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth script not found!");
        }
        if (playerMana == null)
        {
            Debug.LogError("PlayerMana script not found!");
        }
    }

    private void Update()
    {
        // Bấm 1 để dùng Health Potion
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseHealthPotion();
        }

        // Bấm 2 để dùng Mana Potion
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseManaPotion();
        }
    }

    // Nhặt Health Potion
    public void PickupHealthPotion(int amount = 1)
    {
        healthPotionCount += amount;
        Debug.Log($"Picked up Health Potion! Total: {healthPotionCount}");
    }

    // Nhặt Mana Potion
    public void PickupManaPotion(int amount = 1)
    {
        manaPotionCount += amount;
        Debug.Log($"Picked up Mana Potion! Total: {manaPotionCount}");
    }

    // Dùng Health Potion
    public void UseHealthPotion()
    {
        if (healthPotionCount <= 0)
        {
            Debug.Log("No Health Potion available!");
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth not found!");
            return;
        }

        playerHealth.Heal(healAmount);
        healthPotionCount--;
        Debug.Log($"Used Health Potion! Remaining: {healthPotionCount}");
    }

    // Dùng Mana Potion
    public void UseManaPotion()
    {
        if (manaPotionCount <= 0)
        {
            Debug.Log("No Mana Potion available!");
            return;
        }

        if (playerMana == null)
        {
            Debug.LogError("PlayerMana not found!");
            return;
        }

        playerMana.RegenerateMana(manaRestoreAmount);
        manaPotionCount--;
        Debug.Log($"Used Mana Potion! Remaining: {manaPotionCount}");
    }

    public int GetHealthPotionCount()
    {
        return healthPotionCount;
    }

    public int GetManaPotionCount()
    {
        return manaPotionCount;
    }
}