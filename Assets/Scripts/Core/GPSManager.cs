using System;
using System.Collections;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance { get; private set; }

    [Header("GPS Ayarları")]
    [SerializeField] private float updateInterval = 1f;
    [SerializeField] private float desiredAccuracy = 10f;

    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public float Accuracy { get; private set; }
    public bool IsGPSActive { get; private set; }
    public string GPSStatus { get; private set; } = "Başlatılıyor...";

    public event Action<double, double> OnLocationUpdated;
    public event Action<string> OnGPSStatusChanged;

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
        StartCoroutine(StartGPSWithPermission());
    }

    private IEnumerator StartGPSWithPermission()
    {
        GPSStatus = "İzin kontrol ediliyor...";
        OnGPSStatusChanged?.Invoke(GPSStatus);

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android izin kontrolü
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            GPSStatus = "GPS izni isteniyor...";
            OnGPSStatusChanged?.Invoke(GPSStatus);

            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);

            // Bekle
            yield return new WaitForSeconds(0.5f);

            // Kullanıcı cevap verene kadar bekle
            float waitTime = 0;
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation) && waitTime < 30f)
            {
                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;
            }
        }
#endif

        yield return new WaitForSeconds(0.5f);

        // GPS başlat
        yield return StartCoroutine(InitializeGPS());
    }

    private IEnumerator InitializeGPS()
    {
        // Konum servisi kontrolü
        if (!Input.location.isEnabledByUser)
        {
            GPSStatus = "GPS kapalı! Telefondan açın";
            OnGPSStatusChanged?.Invoke(GPSStatus);
            Debug.LogWarning("GPS: Konum servisi kapalı");

            // Birkaç saniye bekle ve tekrar dene
            yield return new WaitForSeconds(5f);

            if (!Input.location.isEnabledByUser)
            {
                GPSStatus = "GPS hala kapalı";
                OnGPSStatusChanged?.Invoke(GPSStatus);
                yield break;
            }
        }

        // GPS başlat
        GPSStatus = "GPS başlatılıyor...";
        OnGPSStatusChanged?.Invoke(GPSStatus);

        Input.location.Start(desiredAccuracy, updateInterval);

        // Başlamasını bekle
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            GPSStatus = $"Bağlanıyor... ({maxWait})";
            OnGPSStatusChanged?.Invoke(GPSStatus);
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            GPSStatus = "GPS zaman aşımı";
            OnGPSStatusChanged?.Invoke(GPSStatus);
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            GPSStatus = "GPS başarısız";
            OnGPSStatusChanged?.Invoke(GPSStatus);
            yield break;
        }

        // Başarılı
        IsGPSActive = true;
        GPSStatus = "GPS aktif";
        OnGPSStatusChanged?.Invoke(GPSStatus);

        // Güncelleme döngüsü
        while (IsGPSActive)
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                Latitude = Input.location.lastData.latitude;
                Longitude = Input.location.lastData.longitude;
                Accuracy = Input.location.lastData.horizontalAccuracy;

                GPSStatus = $"{Latitude:F4}, {Longitude:F4}";
                OnGPSStatusChanged?.Invoke(GPSStatus);
                OnLocationUpdated?.Invoke(Latitude, Longitude);
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }

    public static float CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (float)(R * c);
    }

    public bool IsWithinRadius(double targetLat, double targetLon, float radiusMeters)
    {
        if (!IsGPSActive) return false;
        return CalculateDistance(Latitude, Longitude, targetLat, targetLon) <= radiusMeters;
    }

    public float GetDistanceTo(double targetLat, double targetLon)
    {
        if (!IsGPSActive) return float.MaxValue;
        return CalculateDistance(Latitude, Longitude, targetLat, targetLon);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            IsGPSActive = false;
            Input.location.Stop();
        }
    }
}
