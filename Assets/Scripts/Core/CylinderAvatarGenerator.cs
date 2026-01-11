using UnityEngine;

/// <summary>
/// Runtime'da silindir avatar oluşturur
/// AR görsel algılandığında bu avatar spawn edilir
/// </summary>
public class CylinderAvatarGenerator : MonoBehaviour
{
    [Header("Silindir Ayarları")]
    public float height = 0.15f;     // 15cm yükseklik - AR için uygun
    public float radius = 0.04f;     // 4cm yarıçap
    public Color avatarColor = new Color(0.2f, 0.8f, 0.4f); // Yeşil
    public bool addRotation = true;
    public float rotationSpeed = 50f;

    [Header("Efektler")]
    public bool addGlow = true;
    public Color glowColor = new Color(0.4f, 1f, 0.6f);

    private void Start()
    {
        // Eğer bu bir prefab olarak kullanılıyorsa, görsel oluştur
        if (GetComponent<MeshFilter>() == null)
        {
            SetupCylinderVisual();
        }
    }

    private void Update()
    {
        if (addRotation)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Silindir görselini oluştur
    /// </summary>
    public void SetupCylinderVisual()
    {
        // Silindir mesh
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(transform, false);
        cylinder.transform.localPosition = new Vector3(0, height / 2, 0);
        cylinder.transform.localScale = new Vector3(radius * 2, height / 2, radius * 2);

        // Material
        Renderer renderer = cylinder.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = avatarColor;

            // Emission (glow efekti)
            if (addGlow)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", glowColor * 0.5f);
            }

            renderer.material = mat;
        }

        // Collider'ı ayarla (tıklama için)
        Collider col = cylinder.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Child objenin adını değiştir
        cylinder.name = "CylinderMesh";
    }

    /// <summary>
    /// Statik: Yeni bir silindir avatar GameObject'i oluştur
    /// </summary>
    public static GameObject CreateCylinderAvatar(string name = "CylinderAvatar", Color? color = null)
    {
        GameObject avatar = new GameObject(name);

        CylinderAvatarGenerator generator = avatar.AddComponent<CylinderAvatarGenerator>();
        if (color.HasValue)
        {
            generator.avatarColor = color.Value;
            generator.glowColor = color.Value * 1.2f;
        }
        generator.SetupCylinderVisual();

        return avatar;
    }
}
