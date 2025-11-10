// Assets/00_Scripts/_Core/GameManager.cs

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : Singleton<GameManager>
{
    [Header("Game State")]
    [SerializeField] private GameState _currentState = GameState.Menu;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference _pauseActionReference;

    // События для изменения состояния игры
    public event Action<GameState, GameState> OnGameStateChanged; // (previousState, newState)
    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameEnded;
    public event Action OnBuildingModeEntered;
    public event Action OnBuildingModeExited;

    public GameState CurrentState
    {
        get => _currentState;
        private set => _currentState = value;
    }

    public bool IsPlaying => _currentState == GameState.Playing;
    public bool IsPaused => _currentState == GameState.Paused;
    public bool IsBuilding => _currentState == GameState.Building;
    public bool IsGameOver => _currentState == GameState.GameOver;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("GameManager Awake called");
        InitializeInputSystem();
        InitializeGame();
    }


    private void Start()
    {
        // Переходим в начальное состояние
        SetState(GameState.Menu);

        // Пытаемся показать меню, но не падаем если UIManager еще не готов
        try
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMainMenu();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"UIManager not ready: {e.Message}");
        }
    }

    private void InitializeGame()
    {
        Debug.Log("Initializing Game Manager...");

        // Здесь позже будут регистрироваться все менеджеры
        // ServiceLocator.Register<ResourceManager>(ResourceManager.Instance);
        // ServiceLocator.Register<ColonistManager>(ColonistManager.Instance);
    }

    private void InitializeInputSystem()
    {
        // Настройка действия паузы
        if (_pauseActionReference != null)
        {
            _pauseActionReference.action.Enable();
            _pauseActionReference.action.performed += OnPausePerformed;
        }
        else
        {
            Debug.LogWarning("Pause Action Reference not assigned in GameManager!");
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        switch (_currentState)
        {
            case GameState.Playing:
                PauseGame();
                break;
            case GameState.Paused:
                ResumeGame();
                break;
            case GameState.Building:
                ExitBuildingMode();
                break;
        }
    }

    private void HandleGlobalInput()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            switch (_currentState)
            {
                case GameState.Playing:
                    PauseGame();
                    break;
                case GameState.Paused:
                    ResumeGame();
                    break;
                case GameState.Building:
                    ExitBuildingMode();
                    break;
                case GameState.Menu:
                    // Если в настройках - возврат в главное меню
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ReturnToMainMenu();
                    }
                    break;
            }
        }
    }

    public void SetState(GameState newState)
    {
        if (_currentState == newState) return;

        GameState previousState = _currentState;
        _currentState = newState;

        Debug.Log($"Game state changed: {previousState} -> {newState}");

        // Обработка перехода между состояниями
        HandleStateTransition(previousState, newState);

        // Вызываем событие
        OnGameStateChanged?.Invoke(previousState, newState);
    }

    private void HandleStateTransition(GameState from, GameState to)
    {
        switch (to)
        {
            case GameState.Playing:
                if (from == GameState.Menu)
                {
                    OnGameStarted?.Invoke();
                }
                else if (from == GameState.Paused)
                {
                    OnGameResumed?.Invoke();
                }
                else if (from == GameState.Building)
                {
                    OnBuildingModeExited?.Invoke();
                }
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                OnGamePaused?.Invoke();
                Time.timeScale = 0f;
                break;

            case GameState.Building:
                OnBuildingModeEntered?.Invoke();
                Time.timeScale = 1f;
                break;

            case GameState.GameOver:
                OnGameEnded?.Invoke();
                Time.timeScale = 0f;
                break;

            case GameState.Menu:
                Time.timeScale = 1f;
                break;
        }
    }

    #region Public Methods

    public void StartGame()
    {
        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        SetState(GameState.Playing);
    }

    public void EnterBuildingMode()
    {
        SetState(GameState.Building);
    }

    public void ExitBuildingMode()
    {
        SetState(GameState.Playing);
    }

    public void GameOver()
    {
        SetState(GameState.GameOver);
    }

    public void ReturnToMenu()
    {
        SetState(GameState.Menu);
        // Здесь позже будет логика перезагрузки сцены
    }

    public void StartTutorial()
    {
        SetState(GameState.Tutorial);
    }

    #endregion
    
    private void Update()
    {
        // Временная проверка
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("Escape pressed - Current state: " + CurrentState);
        }

        HandleFallbackInput();
    }

    private void HandleFallbackInput()
    {
        // Если Input System не настроен, используем старый ввод
        if (_pauseActionReference == null && Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                switch (_currentState)
                {
                    case GameState.Playing:
                        PauseGame();
                        break;
                    case GameState.Paused:
                        ResumeGame();
                        break;
                    case GameState.Building:
                        ExitBuildingMode();
                        break;
                }
            }
        }
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Отписываемся от событий ввода
        if (_pauseActionReference != null)
        {
            _pauseActionReference.action.performed -= OnPausePerformed;
            _pauseActionReference.action.Disable();
        }
    }

    #region Debug Methods

    [ContextMenu("Debug - Set State Playing")]
    private void DebugSetPlaying() => SetState(GameState.Playing);

    [ContextMenu("Debug - Set State Paused")]
    private void DebugSetPaused() => SetState(GameState.Paused);

    [ContextMenu("Debug - Set State Building")]
    private void DebugSetBuilding() => SetState(GameState.Building);

    #endregion
}