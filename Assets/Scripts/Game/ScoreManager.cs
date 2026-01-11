using System;
using UnityEngine;

/// <summary>
/// Skor sistemini yönetir
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Referanslar")]
    [SerializeField] private CampusLocationsData campusData;

    [Header("Ayarlar")]
    [SerializeField] private int bonusScorePerMilestone = 50; // Her 5 avatarda bonus

    // Events
    public event Action<int> OnScoreChanged;           // Yeni skor
    public event Action<int, int> OnCollectionChanged; // Toplanan, Toplam
    public event Action<int> OnMilestoneReached;       // Milestone numarası

    public int CurrentScore => GameData.TotalScore;
    public int CollectedCount => GameData.CollectedCount;
    public int TotalLocations => campusData != null ? campusData.GetTotalCount() : 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // LocationVerifier event'ine abone ol
        if (LocationVerifier.Instance != null)
        {
            LocationVerifier.Instance.OnAvatarCollected += OnAvatarCollected;
        }
    }

    private void OnDestroy()
    {
        if (LocationVerifier.Instance != null)
        {
            LocationVerifier.Instance.OnAvatarCollected -= OnAvatarCollected;
        }
    }

    /// <summary>
    /// Avatar toplandığında çağrılır
    /// </summary>
    private void OnAvatarCollected(CampusLocation location)
    {
        // Skor ekle
        int scoreToAdd = location.scoreValue;

        // Milestone kontrolü
        int newCollectedCount = GameData.CollectedCount + 1;
        if (newCollectedCount % 5 == 0) // Her 5 avatarda bonus
        {
            scoreToAdd += bonusScorePerMilestone;
            OnMilestoneReached?.Invoke(newCollectedCount / 5);
        }

        GameData.AddScore(scoreToAdd);
        GameData.IncrementCollectedCount();

        // Event'leri tetikle
        OnScoreChanged?.Invoke(GameData.TotalScore);
        OnCollectionChanged?.Invoke(GameData.CollectedCount, TotalLocations);

        Debug.Log($"Score: +{scoreToAdd} = {GameData.TotalScore} | Collection: {GameData.CollectedCount}/{TotalLocations}");
    }

    /// <summary>
    /// Manuel skor ekleme (test için)
    /// </summary>
    public void AddScore(int amount)
    {
        GameData.AddScore(amount);
        OnScoreChanged?.Invoke(GameData.TotalScore);
    }

    /// <summary>
    /// İlerleme yüzdesi
    /// </summary>
    public float GetProgressPercentage()
    {
        if (TotalLocations == 0) return 0f;
        return (float)CollectedCount / TotalLocations * 100f;
    }

    /// <summary>
    /// Tamamlanma durumu
    /// </summary>
    public bool IsCompleted()
    {
        return CollectedCount >= TotalLocations && TotalLocations > 0;
    }

    /// <summary>
    /// Tüm ilerlemeyi sıfırla
    /// </summary>
    public void ResetProgress()
    {
        GameData.ResetAllData();

        if (campusData != null)
        {
            LocationDataHelper.ResetAllSavedData(campusData);
        }

        OnScoreChanged?.Invoke(0);
        OnCollectionChanged?.Invoke(0, TotalLocations);
    }

    // Debug
    #if UNITY_EDITOR
    [ContextMenu("Add 100 Score")]
    private void DebugAddScore()
    {
        AddScore(100);
    }

    [ContextMenu("Reset All Progress")]
    private void DebugResetProgress()
    {
        ResetProgress();
    }
    #endif
}
