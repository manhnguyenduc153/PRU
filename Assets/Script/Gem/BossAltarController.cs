using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossAltarController : MonoBehaviour
{
    [Header("Altar Settings")]
    [SerializeField] private int requiredBossItems = 4;
    [SerializeField] private bool requireAllItems = true;

    [Header("Specific Items Required (Optional)")]
    [SerializeField] private bool useSpecificItems = false;
    [SerializeField] private List<BossItemType> specificItemsRequired = new List<BossItemType>();

    [Header("References (Auto-detect if null)")]
    [SerializeField] private GameObject runeParent;         // Object chứa 4 rune con (inactive sẵn)
    [SerializeField] private GameObject goToDungeonParent;  // Object chứa Portal + TransitionScene (inactive sẵn)
    [SerializeField] private Animator altarAnimator;
    [SerializeField] private ParticleSystem activateEffect;
    [SerializeField] private AudioClip activateSound;

    [Header("UI")]
    [SerializeField] private GameObject interactPrompt; // chứa Text “Press E” (đặt ẩn mặc định)
    [SerializeField] private TextMeshProUGUI interactText; // text con bên trong
    [SerializeField] private Image lockIcon;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    private BossItemInventory inventory;
    private bool isActivated = false;
    private bool playerInRange = false;

    private void Start()
    {
        // Auto-find Rune & GoToDungeon
        if (runeParent == null)
            runeParent = transform.Find("Rune")?.gameObject;

        if (goToDungeonParent == null)
            goToDungeonParent = transform.Find("GoToDungeon")?.gameObject;

        inventory = BossItemInventory.Instance;
        if (inventory == null)
            return;

        inventory.OnBossItemCollected += OnItemCollected;

        UpdateAltarStatus();

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnBossItemCollected -= OnItemCollected;
    }

    private void Update()
    {
        if (playerInRange && !isActivated && CanActivateAltar())
        {
            if (Input.GetKeyDown(KeyCode.E))
                ActivateAltar();
        }
        else if (playerInRange && isActivated)
        {
            // Khi altar đã được kích hoạt, nhấn E để teleport
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Bạn có thể gọi vào TransitionScene script tại đây nếu muốn chuyển cảnh thật
                // Ví dụ: goToDungeonParent.GetComponentInChildren<TransitionScene>()?.StartTransition();
            }
        }
    }

    private void OnItemCollected(BossItemType itemType)
    {
        UpdateAltarStatus();
    }

    private bool CanActivateAltar()
    {
        if (useSpecificItems)
        {
            foreach (var item in specificItemsRequired)
            {
                if (!inventory.HasBossItem(item))
                    return false;
            }
            return true;
        }
        else if (requireAllItems)
        {
            return inventory.HasAllBossItems();
        }
        else
        {
            return inventory.HasMinimumBossItems(requiredBossItems);
        }
    }

    private void UpdateAltarStatus()
    {
        bool canOpen = CanActivateAltar();

        if (lockIcon != null)
            lockIcon.color = canOpen ? unlockedColor : lockedColor;

        if (interactPrompt != null && playerInRange)
        {
            interactPrompt.SetActive(canOpen);
            if (interactText != null)
                interactText.text = canOpen ? "Press E" : "";
        }
    }

    private void ActivateAltar()
    {
        if (isActivated) return;
        isActivated = true;

        // 1️⃣ Bật Rune
        if (runeParent != null)
        {
            runeParent.SetActive(true);
            for (int i = 0; i < runeParent.transform.childCount; i++)
                runeParent.transform.GetChild(i).gameObject.SetActive(true);
        }

        // 2️⃣ Bật GoToDungeon
        if (goToDungeonParent != null)
        {
            goToDungeonParent.SetActive(true);

            Transform portal = goToDungeonParent.transform.Find("Portal");
            Transform transition = goToDungeonParent.transform.Find("TransitionScene");

            if (portal != null)
                portal.gameObject.SetActive(true);

            if (transition != null)
                transition.gameObject.SetActive(true);
        }

        // 3️⃣ Hiệu ứng
        if (altarAnimator != null)
            altarAnimator.SetTrigger("Activate");

        if (activateEffect != null)
            activateEffect.Play();

        if (activateSound != null)
            AudioSource.PlayClipAtPoint(activateSound, transform.position);

        // 4️⃣ Cập nhật text prompt
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(true);
            if (interactText != null)
                interactText.text = "Press E to teleport";
        }

        if (lockIcon != null)
            lockIcon.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (!isActivated)
        {
            if (lockIcon != null)
                lockIcon.gameObject.SetActive(true);

            if (CanActivateAltar() && interactPrompt != null)
            {
                interactPrompt.SetActive(true);
                if (interactText != null)
                    interactText.text = "Press E";
            }
        }
        else
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
                if (interactText != null)
                    interactText.text = "Press E to teleport";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (!isActivated && lockIcon != null)
            lockIcon.gameObject.SetActive(false);
    }
}
