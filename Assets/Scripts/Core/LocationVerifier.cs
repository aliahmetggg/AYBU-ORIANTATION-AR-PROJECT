using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// GPS + AR birleştirme sistemi
/// Avatar sadece her iki koşul da sağlandığında görünür:
/// 1. GPS lokasyonu doğru (radius içinde)
/// 2. AR referans görseli algılandı
/// </summary>
public class LocationVerifier : MonoBehaviour
{
    public static LocationVerifier Instance { get; private set; }

    [Header("Veri")]
    [SerializeField] private CampusLocationsData campusData;

    [Header("AR Referans")]
    [SerializeField] private ARTrackedImageManager imageManager;

    [Header("Ayarlar")]
    [SerializeField] private bool requireBothConditions = true; // AND mantığı
    [SerializeField] private bool debugMode = true;

    [Header("Test Modu")]
    [Tooltip("True yapınca ekrana dokunarak avatar spawn edebilirsiniz")]
    [SerializeField] private bool testModeEnabled = false;

    // Mevcut durumlar
    private Dictionary<string, bool> gpsVerified = new Dictionary<string, bool>();
    private Dictionary<string, bool> arVerified = new Dictionary<string, bool>();
    private Dictionary<string, GameObject> spawnedAvatars = new Dictionary<string, GameObject>();

    // Events
    public event Action<CampusLocation> OnLocationVerified;      // Her iki koşul sağlandı
    public event Action<CampusLocation> OnLocationLost;          // Koşullar artık sağlanmıyor
    public event Action<CampusLocation> OnAvatarCollected;       // Avatar toplandı

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Kayıtlı durumları yükle
        if (campusData != null)
        {
            LocationDataHelper.LoadAllStates(campusData);
        }

        // GPS event'ine abone ol
        if (GPSManager.Instance != null)
        {
            GPSManager.Instance.OnLocationUpdated += OnGPSUpdated;
        }

