using UnityEngine;
public class RangeIndicator : MonoBehaviour
{
    [Header("References")]
    public SkillManager skillManager;
    public Transform player;
    [Header("Visual Settings")]
    public Color inRangeColor = new Color(0, 1, 0, 0.3f); // Xanh lá trong suốt
    public Color outRangeColor = new Color(1, 0, 0, 0.3f); // Đỏ trong suốt
    public float lineWidth = 0.1f;
    private LineRenderer circleRenderer;
    private LineRenderer mouseLineRenderer;
    private SpriteRenderer mouseIndicator;
    private int segments = 50;
    void Start()
    {
        CreateRangeCircle();
        CreateMouseLine();
        CreateMouseIndicator();
    }
    void CreateRangeCircle()
    {
        GameObject circleObj = new GameObject("RangeCircle");
        circleObj.transform.SetParent(player);
        circleObj.transform.localPosition = Vector3.zero;
        circleRenderer = circleObj.AddComponent<LineRenderer>();
        circleRenderer.positionCount = segments + 1;
        circleRenderer.useWorldSpace = false;
        circleRenderer.startWidth = lineWidth;
        circleRenderer.endWidth = lineWidth;
        circleRenderer.loop = true;
        // Material màu cyan
        circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        circleRenderer.startColor = Color.cyan;
        circleRenderer.endColor = Color.cyan;
        circleRenderer.enabled = false; // ẨN vòng tròn
        UpdateCirclePositions();
    }
    void CreateMouseLine()
    {
        GameObject lineObj = new GameObject("MouseLine");
        lineObj.transform.SetParent(transform);
        mouseLineRenderer = lineObj.AddComponent<LineRenderer>();
        mouseLineRenderer.positionCount = 2;
        mouseLineRenderer.startWidth = lineWidth * 0.5f;
        mouseLineRenderer.endWidth = lineWidth * 0.5f;
        mouseLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }
    void CreateMouseIndicator()
    {
        GameObject indicatorObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(indicatorObj.GetComponent<Collider>());
        indicatorObj.transform.SetParent(transform);
        indicatorObj.transform.localScale = Vector3.one * skillManager.indicatorSize;
        // ẨN Renderer mặc định của Sphere
        Renderer renderer = indicatorObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        mouseIndicator = indicatorObj.GetComponent<SpriteRenderer>();
        if (mouseIndicator == null)
        {
            mouseIndicator = indicatorObj.AddComponent<SpriteRenderer>();
        }
        mouseIndicator.enabled = false;
    }
    void Update()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(player.position, mouseWorldPos);
        bool isInRange = distance <= skillManager.maxSkillRange;
        // Cập nhật màu
        Color currentColor = isInRange ? inRangeColor : outRangeColor;
        if (mouseIndicator != null)
        {
            mouseIndicator.color = currentColor;
            mouseIndicator.transform.position = mouseWorldPos;
        }
        if (mouseLineRenderer != null)
        {
            mouseLineRenderer.SetPosition(0, player.position);
            mouseLineRenderer.SetPosition(1, mouseWorldPos);
            mouseLineRenderer.startColor = currentColor;
            mouseLineRenderer.endColor = currentColor;
        }
    }
    void UpdateCirclePositions()
    {
        float radius = skillManager.maxSkillRange;
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            circleRenderer.SetPosition(i, new Vector3(x, y, 0));
            angle += 2 * Mathf.PI / segments;
        }
    }
}