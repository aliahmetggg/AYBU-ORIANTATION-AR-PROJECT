using UnityEngine;

/// <summary>
/// Belirli GPS konumunda yıldız spawn eder
/// AR görsel gerektirmez, sadece GPS yeterli
/// </summary>
public class GPSStarSpawner : MonoBehaviour
{
    [Header("Hedef GPS Konumu")]
    public double targetLatitude = 39.970835;
    public double targetLongitude = 32.818170;
    public float activationRadius = 30f; // Metre

    [Header("Yıldız Ayarları")]
    public float starSize = 2.5f;
    public Color starColor = new Color(1f, 0.85f, 0f); // Altın
    public float spawnDistance = 3f; // Kameradan uzaklık
    public float spawnHeight = 1.5f; // Yerden yükseklik

    [Header("Debug")]
    public bool debugMode = true;

    private GameObject spawnedStar;
    private bool isInRange = false;
    private bool hasCollected = false;

    private const string COLLECTED_KEY = "AYBU_GoldStar_Collected";

    private void Start()
    {
        // Daha önce toplanmış mı kontrol et
        hasCollected = PlayerPrefs.GetInt(COLLECTED_KEY, 0) == 1;

        if (GPSManager.Instance != null)
        {
            GPSManager.Instance.OnLocationUpdated += OnGPSUpdated;
        }

        if (debugMode)
        {
            Debug.Log($"GPSStarSpawner: Hedef konum ({targetLatitude}, {targetLongitude}), Yarıçap: {activationRadius}m");
        }
    }

    private void OnDestroy()
    {
        if (GPSManager.Instance != null)
        {
            GPSManager.Instance.OnLocationUpdated -= OnGPSUpdated;
        }
    }

    private void OnGPSUpdated(double lat, double lon)
    {
        if (hasCollected) return;

        bool wasInRange = isInRange;
        isInRange = GPSManager.Instance.IsWithinRadius(targetLatitude, targetLongitude, activationRadius);

        if (debugMode && isInRange != wasInRange)
        {
            Debug.Log($"GPSStarSpawner: Alan içinde = {isInRange}");
        }

        if (isInRange && spawnedStar == null)
        {
            SpawnStar();
        }
        else if (!isInRange && spawnedStar != null)
        {
            HideStar();
        }
    }

    private void SpawnStar()
    {
        if (spawnedStar != null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Kameranın önünde spawn et
        Vector3 spawnPos = mainCam.transform.position + mainCam.transform.forward * spawnDistance;
        spawnPos.y = spawnHeight;

        spawnedStar = new GameObject("GoldStar");
        spawnedStar.transform.position = spawnPos;

        // Yıldız görselini ekle
        StarAvatarGenerator starGen = spawnedStar.AddComponent<StarAvatarGenerator>();
        starGen.size = starSize;
        starGen.starColor = starColor;
        starGen.glowColor = starColor * 1.3f;
        starGen.addFloating = true;
        starGen.floatAmount = 0.3f;
        starGen.SetupStarVisual();

        // Toplanabilir yap
        StarCollectable collectable = spawnedStar.AddComponent<StarCollectable>();
        collectable.Initialize(this);

        if (debugMode)
        {
            Debug.Log("GPSStarSpawner: Altın yıldız spawn edildi!");
        }
    }

    private void HideStar()
    {
        if (spawnedStar != null)
        {
            spawnedStar.SetActive(false);
        }
    }

    public void CollectStar()
    {
        hasCollected = true;
        PlayerPrefs.SetInt(COLLECTED_KEY, 1);
        PlayerPrefs.Save();

        if (spawnedStar != null)
        {
            Destroy(spawnedStar);
            spawnedStar = null;
        }

        // Puan ekle
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(50); // Bonus puan
        }

        if (debugMode)
        {
            Debug.Log("GPSStarSpawner: Altın yıldız toplandı! +50 puan");
        }
    }

    // Debug için sıfırlama
    [ContextMenu("Reset Star Collection")]
    public void ResetCollection()
    {
        PlayerPrefs.DeleteKey(COLLECTED_KEY);
        hasCollected = false;
        Debug.Log("GPSStarSpawner: Yıldız sıfırlandı");
    }
}

/// <summary>
/// Yıldız için toplanabilir component
/// </summary>
public class StarCollectable : MonoBehaviour
{
    private GPSStarSpawner spawner;
    private bool isCollectable = true;

    public void Initialize(GPSStarSpawner s)
    {
        spawner = s;
    }

    public void Collect()
    {
        if (!isCollectable) return;
        isCollectable = false;

        StartCoroutine(CollectAnimation());
    }

    private System.Collections.IEnumerator CollectAnimation()
    {
        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Büyü, döndür ve yukarı git
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.5f, t * 0.5f);
            transform.position = startPos + Vector3.up * t * 2f;
            transform.Rotate(Vector3.up, 720 * Time.deltaTime); // Hızlı dön

            // Sonra küçül
            if (t > 0.5f)
            {
                transform.localScale = Vector3.Lerp(startScale * 1.5f, Vector3.zero, (t - 0.5f) * 2f);
            }

            yield return null;
        }

        spawner.CollectStar();
    }

    private void OnMouseDown()
    {
        Collect();
    }
}
