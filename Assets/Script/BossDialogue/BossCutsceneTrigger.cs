using UnityEngine;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class BossCutsceneTrigger : MonoBehaviour
{
    [Header("Cutscene Settings")]
    public bool triggerOnce = true;
    private bool hasTriggered = false;

    [Header("Boss Settings")]
    public EnemyHealth bossHealth;
    [TextArea(3, 10)]
    public string[] dialogueLines;
    public string[] speakerNames;

    [Header("Camera")]
    public CinemachineVirtualCamera cutsceneVCam;

    private Transform playerTransform;

    private void Start()
    {
        // --- Lấy player để focus lại sau cutscene ---
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // --- Nếu camera chưa gán (do prefab), tự động tìm trong scene ---
        if (cutsceneVCam == null)
        {
            cutsceneVCam = FindObjectOfType<CinemachineVirtualCamera>();
            if (cutsceneVCam == null)
                Debug.LogError("❌ No CinemachineVirtualCamera found in scene!");
        }

        // --- Nếu bossHealth chưa gán (ví dụ prefab spawn boss) ---
        if (bossHealth == null)
        {
            bossHealth = GetComponentInParent<EnemyHealth>();
            if (bossHealth == null)
                Debug.LogError("❌ BossCutsceneTrigger: Missing EnemyHealth reference!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            hasTriggered = true;
            StartCutscene();
        }
    }

    private void StartCutscene()
    {
        if (bossHealth == null)
        {
            Debug.LogError("❌ BossHealth not assigned in " + name);
            return;
        }

        // --- Focus camera vào boss ---
        if (cutsceneVCam != null)
        {
            cutsceneVCam.Follow = bossHealth.transform;
            cutsceneVCam.LookAt = null;
            cutsceneVCam.gameObject.SetActive(true);
        }

        // --- Delay để camera focus hoàn toàn ---
        StartCoroutine(StartCutsceneAfterCamera());
    }

    private IEnumerator StartCutsceneAfterCamera()
    {
        yield return new WaitForSecondsRealtime(0.3f); // Delay cho camera focus

        // --- Gọi CutsceneManager ---
        CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();
        if (cutsceneManager != null)
        {
            cutsceneManager.StartBossCutscene(
                dialogueLines,
                speakerNames,
                bossHealth,
                cutsceneVCam,
                playerTransform // Truyền player để cutsceneManager focus lại sau
            );
        }
        else
        {
            Debug.LogError("❌ CutsceneManager not found in scene!");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
            Gizmos.DrawCube(transform.position + (Vector3)boxCol.offset, boxCol.size);
    }
}
