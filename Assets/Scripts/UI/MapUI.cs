using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kampüs haritası UI kontrolcüsü
/// Lokasyonları ve durumlarını gösterir
/// </summary>
public class MapUI : MonoBehaviour
{
    [Header("Harita")]
    [SerializeField] private Image mapImage;
    [SerializeField] private RectTransform mapContainer;

    [Header("Lokasyon İşaretleri")]
    [SerializeField] private GameObject locationMarkerPrefab;
    [SerializeField] private Transform markersParent;

    [Header("Renkler")]
    [SerializeField] private Color collectedColor = Color.green;
    [SerializeField] private Color discoveredColor = Color.yellow;
    [SerializeField] private Color undiscoveredColor = Color.gray;
    [SerializeField] private Color currentLocationColor = Color.cyan;

    [Header("Butonlar")]
    [SerializeField] private Button closeButton;

    [Header("Bilgi Paneli")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Text locationNameText;
    [SerializeField] private Text locationDescText;
    [SerializeField] private Text locationStatusText;

    [Header("Referanslar")]
    [SerializeField] private CampusLocationsData campusData;

    // Harita sınırları (koordinatları UI pozisyonuna çevirmek için)
    [Header("Harita Koordinat Sınırları")]
    [SerializeField] private double minLatitude = 39.930;
    [SerializeField] private double maxLatitude = 39.940;
    [SerializeField] private double minLongitude = 32.850;
    [SerializeField] private double maxLongitude = 32.870;

    private Dictionary<string, GameObject> markers = new Dictionary<string, GameObject>();

    private void Awake()
    {
        LoadMapImage();
    }

    private void Start()
    {
        SetupButtons();
        CreateLocationMarkers();

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    private void LoadMapImage()
    {
        if (mapImage == null) return;

        // Resources klasöründen harita resmini yükle
        Sprite mapSprite = Resources.Load<Sprite>("campus_map");
        if (mapSprite != null)
        {
            mapImage.sprite = mapSprite;
            mapImage.color = Color.white; // Sprite görünür olması için
            Debug.Log("Harita yüklendi: campus_map");
        }
        else
        {
            // Texture olarak yüklemeyi dene
            Texture2D mapTexture = Resources.Load<Texture2D>("campus_map");
            if (mapTexture != null)
            {
                Sprite newSprite = Sprite.Create(mapTexture,
                    new Rect(0, 0, mapTexture.width, mapTexture.height),
                    new Vector2(0.5f, 0.5f));
                mapImage.sprite = newSprite;
                mapImage.color = Color.white;
                Debug.Log("Harita texture olarak yüklendi: campus_map");
            }
            else
            {
                Debug.LogWarning("Harita yüklenemedi: Resources/campus_map");
            }
        }
    }

    private void OnEnable()
    {
        UpdateMarkers();
    }

    private void SetupButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    /// <summary>
    /// Lokasyon işaretlerini oluştur
    /// </summary>
    private void CreateLocationMarkers()
    {
        if (campusData == null || locationMarkerPrefab == null) return;

        foreach (var location in campusData.locations)
        {
            Vector2 uiPosition = GPSToUIPosition(location.latitude, location.longitude);

            GameObject marker = Instantiate(locationMarkerPrefab, markersParent != null ? markersParent : mapContainer);
            RectTransform rt = marker.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = uiPosition;
            }

            // Marker setup
            LocationMarker markerScript = marker.GetComponent<LocationMarker>();
            if (markerScript == null)
            {
                markerScript = marker.AddComponent<LocationMarker>();
            }
            markerScript.Initialize(location, this);

            markers[location.locationId] = marker;
        }

        UpdateMarkers();
    }

    /// <summary>
    /// GPS koordinatlarını UI pozisyonuna çevir
    /// </summary>
    private Vector2 GPSToUIPosition(double lat, double lon)
    {
        if (mapContainer == null) return Vector2.zero;

        Rect rect = mapContainer.rect;

        // Normalize
        float normalizedX = (float)((lon - minLongitude) / (maxLongitude - minLongitude));
        float normalizedY = (float)((lat - minLatitude) / (maxLatitude - minLatitude));

        // UI pozisyonuna çevir
        float x = rect.width * normalizedX - rect.width / 2;
        float y = rect.height * normalizedY - rect.height / 2;

        return new Vector2(x, y);
    }

    /// <summary>
    /// Tüm işaretleri güncelle
    /// </summary>
    public void UpdateMarkers()
    {
        if (campusData == null) return;

        foreach (var location in campusData.locations)
        {
            if (markers.ContainsKey(location.locationId))
            {
                UpdateMarkerColor(markers[location.locationId], location);
            }
        }
    }

    /// <summary>
    /// İşaret rengini güncelle
    /// </summary>
    private void UpdateMarkerColor(GameObject marker, CampusLocation location)
    {
        Image markerImage = marker.GetComponent<Image>();
        if (markerImage == null) return;

        Color targetColor;

        if (location.isCollected)
        {
            targetColor = collectedColor;
        }
        else if (location.isDiscovered)
        {
            targetColor = discoveredColor;
        }
        else
        {
            targetColor = undiscoveredColor;
        }

        // Mevcut lokasyon kontrolü
        if (GPSManager.Instance != null && GPSManager.Instance.IsGPSActive)
        {
            if (GPSManager.Instance.IsWithinRadius(location.latitude, location.longitude, location.radius))
            {
                targetColor = currentLocationColor;
            }
        }

        markerImage.color = targetColor;
    }

    /// <summary>
    /// Lokasyon bilgisini göster
    /// </summary>
    public void ShowLocationInfo(CampusLocation location)
    {
        if (infoPanel == null) return;

        infoPanel.SetActive(true);

        if (locationNameText != null)
            locationNameText.text = location.locationName;

        if (locationDescText != null)
            locationDescText.text = location.description;

        if (locationStatusText != null)
        {
            string status;
            if (location.isCollected)
                status = "Toplandı";
            else if (location.isDiscovered)
                status = "Keşfedildi";
            else
                status = "Keşfedilmedi";

            locationStatusText.text = status;
        }
    }

    /// <summary>
    /// Bilgi panelini gizle
    /// </summary>
    public void HideLocationInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    private void OnCloseClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.HideMap();
    }
}

/// <summary>
/// Harita üzerindeki lokasyon işareti
/// </summary>
public class LocationMarker : MonoBehaviour
{
    private CampusLocation location;
    private MapUI mapUI;
    private Button button;

    public void Initialize(CampusLocation loc, MapUI map)
    {
        location = loc;
        mapUI = map;

        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        button.onClick.AddListener(OnMarkerClicked);

        // Tooltip veya isim
        Text label = GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = location.locationName;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnMarkerClicked);
    }

    private void OnMarkerClicked()
    {
        if (mapUI != null && location != null)
        {
            mapUI.ShowLocationInfo(location);
        }
    }
}
