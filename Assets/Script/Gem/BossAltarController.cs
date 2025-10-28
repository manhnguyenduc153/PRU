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
    [SerializeField] private GameObject runeParent;
    [SerializeField] private GameObject goToDungeonParent;
    [SerializeField] private Animator altarAnimator;
    [SerializeField] private ParticleSystem activateEffect;
    [SerializeField] private AudioClip activateSound;

    [Header("Sound Settings")]
    [SerializeField] private float maxHearingDistance = 10f; // khoảng cách tối đa nghe được
    [SerializeField] private float minVolumeDistance = 2f;   // khoảng cách gần nhất (âm lượng max)
    [SerializeField] private float maxVolume = 1f;           // âm lượng tối đa khi player gần
    [SerializeField] private float fadeSpeed = 2f;           // tốc độ fade mượt
    private AudioSource loopingAudioSource;
    private Transform playerTransform;
    private float targetVolume = 0f;
    private float volumeVelocity = 0f; // cho SmoothDamp

    [Header("UI")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    private BossItemInventory inventory;
    private bool isActivated = false;
    private bool playerInRange = false;

    private void Start()
    {
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

        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnBossItemCollected -= OnItemCollected;

        if (loopingAudioSource != null)
            Destroy(loopingAudioSource.gameObject);
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
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Gọi TransitionScene tại đây nếu cần
            }
        }

        UpdateSoundVolume();
    }

    private void UpdateSoundVolume()
    {
        if (loopingAudioSource == null || playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Xác định targetVolume dựa trên khoảng cách
        if (distance > maxHearingDistance)
        {
            targetVolume = 0f;
        }
        else
        {
            float t = Mathf.InverseLerp(maxHearingDistance, minVolumeDistance, distance);
            targetVolume = Mathf.Lerp(0f, maxVolume, 1 - t);
        }

        // 🔊 Làm mượt chuyển đổi âm lượng
        loopingAudioSource.volume = Mathf.SmoothDamp(loopingAudioSource.volume, targetVolume, ref volumeVelocity, 1f / fadeSpeed);
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

        if (runeParent != null)
        {
            runeParent.SetActive(true);
            for (int i = 0; i < runeParent.transform.childCount; i++)
                runeParent.transform.GetChild(i).gameObject.SetActive(true);
        }

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

        if (altarAnimator != null)
            altarAnimator.SetTrigger("Activate");

        if (activateEffect != null)
            activateEffect.Play();

        // 🔊 Tạo loop sound và phát
        if (activateSound != null)
        {
            loopingAudioSource = new GameObject("Altar_LoopSound").AddComponent<AudioSource>();
            loopingAudioSource.clip = activateSound;
            loopingAudioSource.loop = true;
            loopingAudioSource.spatialBlend = 0f;
            loopingAudioSource.playOnAwake = false;
            loopingAudioSource.volume = 0f;
            loopingAudioSource.transform.position = transform.position;
            loopingAudioSource.Play();
        }

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
