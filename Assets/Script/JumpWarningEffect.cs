using System.Collections;
using UnityEngine;

public class JumpWarningEffect : MonoBehaviour
{
    [Header("Visual Settings")]
    public SpriteRenderer spriteRenderer;
    public Color warningColor = new Color(1f, 0f, 0f, 0.5f);
    public float pulseSpeed = 3f;
    public bool scaleAnimation = true;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = warningColor;
        }

        StartCoroutine(AnimateWarning());
    }

    IEnumerator AnimateWarning()
    {
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (true)
        {
            elapsed += Time.deltaTime * pulseSpeed;

            // Pulse alpha
            if (spriteRenderer != null)
            {
                Color col = warningColor;
                col.a = Mathf.Lerp(0.3f, 0.8f, (Mathf.Sin(elapsed) + 1f) / 2f);
                spriteRenderer.color = col;
            }

            // Scale animation
            if (scaleAnimation)
            {
                float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(elapsed) + 1f) / 2f);
                transform.localScale = originalScale * scale;
            }

            yield return null;
        }
    }
}