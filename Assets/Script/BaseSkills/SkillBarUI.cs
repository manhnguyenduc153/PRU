using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillBarUI : MonoBehaviour
{
    [Header("References")]
    public SkillManager skillManager; // Kéo Player (có SkillManager) vào đây

    [Header("UI Skill Slots")]
    public List<SkillSlotUI> skillSlots = new List<SkillSlotUI>(); // Các slot UI

    void Start()
    {
        // Tự động tìm SkillManager nếu chưa gán
        if (skillManager == null)
        {
            skillManager = FindObjectOfType<SkillManager>();

            if (skillManager == null)
            {
                Debug.LogError("Không tìm thấy SkillManager! Hãy gán Player vào SkillBarUI.");
                return;
            }
        }

        // Khởi tạo UI cho từng skill
        InitializeSkillSlots();
    }

    void InitializeSkillSlots()
    {
        for (int i = 0; i < skillSlots.Count && i < skillManager.skills.Count; i++)
        {
            SkillData skill = skillManager.skills[i];
            SkillSlotUI slotUI = skillSlots[i];

            // Set icon và keybind text
            if (slotUI.skillIcon != null && skill.skillIcon != null)
            {
                slotUI.skillIcon.sprite = skill.skillIcon;
            }

            if (slotUI.keyBindText != null)
            {
                slotUI.keyBindText.text = skill.keyBinding.ToString();
            }

            // Ẩn cooldown overlay ban đầu
            if (slotUI.cooldownOverlay != null)
            {
                slotUI.cooldownOverlay.fillAmount = 0f;
            }

            if (slotUI.cooldownText != null)
            {
                slotUI.cooldownText.text = "";
            }
        }
    }

    void Update()
    {
        if (skillManager == null) return;

        // Cập nhật UI cooldown cho từng skill
        for (int i = 0; i < skillSlots.Count && i < skillManager.skills.Count; i++)
        {
            SkillData skill = skillManager.skills[i];
            SkillSlotUI slotUI = skillSlots[i];

            float remainingCooldown = skillManager.GetCooldown(skill.keyBinding);
            bool isReady = skillManager.IsSkillReady(skill.keyBinding);

            UpdateSlotUI(slotUI, skill, remainingCooldown, isReady);
        }
    }

    void UpdateSlotUI(SkillSlotUI slotUI, SkillData skill, float remainingCooldown, bool isReady)
    {
        // Cập nhật overlay (fill image)
        if (slotUI.cooldownOverlay != null)
        {
            if (isReady)
            {
                slotUI.cooldownOverlay.fillAmount = 0f; // Skill sẵn sàng
            }
            else
            {
                float fillAmount = remainingCooldown / skill.cooldownTime;
                slotUI.cooldownOverlay.fillAmount = fillAmount;
            }
        }

        // Cập nhật text cooldown
        if (slotUI.cooldownText != null)
        {
            if (isReady)
            {
                slotUI.cooldownText.text = "";
            }
            else
            {
                slotUI.cooldownText.text = remainingCooldown.ToString("F1") + "s";
            }
        }

        // Làm mờ icon khi cooldown (optional)
        if (slotUI.skillIcon != null)
        {
            Color iconColor = slotUI.skillIcon.color;
            iconColor.a = isReady ? 1f : 0.5f;
            slotUI.skillIcon.color = iconColor;
        }
    }
}

// Class chứa các UI elements của 1 skill slot
[System.Serializable]
public class SkillSlotUI
{
    public Image skillIcon;           // Icon của skill
    public Image cooldownOverlay;     // Overlay tối màu khi cooldown (Image type: Filled)
    public TextMeshProUGUI cooldownText;  // Text hiển thị thời gian cooldown
    public TextMeshProUGUI keyBindText;   // Text hiển thị phím tắt (Q, W, E...)
}