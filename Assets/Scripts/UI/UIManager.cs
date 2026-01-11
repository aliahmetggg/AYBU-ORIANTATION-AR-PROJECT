using System;
using UnityEngine;

/// <summary>
/// UI ekranlarını yönetir
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panelleri")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameHUDPanel;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("AR Kamera")]
    [SerializeField] private GameObject arSessionOrigin;

    public enum UIState
    {
        MainMenu,
        Game,
        Map,
        Paused
    }

    public UIState CurrentState { get; private set; } = UIState.MainMenu;

    public event Action<UIState> OnStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// Ana menüyü göster
    /// </summary>
    public void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(gameHUDPanel, false);
        SetPanelActive(mapPanel, false);
        SetPanelActive(pausePanel, false);

        // AR'ı kapat
        if (arSessionOrigin != null)
            arSessionOrigin.SetActive(false);

        CurrentState = UIState.MainMenu;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// Oyunu başlat
    /// </summary>
    public void StartGame()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(gameHUDPanel, true);
        SetPanelActive(mapPanel, false);
        SetPanelActive(pausePanel, false);

        // AR'ı aç
        if (arSessionOrigin != null)
            arSessionOrigin.SetActive(true);

        CurrentState = UIState.Game;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// Haritayı göster
    /// </summary>
    public void ShowMap()
    {
        SetPanelActive(mapPanel, true);

        CurrentState = UIState.Map;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// Haritayı kapat
    /// </summary>
    public void HideMap()
    {
        SetPanelActive(mapPanel, false);

        CurrentState = UIState.Game;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// Harita toggle
    /// </summary>
    public void ToggleMap()
    {
        if (CurrentState == UIState.Map)
            HideMap();
        else
            ShowMap();
    }

    /// <summary>
    /// Pause menüsünü göster
    /// </summary>
    public void ShowPause()
    {
        SetPanelActive(pausePanel, true);
        Time.timeScale = 0f;

        CurrentState = UIState.Paused;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// Oyuna devam et
    /// </summary>
    public void ResumeGame()
    {
        SetPanelActive(pausePanel, false);
        Time.timeScale = 1f;

        CurrentState = UIState.Game;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// Oyundan çık
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Panel aktifliğini ayarla
    /// </summary>
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    /// <summary>
    /// Back butonu işleme (Android)
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }

    private void HandleBackButton()
    {
        switch (CurrentState)
        {
            case UIState.Game:
                ShowPause();
                break;
            case UIState.Map:
                HideMap();
                break;
            case UIState.Paused:
                ResumeGame();
                break;
            case UIState.MainMenu:
                QuitGame();
                break;
        }
    }
}
