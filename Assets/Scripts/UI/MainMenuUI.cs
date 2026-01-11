using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ana menü UI kontrolcüsü
/// AYBU ORIENTATION başlık ekranı
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Başlık")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text subtitleText;

    [Header("Butonlar")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button resetButton;

    [Header("İstatistikler")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text collectionText;
    [SerializeField] private Slider progressSlider;

    [Header("Referanslar")]
    [SerializeField] private CampusLocationsData campusData;

    private void Start()
    {
        SetupButtons();
        UpdateStats();

        // Başlık
        if (titleText != null)
            titleText.text = "AYBU ORIENTATION";

        if (subtitleText != null)
            subtitleText.text = "Kampüs Keşif Oyunu";
    }

    private void OnEnable()
    {
        UpdateStats();
    }

    private void SetupButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (mapButton != null)
            mapButton.onClick.AddListener(OnMapClicked);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);
    }

    private void UpdateStats()
    {
        int score = GameData.TotalScore;
        int collected = GameData.CollectedCount;
        int total = campusData != null ? campusData.GetTotalCount() : 0;

        if (scoreText != null)
            scoreText.text = $"Skor: {score}";

        if (collectionText != null)
            collectionText.text = $"Toplanan: {collected}/{total}";

        if (progressSlider != null)
        {
            progressSlider.maxValue = total > 0 ? total : 1;
            progressSlider.value = collected;
        }
    }

    private void OnStartClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.StartGame();
    }

    private void OnMapClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartGame();
            UIManager.Instance.ShowMap();
        }
    }

    private void OnResetClicked()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetProgress();
        }
        else
        {
            GameData.ResetAllData();
            if (campusData != null)
                LocationDataHelper.ResetAllSavedData(campusData);
        }

        UpdateStats();
        Debug.Log("İlerleme sıfırlandı!");
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);

        if (mapButton != null)
            mapButton.onClick.RemoveListener(OnMapClicked);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetClicked);
    }
}
