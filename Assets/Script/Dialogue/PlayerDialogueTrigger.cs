using UnityEngine;

public class PlayerDialogueTrigger : MonoBehaviour
{
    [Header("Cài đặt")]
    [Tooltip("Khoảng cách tối đa để tiếp tục hội thoại")]
    public float maxDialogueDistance = 3f;

    private NPCDialogue currentNPC;
    private Transform npcTransform;

    void Update()
    {
        // 🆕 Kiểm tra khoảng cách khi đang nói chuyện
        if (DialogueManager.Instance.IsTalking() && npcTransform != null)
        {
            float distance = Vector2.Distance(transform.position, npcTransform.position);

            if (distance > maxDialogueDistance)
            {
                Debug.Log($"🚶 Player đi xa quá ({distance:F1}m > {maxDialogueDistance}m), tắt hội thoại");
                DialogueManager.Instance.EndDialogue();
                currentNPC = null;
                npcTransform = null;
                return;
            }
        }

        // Khi ở gần NPC và nhấn E
        if (currentNPC != null && Input.GetKeyDown(KeyCode.E))
        {
            // Nếu chưa nói chuyện thì bắt đầu
            if (!DialogueManager.Instance.IsTalking())
            {
                Debug.Log($"▶️ Bắt đầu nói chuyện với {currentNPC.npcName}");
                DialogueManager.Instance.StartDialogue(
                    currentNPC.npcName,
                    currentNPC.dialogueLines
                );
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi player chạm vào NPC
        if (collision.CompareTag("NPC"))
        {
            currentNPC = collision.GetComponent<NPCDialogue>();

            if (currentNPC != null)
            {
                npcTransform = collision.transform; // 🆕 Lưu vị trí NPC
                Debug.Log($"🚶 Player đến gần {currentNPC.npcName}. Nhấn E để nói chuyện.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Khi player rời khỏi NPC
        if (collision.CompareTag("NPC"))
        {
            NPCDialogue npc = collision.GetComponent<NPCDialogue>();

            if (npc == currentNPC)
            {
                Debug.Log($"🚶 Player rời khỏi {currentNPC.npcName}");

                // 🆕 Nếu đang nói chuyện thì tắt luôn
                if (DialogueManager.Instance.IsTalking())
                {
                    Debug.Log("🔚 Tắt hội thoại vì rời xa NPC");
                    DialogueManager.Instance.EndDialogue();
                }

                currentNPC = null;
                npcTransform = null;
            }
        }
    }
}