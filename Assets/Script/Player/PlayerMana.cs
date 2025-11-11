using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    public int maxMana = 500;
    private int currentMana;

    [Header("Mana Regeneration")]
    public int manaRegenPerSecond = 10;
    private float regenTimer = 0f;

    private void Start()
    {
        currentMana = maxMana;
        Debug.Log($"Player Mana Initialized: {currentMana}/{maxMana}");
    }

    private void Update()
    {
        // Mana regeneration
        regenTimer += Time.deltaTime;
        if (regenTimer >= 1f)
        {
            RegenerateMana(manaRegenPerSecond);
            regenTimer = 0f;
        }

        // Test: Bấm Space để giảm mana
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    UseMana(50);
        //}
    }

    public void UseMana(int amount)
    {
        if (currentMana < amount)
        {
            Debug.Log($"Not enough mana! Need {amount}, but only have {currentMana}");
            return;
        }

        currentMana -= amount;
        Debug.Log($"Used {amount} mana! Current Mana: {currentMana}/{maxMana}");
    }

    public void RegenerateMana(int amount)
    {
        currentMana += amount;
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
        // Debug.Log($"Mana regenerated {amount}! Current Mana: {currentMana}/{maxMana}");
    }

    public int GetCurrentMana()
    {
        return currentMana;
    }

    public int GetMaxMana()
    {
        return maxMana;
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMana >= amount;
    }

    // Phương thức cho SaveSystem
    public void SetMana(int mana, int max)
    {
        currentMana = mana;
        maxMana = max;
        Debug.Log($"Mana loaded: {currentMana}/{maxMana}");
    }
}