        // AR event'ine abone ol
        if (imageManager != null)
        {
            imageManager.trackedImagesChanged += OnARImagesChanged;
        }
    }

    private void OnDestroy()
    {
        if (GPSManager.Instance != null)
        {
            GPSManager.Instance.OnLocationUpdated -= OnGPSUpdated;
        }

        if (imageManager != null)
        {
            imageManager.trackedImagesChanged -= OnARImagesChanged;
        }
    }

    private void Update()
    {
        // Test modu: Ekrana dokunarak avatar spawn et
        if (testModeEnabled)
        {
            // Mobil dokunma
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Debug.Log("TEST: Ekrana dokunuldu (touch)");
                TestSpawnAvatar();
            }
            // Editor/PC için mouse
            else if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("TEST: Ekrana tıklandı (mouse)");
                TestSpawnAvatar();
            }
        }
    }

    /// <summary>
    /// Test için: Kamera önünde avatar spawn et
    /// </summary>
    private void TestSpawnAvatar()
    {
        Debug.Log("TEST: TestSpawnAvatar çağrıldı");

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("TEST: Main Camera bulunamadı!");
            return;
        }

        // Kamera önünde spawn pozisyonu
        Vector3 spawnPos = mainCam.transform.position + mainCam.transform.forward * 2f;
        spawnPos.y = mainCam.transform.position.y - 0.5f; // Biraz aşağıda

        Debug.Log($"TEST: Spawn pozisyonu: {spawnPos}");

        // CampusData yoksa veya boşsa direkt silindir oluştur
        if (campusData == null || campusData.locations.Count == 0)
        {
            Debug.Log("TEST: CampusData yok, direkt silindir oluşturuluyor");
            CreateTestCylinder(spawnPos);
            return;
        }

        // İlk toplanmamış lokasyonu bul
        CampusLocation testLocation = null;
        foreach (var loc in campusData.locations)
        {
            if (!loc.isCollected && !spawnedAvatars.ContainsKey(loc.locationId))
            {
                testLocation = loc;
                break;
            }
        }

        if (testLocation == null)
        {
            Debug.Log("TEST: Tüm lokasyonlar spawn edilmiş, yeni silindir oluşturuluyor");
            CreateTestCylinder(spawnPos);
            return;
        }

        Debug.Log($"TEST: {testLocation.locationName} için avatar oluşturuluyor");

        if (testLocation.avatarPrefab != null)
        {
            GameObject avatar = Instantiate(testLocation.avatarPrefab, spawnPos, Quaternion.identity);
            avatar.name = $"Avatar_{testLocation.locationId}";

            CollectableAvatar collectable = avatar.GetComponent<CollectableAvatar>();
            if (collectable == null)
            {
                collectable = avatar.AddComponent<CollectableAvatar>();
            }
            collectable.Initialize(testLocation, this);

            spawnedAvatars[testLocation.locationId] = avatar;
            Debug.Log($"TEST: {testLocation.locationName} prefab spawn edildi!");
        }
        else
        {
            GameObject avatar = CylinderAvatarGenerator.CreateCylinderAvatar($"Avatar_{testLocation.locationId}");
            avatar.transform.position = spawnPos;

            CollectableAvatar collectable = avatar.AddComponent<CollectableAvatar>();
            collectable.Initialize(testLocation, this);

            spawnedAvatars[testLocation.locationId] = avatar;
            Debug.Log($"TEST: {testLocation.locationName} silindir spawn edildi!");
        }
    }

    /// <summary>
    /// Test için basit silindir oluştur
    /// </summary>
    private void CreateTestCylinder(Vector3 position)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "TestCylinder_" + Time.time;
        cylinder.transform.position = position;
        cylinder.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

        // Yeşil renk ver
        Renderer rend = cylinder.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.color = Color.green;
        }

        Debug.Log($"TEST: Silindir oluşturuldu pozisyon: {position}");
    }

    /// <summary>
    /// GPS güncellendiğinde çağrılır
    /// </summary>
    private void OnGPSUpdated(double lat, double lon)
    {
        if (campusData == null) return;

        foreach (var location in campusData.locations)
        {
            if (location.isCollected) continue;

            bool wasVerified = gpsVerified.ContainsKey(location.locationId) && gpsVerified[location.locationId];
            bool isNowVerified = GPSManager.Instance.IsWithinRadius(location.latitude, location.longitude, location.radius);

            gpsVerified[location.locationId] = isNowVerified;

            if (debugMode && isNowVerified != wasVerified)
            {
                Debug.Log($"GPS Verify [{location.locationName}]: {isNowVerified}");
            }

            // Koşulları kontrol et
            CheckVerificationStatus(location);
        }
    }

    /// <summary>
    /// AR görsel değiştiğinde çağrılır
    /// </summary>
    private void OnARImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Yeni algılanan görseller
        foreach (var newImage in eventArgs.added)
        {
            ProcessARImage(newImage, true);
        }

        // Güncellenen görseller
        foreach (var updatedImage in eventArgs.updated)
        {
            bool isTracking = updatedImage.trackingState == TrackingState.Tracking;
            ProcessARImage(updatedImage, isTracking);
        }

        // Kaldırılan görseller
        foreach (var removedImage in eventArgs.removed)
        {
            ProcessARImage(removedImage, false);
        }
    }

    private void ProcessARImage(ARTrackedImage image, bool isTracking)
    {
        if (campusData == null) return;

        string imageName = image.referenceImage.name;
        CampusLocation location = campusData.GetLocationByImageName(imageName);

        if (location == null || location.isCollected) return;

        bool wasVerified = arVerified.ContainsKey(location.locationId) && arVerified[location.locationId];
        arVerified[location.locationId] = isTracking;

        if (debugMode && isTracking != wasVerified)
        {
            Debug.Log($"AR Verify [{location.locationName}]: {isTracking}");
        }

        // Koşulları kontrol et
        CheckVerificationStatus(location, isTracking ? image : null);
    }

    /// <summary>
    /// GPS ve AR koşullarını kontrol et
    /// </summary>
    private void CheckVerificationStatus(CampusLocation location, ARTrackedImage arImage = null)
    {
        if (location.isCollected) return;

        bool gpsOk = gpsVerified.ContainsKey(location.locationId) && gpsVerified[location.locationId];
        bool arOk = arVerified.ContainsKey(location.locationId) && arVerified[location.locationId];

        bool verified;
        if (requireBothConditions)
        {
            // AND mantığı: Her ikisi de gerekli
            verified = gpsOk && arOk;
        }
        else
        {
            // OR mantığı: Herhangi biri yeterli (test için)
            verified = gpsOk || arOk;
        }

        bool hasSpawnedAvatar = spawnedAvatars.ContainsKey(location.locationId);

        if (verified && !hasSpawnedAvatar)
        {
            // Avatar spawn et
            SpawnAvatar(location, arImage);
            location.isDiscovered = true;
            LocationDataHelper.SaveLocationState(location);
            OnLocationVerified?.Invoke(location);

            if (debugMode)
            {
                Debug.Log($"VERIFIED [{location.locationName}]: GPS={gpsOk}, AR={arOk}");
            }
        }
        else if (!verified && hasSpawnedAvatar)
        {
            // Avatar gizle (ama yok etme)
            if (spawnedAvatars[location.locationId] != null)
            {
                spawnedAvatars[location.locationId].SetActive(false);
            }
            OnLocationLost?.Invoke(location);
        }
        else if (verified && hasSpawnedAvatar)
        {
            // Avatar tekrar göster
            if (spawnedAvatars[location.locationId] != null)
            {
                spawnedAvatars[location.locationId].SetActive(true);
            }
        }
    }

    /// <summary>
    /// Avatar spawn et
    /// </summary>
    private void SpawnAvatar(CampusLocation location, ARTrackedImage arImage)
    {
        if (location.avatarPrefab == null)
        {
            Debug.LogWarning($"Avatar prefab atanmamış: {location.locationName}");
            return;
        }

        Vector3 spawnPosition;
        Quaternion spawnRotation = Quaternion.identity;
        Transform parent = null;

        if (arImage != null)
        {
            // AR görselin HEMEN ÜSTÜNDE spawn (3cm yukarıda)
            spawnPosition = arImage.transform.position + Vector3.up * 0.03f;
            spawnRotation = arImage.transform.rotation;
            // Parent olarak arImage kullan, görsel ile birlikte hareket etsin
            parent = arImage.transform;
        }
        else
        {
            // Kamera önünde spawn (sadece GPS ile)
            Camera mainCam = Camera.main;
            spawnPosition = mainCam.transform.position + mainCam.transform.forward * 2f;
        }

        GameObject avatar = Instantiate(location.avatarPrefab, spawnPosition, spawnRotation, parent);
        avatar.name = $"Avatar_{location.locationId}";

        // Collectable component ekle
        CollectableAvatar collectable = avatar.GetComponent<CollectableAvatar>();
        if (collectable == null)
        {
            collectable = avatar.AddComponent<CollectableAvatar>();
        }
        collectable.Initialize(location, this);

        spawnedAvatars[location.locationId] = avatar;
    }

    /// <summary>
    /// Avatar toplandığında çağrılır
    /// </summary>
    public void CollectAvatar(CampusLocation location)
    {
        if (location.isCollected) return;

        location.isCollected = true;
        LocationDataHelper.SaveLocationState(location);

        // Avatarı kaldır
        if (spawnedAvatars.ContainsKey(location.locationId))
        {
            Destroy(spawnedAvatars[location.locationId]);
            spawnedAvatars.Remove(location.locationId);
        }

        OnAvatarCollected?.Invoke(location);

        if (debugMode)
        {
            Debug.Log($"COLLECTED [{location.locationName}]: +{location.scoreValue} puan");
        }
    }

    /// <summary>
    /// Mevcut lokasyonun adını döndür
    /// </summary>
    public string GetCurrentLocationName()
    {
        if (campusData == null) return "Bilinmiyor";

        foreach (var location in campusData.locations)
        {
            if (gpsVerified.ContainsKey(location.locationId) && gpsVerified[location.locationId])
            {
                return location.locationName;
            }
        }

        return "Kampüs dışı";
    }

    // Debug için
    #if UNITY_EDITOR
    [ContextMenu("Toggle Require Both Conditions")]
    private void ToggleRequireBothConditions()
    {
        requireBothConditions = !requireBothConditions;
        Debug.Log($"Require Both Conditions: {requireBothConditions}");
    }
    #endif

    // Test modu için ekran butonu
    private void OnGUI()
    {
        if (!testModeEnabled) return;

        // Büyük test butonu
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 30;

        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height - 200, 300, 100), "TEST: SİLİNDİR SPAWN", buttonStyle))
        {
            Debug.Log("TEST: Buton tıklandı!");
            TestSpawnAvatar();
        }

        // Debug bilgisi
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 20;
        labelStyle.normal.textColor = Color.yellow;

        string info = $"Test Modu: AÇIK\n";
        info += $"CampusData: {(campusData != null ? campusData.locations.Count + " lokasyon" : "YOK")}\n";
        info += $"Spawn edilen: {spawnedAvatars.Count}";

        GUI.Label(new Rect(10, Screen.height - 300, 400, 100), info, labelStyle);
    }
}

/// <summary>
/// Avatar üzerine eklenen toplanabilir component
/// </summary>
public class CollectableAvatar : MonoBehaviour
{
    private CampusLocation location;
    private LocationVerifier verifier;
    private bool isCollectable = true;

    public void Initialize(CampusLocation loc, LocationVerifier ver)
    {
        location = loc;
        verifier = ver;
    }

    /// <summary>
    /// Dokunma ile toplama
    /// </summary>
    public void Collect()
    {
        if (!isCollectable || location == null) return;

        isCollectable = false;

        // Toplama animasyonu
        StartCoroutine(CollectAnimation());
    }

    private System.Collections.IEnumerator CollectAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Küçült ve yukarı hareket et
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            transform.position += Vector3.up * Time.deltaTime * 2f;

            yield return null;
        }

        verifier.CollectAvatar(location);
    }

    private void OnMouseDown()
    {
        Collect();
    }
}
