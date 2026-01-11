#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// AYBU Orientation otomatik kurulum sihirbazı
/// Unity menüsünden: AYBU > Setup Wizard
/// </summary>
public class AYBUSetupWizard : EditorWindow
{
    private bool createLocationsData = true;
    private bool createAvatarPrefab = true;
    private bool createCylinderAvatar = true;
    private bool createUICanvas = true;
    private bool createGameManager = true;
    private bool createMapMarkerPrefab = true;

    [MenuItem("AYBU/Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<AYBUSetupWizard>("AYBU Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("AYBU Orientation Kurulum Sihirbazı", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Bu sihirbaz projeniz için gerekli asset'leri ve objeleri otomatik oluşturur.", MessageType.Info);
        GUILayout.Space(10);

        GUILayout.Label("Oluşturulacaklar:", EditorStyles.boldLabel);
        createLocationsData = EditorGUILayout.Toggle("Campus Locations Data", createLocationsData);
        createAvatarPrefab = EditorGUILayout.Toggle("Sphere Avatar Prefab", createAvatarPrefab);
        createCylinderAvatar = EditorGUILayout.Toggle("Cylinder Avatar Prefab", createCylinderAvatar);
        createMapMarkerPrefab = EditorGUILayout.Toggle("Map Marker Prefab", createMapMarkerPrefab);
        createGameManager = EditorGUILayout.Toggle("GameManager Object", createGameManager);
        createUICanvas = EditorGUILayout.Toggle("UI Canvas", createUICanvas);

        GUILayout.Space(20);

        if (GUILayout.Button("Kurulumu Başlat", GUILayout.Height(40)))
        {
            RunSetup();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Demo: Sadece mescit lokasyonu ile test için", MessageType.None);

        if (GUILayout.Button("Mescit Demo Setup", GUILayout.Height(35)))
        {
            CreateMescitDemoSetup();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Sadece Örnek Lokasyonları Oluştur"))
        {
            CreateSampleLocationsData();
        }
    }

    private void RunSetup()
    {
        if (createLocationsData)
            CreateSampleLocationsData();

        if (createAvatarPrefab)
            CreateAvatarPrefab();

        if (createCylinderAvatar)
            CreateCylinderAvatarPrefab();

        if (createMapMarkerPrefab)
            CreateMapMarkerPrefab();

        if (createGameManager)
            CreateGameManagerObject();

        if (createUICanvas)
            CreateUICanvas();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Kurulum Tamamlandı",
            "AYBU Orientation kurulumu tamamlandı!\n\n" +
            "Sonraki adımlar:\n" +
            "1. AYBULocations asset'inde GPS koordinatlarını güncelleyin\n" +
            "2. AR referans görsellerini ekleyin\n" +
            "3. UI panellerini düzenleyin",
            "Tamam");
    }

    /// <summary>
    /// Mescit demo setup - sadece mescit lokasyonu ile hızlı test
    /// </summary>
    private void CreateMescitDemoSetup()
    {
        // Data klasörü oluştur
        string path = "Assets/Data";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }

        // Silindir avatar prefab oluştur
        GameObject cylinderPrefab = CreateCylinderAvatarPrefab();

        // Mescit lokasyon verisi oluştur
        CampusLocationsData data = ScriptableObject.CreateInstance<CampusLocationsData>();

        var mescit = new CampusLocation
        {
            locationId = "mescit",
            locationName = "Mescit",
            description = "AYBU Kampüs Mescidi - Namaz vakitlerinde açıktır",
            latitude = 39.9712,
            longitude = 32.8182,
            radius = 30f,
            referenceImageName = "aybu_mescid1",
            additionalImageNames = new System.Collections.Generic.List<string> { "aybu_mescid2", "aybu_mescid3" },
            scoreValue = 15
        };

        // Avatar prefab'ı bağla
        string prefabPath = "Assets/Prefabs/CylinderAvatar.prefab";
        GameObject loadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (loadedPrefab != null)
        {
            mescit.avatarPrefab = loadedPrefab;
        }

        data.locations = new System.Collections.Generic.List<CampusLocation> { mescit };

        AssetDatabase.CreateAsset(data, "Assets/Data/MescitDemo.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Mescit Demo Hazır!",
            "Mescit demo setup tamamlandı!\n\n" +
            "Oluşturulanlar:\n" +
            "• MescitDemo.asset (Data klasöründe)\n" +
            "• CylinderAvatar.prefab (Prefabs klasöründe)\n\n" +
            "Sonraki adımlar:\n" +
            "1. XR Origin > AR Tracked Image Manager'da\n" +
            "   Serialized Library'e mescit fotoğraflarını ekleyin:\n" +
            "   - aybu_mescid1\n" +
            "   - aybu_mescid2\n" +
            "   - aybu_mescid3\n\n" +
            "2. LocationVerifier'a MescitDemo.asset'i bağlayın\n\n" +
            "3. Test için requireBothConditions = false yapabilirsiniz",
            "Tamam");

        Selection.activeObject = data;
    }

    private void CreateSampleLocationsData()
    {
        string path = "Assets/Data";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }

        CampusLocationsData data = ScriptableObject.CreateInstance<CampusLocationsData>();

        // Örnek AYBU lokasyonları
        data.locations = new System.Collections.Generic.List<CampusLocation>
        {
            new CampusLocation
            {
                locationId = "kutuphane",
                locationName = "Kütüphane",
                description = "AYBU Merkez Kütüphanesi",
                latitude = 39.9334,
                longitude = 32.8597,
                radius = 25f,
                referenceImageName = "kutuphane_marker",
                scoreValue = 10
            },
            new CampusLocation
            {
                locationId = "mescit",
                locationName = "Mescit",
                description = "AYBU Kampüs Mescidi",
                latitude = 39.9712,
                longitude = 32.8182,
                radius = 30f,
                referenceImageName = "aybu_mescid1",
                additionalImageNames = new System.Collections.Generic.List<string> { "aybu_mescid2", "aybu_mescid3" },
                scoreValue = 15
            },
            new CampusLocation
            {
                locationId = "yemekhane",
                locationName = "Yemekhane",
                description = "Merkez Yemekhane",
                latitude = 39.9330,
                longitude = 32.8590,
                radius = 30f,
                referenceImageName = "yemekhane_marker",
                scoreValue = 10
            },
            new CampusLocation
            {
                locationId = "rektorluk",
                locationName = "Rektörlük",
                description = "AYBU Rektörlük Binası",
                latitude = 39.9345,
                longitude = 32.8610,
                radius = 20f,
                referenceImageName = "rektorluk_marker",
                scoreValue = 15
            },
            new CampusLocation
            {
                locationId = "spor_salonu",
                locationName = "Spor Salonu",
                description = "Kapalı Spor Salonu",
                latitude = 39.9325,
                longitude = 32.8580,
                radius = 25f,
                referenceImageName = "spor_marker",
                scoreValue = 10
            }
        };

        AssetDatabase.CreateAsset(data, "Assets/Data/AYBULocations.asset");
        Debug.Log("AYBULocations.asset oluşturuldu!");
        Selection.activeObject = data;
    }

    private void CreateAvatarPrefab()
    {
        string prefabPath = "Assets/Prefabs/CollectableAvatar.prefab";

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject avatar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        avatar.name = "CollectableAvatar";
        avatar.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        // Material
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
            mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.6f, 1f);

        string matPath = "Assets/Prefabs/AvatarMaterial.mat";
        AssetDatabase.CreateAsset(mat, matPath);

        avatar.GetComponent<Renderer>().sharedMaterial = mat;
        avatar.AddComponent<SimpleAvatar>();

        PrefabUtility.SaveAsPrefabAsset(avatar, prefabPath);
        DestroyImmediate(avatar);

        Debug.Log("CollectableAvatar.prefab oluşturuldu!");
    }

