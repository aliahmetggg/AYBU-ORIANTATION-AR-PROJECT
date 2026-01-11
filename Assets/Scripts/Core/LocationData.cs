using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CampusLocation
{
    public string locationId;           // Benzersiz ID
    public string locationName;         // "Kütüphane", "Mescit" vb.
    public string description;          // Açıklama
    public double latitude;
    public double longitude;
    public float radius = 20f;          // Metre cinsinden yarıçap
    public string referenceImageName;   // AR için referans görsel adı (ana)
    public List<string> additionalImageNames = new List<string>(); // Ek referans görseller
    public GameObject avatarPrefab;     // Spawn edilecek avatar
    public int scoreValue = 10;         // Toplama puanı

    [Header("Durum")]
    public bool isCollected;            // Toplanmış mı?
    public bool isDiscovered;           // Keşfedilmiş mi?

    /// <summary>
    /// Verilen görsel adının bu lokasyona ait olup olmadığını kontrol et
    /// </summary>
    public bool MatchesImageName(string imageName)
    {
        if (string.IsNullOrEmpty(imageName)) return false;

        // Ana referans görsel
        if (referenceImageName == imageName) return true;

        // Ek referans görseller
        if (additionalImageNames != null && additionalImageNames.Contains(imageName)) return true;

        return false;
    }
}

[CreateAssetMenu(fileName = "CampusLocations", menuName = "AYBU/Campus Locations Data")]
public class CampusLocationsData : ScriptableObject
{
    [Header("AYBU Kampüs Lokasyonları")]
    public List<CampusLocation> locations = new List<CampusLocation>();

    /// <summary>
    /// ID'ye göre lokasyon bul
    /// </summary>
    public CampusLocation GetLocationById(string id)
    {
        return locations.Find(l => l.locationId == id);
    }

    /// <summary>
    /// Referans görsel adına göre lokasyon bul (ana ve ek görseller dahil)
    /// </summary>
    public CampusLocation GetLocationByImageName(string imageName)
    {
        return locations.Find(l => l.MatchesImageName(imageName));
    }

    /// <summary>
    /// Toplanmamış lokasyonları getir
    /// </summary>
    public List<CampusLocation> GetUncollectedLocations()
    {
        return locations.FindAll(l => !l.isCollected);
    }

    /// <summary>
    /// Toplanan avatar sayısı
    /// </summary>
    public int GetCollectedCount()
    {
        return locations.FindAll(l => l.isCollected).Count;
    }

    /// <summary>
    /// Toplam avatar sayısı
    /// </summary>
    public int GetTotalCount()
    {
        return locations.Count;
    }

    /// <summary>
    /// Tüm ilerlemeyi sıfırla
    /// </summary>
    public void ResetAllProgress()
    {
        foreach (var loc in locations)
        {
            loc.isCollected = false;
            loc.isDiscovered = false;
        }
    }
}

/// <summary>
/// Runtime'da lokasyon durumunu yönetmek için yardımcı sınıf
/// </summary>
public static class LocationDataHelper
{
    private const string COLLECTED_KEY_PREFIX = "AYBU_Collected_";
    private const string DISCOVERED_KEY_PREFIX = "AYBU_Discovered_";

    public static void SaveLocationState(CampusLocation location)
    {
        PlayerPrefs.SetInt(COLLECTED_KEY_PREFIX + location.locationId, location.isCollected ? 1 : 0);
        PlayerPrefs.SetInt(DISCOVERED_KEY_PREFIX + location.locationId, location.isDiscovered ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void LoadLocationState(CampusLocation location)
    {
        location.isCollected = PlayerPrefs.GetInt(COLLECTED_KEY_PREFIX + location.locationId, 0) == 1;
        location.isDiscovered = PlayerPrefs.GetInt(DISCOVERED_KEY_PREFIX + location.locationId, 0) == 1;
    }

    public static void LoadAllStates(CampusLocationsData data)
    {
        foreach (var loc in data.locations)
        {
            LoadLocationState(loc);
        }
    }

    public static void ResetAllSavedData(CampusLocationsData data)
    {
        foreach (var loc in data.locations)
        {
            PlayerPrefs.DeleteKey(COLLECTED_KEY_PREFIX + loc.locationId);
            PlayerPrefs.DeleteKey(DISCOVERED_KEY_PREFIX + loc.locationId);
            loc.isCollected = false;
            loc.isDiscovered = false;
        }
        PlayerPrefs.Save();
    }
}
