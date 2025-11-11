using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneInitializer : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[GameSceneInitializer] Starting scene initialization...");
        // Đợi 1 frame để đảm bảo tất cả objects đã được khởi tạo
        StartCoroutine(InitializeScene());
    }

    System.Collections.IEnumerator InitializeScene()
    {
        // Đợi nhiều frame hơn để đảm bảo GameManager đã xong
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);

        Debug.Log("[GameSceneInitializer] Checking for save data...");

        // Kiểm tra nếu đang load từ save
        if (SaveSystem.Instance != null)
        {
            Debug.Log("[GameSceneInitializer] SaveSystem found, applying save data...");
            SaveSystem.Instance.ApplySaveData();
        }
        else
        {
            Debug.LogWarning("[GameSceneInitializer] SaveSystem not found!");
        }
    }
}
