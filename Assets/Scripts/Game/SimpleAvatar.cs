using UnityEngine;

/// <summary>
/// Basit avatar davranışı
/// Dönen ve yukarı-aşağı hareket eden bir obje
/// </summary>
public class SimpleAvatar : MonoBehaviour
{
    [Header("Dönme")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Yukarı-Aşağı Hareket")]
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.1f;

    [Header("Parıltı")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseMinScale = 0.9f;
    [SerializeField] private float pulseMaxScale = 1.1f;

    private Vector3 startPosition;
    private Vector3 baseScale;
    private float bobOffset;
    private float pulseOffset;

    private void Start()
    {
        startPosition = transform.localPosition;
        baseScale = transform.localScale;

        // Rastgele offset (birden fazla avatar varsa farklı hareket etsinler)
        bobOffset = Random.Range(0f, Mathf.PI * 2);
        pulseOffset = Random.Range(0f, Mathf.PI * 2);
    }

    private void Update()
    {
        // Dönme
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);

        // Yukarı-aşağı hareket
        if (enableBobbing)
        {
            float newY = startPosition.y + Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight;
            transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        }

        // Parıltı (pulse)
        if (enablePulse)
        {
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale,
                (Mathf.Sin((Time.time + pulseOffset) * pulseSpeed) + 1f) / 2f);
            transform.localScale = baseScale * scale;
        }
    }

    /// <summary>
    /// Avatar rengini ayarla
    /// </summary>
    public void SetColor(Color color)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Material instance oluştur (paylaşılan material'ı değiştirmemek için)
            renderer.material.color = color;
        }
    }

    /// <summary>
    /// Glow efekti ekle
    /// </summary>
    public void SetEmission(Color emissionColor, float intensity = 1f)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor * intensity);
        }
    }
}
