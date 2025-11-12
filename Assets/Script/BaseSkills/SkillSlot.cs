using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlot : MonoBehaviour
{
    [Header("UI References")]
    public Image skillIcon;
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI keyBindText;

    [Header("Skill Data")]
    public SkillData skillData;

    private float currentCooldown = 0f;
    private bool isOnCooldown = false;

    void Start()
    {
        if (skillData != null)
        {
            skillIcon.sprite = skillData.skillIcon;
            keyBindText.text = skillData.keyBinding.ToString();
            cooldownOverlay.fillAmount = 0f;
            cooldownText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Check key press
        if (Input.GetKeyDown(skillData.keyBinding) && !isOnCooldown)
        {
            UseSkill();
        }

        // Update cooldown
        if (isOnCooldown)
        {
            currentCooldown -= Time.deltaTime;

            // Update UI
            cooldownOverlay.fillAmount = currentCooldown / skillData.cooldownTime;
            cooldownText.text = Mathf.Ceil(currentCooldown).ToString();

            if (currentCooldown <= 0f)
            {
                EndCooldown();
            }
        }
    }

    void UseSkill()
    {
        Debug.Log("Used skill: " + skillData.skillName);

        // Spawn skill effect at player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && skillData.skillPrefab != null)
        {
            Vector3 spawnPos = player.transform.position;
            GameObject skillEffect = Instantiate(skillData.skillPrefab, spawnPos, Quaternion.identity);

            // Destroy effect after animation
            Destroy(skillEffect, 2f);
        }

        // Start cooldown
        StartCooldown();
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        currentCooldown = skillData.cooldownTime;
        cooldownText.gameObject.SetActive(true);
        skillIcon.color = new Color(0.5f, 0.5f, 0.5f); // Dim the icon
    }

    void EndCooldown()
    {
        isOnCooldown = false;
        currentCooldown = 0f;
        cooldownOverlay.fillAmount = 0f;
        cooldownText.gameObject.SetActive(false);
        skillIcon.color = Color.white;
    }
}