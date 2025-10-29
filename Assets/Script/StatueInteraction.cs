using UnityEngine;

public class StatueInteraction : MonoBehaviour
{
    [Header("References")]
    public GameObject rune;              // Child Rune
    public Transform spawnPoint;         // Child spawnPoint
    public GameObject bossPrefab;        // Prefab của boss
    public Canvas interactCanvas;        // Canvas chứa text "E - Summon"

    private bool playerInRange = false;
    private bool bossSpawned = false;

    private void Start()
    {
        if (rune != null)
            rune.SetActive(false);

        if (interactCanvas != null)
            interactCanvas.gameObject.SetActive(false); // ẩn text lúc đầu
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !bossSpawned)
        {
            ActivateRuneAndSpawnBoss();
        }
    }

    private void ActivateRuneAndSpawnBoss()
    {
        if (rune != null)
            rune.SetActive(true);

        if (bossPrefab != null && spawnPoint != null)
        {
            Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
            bossSpawned = true;
        }

        if (interactCanvas != null)
            interactCanvas.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactCanvas != null && !bossSpawned)
                interactCanvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactCanvas != null)
                interactCanvas.gameObject.SetActive(false);
        }
    }
}
