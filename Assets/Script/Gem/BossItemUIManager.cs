using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossItemUIManager : MonoBehaviour
{
    [System.Serializable]
    public class BossItemDisplay
    {
        public BossItemType itemType;
        public GameObject itemIcon;
        public Image iconImage;
        public Sprite sprite;
    }

    [Header("UI References")]
    [SerializeField] private Transform itemIconContainer;
    [SerializeField] private GameObject itemIconPrefab;

    [Header("Boss Item Icons")]
    [SerializeField] private Sprite boss1ItemIcon;
    [SerializeField] private Sprite boss2ItemIcon;
    [SerializeField] private Sprite boss3ItemIcon;
    [SerializeField] private Sprite boss4ItemIcon;

    [Header("Visual Settings")]
    [SerializeField] private Color collectedColor = Color.white;
    [SerializeField] private Color unCollectedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float scaleUpAmount = 1.2f;

    private BossItemInventory inventory;
    private Dictionary<BossItemType, BossItemDisplay> itemDisplays = new Dictionary<BossItemType, BossItemDisplay>();
    private Dictionary<BossItemType, Sprite> itemSprites = new Dictionary<BossItemType, Sprite>();

    private void Start()
    {
        inventory = BossItemInventory.Instance;
        if (inventory == null)
        {
            Debug.LogError("BossItemInventory Instance not found!");
            return;
        }

        // Setup sprite dictionary
        SetupSpriteMapping();

        // Tạo UI cho tất cả boss items (slots cố định)
        InitializeBossItemUI();

        // Subscribe to collection event
        inventory.OnBossItemCollected += OnBossItemCollected;
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnBossItemCollected -= OnBossItemCollected;
        }
    }

    private void SetupSpriteMapping()
    {
        itemSprites[BossItemType.Boss1Item] = boss1ItemIcon;
        itemSprites[BossItemType.Boss2Item] = boss2ItemIcon;
        itemSprites[BossItemType.Boss3Item] = boss3ItemIcon;
        itemSprites[BossItemType.Boss4Item] = boss4ItemIcon;
    }

    private void InitializeBossItemUI()
    {
        // Tạo icon slot cho từng boss item type (cố định vị trí)
        foreach (BossItemType itemType in System.Enum.GetValues(typeof(BossItemType)))
        {
            CreateBossItemSlot(itemType);
        }
    }

    private void CreateBossItemSlot(BossItemType itemType)
    {
        if (!itemSprites.ContainsKey(itemType) || itemSprites[itemType] == null)
        {
            Debug.LogWarning($"No sprite assigned for {itemType}");
            return;
        }

        // Instantiate icon slot
        GameObject iconGO = Instantiate(itemIconPrefab, itemIconContainer);
        iconGO.name = itemType.ToString() + "Slot";

        // Setup image
        Image iconImage = iconGO.GetComponent<Image>();
        if (iconImage == null)
            iconImage = iconGO.GetComponentInChildren<Image>();

        if (iconImage != null)
        {
            iconImage.sprite = itemSprites[itemType];

            // Kiểm tra xem đã nhặt chưa
            bool isCollected = inventory.HasBossItem(itemType);
            iconImage.color = isCollected ? collectedColor : unCollectedColor;
        }

        // Add canvas group để control alpha
        CanvasGroup canvasGroup = iconGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = iconGO.AddComponent<CanvasGroup>();

        // Set alpha dựa vào trạng thái
        bool collected = inventory.HasBossItem(itemType);
        canvasGroup.alpha = collected ? 1f : 0.5f;

        // Add to dictionary với key là itemType
        BossItemDisplay display = new BossItemDisplay
        {
            itemType = itemType,
            itemIcon = iconGO,
            iconImage = iconImage,
            sprite = itemSprites[itemType]
        };
        itemDisplays[itemType] = display;
    }

    private void OnBossItemCollected(BossItemType itemType)
    {
        // Tìm display tương ứng với itemType đã nhặt
        if (itemDisplays.ContainsKey(itemType))
        {
            BossItemDisplay display = itemDisplays[itemType];
            StartCoroutine(AnimateItemCollection(display));
        }
        else
        {
            Debug.LogWarning($"No display found for {itemType}");
        }
    }

    private IEnumerator AnimateItemCollection(BossItemDisplay display)
    {
        if (display.iconImage == null || display.itemIcon == null)
            yield break;

        // Fade in và scale up animation
        CanvasGroup canvasGroup = display.itemIcon.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = display.itemIcon.AddComponent<CanvasGroup>();

        Vector3 originalScale = display.itemIcon.transform.localScale;
        Vector3 targetScale = originalScale * scaleUpAmount;

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            // Fade to full color
            display.iconImage.color = Color.Lerp(unCollectedColor, collectedColor, t);

            // Scale up then back to normal
            float scaleT = Mathf.Sin(t * Mathf.PI);
            display.itemIcon.transform.localScale = Vector3.Lerp(originalScale, targetScale, scaleT);

            // Fade in alpha
            canvasGroup.alpha = Mathf.Lerp(0.5f, 1f, t);

            yield return null;
        }

        // Ensure final state
        display.iconImage.color = collectedColor;
        display.itemIcon.transform.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }

    // Public method để refresh UI (nếu cần)
    public void RefreshUI()
    {
        foreach (var kvp in itemDisplays)
        {
            BossItemType itemType = kvp.Key;
            BossItemDisplay display = kvp.Value;

            bool isCollected = inventory.HasBossItem(itemType);
            if (display.iconImage != null)
            {
                display.iconImage.color = isCollected ? collectedColor : unCollectedColor;

                CanvasGroup canvasGroup = display.itemIcon.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = isCollected ? 1f : 0.5f;
                }
            }
        }
    }
}