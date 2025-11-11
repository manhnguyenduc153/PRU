using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Pathfinding;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Persistent GameObjects Tags")]
    [SerializeField]
    private List<string> persistentObjectTags = new List<string>
    {
        "Player",
        "CoinManager",
        "MainCanvas",
        "BuffManager",
        "AstarPath",
        "BossItemInventory",
        "SceneTransitionManager"
    };

    // --- SpawnPoint management ---
    private Dictionary<string, GameObject> persistentSpawnParents = new Dictionary<string, GameObject>();

    // Lưu trạng thái hasSpawned của SpawnZone
    private Dictionary<string, bool> spawnZoneStates = new Dictionary<string, bool>();

    // Lưu danh sách enemies đã spawn từ mỗi zone
    private Dictionary<string, List<GameObject>> spawnedEnemiesByZone = new Dictionary<string, List<GameObject>>();

    private string previousScene = "";
    private bool isFirstLoad = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePersistentObjects();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // --- Initialize persistent objects by tag ---
    private void InitializePersistentObjects()
    {
        foreach (string tag in persistentObjectTags)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects)
            {
                DontDestroyOnLoad(obj);
            }
        }
    }

    // --- Lưu trạng thái trước khi unload scene ---
    private void OnSceneUnloaded(Scene scene)
    {
        if (persistentSpawnParents.ContainsKey(scene.name))
        {
            SaveSpawnZoneStates(scene.name);
        }
    }

    // --- Scene loaded ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameOver")
        {
            CleanupForGameOver();
        }

        StartCoroutine(HandleSceneLoadedDelayed(scene));
    }

    private IEnumerator HandleSceneLoadedDelayed(Scene scene)
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        // --- Handle SpawnPoints ---
        HandleSceneSpawnPoints(scene);

        // --- Find Player ---
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Kiểm tra nếu đang load từ save game
            bool isLoadingFromSave = PlayerPrefs.GetInt("IsLoadingFromSave", 0) == 1;

            if (isLoadingFromSave)
            {
                Debug.Log("[GameManager] Loading from save - skipping spawn point logic");
                // KHÔNG di chuyển player, để SaveSystem xử lý
            }
            else
            {
                // Logic spawn point bình thường
                SceneSpawnPoint[] spawnPoints = FindObjectsOfType<SceneSpawnPoint>();
                Vector3 spawnPosition = player.transform.position;
                bool foundSpawn = false;

                // Check previous scene
                if (!isFirstLoad && !string.IsNullOrEmpty(previousScene))
                {
                    foreach (SceneSpawnPoint sp in spawnPoints)
                    {
                        if (!string.IsNullOrEmpty(sp.fromScene) && sp.fromScene.Equals(previousScene, System.StringComparison.OrdinalIgnoreCase))
                        {
                            spawnPosition = sp.transform.position;
                            foundSpawn = true;
                            break;
                        }
                    }
                }

                // Default spawn
                if (!foundSpawn)
                {
                    foreach (SceneSpawnPoint sp in spawnPoints)
                    {
                        if (sp.isDefaultSpawn)
                        {
                            spawnPosition = sp.transform.position;
                            foundSpawn = true;
                            break;
                        }
                    }
                }

                // Fallback to first spawn
                if (!foundSpawn && spawnPoints.Length > 0)
                    spawnPosition = spawnPoints[0].transform.position;

                // Move player
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                player.transform.position = spawnPosition;
            }

            // --- Cinemachine camera ---
            CinemachineStateDrivenCamera sdc = FindObjectOfType<CinemachineStateDrivenCamera>();
            if (sdc != null)
            {
                foreach (var cam in sdc.ChildCameras)
                {
                    if (cam != null)
                    {
                        cam.Follow = player.transform;
                        cam.OnTargetObjectWarped(player.transform, player.transform.position - cam.transform.position);
                    }
                }
            }
            else
            {
                CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
                if (vcam != null)
                {
                    vcam.Follow = player.transform;
                    vcam.OnTargetObjectWarped(player.transform, player.transform.position - vcam.transform.position);
                }
            }

            // --- A* rescan ---
            GameObject astarObj = GameObject.FindGameObjectWithTag("AstarPath");
            if (astarObj != null)
            {
                AstarPath astar = astarObj.GetComponent<AstarPath>();
                if (astar != null)
                    astar.Scan();
            }
        }

        previousScene = scene.name;
        isFirstLoad = false;
    }

    // --- Handle SpawnPoints per scene ---
    private void HandleSceneSpawnPoints(Scene scene)
    {
        string parentName = "AreaSpawnPoints_" + scene.name;
        GameObject parent = GameObject.Find(parentName);

        if (parent != null)
        {
            if (!persistentSpawnParents.ContainsKey(scene.name))
            {
                DontDestroyOnLoad(parent);
                persistentSpawnParents.Add(scene.name, parent);

                // Restore ngay hasSpawned cho các SpawnZone
                RestoreSpawnZoneStates(scene.name);
            }
            else
            {
                // Parent đã tồn tại, destroy bản duplicate mới từ scene
                Destroy(parent);
            }
        }
    }

    // --- Lưu trạng thái hasSpawned của tất cả SpawnZones ---
    private void SaveSpawnZoneStates(string sceneName)
    {
        if (!persistentSpawnParents.TryGetValue(sceneName, out GameObject parent))
            return;

        SpawnZone[] spawnZones = parent.GetComponentsInChildren<SpawnZone>(true);

        foreach (SpawnZone zone in spawnZones)
        {
            string key = GetSpawnZoneKey(sceneName, zone.gameObject);

            FieldInfo hasSpawnedField = typeof(SpawnZone).GetField("hasSpawned", BindingFlags.NonPublic | BindingFlags.Instance);
            if (hasSpawnedField != null)
            {
                bool hasSpawned = (bool)hasSpawnedField.GetValue(zone);
                spawnZoneStates[key] = hasSpawned;
            }

            FieldInfo spawnedEnemiesField = typeof(SpawnZone).GetField("spawnedEnemies", BindingFlags.NonPublic | BindingFlags.Instance);
            if (spawnedEnemiesField != null)
            {
                List<GameObject> enemies = (List<GameObject>)spawnedEnemiesField.GetValue(zone);
                if (enemies != null && enemies.Count > 0)
                {
                    spawnedEnemiesByZone[key] = new List<GameObject>(enemies);
                    foreach (GameObject enemy in enemies)
                        if (enemy != null) DontDestroyOnLoad(enemy);
                }
            }
        }
    }

    // --- Khôi phục trạng thái hasSpawned ---
    private void RestoreSpawnZoneStates(string sceneName)
    {
        if (!persistentSpawnParents.TryGetValue(sceneName, out GameObject parent))
            return;

        SpawnZone[] spawnZones = parent.GetComponentsInChildren<SpawnZone>(true);

        foreach (SpawnZone zone in spawnZones)
        {
            string key = GetSpawnZoneKey(sceneName, zone.gameObject);

            // Restore hasSpawned
            if (spawnZoneStates.ContainsKey(key))
            {
                FieldInfo hasSpawnedField = typeof(SpawnZone).GetField("hasSpawned", BindingFlags.NonPublic | BindingFlags.Instance);
                if (hasSpawnedField != null)
                    hasSpawnedField.SetValue(zone, spawnZoneStates[key]);
            }

            // Restore spawnedEnemies
            if (spawnedEnemiesByZone.ContainsKey(key))
            {
                FieldInfo spawnedEnemiesField = typeof(SpawnZone).GetField("spawnedEnemies", BindingFlags.NonPublic | BindingFlags.Instance);
                if (spawnedEnemiesField != null)
                {
                    List<GameObject> savedEnemies = spawnedEnemiesByZone[key];
                    List<GameObject> validEnemies = new List<GameObject>();
                    foreach (GameObject enemy in savedEnemies)
                    {
                        if (enemy != null)
                        {
                            validEnemies.Add(enemy);
                            SceneManager.MoveGameObjectToScene(enemy, SceneManager.GetActiveScene());
                        }
                    }
                    spawnedEnemiesField.SetValue(zone, validEnemies);
                }
            }
        }
    }

    // --- Kiểm tra SpawnZone đã spawn hay chưa ---
    public bool IsSpawned(SpawnZone zone)
    {
        string key = GetSpawnZoneKey(SceneManager.GetActiveScene().name, zone.gameObject);
        if (spawnZoneStates.TryGetValue(key, out bool hasSpawned))
            return hasSpawned;
        return false;
    }

    // --- Cập nhật trạng thái spawn ---
    public void SetSpawned(string key, bool spawned, List<GameObject> enemies)
    {
        spawnZoneStates[key] = spawned;
        if (enemies != null && enemies.Count > 0)
            spawnedEnemiesByZone[key] = new List<GameObject>(enemies);
    }

    // --- Tạo key duy nhất cho mỗi SpawnZone ---
    public string GetSpawnZoneKey(string sceneName, GameObject spawnZone)
    {
        Transform t = spawnZone.transform;
        return $"{sceneName}_{spawnZone.name}_{t.position.x:F2}_{t.position.y:F2}";
    }

    // --- Load scene ---
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;
    }

    // --- Add persistent tag at runtime ---
    public void AddPersistentTag(string tag)
    {
        if (!persistentObjectTags.Contains(tag))
        {
            persistentObjectTags.Add(tag);
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects)
            {
                DontDestroyOnLoad(obj);
            }
        }
    }

    public void DestroyAllPersistentObjectsExcept(List<string> keepTags = null)
    {
        if (keepTags == null) keepTags = new List<string>();

        foreach (string tag in persistentObjectTags)
        {
            // Nếu tag này nằm trong danh sách giữ lại thì skip
            if (keepTags.Contains(tag))
                continue;

            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objs)
            {
                Destroy(obj);
            }
        }

        // Xóa luôn trong dictionary quản lý SpawnParents
        persistentSpawnParents.Clear();
        spawnZoneStates.Clear();
        spawnedEnemiesByZone.Clear();
    }

    private void CleanupForGameOver()
    {
        // 1. Hủy object theo tag (persistentObjectTags)
        DestroyAllPersistentObjectsExcept(new List<string>()); // không giữ gì cả

        // 2. Hủy tất cả object còn lại trong scene DontDestroyOnLoad
        Scene ddolScene = SceneManager.GetSceneByName("DontDestroyOnLoad");
        if (ddolScene.IsValid())
        {
            GameObject[] rootObjects = ddolScene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                // Nếu muốn giữ GameManager, kiểm tra:
                if (obj != this.gameObject)
                    Destroy(obj);
            }
        }

        // 3. Xóa dictionary quản lý spawn
        persistentSpawnParents.Clear();
        spawnZoneStates.Clear();
        spawnedEnemiesByZone.Clear();
    }

}

public static class DontDestroyOnLoadHelper
{
    public static void DestroyAllDontDestroyOnLoad()
    {
        // Lấy Scene nội bộ chứa tất cả object DontDestroyOnLoad
        var currentAssembly = typeof(SceneManager).Assembly;
        var sceneManagerType = typeof(SceneManager);

        Scene ddolScene = SceneManager.GetSceneByName("DontDestroyOnLoad");

        if (ddolScene.IsValid())
        {
            GameObject[] rootObjects = ddolScene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                // Có thể thêm điều kiện giữ lại object cần thiết
                Object.Destroy(obj);
            }
        }
        else
        {
            Debug.LogWarning("DontDestroyOnLoad scene không hợp lệ!");
        }
    }
}

