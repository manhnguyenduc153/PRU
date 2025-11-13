using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    [Header("Icon Settings")]
    public Image iconImage; // Reference to UI Image component
    public Color playerColor = new Color(0, 0.5f, 1f, 1f); // Màu xanh biển
    public Color enemyColor = Color.red; // Màu đỏ
    public float iconSize = 10f; // Kích thước icon

    [Header("Target")]
    public Transform target; // Đối tượng cần hiển thị trên minimap

    private RectTransform rectTransform;
    private Canvas minimapCanvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Tìm minimap canvas
        minimapCanvas = GetComponentInParent<Canvas>();

        // Tạo icon image nếu chưa có
        if (iconImage == null)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(transform);
            iconImage = iconObj.AddComponent<Image>();

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            iconRect.anchoredPosition = Vector2.zero;
        }

        // Set màu sắc dựa trên tag
        SetIconColor();
    }

    void SetIconColor()
    {
        if (target == null) return;

        if (target.CompareTag("Player"))
        {
            iconImage.color = playerColor;
        }
        else if (target.CompareTag("Enemy"))
        {
            iconImage.color = enemyColor;
        }
        else
        {
            iconImage.color = Color.white; // Màu mặc định
        }

        // Tạo hình tròn cho icon
        CreateCircleSprite();
    }

    void CreateCircleSprite()
    {
        // Tạo texture hình tròn
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    colors[y * size + x] = Color.white;
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        Sprite circleSprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f)
        );

        iconImage.sprite = circleSprite;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Icon sẽ tự động theo target thông qua MinimapCamera
        // Hoặc bạn có thể cập nhật vị trí thủ công nếu cần
    }
}