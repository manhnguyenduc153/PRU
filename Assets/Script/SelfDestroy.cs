using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [Tooltip("Thời gian trước khi object tự hủy (giây)")]
    public float destroyAfter = 1.5f;

    private void Start()
    {
        Destroy(gameObject, destroyAfter);
    }
}