    private GameObject CreateCylinderAvatarPrefab()
    {
        string prefabPath = "Assets/Prefabs/CylinderAvatar.prefab";

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Ana obje
        GameObject avatar = new GameObject("CylinderAvatar");

        // Silindir mesh oluştur
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "CylinderMesh";
        cylinder.transform.SetParent(avatar.transform, false);
        cylinder.transform.localPosition = new Vector3(0, 0.15f, 0);
        cylinder.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        // Collider'ı trigger yap
        Collider col = cylinder.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Material oluştur (yeşil parlak)
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
            mat = new Material(Shader.Find("Standard"));

        mat.color = new Color(0.2f, 0.9f, 0.4f); // Yeşil

        // Emission (glow efekti)
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.1f, 0.5f, 0.2f));

        string matPath = "Assets/Prefabs/CylinderMaterial.mat";
        AssetDatabase.CreateAsset(mat, matPath);

        cylinder.GetComponent<Renderer>().sharedMaterial = mat;

        // CylinderAvatarGenerator script ekle
        avatar.AddComponent<CylinderAvatarGenerator>();

        // Prefab olarak kaydet
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(avatar, prefabPath);
        DestroyImmediate(avatar);

        Debug.Log("CylinderAvatar.prefab oluşturuldu!");

        return prefab;
    }

    private void CreateMapMarkerPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/MapMarker.prefab";

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        GameObject marker = new GameObject("MapMarker");
        Image img = marker.AddComponent<Image>();
        img.color = Color.gray;

        RectTransform rt = marker.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(30, 30);

        marker.AddComponent<Button>().targetGraphic = img;

        PrefabUtility.SaveAsPrefabAsset(marker, prefabPath);
        DestroyImmediate(marker);

        Debug.Log("MapMarker.prefab oluşturuldu!");
    }

    private void CreateGameManagerObject()
    {
        if (GameObject.Find("GameManager") != null)
        {
            Debug.LogWarning("GameManager zaten mevcut!");
            return;
        }

        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GPSManager>();
        gm.AddComponent<LocationVerifier>();
        gm.AddComponent<ScoreManager>();
        gm.AddComponent<AvatarManager>();
        gm.AddComponent<GameManager>();

        // CampusLocationsData bağla
        string[] guids = AssetDatabase.FindAssets("t:CampusLocationsData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CampusLocationsData data = AssetDatabase.LoadAssetAtPath<CampusLocationsData>(path);

            SerializedObject so;

            so = new SerializedObject(gm.GetComponent<LocationVerifier>());
            so.FindProperty("campusData").objectReferenceValue = data;
            so.ApplyModifiedProperties();

            so = new SerializedObject(gm.GetComponent<ScoreManager>());
            so.FindProperty("campusData").objectReferenceValue = data;
            so.ApplyModifiedProperties();

            so = new SerializedObject(gm.GetComponent<GameManager>());
            so.FindProperty("campusData").objectReferenceValue = data;
            so.ApplyModifiedProperties();
        }

        Debug.Log("GameManager oluşturuldu!");
        Selection.activeGameObject = gm;
    }

    private void CreateUICanvas()
    {
        if (GameObject.Find("MainCanvas") != null)
        {
            Debug.LogWarning("MainCanvas zaten mevcut!");
            return;
        }

        // Canvas
        GameObject canvasObj = new GameObject("MainCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.AddComponent<UIManager>();

        // EventSystem
        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // === MAIN MENU PANEL ===
        GameObject mainMenu = CreatePanel(canvasObj.transform, "MainMenuPanel", true);
        mainMenu.AddComponent<MainMenuUI>();

        CreateText(mainMenu.transform, "TitleText", "AYBU ORIENTATION", new Vector2(0, 300), 60, TextAnchor.MiddleCenter);
        CreateText(mainMenu.transform, "SubtitleText", "Kampüs Keşif Oyunu", new Vector2(0, 220), 30, TextAnchor.MiddleCenter);
        CreateButton(mainMenu.transform, "StartButton", "BAŞLA", new Vector2(0, 50), new Vector2(300, 80));
        CreateButton(mainMenu.transform, "MapButton", "HARİTA", new Vector2(0, -50), new Vector2(300, 80));
        CreateButton(mainMenu.transform, "ResetButton", "SIFIRLA", new Vector2(0, -150), new Vector2(200, 60));
        CreateText(mainMenu.transform, "ScoreText", "Skor: 0", new Vector2(0, -280), 28, TextAnchor.MiddleCenter);
        CreateText(mainMenu.transform, "CollectionText", "Toplanan: 0/5", new Vector2(0, -330), 24, TextAnchor.MiddleCenter);

        // === GAME HUD PANEL ===
        GameObject hudPanel = CreatePanel(canvasObj.transform, "GameHUDPanel", false);
        hudPanel.AddComponent<HUDController>();

        // Üst bar
        GameObject topBar = CreatePanel(hudPanel.transform, "TopBar", true);
        RectTransform topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0, 1);
        topBarRT.anchorMax = new Vector2(1, 1);
        topBarRT.pivot = new Vector2(0.5f, 1);
        topBarRT.sizeDelta = new Vector2(0, 120);
        topBarRT.anchoredPosition = Vector2.zero;
        topBar.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

        CreateText(topBar.transform, "HUDScoreText", "Skor: 0", new Vector2(-300, -30), 32, TextAnchor.MiddleLeft);
        CreateText(topBar.transform, "HUDLocationText", "Konum: -", new Vector2(0, -30), 24, TextAnchor.MiddleCenter);
        CreateText(topBar.transform, "HUDCollectionText", "0/5", new Vector2(300, -30), 32, TextAnchor.MiddleRight);
        CreateText(topBar.transform, "GPSStatusText", "GPS: Bekleniyor", new Vector2(0, -80), 18, TextAnchor.MiddleCenter);

        CreateButton(hudPanel.transform, "HUDMapButton", "HARİTA", new Vector2(-200, -800), new Vector2(150, 60));
        CreateButton(hudPanel.transform, "HUDMenuButton", "MENÜ", new Vector2(200, -800), new Vector2(150, 60));

        // Bildirim
        GameObject notifPanel = CreatePanel(hudPanel.transform, "NotificationPanel", false);
        RectTransform notifRT = notifPanel.GetComponent<RectTransform>();
        notifRT.anchorMin = new Vector2(0.5f, 0.5f);
        notifRT.anchorMax = new Vector2(0.5f, 0.5f);
        notifRT.sizeDelta = new Vector2(600, 100);
        notifRT.anchoredPosition = new Vector2(0, 200);
        notifPanel.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 0.9f);
        CreateText(notifPanel.transform, "NotificationText", "Bildirim", Vector2.zero, 24, TextAnchor.MiddleCenter);

        // === MAP PANEL ===
        GameObject mapPanel = CreatePanel(canvasObj.transform, "MapPanel", false);
        mapPanel.AddComponent<MapUI>();
        mapPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.95f);

        CreateText(mapPanel.transform, "MapTitle", "KAMPÜS HARİTASI", new Vector2(0, 400), 40, TextAnchor.MiddleCenter);

        GameObject mapImage = new GameObject("MapImage");
        mapImage.transform.SetParent(mapPanel.transform, false);
        Image mapImg = mapImage.AddComponent<Image>();
        mapImg.color = new Color(0.3f, 0.3f, 0.3f);
        RectTransform mapImgRT = mapImage.GetComponent<RectTransform>();
        mapImgRT.sizeDelta = new Vector2(800, 600);

        CreateButton(mapPanel.transform, "CloseMapButton", "KAPAT", new Vector2(0, -400), new Vector2(200, 60));

        GameObject infoPanel = CreatePanel(mapPanel.transform, "LocationInfoPanel", false);
        RectTransform infoRT = infoPanel.GetComponent<RectTransform>();
        infoRT.sizeDelta = new Vector2(400, 150);
        infoRT.anchoredPosition = new Vector2(0, -250);
        infoPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        CreateText(infoPanel.transform, "LocationNameText", "Lokasyon Adı", new Vector2(0, 40), 24, TextAnchor.MiddleCenter);
        CreateText(infoPanel.transform, "LocationDescText", "Açıklama", new Vector2(0, 0), 18, TextAnchor.MiddleCenter);
        CreateText(infoPanel.transform, "LocationStatusText", "Durum", new Vector2(0, -40), 20, TextAnchor.MiddleCenter);

        // === PAUSE PANEL ===
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", false);
        pausePanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        CreateText(pausePanel.transform, "PauseTitle", "DURAKLATILDI", new Vector2(0, 200), 48, TextAnchor.MiddleCenter);
        CreateButton(pausePanel.transform, "ResumeButton", "DEVAM ET", new Vector2(0, 50), new Vector2(300, 80));
        CreateButton(pausePanel.transform, "MainMenuButton", "ANA MENÜ", new Vector2(0, -50), new Vector2(300, 80));
        CreateButton(pausePanel.transform, "QuitButton", "ÇIKIŞ", new Vector2(0, -150), new Vector2(200, 60));

        // UIManager bağla
        SerializedObject soUI = new SerializedObject(canvasObj.GetComponent<UIManager>());
        soUI.FindProperty("mainMenuPanel").objectReferenceValue = mainMenu;
        soUI.FindProperty("gameHUDPanel").objectReferenceValue = hudPanel;
        soUI.FindProperty("mapPanel").objectReferenceValue = mapPanel;
        soUI.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        soUI.ApplyModifiedProperties();

        Debug.Log("UI Canvas oluşturuldu!");
        Selection.activeGameObject = canvasObj;
    }

    private GameObject CreatePanel(Transform parent, string name, bool active)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.2f, 1f);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        panel.SetActive(active);
        return panel;
    }

    private void CreateText(Transform parent, string name, string text, Vector2 position, int fontSize, TextAnchor alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        Text txt = textObj.AddComponent<Text>();
        txt.text = text;
        txt.fontSize = fontSize;
        txt.alignment = alignment;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 100);
        rt.anchoredPosition = position;
    }

    private void CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.8f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = position;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        Text txt = textObj.AddComponent<Text>();
        txt.text = text;
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
    }
}
#endif
