using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    [Tooltip("Scene mà player đến từ đó (để spawn đúng vị trí)")]
    public string fromScene = "";

    [Tooltip("Đánh dấu đây là spawn point mặc định của scene")]
    public bool isDefaultSpawn = false;

    private void OnDrawGizmos()
    {
        // Vẽ gizmo để dễ nhìn trong Editor
        Gizmos.color = isDefaultSpawn ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ label khi được chọn
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}