using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    Slash,
    Lightning,
    TripleShot
}

public class BuffPickup : MonoBehaviour
{
    [SerializeField] private BuffType buffType;
    [SerializeField] private GameObject pickupEffect; // Optional: VFX khi nhặt buff
    [SerializeField] private AudioClip pickupSound; // Optional: Sound khi nhặt buff

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check xem có phải player không
        if (other.CompareTag("Player"))
        {
            ActivateBuff();

            // Spawn pickup effect nếu có
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Play sound nếu có
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Destroy buff pickup object
            Destroy(gameObject);
        }
    }

    private void ActivateBuff()
    {
        if (BuffManager.Instance == null)
        {
            Debug.LogError("BuffManager not found!");
            return;
        }

        switch (buffType)
        {
            case BuffType.Slash:
                BuffManager.Instance.AddSlashBuff();
                break;
            case BuffType.Lightning:
                BuffManager.Instance.AddLightningBuff();
                break;
            case BuffType.TripleShot:
                BuffManager.Instance.AddTripleShotBuff();
                break;
        }
    }
}