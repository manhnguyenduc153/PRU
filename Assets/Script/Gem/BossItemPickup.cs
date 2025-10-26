using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossItemPickup : MonoBehaviour
{
    [SerializeField] private BossItemType bossItemType;
    [SerializeField] private GameObject pickupEffect; // VFX khi nhặt item
    [SerializeField] private AudioClip pickupSound;   // Sound khi nhặt item
    [SerializeField] private float pickupDelay = 0.5f; // Delay trước khi nhặt

    [Header("Visual Settings")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private bool rotateItem = true;
    [SerializeField] private float rotationSpeed = 50f;

    private Vector3 startPosition;
    private bool isPickedUp = false;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!isPickedUp)
        {
            // Hiệu ứng lơ lửng
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);

            // Hiệu ứng xoay
            if (rotateItem)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;

        if (!other.CompareTag("Player")) return;

        isPickedUp = true;
        StartCoroutine(DelayedPickup());
    }

    private IEnumerator DelayedPickup()
    {
        yield return new WaitForSeconds(pickupDelay);

        CollectItem();

        // Spawn pickup effect
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        // Play sound
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // Destroy item object
        Destroy(gameObject);
    }

    private void CollectItem()
    {
        if (BossItemInventory.Instance == null)
        {
            Debug.LogError("BossItemInventory not found!");
            return;
        }

        BossItemInventory.Instance.AddBossItem(bossItemType);
        BossItemInventory.Instance.DebugInventory();
    }
}