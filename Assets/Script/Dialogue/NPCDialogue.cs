using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName = "Trưởng Làng";
    [TextArea(2, 5)]
    public List<string> dialogueLines = new List<string>()
    {
        "Từ thuở xa xưa, thế giới từng bị thống trị bởi Vua Bóng Tối – kẻ mang quyền năng hủy diệt và nuốt chửng ánh sáng.",
        "Bốn tay sai của hắn – Goblin King, Minotaur, Ancient Golem và Lich King – từng gieo rắc chết chóc khắp nơi.",
        "Các pháp sư cổ đại đã phong ấn chúng vào bốn trụ cổ, giam giữ cùng những viên Ngọc Ấn Ma.",
        "Giờ đây phong ấn đang yếu dần… bóng tối lại trỗi dậy.",
        "Con là người được chọn bởi Ánh Sáng, hãy lên đường phá phong ấn, tiêu diệt bốn quái vật và thu hồi bốn viên ngọc.",
        "Khi bốn viên ngọc hợp nhất tại Tế Đàn Tận Cùng, cánh cổng đến cõi của Vua Bóng Tối sẽ mở ra.",
        "Đó sẽ là trận chiến cuối cùng – định đoạt vận mệnh của thế giới này, anh hùng à."
    };
}
