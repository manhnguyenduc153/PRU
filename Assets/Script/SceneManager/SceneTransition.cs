using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private bool useAsyncLoading = true;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private bool requireKeyPress = false;

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[SceneTransition] OnTriggerEnter2D: {other.gameObject.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("[SceneTransition] Player in range!");

            if (!requireKeyPress)
            {
                Debug.Log("[SceneTransition] Auto transition (no key required)");
                TransitionToScene();
            }
            else
            {
                ShowInteractionPrompt(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowInteractionPrompt(false);
            Debug.Log("[SceneTransition] Player left range");
        }
    }

    private void Update()
    {
        if (requireKeyPress && playerInRange && Input.GetKeyDown(interactionKey))
        {
            Debug.Log($"[SceneTransition] Key {interactionKey} pressed");
            TransitionToScene();
        }
    }

    public void TransitionToScene()
    {
        Debug.Log($"[SceneTransition] TransitionToScene called. Target: {targetSceneName}");

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneTransition] Target scene name is not set!");
            return;
        }

        // Sử dụng SceneTransitionManager để có fade effect
        if (SceneTransitionManager.Instance != null)
        {
            Debug.Log("[SceneTransition] SceneTransitionManager found, starting fade transition");
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName, useAsyncLoading);
        }
        else
        {
            // Fallback nếu không có TransitionManager
            Debug.LogWarning("[SceneTransition] SceneTransitionManager not found, loading directly...");

            if (GameManager.Instance != null)
            {
                if (useAsyncLoading)
                {
                    GameManager.Instance.LoadSceneAsync(targetSceneName);
                }
                else
                {
                    GameManager.Instance.LoadScene(targetSceneName);
                }
            }
            else
            {
                Debug.LogError("[SceneTransition] GameManager not found!");
            }
        }
    }

    private void ShowInteractionPrompt(bool show)
    {
        // Implement UI prompt logic here
        // Ví dụ: UIManager.Instance?.ShowPrompt(show ? $"Press {interactionKey} to enter" : "");
        if (show)
        {
            Debug.Log($"[SceneTransition] Show prompt: Press {interactionKey} to enter");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, boxCollider.size);
        }
        else
        {
            CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider != null)
            {
                Gizmos.DrawSphere(transform.position, circleCollider.radius);
            }
        }
    }
}