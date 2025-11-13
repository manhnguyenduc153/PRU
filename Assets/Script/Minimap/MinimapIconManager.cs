using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapIconManager : MonoBehaviour
{
    [Header("Manual Setup (Kéo thả vào nếu tự động không hoạt động)")]
    public Camera minimapCamera;
    public RawImage minimapRawImage;

    [Header("Settings")]
    public bool autoDetect = true;
    public float updateInterval = 0.5f;
    public float iconSize = 8f;
    public Color playerColor = new Color(0, 0.5f, 1f, 1f); // Xanh biển
    public Color enemyColor = Color.red; // Đỏ
    public bool showDebugLogs = true;

    private Transform iconContainer;
    private Dictionary<GameObject, GameObject> iconMap = new Dictionary<GameObject, GameObject>();
    private float lastUpdateTime;
    private RectTransform minimapRect;
    private bool setupComplete = false;

    void Start()
    {
        AutoSetup();

        if (setupComplete && autoDetect)
        {
            DetectAndCreateIcons();
        }
    }

    void AutoSetup()
    {
        if (showDebugLogs) Debug.Log("[Minimap] Bắt đầu setup...");

        // 1. Tìm Minimap Camera
        if (minimapCamera == null)
        {
            // Tìm trong children
            Camera[] cameras = GetComponentsInChildren<Camera>();
            foreach (Camera cam in cameras)
            {
                if (cam.gameObject.name.Contains("Minimap"))
                {
                    minimapCamera = cam;
                    break;
                }
            }

            // Nếu vẫn không có, lấy camera đầu tiên
            if (minimapCamera == null && cameras.Length > 0)
            {
                minimapCamera = cameras[0];
            }
        }

        if (minimapCamera == null)
        {
            Debug.LogError("[Minimap] Không tìm thấy Minimap Camera! Kéo thả vào field 'Minimap Camera'");
            return;
        }
        else
        {
            if (showDebugLogs) Debug.Log("[Minimap] Tìm thấy camera: " + minimapCamera.name);
        }

        // 2. Tìm RawImage của Minimap
        if (minimapRawImage == null)
        {
            // Tìm trong scene
            RawImage[] rawImages = FindObjectsOfType<RawImage>();
            foreach (RawImage img in rawImages)
            {
                if (img.gameObject.name.Contains("Minimap"))
                {
                    minimapRawImage = img;
                    break;
                }
            }
        }

        if (minimapRawImage == null)
        {
            Debug.LogError("[Minimap] Không tìm thấy RawImage! Kéo thả vào field 'Minimap Raw Image'");
            return;
        }
        else
        {
            minimapRect = minimapRawImage.GetComponent<RectTransform>();
            if (showDebugLogs) Debug.Log("[Minimap] Tìm thấy RawImage: " + minimapRawImage.name);
        }

        // 3. Tạo Icon Container
        Transform existingContainer = minimapRawImage.transform.Find("IconContainer");
        if (existingContainer != null)
        {
            iconContainer = existingContainer;
            if (showDebugLogs) Debug.Log("[Minimap] Sử dụng IconContainer có sẵn");
        }
        else
        {
            GameObject containerObj = new GameObject("IconContainer");
            containerObj.transform.SetParent(minimapRawImage.transform, false);

            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.localScale = Vector3.one;

            iconContainer = containerObj.transform;
            if (showDebugLogs) Debug.Log("[Minimap] Đã tạo IconContainer mới");
        }

        setupComplete = true;
        if (showDebugLogs) Debug.Log("[Minimap] Setup hoàn tất!");
    }

    void Update()
    {
        if (!setupComplete) return;

        if (autoDetect && Time.time - lastUpdateTime >= updateInterval)
        {
            DetectAndCreateIcons();
            lastUpdateTime = Time.time;
        }

        UpdateIconPositions();
    }

    void DetectAndCreateIcons()
    {
        int playerCount = 0;
        int enemyCount = 0;

        // Tìm Player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        playerCount = players.Length;
        foreach (GameObject player in players)
        {
            if (!iconMap.ContainsKey(player) && player != null)
            {
                CreateIconFor(player, playerColor);
                if (showDebugLogs) Debug.Log("[Minimap] Tạo icon cho Player: " + player.name);
            }
        }

        // Tìm Enemy
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;
        foreach (GameObject enemy in enemies)
        {
            if (!iconMap.ContainsKey(enemy) && enemy != null)
            {
                CreateIconFor(enemy, enemyColor);
                if (showDebugLogs) Debug.Log("[Minimap] Tạo icon cho Enemy: " + enemy.name);
            }
        }

        // Log một lần để biết có bao nhiêu đối tượng
        if (showDebugLogs && Time.time - lastUpdateTime < 0.1f)
        {
            Debug.Log($"[Minimap] Tìm thấy {playerCount} Players và {enemyCount} Enemies");
        }

        // Xóa icon của đối tượng đã hủy
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in iconMap)
        {
            if (kvp.Key == null)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
        {
            iconMap.Remove(key);
        }
    }

    GameObject CreateIconFor(GameObject target, Color color)
    {
        // Tạo icon GameObject
        GameObject iconObj = new GameObject(target.name + "_Icon");
        iconObj.transform.SetParent(iconContainer, false);

        // Setup RectTransform
        RectTransform rectTransform = iconObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localScale = Vector3.one;

        // Tạo Image component
        Image image = iconObj.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        // Tạo sprite hình tròn
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        Vector2 center = new Vector2(16, 16);

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * 32 + x] = dist <= 16 ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        image.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        // Lưu reference
        iconMap[target] = iconObj;

        // Lưu target vào icon
        MinimapIconData data = iconObj.AddComponent<MinimapIconData>();
        data.target = target.transform;

        return iconObj;
    }

    void UpdateIconPositions()
    {
        if (minimapCamera == null || minimapRect == null) return;

        foreach (var kvp in iconMap)
        {
            if (kvp.Key == null || kvp.Value == null) continue;

            MinimapIconData data = kvp.Value.GetComponent<MinimapIconData>();
            if (data == null || data.target == null) continue;

            // Chuyển đổi vị trí world sang viewport của minimap camera
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(data.target.position);

            // Kiểm tra nếu trong tầm nhìn (bỏ qua z check vì minimap là orthographic)
            if (viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1)
            {
                kvp.Value.SetActive(true);

                // Chuyển viewport sang vị trí trên minimap UI
                RectTransform iconRect = kvp.Value.GetComponent<RectTransform>();
                Vector2 minimapPos = new Vector2(
                    (viewportPos.x - 0.5f) * minimapRect.rect.width,
                    (viewportPos.y - 0.5f) * minimapRect.rect.height
                );
                iconRect.anchoredPosition = minimapPos;
            }
            else
            {
                kvp.Value.SetActive(false);
            }
        }
    }

    // Gọi hàm này để test
    [ContextMenu("Test Setup")]
    void TestSetup()
    {
        showDebugLogs = true;
        AutoSetup();
        DetectAndCreateIcons();
    }
}

public class MinimapIconData : MonoBehaviour
{
    public Transform target;
}