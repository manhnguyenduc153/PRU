using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName = "Người Lạ";
    [TextArea(2, 5)]
    public List<string> dialogueLines = new List<string>()
    {
        "Xin chào, ta là người canh giữ vùng đất này.",
        "Để chiến thắng, con phải tìm được 3 viên đá cổ xưa.",
        "Chúng nằm rải rác trong khu rừng tối phía bắc.",
        "Hãy cẩn thận... bóng tối không dễ gì bị khuất phục."
    };
}
