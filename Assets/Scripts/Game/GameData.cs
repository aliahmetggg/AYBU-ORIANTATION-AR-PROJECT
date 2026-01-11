using UnityEngine;

/// <summary>
/// Oyun verilerini PlayerPrefs ile yönetir
/// </summary>
public static class GameData
{
    private const string SCORE_KEY = "AYBU_TotalScore";
    private const string COLLECTED_COUNT_KEY = "AYBU_CollectedCount";
    private const string PLAYER_NAME_KEY = "AYBU_PlayerName";
    private const string FIRST_PLAY_KEY = "AYBU_FirstPlay";

    /// <summary>
    /// Toplam skor
    /// </summary>
    public static int TotalScore
    {
        get => PlayerPrefs.GetInt(SCORE_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(SCORE_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Toplanan avatar sayısı
    /// </summary>
    public static int CollectedCount
    {
        get => PlayerPrefs.GetInt(COLLECTED_COUNT_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(COLLECTED_COUNT_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Oyuncu adı
    /// </summary>
    public static string PlayerName
    {
        get => PlayerPrefs.GetString(PLAYER_NAME_KEY, "Öğrenci");
        set
        {
            PlayerPrefs.SetString(PLAYER_NAME_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// İlk oyun mu?
    /// </summary>
    public static bool IsFirstPlay
    {
        get => PlayerPrefs.GetInt(FIRST_PLAY_KEY, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(FIRST_PLAY_KEY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Skor ekle
    /// </summary>
    public static void AddScore(int amount)
    {
        TotalScore += amount;
    }

    /// <summary>
    /// Toplanan sayısını artır
    /// </summary>
    public static void IncrementCollectedCount()
    {
        CollectedCount++;
    }

    /// <summary>
    /// Tüm verileri sıfırla
    /// </summary>
    public static void ResetAllData()
    {
        PlayerPrefs.DeleteKey(SCORE_KEY);
        PlayerPrefs.DeleteKey(COLLECTED_COUNT_KEY);
        PlayerPrefs.DeleteKey(FIRST_PLAY_KEY);
        PlayerPrefs.Save();
    }
}
