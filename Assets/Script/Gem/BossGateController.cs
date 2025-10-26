using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossGateController : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private int requiredBossItems = 4; // Số boss items cần để mở cổng
    [SerializeField] private bool requireAllItems = true; // True: cần tất cả items, False: cần số lượng tối thiểu

    [Header("Specific Items Required (Optional)")]
    [SerializeField] private bool useSpecificItems = false;
    [SerializeField] private List<BossItemType> specificItemsRequired = new List<BossItemType>();

    [Header("Visual References")]
    [SerializeField] private GameObject gateBlocker; // Object chặn đường (sẽ bị disable khi mở cổng)
    [SerializeField] private Animator gateAnimator; // Animator của cổng
    [SerializeField] private ParticleSystem unlockEffect; // VFX khi mở cổng
    [SerializeField] private AudioClip unlockSound; // Sound khi mở cổng

    [Header("UI References")]
    [SerializeField] private GameObject interactPrompt; // UI prompt "Press E to open"
    [SerializeField] private Image lockIcon; // Icon ổ khóa

    [Header("Colors")]
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    private BossItemInventory inventory;
    private bool isGateOpen = false;
    private bool playerInRange = false;

    private void Start()
    {
        inventory = BossItemInventory.Instance;
        if (inventory == null)
        {
            Debug.LogError("BossItemInventory not found!");
            return;
        }

        // Subscribe to collection event
        inventory.OnBossItemCollected += OnItemCollected;

        // Initial setup
        UpdateGateStatus();

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnBossItemCollected -= OnItemCollected;
        }
    }

    private void Update()
    {
        if (playerInRange && !isGateOpen && CanOpenGate())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenGate();
            }
        }
    }

    private void OnItemCollected(BossItemType itemType)
    {
        // Update gate status khi nhặt được item mới
        UpdateGateStatus();
    }

    private bool CanOpenGate()
    {
        if (useSpecificItems)
        {
            // Kiểm tra có đủ các items cụ thể không
            foreach (var item in specificItemsRequired)
            {
                if (!inventory.HasBossItem(item))
                    return false;
            }
            return true;
        }
        else if (requireAllItems)
        {
            // Cần tất cả boss items
            return inventory.HasAllBossItems();
        }
        else
        {
            // Cần số lượng tối thiểu
            return inventory.HasMinimumBossItems(requiredBossItems);
        }
    }

    private void UpdateGateStatus()
    {
        bool canOpen = CanOpenGate();

        // Update lock icon
        if (lockIcon != null)
        {
            lockIcon.color = canOpen ? unlockedColor : lockedColor;
        }

        // Nếu đủ điều kiện và player đang ở gần, hiển thị prompt
        if (canOpen && !isGateOpen && playerInRange)
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OpenGate()
    {
        if (isGateOpen) return;

        isGateOpen = true;

        // Disable blocker
        if (gateBlocker != null)
            gateBlocker.SetActive(false);

        // Play animation
        if (gateAnimator != null)
            gateAnimator.SetTrigger("Open");

        // Play unlock effect
        if (unlockEffect != null)
            unlockEffect.Play();

        // Play sound
        if (unlockSound != null)
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);

        // Hide UI
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (lockIcon != null)
            lockIcon.gameObject.SetActive(false);

        Debug.Log("Boss Gate Opened!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!isGateOpen)
            {
                // Hiển thị lock icon
                if (lockIcon != null)
                    lockIcon.gameObject.SetActive(true);

                // Hiển thị interact prompt nếu đủ điều kiện
                if (CanOpenGate() && interactPrompt != null)
                    interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Ẩn UI
            if (interactPrompt != null)
                interactPrompt.SetActive(false);

            if (!isGateOpen && lockIcon != null)
            {
                lockIcon.gameObject.SetActive(false);
            }
        }
    }
}