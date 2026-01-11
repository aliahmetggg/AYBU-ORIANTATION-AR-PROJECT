using UnityEngine;

/// <summary>
/// Ana oyun yöneticisi
/// Tüm sistemleri başlatır ve koordine eder
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referanslar")]
    [SerializeField] private CampusLocationsData campusData;

    [Header("Sistemler")]
    [SerializeField] private GPSManager gpsManager;
    [SerializeField] private LocationVerifier locationVerifier;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private AvatarManager avatarManager;
    [SerializeField] private UIManager uiManager;

    [Header("Özel Objeler")]
    [SerializeField] private bool spawnGoldStar = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    public CampusLocationsData CampusData => campusData;
    public bool IsGameActive { get; private set; }

    private GPSStarSpawner starSpawner;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        // Kayıtlı verileri yükle
        if (campusData != null)
        {
            LocationDataHelper.LoadAllStates(campusData);
        }

        // Altın yıldızı oluştur (39.970835, 32.818170 konumunda)
        if (spawnGoldStar)
        {
            CreateGoldStarSpawner();
        }

        // İlk oyun kontrolü
        if (GameData.IsFirstPlay)
        {
            GameData.IsFirstPlay = false;
            Debug.Log("AYBU Orientation'a hoş geldiniz!");
        }

        Debug.Log($"GameManager başlatıldı. Skor: {GameData.TotalScore}, Toplanan: {GameData.CollectedCount}");
    }

    /// <summary>
    /// Altın yıldız spawner'ını oluştur
    /// </summary>
    private void CreateGoldStarSpawner()
    {
        GameObject starSpawnerObj = new GameObject("GoldStarSpawner");
        starSpawnerObj.transform.SetParent(transform);

        starSpawner = starSpawnerObj.AddComponent<GPSStarSpawner>();
        starSpawner.targetLatitude = 39.970835;
        starSpawner.targetLongitude = 32.818170;
        starSpawner.activationRadius = 25f;
        starSpawner.starSize = 2.5f;
        starSpawner.spawnDistance = 3f;
        starSpawner.spawnHeight = 1.5f;

        Debug.Log("Altın yıldız spawner oluşturuldu (39.970835, 32.818170)");
    }

    /// <summary>
    /// Oyunu başlat
    /// </summary>
    public void StartGame()
    {
        IsGameActive = true;
        Debug.Log("Oyun başladı!");
    }

    /// <summary>
    /// Oyunu duraklat
    /// </summary>
    public void PauseGame()
    {
        IsGameActive = false;
        Debug.Log("Oyun duraklatıldı");
    }

    /// <summary>
    /// Oyunu devam ettir
    /// </summary>
    public void ResumeGame()
    {
        IsGameActive = true;
        Debug.Log("Oyun devam ediyor");
    }

    /// <summary>
    /// İlerlemeyi sıfırla
    /// </summary>
    public void ResetProgress()
    {
        GameData.ResetAllData();

        if (campusData != null)
        {
            LocationDataHelper.ResetAllSavedData(campusData);
        }

        Debug.Log("İlerleme sıfırlandı!");
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"=== AYBU Orientation Debug ===");
        GUILayout.Label($"Skor: {GameData.TotalScore}");
        GUILayout.Label($"Toplanan: {GameData.CollectedCount}/{(campusData != null ? campusData.GetTotalCount() : 0)}");

        if (GPSManager.Instance != null)
        {
            GUILayout.Label($"GPS: {GPSManager.Instance.GPSStatus}");
            if (GPSManager.Instance.IsGPSActive)
            {
                GUILayout.Label($"Konum: {GPSManager.Instance.Latitude:F6}, {GPSManager.Instance.Longitude:F6}");
                GUILayout.Label($"Doğruluk: {GPSManager.Instance.Accuracy:F1}m");
            }
        }

        if (LocationVerifier.Instance != null)
        {
            GUILayout.Label($"Lokasyon: {LocationVerifier.Instance.GetCurrentLocationName()}");
        }

        GUILayout.EndArea();
    }

    #if UNITY_EDITOR
    [ContextMenu("Test - Add 100 Score")]
    private void DebugAddScore()
    {
        GameData.AddScore(100);
    }

    [ContextMenu("Test - Reset All")]
    private void DebugReset()
    {
        ResetProgress();
    }
    #endif
}
