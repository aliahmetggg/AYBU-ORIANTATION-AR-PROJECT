using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// AR sistemini debug eder - neyin calismadigi gosterir
/// </summary>
public class ARDebugger : MonoBehaviour
{
    [Header("AR Bileşenleri (Otomatik bulunur)")]
    public ARSession arSession;
    public ARTrackedImageManager imageManager;

    [Header("Ayarlar")]
    public bool showDebugUI = true;
    public bool spawnOnImageFound = true;

    // Debug bilgileri
    private string arStatus = "Baslatiliyor...";
    private string imageLibraryStatus = "Kontrol ediliyor...";
    private string trackingStatus = "Bekleniyor...";
    private List<string> logMessages = new List<string>();
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        AddLog("ARDebugger baslatildi");

        // AR Session bul
        if (arSession == null)
            arSession = FindObjectOfType<ARSession>();

        if (arSession == null)
        {
            arStatus = "HATA: ARSession BULUNAMADI!";
            AddLog("HATA: ARSession yok!");
        }
        else
        {
            arStatus = "ARSession bulundu";
            AddLog("ARSession OK");
        }

        // Image Manager bul
        if (imageManager == null)
            imageManager = FindObjectOfType<ARTrackedImageManager>();

        if (imageManager == null)
        {
            imageLibraryStatus = "HATA: ARTrackedImageManager BULUNAMADI!";
            AddLog("HATA: ARTrackedImageManager yok!");
        }
        else
        {
            AddLog("ARTrackedImageManager bulundu");

            // Reference library kontrol
            if (imageManager.referenceLibrary == null)
            {
                imageLibraryStatus = "HATA: Reference Library ATANMAMIS!";
                AddLog("HATA: Image library null!");
            }
            else
            {
                int count = imageManager.referenceLibrary.count;
                imageLibraryStatus = $"Library OK: {count} gorsel";
                AddLog($"Image Library: {count} gorsel var");

                // Gorsellerin isimlerini listele
                for (int i = 0; i < count; i++)
                {
                    var img = imageManager.referenceLibrary[i];
                    AddLog($"  - {img.name} ({img.size.x:F2}x{img.size.y:F2}m)");
                }
            }

            // Event'e abone ol
            imageManager.trackedImagesChanged += OnTrackedImagesChanged;
            AddLog("Event listener eklendi");
        }
    }

    void OnDestroy()
    {
        if (imageManager != null)
            imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void Update()
    {
        // AR Session durumunu guncelle
        if (arSession != null)
        {
            arStatus = $"ARSession: {ARSession.state}";
        }

        // Tracking durumunu guncelle
        if (imageManager != null)
        {
            var trackables = imageManager.trackables;
            int tracking = 0;
            int total = 0;

            foreach (var img in trackables)
            {
                total++;
                if (img.trackingState == TrackingState.Tracking)
                    tracking++;
            }

            trackingStatus = $"Tracking: {tracking}/{total} gorsel";
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // Yeni eklenen gorseller
        foreach (var img in args.added)
        {
            string name = img.referenceImage.name;
            AddLog($">>> GORSEL BULUNDU: {name} <<<");
            trackingStatus = $"BULUNDU: {name}";

            if (spawnOnImageFound)
            {
                SpawnCylinderOnImage(img);
            }
        }

        // Guncellenen gorseller
        foreach (var img in args.updated)
        {
            string name = img.referenceImage.name;
            string state = img.trackingState.ToString();

            if (img.trackingState == TrackingState.Tracking)
            {
                // Obje varsa guncelle
                if (spawnedObjects.ContainsKey(name) && spawnedObjects[name] != null)
                {
                    spawnedObjects[name].SetActive(true);
                    UpdateSpawnedPosition(spawnedObjects[name], img);
                }
            }
            else
            {
                // Tracking kaybedildi
                if (spawnedObjects.ContainsKey(name) && spawnedObjects[name] != null)
                {
                    spawnedObjects[name].SetActive(false);
                }
            }
        }

        // Kaldirilan gorseller
        foreach (var img in args.removed)
        {
            string name = img.referenceImage.name;
            AddLog($"Gorsel kayboldu: {name}");

            if (spawnedObjects.ContainsKey(name) && spawnedObjects[name] != null)
            {
                Destroy(spawnedObjects[name]);
                spawnedObjects.Remove(name);
            }
        }
    }

    void SpawnCylinderOnImage(ARTrackedImage img)
    {
        string name = img.referenceImage.name;

        if (spawnedObjects.ContainsKey(name))
        {
            AddLog($"Zaten var: {name}");
            return;
        }

        // Gorselin onunde silindir olustur
        Vector3 pos = img.transform.position + img.transform.forward * 0.1f;

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "AR_Cylinder_" + name;
        cylinder.transform.position = pos;
        cylinder.transform.rotation = img.transform.rotation;
        cylinder.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);

        // Yesil renk
        Renderer rend = cylinder.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.green;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.green * 0.5f);
            rend.material = mat;
        }

        spawnedObjects[name] = cylinder;
        AddLog($"Silindir olusturuldu: {name}");
    }

    void UpdateSpawnedPosition(GameObject obj, ARTrackedImage img)
    {
        Vector3 pos = img.transform.position + img.transform.forward * 0.1f;
        obj.transform.position = pos;
        obj.transform.rotation = img.transform.rotation;
    }

    void AddLog(string msg)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        logMessages.Add($"[{timestamp}] {msg}");

        // Max 15 mesaj tut
        if (logMessages.Count > 15)
            logMessages.RemoveAt(0);

        Debug.Log($"[ARDebug] {msg}");
    }

    void OnGUI()
    {
        if (!showDebugUI) return;

        int boxWidth = 450;
        int boxHeight = 400;

        // Ana kutu
        GUI.Box(new Rect(10, 10, boxWidth, boxHeight), "");

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.cyan;

        GUIStyle normalStyle = new GUIStyle(GUI.skin.label);
        normalStyle.fontSize = 16;
        normalStyle.normal.textColor = Color.white;

        GUIStyle errorStyle = new GUIStyle(GUI.skin.label);
        errorStyle.fontSize = 16;
        errorStyle.normal.textColor = Color.red;

        GUIStyle successStyle = new GUIStyle(GUI.skin.label);
        successStyle.fontSize = 16;
        successStyle.normal.textColor = Color.green;

        int y = 15;

        // Baslik
        GUI.Label(new Rect(20, y, boxWidth - 30, 30), "=== AR DEBUG ===", titleStyle);
        y += 30;

        // AR Status
        GUIStyle statusStyle = arStatus.Contains("HATA") ? errorStyle : successStyle;
        GUI.Label(new Rect(20, y, boxWidth - 30, 25), arStatus, statusStyle);
        y += 22;

        // Image Library Status
        statusStyle = imageLibraryStatus.Contains("HATA") ? errorStyle : successStyle;
        GUI.Label(new Rect(20, y, boxWidth - 30, 25), imageLibraryStatus, statusStyle);
        y += 22;

        // Tracking Status
        statusStyle = trackingStatus.Contains("BULUNDU") ? successStyle : normalStyle;
        GUI.Label(new Rect(20, y, boxWidth - 30, 25), trackingStatus, statusStyle);
        y += 30;

        // Log mesajlari
        GUI.Label(new Rect(20, y, boxWidth - 30, 25), "--- LOG ---", titleStyle);
        y += 25;

        GUIStyle logStyle = new GUIStyle(GUI.skin.label);
        logStyle.fontSize = 12;
        logStyle.normal.textColor = Color.yellow;

        foreach (string log in logMessages)
        {
            GUI.Label(new Rect(20, y, boxWidth - 30, 18), log, logStyle);
            y += 16;
        }
    }
}
