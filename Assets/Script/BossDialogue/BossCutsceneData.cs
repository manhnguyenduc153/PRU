using UnityEngine;

[System.Serializable]
public class BossCutsceneData
{
    public EnemyHealth bossHealth;       // Boss cụ thể cho trigger này
    [TextArea(3, 10)]
    public string[] dialogueLines;       // Các dòng thoại cho boss
    public string[] speakerNames;        // Tên người nói tương ứng
}
