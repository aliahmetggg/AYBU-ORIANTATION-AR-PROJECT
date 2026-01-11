using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime'da UI oluşturur - Editor script'leri çalışmadığında kullan
/// </summary>
public class RuntimeUIBuilder : MonoBehaviour
{
    [Header("Otomatik Oluştur")]
    public bool buildOnStart = true;

    private Canvas canvas;
    private GameObject mainMenuPanel;
    private GameObject hudPanel;
    private GameObject mapPanel;
    private GameObject pausePanel;

    void Start()
    {
        if (buildOnStart)
        {
            BuildUI();
        }
    }

    [ContextMenu("Build UI")]
    public void BuildUI()
    {
        // Canvas bul veya oluştur
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Canvas Scaler düzelt
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // UI Panellerini oluştur
        CreateMainMenu();
        CreateHUD();
        CreateMapPanel();
        CreatePausePanel();

        // UIManager'ı bağla
        UIManager uiManager = canvas.GetComponent<UIManager>();
        if (uiManager == null)
            uiManager = canvas.gameObject.AddComponent<UIManager>();

        // Reflection ile private field'ları set et
        SetPrivateField(uiManager, "mainMenuPanel", mainMenuPanel);
        SetPrivateField(uiManager, "gameHUDPanel", hudPanel);
        SetPrivateField(uiManager, "mapPanel", mapPanel);
        SetPrivateField(uiManager, "pausePanel", pausePanel);

        // XR Origin bul ve bağla
        var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
            SetPrivateField(uiManager, "arSessionOrigin", xrOrigin.gameObject);

        Debug.Log("UI oluşturuldu!");
    }

    void CreateMainMenu()
    {
        mainMenuPanel = CreatePanel("MainMenuPanel", new Color(0.05f, 0.05f, 0.15f, 1f));
        mainMenuPanel.AddComponent<MainMenuUI>();

        // Başlık
        CreateText(mainMenuPanel.transform, "AYBU ORIENTATION", 52, new Vector2(0, 350), Color.white, FontStyle.Bold);
        CreateText(mainMenuPanel.transform, "Kampüs Keşif Oyunu", 32, new Vector2(0, 280), new Color(0.7f, 0.7f, 0.7f));

        // Butonlar
        CreateButton(mainMenuPanel.transform, "StartButton", "BAŞLA", new Vector2(0, 100), new Vector2(400, 100), OnStartClick);
        CreateButton(mainMenuPanel.transform, "MapButton", "HARİTA", new Vector2(0, -20), new Vector2(400, 100), OnMapClick);
        CreateButton(mainMenuPanel.transform, "ResetButton", "SIFIRLA", new Vector2(0, -140), new Vector2(300, 80), OnResetClick);

        // İstatistik
        CreateText(mainMenuPanel.transform, "Skor: 0", 36, new Vector2(0, -280), Color.yellow);
        CreateText(mainMenuPanel.transform, "Toplanan: 0/5", 30, new Vector2(0, -340), Color.cyan);

        mainMenuPanel.SetActive(true);
    }

    void CreateHUD()
    {
        hudPanel = CreatePanel("GameHUDPanel", new Color(0, 0, 0, 0));
        hudPanel.GetComponent<Image>().enabled = false;
        hudPanel.AddComponent<HUDController>();

        // Üst bar
        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(hudPanel.transform, false);
        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(0, 0, 0, 0.8f);
        RectTransform topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0, 1);
        topBarRT.anchorMax = new Vector2(1, 1);
        topBarRT.pivot = new Vector2(0.5f, 1);
        topBarRT.sizeDelta = new Vector2(0, 150);
        topBarRT.anchoredPosition = Vector2.zero;

        CreateText(topBar.transform, "Skor: 0", 40, new Vector2(-250, -40), Color.yellow);
        CreateText(topBar.transform, "0/5", 40, new Vector2(250, -40), Color.cyan);
        CreateText(topBar.transform, "Konum: -", 28, new Vector2(0, -40), Color.white);
        CreateText(topBar.transform, "GPS: Bekleniyor", 22, new Vector2(0, -100), Color.gray);

        // Alt butonlar
        CreateButton(hudPanel.transform, "HUDMapButton", "HARİTA", new Vector2(-150, -550), new Vector2(200, 80), OnMapClick);
        CreateButton(hudPanel.transform, "HUDMenuButton", "MENÜ", new Vector2(150, -550), new Vector2(200, 80), OnPauseClick);

        // Bildirim
        GameObject notif = new GameObject("NotificationPanel");
        notif.transform.SetParent(hudPanel.transform, false);
        Image notifImg = notif.AddComponent<Image>();
        notifImg.color = new Color(0.1f, 0.5f, 0.1f, 0.95f);
        RectTransform notifRT = notif.GetComponent<RectTransform>();
        notifRT.sizeDelta = new Vector2(600, 120);
        notifRT.anchoredPosition = new Vector2(0, 250);
        CreateText(notif.transform, "Bildirim", 32, Vector2.zero, Color.white);
        notif.SetActive(false);

