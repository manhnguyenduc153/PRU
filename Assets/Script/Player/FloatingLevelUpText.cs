using UnityEngine;
using TMPro;

public class FloatingLevelUpText : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float scaleSpeed = 2f;
    [SerializeField] private float maxScale = 1.5f;

    private TextMeshPro textMesh;
    private Color originalColor;
    private float lifetime = 0f;
    private Vector3 targetScale;

    private void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        if (textMesh == null)
        {
            Debug.LogError("TextMeshPro component not found!");
            Destroy(gameObject);
            return;
        }

        originalColor = textMesh.color;
        targetScale = transform.localScale * maxScale;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;

        // Di chuyển lên trên
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Scale animation
        if (lifetime < 0.3f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
        }

        // Fade out
        if (textMesh != null)
        {
            Color currentColor = textMesh.color;
            currentColor.a = Mathf.Lerp(currentColor.a, 0, fadeSpeed * Time.deltaTime);
            textMesh.color = currentColor;

            // Destroy khi gần trong suốt hoàn toàn
            if (currentColor.a < 0.01f)
            {
                Destroy(gameObject);
            }
        }
    }
}

