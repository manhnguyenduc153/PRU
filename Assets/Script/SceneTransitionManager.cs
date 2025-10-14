using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public string targetSceneName; // Tên scene đích
    public string targetSpawnPointID; // ID của spawn point ở scene đích

    [Header("Interaction Settings")]
    public bool requireKeyPress = false;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Visual Feedback (Optional)")]
    public GameObject interactionPrompt; // UI "Press E" (tùy chọn)

    private bool playerInRange = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);

            if (!requireKeyPress)
            {
                TransitionToScene();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (requireKeyPress && playerInRange && Input.GetKeyDown(interactionKey))
        {
            TransitionToScene();
        }
    }

    void TransitionToScene()
    {
        // Lưu thông tin spawn point
        PlayerPrefs.SetString("TargetSpawnPoint", targetSpawnPointID);
        PlayerPrefs.Save();

        // Chuyển scene
        SceneManager.LoadScene(targetSceneName);
    }
}