        hudPanel.SetActive(false);
    }

    void CreateMapPanel()
    {
        mapPanel = CreatePanel("MapPanel", new Color(0, 0, 0, 0.95f));
        mapPanel.AddComponent<MapUI>();

        CreateText(mapPanel.transform, "KAMPÜS HARİTASI", 56, new Vector2(0, 450), Color.white, FontStyle.Bold);

        // Harita resmi
        GameObject mapImg = new GameObject("MapImage");
        mapImg.transform.SetParent(mapPanel.transform, false);
        Image img = mapImg.AddComponent<Image>();

        // Resources'tan harita resmini yükle
        Texture2D mapTexture = Resources.Load<Texture2D>("campus_map");
        if (mapTexture != null)
        {
            Sprite mapSprite = Sprite.Create(mapTexture,
                new Rect(0, 0, mapTexture.width, mapTexture.height),
                new Vector2(0.5f, 0.5f));
            img.sprite = mapSprite;
            img.color = Color.white;
        }
        else
        {
            img.color = new Color(0.2f, 0.3f, 0.2f); // Fallback renk
        }

        RectTransform mapRT = mapImg.GetComponent<RectTransform>();
        mapRT.sizeDelta = new Vector2(650, 650);
        mapRT.anchoredPosition = new Vector2(0, -50);

        // Lokasyon işaretleri (placeholder)
        CreateMapMarker(mapImg.transform, "Kütüphane", new Vector2(-100, 150), Color.gray);
        CreateMapMarker(mapImg.transform, "Mescit", new Vector2(100, 100), Color.gray);
        CreateMapMarker(mapImg.transform, "Yemekhane", new Vector2(-50, -100), Color.gray);
        CreateMapMarker(mapImg.transform, "Rektörlük", new Vector2(150, -50), Color.gray);
        CreateMapMarker(mapImg.transform, "Spor", new Vector2(-150, -150), Color.gray);

        // MapUI component'i mapImage referansını bağla
        MapUI mapUI = mapPanel.GetComponent<MapUI>();
        if (mapUI != null)
        {
            SetPrivateField(mapUI, "mapImage", img);
            SetPrivateField(mapUI, "mapContainer", mapRT);
        }

        CreateButton(mapPanel.transform, "CloseMapButton", "KAPAT", new Vector2(0, -450), new Vector2(250, 80), OnCloseMap);

        mapPanel.SetActive(false);
    }

    void CreatePausePanel()
    {
        pausePanel = CreatePanel("PausePanel", new Color(0, 0, 0, 0.9f));

        CreateText(pausePanel.transform, "DURAKLATILDI", 64, new Vector2(0, 250), Color.white, FontStyle.Bold);
        CreateButton(pausePanel.transform, "ResumeButton", "DEVAM ET", new Vector2(0, 80), new Vector2(400, 100), OnResumeClick);
        CreateButton(pausePanel.transform, "MainMenuButton", "ANA MENÜ", new Vector2(0, -40), new Vector2(400, 100), OnMainMenuClick);
        CreateButton(pausePanel.transform, "QuitButton", "ÇIKIŞ", new Vector2(0, -160), new Vector2(300, 80), OnQuitClick);

        pausePanel.SetActive(false);
    }

    void CreateMapMarker(Transform parent, string name, Vector2 pos, Color color)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        Image img = marker.AddComponent<Image>();
        img.color = color;
        RectTransform rt = marker.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
        rt.anchoredPosition = pos;

        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(marker.transform, false);
        Text txt = label.AddComponent<Text>();
        txt.text = name;
        txt.fontSize = 18;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform labelRT = label.GetComponent<RectTransform>();
        labelRT.sizeDelta = new Vector2(150, 30);
        labelRT.anchoredPosition = new Vector2(0, -45);
    }

    GameObject CreatePanel(string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        return panel;
    }

    void CreateText(Transform parent, string text, int size, Vector2 pos, Color color, FontStyle style = FontStyle.Normal)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.fontSize = size;
        txt.fontStyle = style;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = color;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(700, 100);
        rt.anchoredPosition = pos;
    }

    void CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f);

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(action);

        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f);
        colors.pressedColor = new Color(0.1f, 0.2f, 0.5f);
        btn.colors = colors;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        // Text
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(obj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.text = text;
        txt.fontSize = 36;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform txtRT = txtObj.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
    }

    void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            field.SetValue(obj, value);
    }

    // Button callbacks
    void OnStartClick() { UIManager.Instance?.StartGame(); }
    void OnMapClick() { UIManager.Instance?.ShowMap(); }
    void OnResetClick() { ScoreManager.Instance?.ResetProgress(); }
    void OnCloseMap() { UIManager.Instance?.HideMap(); }
    void OnPauseClick() { UIManager.Instance?.ShowPause(); }
    void OnResumeClick() { UIManager.Instance?.ResumeGame(); }
    void OnMainMenuClick() { UIManager.Instance?.ShowMainMenu(); }
    void OnQuitClick() { Application.Quit(); }
}
