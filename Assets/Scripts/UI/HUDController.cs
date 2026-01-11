using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Oyun içi HUD kontrolcüsü
/// Skor, lokasyon bilgisi ve butonları gösterir
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Üst Panel")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text locationText;
    [SerializeField] private Text collectionText;

    [Header("GPS Durumu")]
    [SerializeField] private Text gpsStatusText;
    [SerializeField] private Image gpsIndicator;
    [SerializeField] private Color gpsActiveColor = Color.green;
    [SerializeField] private Color gpsInactiveColor = Color.red;

    [Header("Butonlar")]
    [SerializeField] private Button mapButton;
    [SerializeField] private Button menuButton;

    [Header("Bildirim")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Text notificationText;
    [SerializeField] private float notificationDuration = 3f;

    [Header("Referanslar")]
    [SerializeField] private CampusLocationsData campusData;

    private Coroutine notificationCoroutine;

    private void Start()
    {
        SetupButtons();
        SubscribeToEvents();
        UpdateHUD();

        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    private void SetupButtons()
    {
        if (mapButton != null)
            mapButton.onClick.AddListener(OnMapClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
    }

    private void SubscribeToEvents()
    {
        // Score events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
            ScoreManager.Instance.OnCollectionChanged += OnCollectionChanged;
            ScoreManager.Instance.OnMilestoneReached += OnMilestoneReached;
        }

        // GPS events
        if (GPSManager.Instance != null)
        {
            GPSManager.Instance.OnGPSStatusChanged += OnGPSStatusChanged;
            GPSManager.Instance.OnLocationUpdated += OnLocationUpdated;
        }

        // Location events
        if (LocationVerifier.Instance != null)
        {
            LocationVerifier.Instance.OnLocationVerified += OnLocationVerified;
            LocationVerifier.Instance.OnAvatarCollected += OnAvatarCollected;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();

        if (mapButton != null)
            mapButton.onClick.RemoveListener(OnMapClicked);

        if (menuButton != null)
            menuButton.onClick.RemoveListener(OnMenuClicked);
    }

    private void UnsubscribeFromEvents()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
            ScoreManager.Instance.OnCollectionChanged -= OnCollectionChanged;
            ScoreManager.Instance.OnMilestoneReached -= OnMilestoneReached;
        }

        if (GPSManager.Instance != null)
        {
            GPSManager.Instance.OnGPSStatusChanged -= OnGPSStatusChanged;
            GPSManager.Instance.OnLocationUpdated -= OnLocationUpdated;
        }

        if (LocationVerifier.Instance != null)
        {
            LocationVerifier.Instance.OnLocationVerified -= OnLocationVerified;
            LocationVerifier.Instance.OnAvatarCollected -= OnAvatarCollected;
        }
    }

    private void UpdateHUD()
    {
        // Skor
        if (scoreText != null)
            scoreText.text = $"Skor: {GameData.TotalScore}";

        // Koleksiyon
        int total = campusData != null ? campusData.GetTotalCount() : 0;
        if (collectionText != null)
            collectionText.text = $"{GameData.CollectedCount}/{total}";

        // Lokasyon
        if (locationText != null && LocationVerifier.Instance != null)
            locationText.text = LocationVerifier.Instance.GetCurrentLocationName();

        // GPS durumu
        UpdateGPSStatus();
    }

    private void UpdateGPSStatus()
    {
        if (GPSManager.Instance == null) return;

        if (gpsStatusText != null)
            gpsStatusText.text = GPSManager.Instance.GPSStatus;

        if (gpsIndicator != null)
            gpsIndicator.color = GPSManager.Instance.IsGPSActive ? gpsActiveColor : gpsInactiveColor;
    }

    #region Event Handlers

    private void OnScoreChanged(int newScore)
    {
        if (scoreText != null)
            scoreText.text = $"Skor: {newScore}";
    }

    private void OnCollectionChanged(int collected, int total)
    {
        if (collectionText != null)
            collectionText.text = $"{collected}/{total}";
    }

    private void OnMilestoneReached(int milestone)
    {
        ShowNotification($"Milestone {milestone}! +50 Bonus Puan!");
    }

    private void OnGPSStatusChanged(string status)
    {
        UpdateGPSStatus();
    }

    private void OnLocationUpdated(double lat, double lon)
    {
        if (locationText != null && LocationVerifier.Instance != null)
            locationText.text = LocationVerifier.Instance.GetCurrentLocationName();
    }

    private void OnLocationVerified(CampusLocation location)
    {
        ShowNotification($"{location.locationName} keşfedildi!");
    }

    private void OnAvatarCollected(CampusLocation location)
    {
        ShowNotification($"{location.locationName} avatarı toplandı! +{location.scoreValue}");
    }

    #endregion

    #region Button Handlers

    private void OnMapClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ToggleMap();
    }

    private void OnMenuClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPause();
    }

    #endregion

    #region Notification

    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null) return;

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
    }

    private IEnumerator ShowNotificationCoroutine(string message)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);

        yield return new WaitForSeconds(notificationDuration);

        notificationPanel.SetActive(false);
    }

    #endregion
}
