using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// SUPER BASIT TEST - Ekrana dokun, silindir ciksin
/// + AR DEBUG bilgisi gosterir
/// </summary>
public class SimpleTestSpawner : MonoBehaviour
{
    [Header("Bu script aktif mi?")]
    public bool isActive = true;

    private int spawnCount = 0;

    // AR Debug
    private ARSession arSession;
    private ARTrackedImageManager imageManager;
    private string arInfo = "AR: Aranıyor...";
    private string lastFoundImage = "Henuz gorsel bulunamadi";
    private Dictionary<string, GameObject> arSpawnedObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        // AR bilesenleri bul
        arSession = FindObjectOfType<ARSession>();
        imageManager = FindObjectOfType<ARTrackedImageManager>();

        if (imageManager != null)
        {
            imageManager.trackedImagesChanged += OnImagesChanged;

            if (imageManager.referenceLibrary != null)
            {
                arInfo = $"AR OK - {imageManager.referenceLibrary.count} gorsel yuklu";
            }
            else
            {
                arInfo = "AR HATA: Library YOK!";
            }
        }
        else
        {
            arInfo = "AR HATA: ImageManager YOK!";
        }
    }

    void OnDestroy()
    {
        if (imageManager != null)
            imageManager.trackedImagesChanged -= OnImagesChanged;
    }

    void OnImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // Yeni gorsel bulundu
        foreach (var img in args.added)
        {
            string name = img.referenceImage.name;
            lastFoundImage = $"BULUNDU: {name}";
            Debug.Log($">>> AR GORSEL BULUNDU: {name} <<<");
            SpawnOnARImage(img);
        }

        // Gorsel guncellendi
        foreach (var img in args.updated)
        {
            string name = img.referenceImage.name;
            if (arSpawnedObjects.ContainsKey(name) && arSpawnedObjects[name] != null)
            {
                bool isTracking = img.trackingState == TrackingState.Tracking;
                arSpawnedObjects[name].SetActive(isTracking);

                if (isTracking)
                {
                    // Pozisyonu guncelle
                    Vector3 pos = img.transform.position + img.transform.forward * 0.1f;
                    arSpawnedObjects[name].transform.position = pos;
                }
            }
        }
    }

    void SpawnOnARImage(ARTrackedImage img)
    {
        string name = img.referenceImage.name;
        if (arSpawnedObjects.ContainsKey(name)) return;

        // Görselin HEMEN ÜSTÜNDE - 3cm yukarıda
        Vector3 pos = img.transform.position + Vector3.up * 0.03f;

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "AR_" + name;
        cylinder.transform.position = pos;
        cylinder.transform.SetParent(img.transform); // Görsele bağla
        cylinder.transform.localScale = new Vector3(0.08f, 0.1f, 0.08f); // Küçük silindir

        Renderer rend = cylinder.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.magenta; // Pembe - AR'dan geldigini belli etsin
            rend.material = mat;
        }

        arSpawnedObjects[name] = cylinder;
    }

    void Update()
    {
        if (!isActive) return;

        // MOBIL: Ekrana dokunma
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Debug.Log(">>> DOKUNMA ALGILANDI! <<<");
            SpawnCylinder();
        }

        // PC: Mouse tiklamasi
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(">>> MOUSE TIKLAMA ALGILANDI! <<<");
            SpawnCylinder();
        }
    }

    void SpawnCylinder()
    {
        spawnCount++;
        Debug.Log($">>> SILINDIR OLUSTURULUYOR #{spawnCount} <<<");

        // Kamerayi bul
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }

        if (cam == null)
        {
            Debug.LogError(">>> KAMERA BULUNAMADI! <<<");
            return;
        }

        // Kameranin 2 metre onunde pozisyon
        Vector3 pos = cam.transform.position + cam.transform.forward * 2f;

        // Silindir olustur
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "TestSilindir_" + spawnCount;
        cylinder.transform.position = pos;
        cylinder.transform.localScale = new Vector3(0.3f, 0.75f, 0.3f);

        // Rastgele renk ver
        Renderer rend = cylinder.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(Random.value, Random.value, Random.value);
            rend.material = mat;
        }

        Debug.Log($">>> SILINDIR OLUSTURULDU: {pos} <<<");
    }

    // Ekranda bilgi goster
    void OnGUI()
    {
        if (!isActive) return;

        // Arka plan - daha buyuk
        GUI.Box(new Rect(10, 10, 400, 250), "");

        // Baslik
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.green;

        GUI.Label(new Rect(20, 15, 380, 30), "=== TEST + AR DEBUG ===", titleStyle);

        // AR Bilgi
        GUIStyle arStyle = new GUIStyle(GUI.skin.label);
        arStyle.fontSize = 18;
        arStyle.normal.textColor = arInfo.Contains("OK") ? Color.green : Color.red;
        GUI.Label(new Rect(20, 45, 380, 25), arInfo, arStyle);

        // Bulunan gorsel
        GUIStyle foundStyle = new GUIStyle(GUI.skin.label);
        foundStyle.fontSize = 18;
        foundStyle.normal.textColor = lastFoundImage.Contains("BULUNDU") ? Color.yellow : Color.gray;
        GUI.Label(new Rect(20, 70, 380, 25), lastFoundImage, foundStyle);

        // AR Session durumu
        GUIStyle sessionStyle = new GUIStyle(GUI.skin.label);
        sessionStyle.fontSize = 16;
        sessionStyle.normal.textColor = Color.cyan;
        string sessionInfo = arSession != null ? $"Session: {ARSession.state}" : "Session: YOK";
        GUI.Label(new Rect(20, 95, 380, 22), sessionInfo, sessionStyle);

        // Spawn sayisi
        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = 16;
        infoStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 117, 380, 22), $"Manuel spawn: {spawnCount} | AR spawn: {arSpawnedObjects.Count}", infoStyle);

        // Butonlar
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 20;

        if (GUI.Button(new Rect(20, 145, 180, 45), "MANUEL SPAWN", buttonStyle))
        {
            SpawnCylinder();
        }

        if (GUI.Button(new Rect(210, 145, 180, 45), "AR RESET", buttonStyle))
        {
            foreach (var obj in arSpawnedObjects.Values)
            {
                if (obj != null) Destroy(obj);
            }
            arSpawnedObjects.Clear();
            lastFoundImage = "Sifirlandi - Tekrar dene";
        }

        // Aciklama
        GUIStyle helpStyle = new GUIStyle(GUI.skin.label);
        helpStyle.fontSize = 14;
        helpStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 195, 380, 50), "Yesil silindir = Manuel\nPembe silindir = AR gorsel bulundu", helpStyle);
    }
}
