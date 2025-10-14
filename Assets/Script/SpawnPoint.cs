using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Point ID")]
    public string spawnPointID; // ID duy nhất

    [Header("Visual (Optional)")]
    public Color gizmoColor = Color.green;

    void OnDrawGizmos()
    {
        // Hiển thị spawn point trong Scene view để dễ nhìn
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.5f);
    }
}