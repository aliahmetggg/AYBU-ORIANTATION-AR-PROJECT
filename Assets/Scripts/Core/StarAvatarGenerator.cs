using UnityEngine;

/// <summary>
/// Runtime'da yıldız avatar oluşturur
/// GPS konumunda bu avatar spawn edilir
/// </summary>
public class StarAvatarGenerator : MonoBehaviour
{
    [Header("Yıldız Ayarları")]
    public float size = 2f;              // Yıldız boyutu
    public int points = 5;               // Köşe sayısı
    public float innerRadius = 0.4f;     // İç yarıçap oranı
    public Color starColor = new Color(1f, 0.85f, 0f); // Altın sarısı
    public bool addRotation = true;
    public float rotationSpeed = 30f;

    [Header("Efektler")]
    public bool addGlow = true;
    public Color glowColor = new Color(1f, 0.95f, 0.5f);
    public bool addFloating = true;
    public float floatSpeed = 1f;
    public float floatAmount = 0.2f;

    private Vector3 startPosition;
    private GameObject starMesh;

    private void Start()
    {
        startPosition = transform.position;

        if (GetComponentInChildren<MeshFilter>() == null)
        {
            SetupStarVisual();
        }
    }

    private void Update()
    {
        if (addRotation && starMesh != null)
        {
            starMesh.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        if (addFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    /// <summary>
    /// Yıldız görselini oluştur
    /// </summary>
    public void SetupStarVisual()
    {
        // Ana yıldız objesi
        starMesh = new GameObject("StarMesh");
        starMesh.transform.SetParent(transform, false);
        starMesh.transform.localPosition = new Vector3(0, size / 2, 0);

        // Mesh oluştur
        MeshFilter meshFilter = starMesh.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = starMesh.AddComponent<MeshRenderer>();

        meshFilter.mesh = CreateStarMesh();

        // Material
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = starColor;

        if (addGlow)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * 0.8f);
        }

        meshRenderer.material = mat;

        // Collider ekle
        MeshCollider collider = starMesh.AddComponent<MeshCollider>();
        collider.convex = true;
        collider.isTrigger = true;

        // Arka yüz için ikinci bir yıldız (çift taraflı görünüm)
        GameObject backStar = Instantiate(starMesh, transform);
        backStar.name = "StarMeshBack";
        backStar.transform.localRotation = Quaternion.Euler(0, 180, 0);
        Destroy(backStar.GetComponent<MeshCollider>());
    }

    /// <summary>
    /// Yıldız mesh'i oluştur
    /// </summary>
    private Mesh CreateStarMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "StarMesh";

        int vertexCount = points * 2 + 1; // Dış + iç noktalar + merkez
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[points * 2 * 3];

        // Merkez nokta
        vertices[0] = Vector3.zero;

        float outerRadius = size / 2;
        float inner = outerRadius * innerRadius;

        // Yıldız noktalarını oluştur
        for (int i = 0; i < points * 2; i++)
        {
            float angle = (i * Mathf.PI / points) - (Mathf.PI / 2); // Üstten başla
            float radius = (i % 2 == 0) ? outerRadius : inner;

            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
        }

        // Üçgenleri oluştur
        for (int i = 0; i < points * 2; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex] = 0; // Merkez
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = (i + 1) % (points * 2) + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Statik: Yeni bir yıldız avatar GameObject'i oluştur
    /// </summary>
    public static GameObject CreateStarAvatar(string name = "StarAvatar", float size = 2f, Color? color = null)
    {
        GameObject avatar = new GameObject(name);

        StarAvatarGenerator generator = avatar.AddComponent<StarAvatarGenerator>();
        generator.size = size;

        if (color.HasValue)
        {
            generator.starColor = color.Value;
            generator.glowColor = color.Value * 1.2f;
        }

        generator.SetupStarVisual();

        return avatar;
    }
}
