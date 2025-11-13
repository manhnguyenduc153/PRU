using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillSlot : MonoBehaviour
{
    [Header("UI References")]
    public Image skillIcon;
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI keyBindText;
    public TextMeshProUGUI manaCostText;

    [Header("Skill Data")]
    public SkillData skillData;

    [Header("Cast Settings")]
    public float castTime = 0.5f;
    public float castDelay = 1.5f;
    public float spawnDistance = 1.5f;

    [Header("Range Limit Settings")]
    public float maxCastRange = 10f;
    public bool showRangeWarning = true;

    [Header("Range Indicator Settings")]
    public bool showRangeCircle = true;
    public float rangeCircleRadius = 100f;
    public Color inRangeColor = new Color(0, 1, 0, 0.3f);
    public Color outOfRangeColor = new Color(1, 0, 0, 0.3f);

    [Header("Skill Speed Override")]
    public bool useCustomSpeed = true;
    public float customSkillSpeed = 0.5f;
    public float customAnimationSpeed = 0.1f;

    private float currentCooldown = 0f;
    private bool isOnCooldown = false;
    private bool isCasting = false;
    private PlayerMana playerMana;
    private Image rangeIndicator;

    private static bool isAnyCasting = false;
    private static float castEndTime = 0f;

    void Start()
    {
        playerMana = FindObjectOfType<PlayerMana>();
        if (playerMana == null)
        {
            Debug.LogError("PlayerMana not found in scene!");
        }

        if (skillData != null)
        {
            skillIcon.sprite = skillData.skillIcon;
            keyBindText.text = skillData.keyBinding.ToString();
            cooldownOverlay.fillAmount = 0f;
            cooldownText.gameObject.SetActive(false);

            if (manaCostText != null)
            {
                manaCostText.text = skillData.manaCost.ToString();
            }
        }

        if (showRangeCircle)
        {
            CreateRangeIndicator();
        }
    }

    void CreateRangeIndicator()
    {
        GameObject indicatorObj = new GameObject("RangeIndicator");

        // Tạo ở cùng cấp với Skill Slot (không phải con)
        Transform canvasTransform = transform.parent;
        indicatorObj.transform.SetParent(canvasTransform, false);

        rangeIndicator = indicatorObj.AddComponent<Image>();
        rangeIndicator.color = inRangeColor;

        // Tạo texture tròn mịn
        Texture2D circleTexture = CreateCircleTexture(128, 128);
        Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 100f);
        rangeIndicator.sprite = circleSprite;
        rangeIndicator.type = Image.Type.Simple;

        // Set kích thước RectTransform
        RectTransform rectTransform = indicatorObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rangeCircleRadius * 2, rangeCircleRadius * 2);

        // Đặt vị trí ở giữa Canvas hoặc vị trí cố định
        rectTransform.anchoredPosition = new Vector2(0, 0);

        Debug.Log("Range Indicator created successfully at center!");
    }

    Texture2D CreateCircleTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        float radius = width / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));

                // Tạo viền mịn (feathering)
                if (distance <= radius - 2)
                {
                    pixels[y * width + x] = Color.white;
                }
                else if (distance <= radius + 2)
                {
                    float alpha = 1f - ((distance - (radius - 2)) / 4f);
                    pixels[y * width + x] = new Color(1, 1, 1, Mathf.Clamp01(alpha));
                }
                else
                {
                    pixels[y * width + x] = new Color(1, 1, 1, 0);
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    void Update()
    {
        if (isAnyCasting && Time.time >= castEndTime)
        {
            isAnyCasting = false;
        }

        // Cập nhật màu của range indicator
        if (showRangeCircle && rangeIndicator != null)
        {
            if (IsMouseInRange())
            {
                rangeIndicator.color = inRangeColor;
            }
            else
            {
                rangeIndicator.color = outOfRangeColor;
            }
        }

        // Check key press
        if (Input.GetKeyDown(skillData.keyBinding) && !isOnCooldown && !isAnyCasting && !isCasting)
        {
            if (IsMouseInRange())
            {
                if (playerMana != null && playerMana.HasEnoughMana(skillData.manaCost))
                {
                    StartCoroutine(CastSkillWithDelay());
                }
                else
                {
                    Debug.Log("Not enough mana! Need " + skillData.manaCost + " mana.");
                }
            }
            else
            {
                if (showRangeWarning)
                {
                    Debug.Log($"Target is too far! Maximum range: {maxCastRange}");
                }
            }
        }

        // Update cooldown
        if (isOnCooldown)
        {
            currentCooldown -= Time.deltaTime;

            cooldownOverlay.fillAmount = currentCooldown / skillData.cooldownTime;
            cooldownText.text = Mathf.Ceil(currentCooldown).ToString();

            if (currentCooldown <= 0f)
            {
                EndCooldown();
            }
        }
    }

    bool IsMouseInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found!");
            return false;
        }

        // Dùng Raycast để tìm vị trí chính xác
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 mousePos = ray.origin + ray.direction * enter;
            float distance = Vector3.Distance(player.transform.position, mousePos);
            return distance <= maxCastRange;
        }

        return false;
    }

    IEnumerator CastSkillWithDelay()
    {
        isCasting = true;
        isAnyCasting = true;
        castEndTime = Time.time + castTime;

        Debug.Log($"Bắt đầu cast {skillData.skillName}...");

        if (playerMana != null)
        {
            playerMana.UseMana(skillData.manaCost);
        }

        yield return new WaitForSeconds(castDelay);

        SpawnSkill();

        StartCooldown();

        Debug.Log($"Skill đã được bắn! Có thể dùng skill khác sau {castTime} giây");

        isCasting = false;
    }

    void SpawnSkill()
    {
        Debug.Log("Spawning skill: " + skillData.skillName);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && skillData.skillPrefab != null)
        {
            // Dùng Raycast để tìm vị trí chuột chính xác
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

            Vector3 mousePos = Vector3.zero;
            if (groundPlane.Raycast(ray, out float enter))
            {
                mousePos = ray.origin + ray.direction * enter;
            }

            Vector3 spawnPos = player.transform.position;
            Vector2 direction = (mousePos - spawnPos).normalized;

            spawnPos += (Vector3)direction * spawnDistance;

            GameObject skillEffect = Instantiate(skillData.skillPrefab, spawnPos, Quaternion.identity);

            SkillEffect skillScript = skillEffect.GetComponent<SkillEffect>();
            if (skillScript != null)
            {
                skillScript.SetDirection(direction);
                skillScript.damage = skillData.damage;

                if (useCustomSpeed)
                {
                    skillScript.SetSpeed(customSkillSpeed);
                    skillScript.SetAnimationSpeed(customAnimationSpeed);
                    Debug.Log($"Set skill speed: {customSkillSpeed}, animation speed: {customAnimationSpeed}");
                }
            }

            Destroy(skillEffect, 10f);
        }
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        currentCooldown = skillData.cooldownTime;
        cooldownText.gameObject.SetActive(true);
        skillIcon.color = new Color(0.5f, 0.5f, 0.5f);
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