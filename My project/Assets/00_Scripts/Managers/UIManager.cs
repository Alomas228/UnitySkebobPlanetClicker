// Assets/00_Scripts/Managers/UIManager.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : Singleton<UIManager>
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject confirmationDialog;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Confirmation Dialog")]
    [SerializeField] private TextMeshProUGUI dialogTitle;
    [SerializeField] private TextMeshProUGUI dialogMessage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Menu Texts")]
    [SerializeField] private TextMeshProUGUI lastSaveText;
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("Settings Panel")]
    [SerializeField] private Button backButton;

    // События UI
    public event Action OnGameStarted;
    public event Action OnReturnToMenu;

    private Action _pendingConfirmationAction;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("UIManager Awake called");
        ServiceLocator.Register<UIManager>(this);
    }

    private void Start()
    {
        InitializeUI();
        ShowMainMenu();
    }

    private void InitializeUI()
    {
        Debug.Log("InitializeUI started");

        // Инициализируем только ОСНОВНЫЕ кнопки меню
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        // Настройка кнопки Назад
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
            Debug.Log("BackButton initialized");
        }

        // Временно закомментируем диалог
        // if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
        // if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClicked);

        if (versionText != null) versionText.text = $"Версия: {Application.version}";

        UpdateContinueButton();
        Debug.Log("InitializeUI completed");
    }

    private void UpdateContinueButton()
    {
        bool saveExists = SaveManager.Instance.SaveExists;
        continueButton.interactable = saveExists;

        if (saveExists)
        {
            DateTime lastSave = SaveManager.Instance.GetLastSaveTime();
            lastSaveText.text = $"Последнее сохранение: {lastSave:dd.MM.yyyy HH:mm}";
        }
        else
        {
            lastSaveText.text = "Нет сохраненной игры";
            continueButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }
    }

    #region Button Handlers

    private void OnContinueClicked()
    {
        Debug.Log("Продолжить игру");

        if (!SaveManager.Instance.SaveExists)
        {
            Debug.LogWarning("Нет сохранения для продолжения");
            return;
        }

        // Загружаем игру
        GameData loadedData = SaveManager.Instance.LoadGame();

        // Переходим в игру
        StartGame();

        // TODO: Загрузить данные в игровые системы
        Debug.Log($"Загружена игра с {loadedData.colonyPopulation} колонистами");
    }

    private void OnNewGameClicked()
    {
        Debug.Log("Новая игра");

        if (SaveManager.Instance.SaveExists)
        {
            ShowConfirmationDialog(
                "Новая игра",
                "Вы уверены? Все прогресс текущей игры будет потерян.",
                StartNewGame
            );
        }
        else
        {
            StartNewGame();
        }
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Настройки");
        ShowSettings();
    }

    private void OnExitClicked()
    {
        Debug.Log("Выход");

        ShowConfirmationDialog(
            "Выход",
            "Вы уверены, что хотите выйти?",
            QuitGame
        );
    }

    private void OnBackClicked()
    {
        Debug.Log("Back clicked");
        ShowMainMenu();
    }

    private void OnConfirmClicked()
    {
        confirmationDialog.SetActive(false);
        _pendingConfirmationAction?.Invoke();
        _pendingConfirmationAction = null;
    }

    private void OnCancelClicked()
    {
        confirmationDialog.SetActive(false);
        _pendingConfirmationAction = null;
    }

    #endregion

    #region Game Flow Methods

    private void StartGame()
    {
        GameManager.Instance.StartGame();
        ShowGameUI();
        OnGameStarted?.Invoke();
    }

    private void StartNewGame()
    {
        Debug.Log("Создание новой игры");

        // Удаляем старое сохранение
        SaveManager.Instance.DeleteSave();

        // Создаем чистые данные
        GameData newGameData = CreateNewGameData();

        // Сохраняем новую игру
        SaveManager.Instance.SaveGame(newGameData);

        // Переходим в игру
        StartGame();

        Debug.Log("Новая игра создана");
    }

    private GameData CreateNewGameData()
    {
        GameData newData = new GameData();

        // Стартовые ресурсы
        newData.resources.Add("Weightome", 10);
        newData.resources.Add("Food", 5);
        newData.resources.Add("Metal", 0);
        newData.resources.Add("Wood", 0);

        // Стартовое население
        newData.colonyPopulation = 0;
        newData.daysPassed = 0;

        return newData;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion

    #region Panel Management

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        gameUI.SetActive(false);
        confirmationDialog.SetActive(false);

        GameManager.Instance.ReturnToMenu();
        UpdateContinueButton();
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        gameUI.SetActive(false);
        confirmationDialog.SetActive(false);
    }

    public void ShowGameUI()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameUI.SetActive(true);
        confirmationDialog.SetActive(false);
    }

    private void ShowConfirmationDialog(string title, string message, Action confirmAction)
    {
        dialogTitle.text = title;
        dialogMessage.text = message;
        _pendingConfirmationAction = confirmAction;

        confirmationDialog.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        ShowMainMenu();
        OnReturnToMenu?.Invoke();
    }

    #endregion

    #region Public Methods

    public void ShowNotification(string message, float duration = 3f)
    {
        // TODO: Реализовать систему уведомлений
        Debug.Log($"UI Notification: {message}");
    }

    public void UpdateResourceDisplay(string resourceType, int amount)
    {
        // TODO: Обновление отображения ресурсов в игровом UI
        Debug.Log($"Resource updated: {resourceType} = {amount}");
    }

    #endregion
}