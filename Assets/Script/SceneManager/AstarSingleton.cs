using UnityEngine;
using Pathfinding;

public class AstarSingleton : MonoBehaviour
{
    private static AstarPath instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = GetComponent<AstarPath>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
