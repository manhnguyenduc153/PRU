using UnityEngine;
public class MinimapCamera : MonoBehaviour
{
    public Transform player;
    public float heightOffset = 20f;

    void Start()
    {
        // Tự động tìm player nếu chưa assign
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.z = -heightOffset;
            transform.position = newPosition;
        }
    }
}