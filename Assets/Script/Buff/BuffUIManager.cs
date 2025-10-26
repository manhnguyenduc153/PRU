using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffUIManager : MonoBehaviour
{
    [System.Serializable]
    public class BuffDisplay
    {
        public string buffName;
        public GameObject buffIcon;
        public Image iconImage;
        public TextMeshProUGUI levelText;
        public Sprite sprite;
    }

    [SerializeField] private Transform buffIconContainer;
    [SerializeField] private GameObject buffIconPrefab;

    [Header("Buff Icons")]
    [SerializeField] private Sprite slashBuffIcon;
    [SerializeField] private Sprite lightningBuffIcon;
    [SerializeField] private Sprite tripleShotBuffIcon;
    [SerializeField] private Sprite attackBuffIcon;
    [SerializeField] private Sprite manaBuffIcon;
    [SerializeField] private Sprite hpBuffIcon;

    private BuffManager buffManager;
    private List<BuffDisplay> buffDisplays = new List<BuffDisplay>();

    private int previousSlashLevel = 0;
    private int previousLightningLevel = 0;
    private int previousTripleShotLevel = 0;
    private int previousAttackLevel = 0;
    private int previousManaLevel = 0;
    private int previousHpLevel = 0;

    private void Start()
    {
        buffManager = BuffManager.Instance;
        if (buffManager == null)
        {
            Debug.LogError("BuffManager Instance not found!");
            return;
        }
        UpdateBuffUI();
    }

    private void Update()
    {
        // Lấy level hiện tại
        int currentSlashLevel = buffManager.GetSlashLevel();
        int currentLightningLevel = buffManager.GetLightningLevel();
        int currentTripleShotLevel = buffManager.GetTripleShotLevel();
        int currentAttackLevel = buffManager.GetAttackLevel();
        int currentManaLevel = buffManager.GetManaLevel();
        int currentHpLevel = buffManager.GetHpLevel();

        // Cập nhật level text
        UpdateLevelDisplay("Slash", currentSlashLevel);
        UpdateLevelDisplay("Lightning", currentLightningLevel);
        UpdateLevelDisplay("TripleShot", currentTripleShotLevel);
        UpdateLevelDisplay("Attack", currentAttackLevel);
        UpdateLevelDisplay("Mana", currentManaLevel);
        UpdateLevelDisplay("Hp", currentHpLevel);

        // Nếu có buff mới nhặt
        if ((currentSlashLevel > 0 && previousSlashLevel == 0) ||
            (currentLightningLevel > 0 && previousLightningLevel == 0) ||
            (currentTripleShotLevel > 0 && previousTripleShotLevel == 0) ||
            (currentAttackLevel > 0 && previousAttackLevel == 0) ||
            (currentManaLevel > 0 && previousManaLevel == 0) ||
            (currentHpLevel > 0 && previousHpLevel == 0))
        {
            UpdateBuffUI();
        }

        previousSlashLevel = currentSlashLevel;
        previousLightningLevel = currentLightningLevel;
        previousTripleShotLevel = currentTripleShotLevel;
        previousAttackLevel = currentAttackLevel;
        previousManaLevel = currentManaLevel;
        previousHpLevel = currentHpLevel;
    }

    private void UpdateBuffUI()
    {
        if (buffManager == null) return;

        // Xóa UI cũ
        foreach (var display in buffDisplays)
            Destroy(display.buffIcon);
        buffDisplays.Clear();

        // Buff cũ
        if (buffManager.GetSlashLevel() > 0) CreateBuffIcon("Slash", slashBuffIcon);
        if (buffManager.GetLightningLevel() > 0) CreateBuffIcon("Lightning", lightningBuffIcon);
        if (buffManager.GetTripleShotLevel() > 0) CreateBuffIcon("TripleShot", tripleShotBuffIcon);

        // Buff mới
        if (buffManager.GetAttackLevel() > 0) CreateBuffIcon("Attack", attackBuffIcon);
        if (buffManager.GetManaLevel() > 0) CreateBuffIcon("Mana", manaBuffIcon);
        if (buffManager.GetHpLevel() > 0) CreateBuffIcon("Hp", hpBuffIcon);
    }

    private void CreateBuffIcon(string buffName, Sprite icon)
    {
        GameObject iconGO = Instantiate(buffIconPrefab, buffIconContainer);
        iconGO.name = buffName + "Icon";

        Image iconImage = iconGO.transform.GetChild(0).GetComponent<Image>();
        if (iconImage != null && icon != null) iconImage.sprite = icon;

        TextMeshProUGUI levelText = iconGO.GetComponentInChildren<TextMeshProUGUI>();

        CanvasGroup canvasGroup = iconGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = iconGO.AddComponent<CanvasGroup>();
        StartCoroutine(FadeIn(canvasGroup));

        BuffDisplay display = new BuffDisplay
        {
            buffName = buffName,
            buffIcon = iconGO,
            iconImage = iconImage,
            levelText = levelText,
            sprite = icon
        };
        buffDisplays.Add(display);
    }

    private IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private void UpdateLevelDisplay(string buffName, int level)
    {
        BuffDisplay display = buffDisplays.Find(x => x.buffName == buffName);
        if (display != null && display.levelText != null)
            display.levelText.text = level.ToString();
    }
}
