using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public int playerHealth;
    public int playerMaxHealth;
    public int playerMana;
    public int playerMaxMana;
    public int coins;

    // Experience & Level
    public int playerLevel = 1;
    public int playerExperience = 0;

    // Boss Items (ngọc)
    public bool hasBoss1Item;
    public bool hasBoss2Item;
    public bool hasBoss3Item;
    public bool hasBoss4Item;

    // Potions (ô skill/item)
    public int healthPotionCount;
    public int manaPotionCount;

    // Passive Skills (Buff levels)
    public int slashBuffLevel;
    public int lightningBuffLevel;
    public int tripleShotBuffLevel;
    public int attackBuffLevel;
    public int manaBuffLevel;
    public int hpBuffLevel;

    public string saveTime;
}
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private string saveFilePath;
    private const string SAVE_FILE_NAME = "savegame.json";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        Debug.Log("Save file path: " + saveFilePath);
    }

    // Lưu game
    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Lưu scene hiện tại
        data.sceneName = SceneManager.GetActiveScene().name;
        data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Tìm Player và lưu vị trí
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
            data.playerPosZ = player.transform.position.z;

            // Lưu health
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                data.playerHealth = playerHealth.GetCurrentHealth();
                data.playerMaxHealth = playerHealth.GetMaxHealth();
            }

            // Lưu mana
            PlayerMana playerMana = player.GetComponent<PlayerMana>();
            if (playerMana != null)
            {
                data.playerMana = playerMana.GetCurrentMana();
                data.playerMaxMana = playerMana.GetMaxMana();
            }

            // Lưu level & experience
            PlayerExperience playerExp = player.GetComponent<PlayerExperience>();
            if (playerExp != null)
            {
                data.playerLevel = playerExp.GetCurrentLevel();
                data.playerExperience = playerExp.GetCurrentExperience();
                Debug.Log($"[SaveSystem] Saved Level: {data.playerLevel}, XP: {data.playerExperience}");
            }
        }

        // Lưu coins
        if (CoinManager.Instance != null)
        {
            data.coins = CoinManager.Instance.GetCurrentCoins();
        }

        // Lưu boss items (ngọc)
        if (BossItemInventory.Instance != null)
        {
            data.hasBoss1Item = BossItemInventory.Instance.HasBossItem(BossItemType.Boss1Item);
            data.hasBoss2Item = BossItemInventory.Instance.HasBossItem(BossItemType.Boss2Item);
            data.hasBoss3Item = BossItemInventory.Instance.HasBossItem(BossItemType.Boss3Item);
            data.hasBoss4Item = BossItemInventory.Instance.HasBossItem(BossItemType.Boss4Item);
            Debug.Log($"[SaveSystem] Saved boss items: B1={data.hasBoss1Item}, B2={data.hasBoss2Item}, B3={data.hasBoss3Item}, B4={data.hasBoss4Item}");
        }

        // Lưu potions (ô skill/item)
        if (player != null)
        {
            ItemInventory itemInventory = player.GetComponent<ItemInventory>();
            if (itemInventory != null)
            {
                data.healthPotionCount = itemInventory.GetHealthPotionCount();
                data.manaPotionCount = itemInventory.GetManaPotionCount();
                Debug.Log($"[SaveSystem] Saved potions: HP={data.healthPotionCount}, MP={data.manaPotionCount}");
            }
        }

        // Lưu passive skills (buff levels)
        if (BuffManager.Instance != null)
        {
            data.slashBuffLevel = BuffManager.Instance.GetSlashLevel();
            data.lightningBuffLevel = BuffManager.Instance.GetLightningLevel();
            data.tripleShotBuffLevel = BuffManager.Instance.GetTripleShotLevel();
            data.attackBuffLevel = BuffManager.Instance.GetAttackLevel();
            data.manaBuffLevel = BuffManager.Instance.GetManaLevel();
            data.hpBuffLevel = BuffManager.Instance.GetHpLevel();
            Debug.Log($"[SaveSystem] Saved buffs: Slash={data.slashBuffLevel}, Lightning={data.lightningBuffLevel}, TripleShot={data.tripleShotBuffLevel}, Atk={data.attackBuffLevel}, Mana={data.manaBuffLevel}, HP={data.hpBuffLevel}");
        }

        // Chuyển sang JSON và lưu vào file
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"Game saved! Scene: {data.sceneName}, Position: ({data.playerPosX}, {data.playerPosY}, {data.playerPosZ})");
    }

    // Load game
    public void LoadGame()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("No save file found!");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"Loading game... Scene: {data.sceneName}, Position: ({data.playerPosX}, {data.playerPosY}, {data.playerPosZ})");

        // Lưu data tạm để dùng sau khi load scene
        PlayerPrefs.SetString("LoadedSaveData", json);
        PlayerPrefs.SetInt("IsLoadingFromSave", 1); // Flag để báo GameManager không di chuyển player
        PlayerPrefs.Save();

        // Load scene
        SceneManager.LoadScene(data.sceneName);
    }

    // Áp dụng save data sau khi scene đã load
    public void ApplySaveData()
    {
        string json = PlayerPrefs.GetString("LoadedSaveData", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("[SaveSystem] No save data to apply (PlayerPrefs is empty)");
            return;
        }

        Debug.Log("[SaveSystem] Applying save data from PlayerPrefs...");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[SaveSystem] Save data: Scene={data.sceneName}, Pos=({data.playerPosX},{data.playerPosY},{data.playerPosZ})");

        // Tìm Player và set vị trí
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"[SaveSystem] Player found at {player.transform.position}, moving to save position...");
            player.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
            Debug.Log($"[SaveSystem] Player moved to {player.transform.position}");

            // Set health
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.SetHealth(data.playerHealth, data.playerMaxHealth);
                Debug.Log($"[SaveSystem] Health set to {data.playerHealth}/{data.playerMaxHealth}");
            }

            // Set mana
            PlayerMana playerMana = player.GetComponent<PlayerMana>();
            if (playerMana != null)
            {
                playerMana.SetMana(data.playerMana, data.playerMaxMana);
                Debug.Log($"[SaveSystem] Mana set to {data.playerMana}/{data.playerMaxMana}");
            }

            // Set level & experience
            PlayerExperience playerExp = player.GetComponent<PlayerExperience>();
            if (playerExp != null)
            {
                playerExp.SetLevelAndExperience(data.playerLevel, data.playerExperience);
                Debug.Log($"[SaveSystem] Level & XP set to Level {data.playerLevel}, XP {data.playerExperience}");
            }
        }
        else
        {
            Debug.LogError("[SaveSystem] Player not found in scene!");
        }

        // Set coins
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SetCoins(data.coins);
            Debug.Log($"[SaveSystem] Coins set to {data.coins}");
        }

        // Restore boss items (ngọc)
        if (BossItemInventory.Instance != null)
        {
            BossItemInventory.Instance.ResetInventory(); // Reset trước

            if (data.hasBoss1Item) BossItemInventory.Instance.AddBossItem(BossItemType.Boss1Item);
            if (data.hasBoss2Item) BossItemInventory.Instance.AddBossItem(BossItemType.Boss2Item);
            if (data.hasBoss3Item) BossItemInventory.Instance.AddBossItem(BossItemType.Boss3Item);
            if (data.hasBoss4Item) BossItemInventory.Instance.AddBossItem(BossItemType.Boss4Item);

            Debug.Log($"[SaveSystem] Boss items restored: B1={data.hasBoss1Item}, B2={data.hasBoss2Item}, B3={data.hasBoss3Item}, B4={data.hasBoss4Item}");
        }

        // Restore potions (ô skill/item)
        if (player != null)
        {
            ItemInventory itemInventory = player.GetComponent<ItemInventory>();
            if (itemInventory != null)
            {
                itemInventory.SetPotions(data.healthPotionCount, data.manaPotionCount);
                Debug.Log($"[SaveSystem] Potions restored: HP={data.healthPotionCount}, MP={data.manaPotionCount}");
            }
        }

        // Restore passive skills (buff levels)
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.SetBuffLevels(
                data.slashBuffLevel,
                data.lightningBuffLevel,
                data.tripleShotBuffLevel,
                data.attackBuffLevel,
                data.manaBuffLevel,
                data.hpBuffLevel
            );
            Debug.Log($"[SaveSystem] Buffs restored: Slash={data.slashBuffLevel}, Lightning={data.lightningBuffLevel}, TripleShot={data.tripleShotBuffLevel}, Atk={data.attackBuffLevel}, Mana={data.manaBuffLevel}, HP={data.hpBuffLevel}");
        }

        // Xóa save data tạm
        PlayerPrefs.DeleteKey("LoadedSaveData");
        PlayerPrefs.DeleteKey("IsLoadingFromSave"); // Xóa flag
        PlayerPrefs.Save();

        Debug.Log("[SaveSystem] Save data applied successfully!");
    }

    // Kiểm tra có save file không
    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    // Xóa save file
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Save file deleted!");
        }
    }

    // Get save info để hiển thị
    public SaveData GetSaveInfo()
    {
        if (!HasSaveFile()) return null;

        string json = File.ReadAllText(saveFilePath);
        return JsonUtility.FromJson<SaveData>(json);
    }
}
