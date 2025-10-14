using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        // Tìm player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Không tìm thấy Player!");
            return;
        }

        // Lấy ID spawn point từ PlayerPrefs
        string targetSpawnID = PlayerPrefs.GetString("TargetSpawnPoint", "DefaultSpawn");

        // Tìm tất cả spawn points trong scene
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();

        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.spawnPointID == targetSpawnID)
            {
                // Đặt player vào vị trí spawn point
                player.transform.position = sp.transform.position;
                Debug.Log($"Player spawned at: {sp.spawnPointID}");

                // Xóa PlayerPrefs sau khi dùng
                PlayerPrefs.DeleteKey("TargetSpawnPoint");
                return;
            }
        }

        // Nếu không tìm thấy, tìm spawn point mặc định
        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.spawnPointID == "DefaultSpawn")
            {
                player.transform.position = sp.transform.position;
                Debug.Log("Player spawned at DefaultSpawn");
                return;
            }
        }

        Debug.LogWarning($"Không tìm thấy spawn point: {targetSpawnID}");
    }
}