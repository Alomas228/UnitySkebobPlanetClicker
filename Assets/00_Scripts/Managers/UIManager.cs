// Assets/00_Scripts/Managers/UIManager.cs

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    // Поля для игрового UI
    [Header("Game UI Elements")]
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI weightomeText;
    [SerializeField] private TextMeshProUGUI metalText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI colonistsText;
    [SerializeField] private TextMeshProUGUI currentQuestText;

    // Меню строительства
    [Header("Building Menu")]
    [SerializeField] private GameObject buildingMenu;
    [SerializeField] private Button createButton;
    [SerializeField] private Button assignButton;
    [SerializeField] private Button closeBuildingMenuButton;

    // Меню распределения
    [Header("Assign Menu")]
    [SerializeField] private GameObject assignMenu;
    [SerializeField] private Button closeAssignMenuButton;
    [SerializeField] private TextMeshProUGUI totalColonistsText;

    // Данные для распределения
    private Dictionary<string, int> workplaceAssignments = new Dictionary<string, int>();
    private int totalColonists = 0;
    private int maxColonists = 10;

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
        if (createButton != null) createButton.onClick.AddListener(OnCreateClicked);
        if (assignButton != null) assignButton.onClick.AddListener(OnAssignClicked);
        if (closeBuildingMenuButton != null) closeBuildingMenuButton.onClick.AddListener(OnCloseBuildingMenuClicked);
        if (closeAssignMenuButton != null)
            closeAssignMenuButton.onClick.AddListener(OnCloseAssignMenuClicked);

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

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            Debug.Log("ConfirmButton initialized");
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
            Debug.Log("CancelButton initialized");
        }

        UpdateContinueButton();
        Debug.Log("InitializeUI completed");

    }

    private void OnAssignClicked()
    {
        Debug.Log("Меню распределения открыто");
        ShowAssignMenu();
        UpdateAssignMenu();
    }

    private void OnCloseAssignMenuClicked()
    {
        Debug.Log("Меню распределения закрыто");
        HideAssignMenu();
    }

    public void ShowAssignMenu()
    {
        if (assignMenu != null)
        {
            assignMenu.SetActive(true);
            HideBuildingMenu(); // Закрываем другое меню
        }
    }

    public void HideAssignMenu()
    {
        if (assignMenu != null) assignMenu.SetActive(false);
    }

    private void UpdateAssignMenu()
    {
        // Обновляем текст общего количества
        if (totalColonistsText != null)
            totalColonistsText.text = $"Всего: {totalColonists}/{maxColonists}";
    }
    private void OnCreateClicked()
    {
        Debug.Log("Меню создания открыто");
        ShowBuildingMenu();
    }

    private void InitializeWorkplaces()
    {
        workplaceAssignments.Add("Теплолист", 5);
        workplaceAssignments.Add("Ферма", 3);
        workplaceAssignments.Add("Шахта", 2);
        totalColonists = 10;
        maxColonists = 10;
    }


    private void OnCloseBuildingMenuClicked()
    {
        Debug.Log("Меню создания закрыто");
        HideBuildingMenu();
    }

    public void ShowBuildingMenu()
    {
        if (buildingMenu != null) buildingMenu.SetActive(true);
    }

    public void HideBuildingMenu()
    {
        if (buildingMenu != null) buildingMenu.SetActive(false);
    }
    public void UpdateResourceDisplay(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Food:
                if (foodText != null) foodText.text = $"{amount}";
                break;
            case ResourceType.Weightome:
                if (weightomeText != null) weightomeText.text = $"{amount}";
                break;
            case ResourceType.Metal:
                if (metalText != null) metalText.text = $"{amount}";
                break;
            case ResourceType.Wood:
                if (woodText != null) woodText.text = $"{amount}";
                break;
        }
    }

    public void UpdateColonistsDisplay(int current, int max)
    {
        if (colonistsText != null)
            colonistsText.text = $"{current}/{max}";
    }

    public void UpdateQuestDisplay(string questText)
    {
        if (currentQuestText != null)
            currentQuestText.text = questText;
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
        Debug.Log("Нажата кнопка 'Новая игра'");

        if (SaveManager.Instance.SaveExists)
        {
            Debug.Log("Есть сохранение - показываем диалог");
            ShowConfirmationDialog(
                "Новая игра",
                "Вы уверены, что хотите начать новую игру?\nТекущий прогресс будет потерян.",
                StartNewGame
            );
        }
        else
        {
            Debug.Log("Нет сохранения - начинаем сразу");
            StartNewGame();
        }
    }

    private System.Collections.IEnumerator TemporaryConfirmationDialog()
    {
        Debug.Log("=== ДИАЛОГ ПОДТВЕРЖДЕНИЯ ===");
        Debug.Log("Начать новую игру? Текущий прогресс будет потерян.");
        Debug.Log("Нажмите Y - Да, N - Нет");

        // Ждем ввода пользователя
        while (true)
        {
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                Debug.Log("Пользователь подтвердил новую игру");
                StartNewGame();
                yield break;
            }
            else if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                Debug.Log("Пользователь отменил новую игру");
                yield break;
            }
            yield return null;
        }
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Настройки");
        ShowSettings();
    }

    private void OnExitClicked()
    {
        Debug.Log("Выход из игры");
        QuitGame();
    }


    private void OnBackClicked()
    {
        Debug.Log("Back clicked");
        ShowMainMenu();
    }

    private void OnConfirmClicked()
    {
        Debug.Log("Нажата кнопка Подтвердить");
        confirmationDialog.SetActive(false);

        // ВКЛЮЧАЕМ главное меню обратно (или переходим в игру)
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            Debug.Log("MainMenuPanel включен");
        }

        _pendingConfirmationAction?.Invoke();
        _pendingConfirmationAction = null;
    }

    private void OnCancelClicked()
    {
        Debug.Log("Нажата кнопка Отмена");
        confirmationDialog.SetActive(false);

        // ВКЛЮЧАЕМ главное меню обратно
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            Debug.Log("MainMenuPanel включен");
        }

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
        Debug.Log("?? ЗАПУСК НОВОЙ ИГРЫ!");

        // Удаляем старое сохранение
        SaveManager.Instance.DeleteSave();

        // Создаем чистые данные
        GameData newGameData = CreateNewGameData();
        SaveManager.Instance.SaveGame(newGameData);

        // Переходим в игровой режим
        StartGame();

        // ОБНОВЛЯЕМ ИГРОВОЙ UI
        UpdateResourceDisplay(ResourceType.Food, 5);
        UpdateResourceDisplay(ResourceType.Weightome, 10);
        UpdateResourceDisplay(ResourceType.Metal, 0);
        UpdateResourceDisplay(ResourceType.Wood, 0);
        UpdateColonistsDisplay(0, 0);
        UpdateQuestDisplay("Устроить на работу 100 колонистов");

        Debug.Log("Новая игра успешно создана!");
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

        Debug.Log("Созданы стартовые данные для новой игры");

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
        Debug.Log($"Показываем диалог: {title}");

        // ВЫКЛЮЧАЕМ главное меню когда показываем диалог
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            Debug.Log("MainMenuPanel выключен");
        }

        // Устанавливаем текст
        dialogTitle.text = title;
        dialogMessage.text = message;
        _pendingConfirmationAction = confirmAction;

        // Показываем диалог
        confirmationDialog.SetActive(true);
        Debug.Log("Диалог показан успешно");
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