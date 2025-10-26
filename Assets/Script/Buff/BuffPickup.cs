using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    Slash,
    Lightning,
    TripleShot,
    Attack,
    Mana,
    Hp
}

public class BuffPickup : MonoBehaviour
{
    [SerializeField] private BuffType buffType;
    [SerializeField] private GameObject pickupEffect; // Optional: VFX khi nhặt buff
    [SerializeField] private AudioClip pickupSound;   // Optional: Sound khi nhặt buff
    [SerializeField] private float pickupDelay = 0.5f; // Thời gian delay trước khi nhặt

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Bắt đầu coroutine delay
        StartCoroutine(DelayedPickup());
    }

    private IEnumerator DelayedPickup()
    {
        yield return new WaitForSeconds(pickupDelay); // chờ 0.5 giây

        ActivateBuff();

        // Spawn pickup effect nếu có
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        // Play sound nếu có
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // Destroy buff pickup object
        Destroy(gameObject);
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
            case BuffType.Attack:
                BuffManager.Instance.AddAttackBuff();
                break;
            case BuffType.Mana:
                BuffManager.Instance.AddManaBuff();
                break;
            case BuffType.Hp:
                BuffManager.Instance.AddHpBuff();
                break;
        }
    }
}
