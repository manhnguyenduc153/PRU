using UnityEngine;

public class NPCTalkPrompt : MonoBehaviour
{
    public GameObject prompt;  // gắn TextMeshPro object ở đây
    private bool playerInRange = false;

    void Start()
    {
        if (prompt != null)
            prompt.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (prompt != null)
                prompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (prompt != null)
                prompt.SetActive(false);
        }
    }
}
