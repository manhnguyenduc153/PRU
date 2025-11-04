using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject cutscenePanel;
    public Image topBlackBar;
    public Image bottomBlackBar;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public GameObject continueIndicator;

    [Header("Animation Settings")]
    public float barAnimationDuration = 0.8f;
    public float textTypeSpeed = 0.05f;

    [Header("Input")]
    public KeyCode continueKey = KeyCode.Space;

    private string[] currentDialogues;
    private string[] currentSpeakers;
    private int currentDialogueIndex;
    private bool isTyping;
    private bool cutsceneActive;
    private Coroutine typingCoroutine;
    private EnemyHealth currentBossHealth;
    private EnemyHealthUI bossHealthUI;
    private CinemachineVirtualCamera cutsceneVCam;
    private Transform playerTransform;

    private float topBarOriginalHeight;
    private float bottomBarOriginalHeight;

    private void Start()
    {
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);
    }

    private void Update()
    {
        if (!cutsceneActive) return;

        if (Input.GetKeyDown(continueKey))
        {
            if (isTyping)
            {
                StopTyping();
                dialogueText.text = currentDialogues[currentDialogueIndex];
                isTyping = false;
                ShowContinueIndicator(true);
            }
            else
            {
                NextDialogue();
            }
        }
    }

    public void StartBossCutscene(string[] dialogues, string[] speakers, EnemyHealth bossHealth, CinemachineVirtualCamera vcam, Transform player)
    {
        currentDialogues = dialogues;
        currentSpeakers = speakers;
        currentDialogueIndex = 0;
        currentBossHealth = bossHealth;
        cutsceneVCam = vcam;
        playerTransform = player;

        // Hide boss health UI
        bossHealthUI = FindObjectOfType<EnemyHealthUI>();
        if (bossHealthUI != null)
            bossHealthUI.HideBossUI();

        // Disable boss AI
        if (bossHealth != null)
        {
            var enemyAI = bossHealth.GetComponent<EnemyAI>();
            if (enemyAI != null) enemyAI.enabled = false;

            var rb = bossHealth.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.simulated = false;
            }
        }

        // Disable player
        var playerComp = playerTransform?.GetComponent<PlayerController>();
        if (playerComp != null)
            playerComp.enabled = false;

        // Dừng game
        Time.timeScale = 0f;
        cutsceneActive = true;

        // Hiện UI
        cutscenePanel.SetActive(true);

        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        topBarOriginalHeight = topBlackBar.rectTransform.sizeDelta.y;
        bottomBarOriginalHeight = bottomBlackBar.rectTransform.sizeDelta.y;

        dialogueText.text = "";
        speakerNameText.text = "";
        ShowContinueIndicator(false);

        // Animate top & bottom bars
        yield return StartCoroutine(AnimateBars(true));

        ShowDialogue(0);
    }

    private void ShowDialogue(int index)
    {
        if (index >= currentDialogues.Length)
        {
            EndCutscene();
            return;
        }

        currentDialogueIndex = index;

        if (currentSpeakers != null && index < currentSpeakers.Length)
            speakerNameText.text = currentSpeakers[index];

        ShowContinueIndicator(false);
        typingCoroutine = StartCoroutine(TypeText(currentDialogues[index]));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(textTypeSpeed);
        }

        isTyping = false;
        ShowContinueIndicator(true);
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private void NextDialogue()
    {
        ShowDialogue(currentDialogueIndex + 1);
    }

    private void EndCutscene()
    {
        StartCoroutine(EndCutsceneCoroutine());
    }

    private IEnumerator EndCutsceneCoroutine()
    {
        ShowContinueIndicator(false);
        yield return StartCoroutine(AnimateBars(false));

        cutscenePanel.SetActive(false);

        // Camera focus lại player
        if (cutsceneVCam != null && playerTransform != null)
            cutsceneVCam.Follow = playerTransform;

        // Show boss health UI
        if (bossHealthUI != null)
            bossHealthUI.ShowBossUI();

        // Enable boss AI
        if (currentBossHealth != null)
        {
            var enemyAI = currentBossHealth.GetComponent<EnemyAI>();
            if (enemyAI != null) enemyAI.enabled = true;

            var rb = currentBossHealth.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = true;
        }

        // Enable player
        var playerComp = playerTransform?.GetComponent<PlayerController>();
        if (playerComp != null)
            playerComp.enabled = true;

        cutsceneActive = false;
        Time.timeScale = 1f;
    }

    private IEnumerator AnimateBars(bool slideIn)
    {
        float elapsed = 0f;
        RectTransform topRect = topBlackBar.rectTransform;
        RectTransform bottomRect = bottomBlackBar.rectTransform;

        float topStart = slideIn ? 0f : topBarOriginalHeight;
        float topEnd = slideIn ? topBarOriginalHeight : 0f;
        float bottomStart = slideIn ? 0f : bottomBarOriginalHeight;
        float bottomEnd = slideIn ? bottomBarOriginalHeight : 0f;

        while (elapsed < barAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / barAnimationDuration);

            topRect.sizeDelta = new Vector2(topRect.sizeDelta.x, Mathf.Lerp(topStart, topEnd, t));
            bottomRect.sizeDelta = new Vector2(bottomRect.sizeDelta.x, Mathf.Lerp(bottomStart, bottomEnd, t));

            yield return null;
        }

        topRect.sizeDelta = new Vector2(topRect.sizeDelta.x, topEnd);
        bottomRect.sizeDelta = new Vector2(bottomRect.sizeDelta.x, bottomEnd);
    }

    private void ShowContinueIndicator(bool show)
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(show);
    }
}
