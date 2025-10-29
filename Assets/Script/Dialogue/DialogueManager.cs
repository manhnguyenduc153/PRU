using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("⚠️ KÉO THẢ THỦ CÔNG TẤT CẢ Ở ĐÂY")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public Button exitButton;

    private Queue<string> sentences = new Queue<string>();
    private bool isTalking = false;

    void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Kiểm tra đã gán đủ chưa
        if (dialoguePanel == null || nameText == null || dialogueText == null ||
            continueButton == null || exitButton == null)
        {
            Debug.LogError("❌ CHƯA GÁN ĐỦ REFERENCES TRONG INSPECTOR!");
            return;
        }

        // Gắn sự kiện cho button
        continueButton.onClick.AddListener(OnContinueClick);
        exitButton.onClick.AddListener(OnExitClick);

        // Ẩn panel
        dialoguePanel.SetActive(false);

        Debug.Log("✅ DialogueManager đã khởi tạo thành công");
    }

    // HÀM NÀY ĐƯỢC GỌI KHI BẤM CONTINUE
    public void OnContinueClick()
    {
        Debug.Log("🔔 Continue Button Clicked!");
        DisplayNextSentence();
    }

    // HÀM NÀY ĐƯỢC GỌI KHI BẤM EXIT
    public void OnExitClick()
    {
        Debug.Log("🔔 Exit Button Clicked!");
        EndDialogue();
    }

    // Bắt đầu hội thoại
    public void StartDialogue(string npcName, List<string> dialogueLines)
    {
        if (isTalking)
        {
            Debug.Log("⚠️ Đang nói chuyện rồi!");
            return;
        }

        Debug.Log($"🎬 Bắt đầu hội thoại với {npcName}");

        isTalking = true;
        dialoguePanel.SetActive(true);

        nameText.text = npcName;

        sentences.Clear();
        foreach (string line in dialogueLines)
        {
            sentences.Enqueue(line);
        }

        Debug.Log($"📝 Đã load {sentences.Count} câu thoại");

        DisplayNextSentence();
    }

    // Hiển thị câu tiếp theo
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            Debug.Log("⚠️ Hết câu, kết thúc hội thoại");
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;

        Debug.Log($"💬 Hiển thị: \"{sentence}\" (Còn {sentences.Count} câu)");
    }

    // Kết thúc hội thoại
    public void EndDialogue()
    {
        Debug.Log("🔚 Kết thúc hội thoại");

        dialoguePanel.SetActive(false);
        isTalking = false;
        sentences.Clear();
    }

    public bool IsTalking()
    {
        return isTalking;
    }

    // TEST BẰNG PHÍM (không cần button)
    void Update()
    {
        if (isTalking)
        {
            // Nhấn Space = Next
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("🧪 Test: Space pressed");
                DisplayNextSentence();
            }

            // Nhấn Escape = Exit
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("🧪 Test: Escape pressed");
                EndDialogue();
            }
        }
    }
}