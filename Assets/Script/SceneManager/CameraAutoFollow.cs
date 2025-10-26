using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class CameraAutoFollow : MonoBehaviour
{
    private CinemachineStateDrivenCamera stateDrivenCamera;
    private CinemachineVirtualCamera virtualCamera;

    private void Awake()
    {
        stateDrivenCamera = GetComponent<CinemachineStateDrivenCamera>();
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        AssignFollowTarget();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignFollowTarget();
    }

    private void AssignFollowTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (stateDrivenCamera != null)
        {
            foreach (var cam in stateDrivenCamera.ChildCameras)
            {
                if (cam != null)
                {
                    cam.Follow = player.transform;
                    cam.OnTargetObjectWarped(player.transform, Vector3.zero);
                }
            }
        }

        if (virtualCamera != null)
        {
            virtualCamera.Follow = player.transform;
            virtualCamera.OnTargetObjectWarped(player.transform, Vector3.zero);
        }

        Debug.Log($"🎥 Camera in scene '{SceneManager.GetActiveScene().name}' is now following: {player.name}");

        var confiner = GetComponent<CinemachineConfiner2D>();
        if (confiner != null)
            confiner.InvalidateCache();

    }
}
