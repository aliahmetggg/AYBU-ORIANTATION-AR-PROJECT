using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// AR görsel takibi - Eski sistem (geriye uyumluluk için korundu)
/// Yeni sistem için LocationVerifier.cs kullanılır
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class MultiImageTracker : MonoBehaviour
{
    [System.Serializable]
    public struct ResimObjeEslesmesi
    {
        public string resimAdi;
        public GameObject objePrefab;
    }

    [Header("Mod Seçimi")]
    [Tooltip("True: Yeni GPS+AR sistemi, False: Eski sadece AR sistemi")]
    public bool useNewSystem = true;

    [Header("Eski Sistem - Eşleşme Listesi")]
    public List<ResimObjeEslesmesi> eslesmeler;

    // Her resim için oluşturulan objeyi takip etmek için (Çift oluşumu engeller)
    private Dictionary<string, GameObject> olusanObjeler = new Dictionary<string, GameObject>();
    private ARTrackedImageManager imageManager;

    // Event - Yeni sistem için
    public event System.Action<ARTrackedImage, bool> OnImageTrackingChanged;

    void Awake()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        imageManager.trackedImagesChanged += OnChanged;
    }

    void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnChanged;
    }

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Yeni sistem aktifse, sadece event tetikle
        if (useNewSystem)
        {
            ProcessForNewSystem(eventArgs);
            return;
        }

        // Eski sistem - doğrudan obje spawn
        ProcessForLegacySystem(eventArgs);
    }

    /// <summary>
    /// Yeni sistem için event tabanlı işleme
    /// LocationVerifier bu event'leri dinler
    /// </summary>
    private void ProcessForNewSystem(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            OnImageTrackingChanged?.Invoke(newImage, true);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            bool isTracking = updatedImage.trackingState == TrackingState.Tracking;
            OnImageTrackingChanged?.Invoke(updatedImage, isTracking);
        }

        foreach (var removedImage in eventArgs.removed)
        {
            OnImageTrackingChanged?.Invoke(removedImage, false);
        }
    }

    /// <summary>
    /// Eski sistem - Doğrudan obje spawn (geriye uyumluluk)
    /// </summary>
    private void ProcessForLegacySystem(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // YENİ EKLENEN RESİMLER
        foreach (var newImage in eventArgs.added)
        {
            string resimIsmi = newImage.referenceImage.name;

            // Eğer bu resim için zaten bir obje oluşturduysak, tekrar oluşturma! (KORUMA)
            if (olusanObjeler.ContainsKey(resimIsmi)) continue;

            foreach (var eslesme in eslesmeler)
            {
                // İsim eşleşiyorsa
                if (eslesme.resimAdi == resimIsmi)
                {
                    // Prefabı oluştur
                    GameObject yeniObje = Instantiate(eslesme.objePrefab, newImage.transform);

                    // Pozisyonunu sıfırla ki resmin tam ortasında dursun
                    yeniObje.transform.localPosition = Vector3.zero;
                    yeniObje.transform.localRotation = Quaternion.identity;

                    // Sözlüğe kaydet
                    olusanObjeler[resimIsmi] = yeniObje;

                    // BULDUK! Artık döngüyü kır, diğerlerine bakma (ÇİFT BASMAYI ENGELLER)
                    break;
                }
            }
        }

        // GÜNCELLENEN RESİMLER (Görünürlük Ayarı)
        foreach (var updatedImage in eventArgs.updated)
        {
            string resimIsmi = updatedImage.referenceImage.name;
            if (olusanObjeler.ContainsKey(resimIsmi))
            {
                GameObject obje = olusanObjeler[resimIsmi];
                bool gorunuyorMu = (updatedImage.trackingState == TrackingState.Tracking);
                obje.SetActive(gorunuyorMu);
            }
        }
    }

    /// <summary>
    /// ARTrackedImageManager'a erişim
    /// </summary>
    public ARTrackedImageManager GetImageManager()
    {
        return imageManager;
    }
}