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
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!requireKeyPress)
            {
                TransitionToScene();
            }
            else
            {
                // Hiển thị UI prompt nếu cần
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
        }
    }

    private void Update()
    {
        if (requireKeyPress && playerInRange && Input.GetKeyDown(interactionKey))
        {
            TransitionToScene();
        }
    }

    public void TransitionToScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target scene name is not set!");
            return;
        }

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
            Debug.LogError("GameManager not found!");
        }
    }

    private void ShowInteractionPrompt(bool show)
    {
        // Implement UI prompt logic here
        // Ví dụ: UIManager.Instance?.ShowPrompt(show ? $"Press {interactionKey} to enter" : "");
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