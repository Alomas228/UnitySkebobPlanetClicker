// Assets/00_Scripts/_Core/SaveManager.cs

using System;
using System.IO;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    private const string SAVE_FILE_NAME = "colony_save.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    public event Action OnGameSaved;
    public event Action OnGameLoaded;

    public bool SaveExists => File.Exists(SavePath);

    protected override void Awake()
    {
        base.Awake();
        ServiceLocator.Register<SaveManager>(this);

        // Создаем папку для сохранений если не существует
        string saveDirectory = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    public void SaveGame(GameData gameData)
    {
        try
        {
            gameData.lastSaveTime = DateTime.Now;
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(SavePath, json);

            Debug.Log($"Игра сохранена: {SavePath}");
            OnGameSaved?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка сохранения: {e.Message}");
        }
    }

    public GameData LoadGame()
    {
        if (!SaveExists)
        {
            Debug.Log("Сохранение не найдено");
            return new GameData();
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameData loadedData = JsonUtility.FromJson<GameData>(json);

            Debug.Log("Игра загружена");
            OnGameLoaded?.Invoke();
            return loadedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка загрузки: {e.Message}");
            return new GameData();
        }
    }

    public void DeleteSave()
    {
        if (SaveExists)
        {
            File.Delete(SavePath);
            Debug.Log("Сохранение удалено");
        }
    }

    public DateTime GetLastSaveTime()
    {
        if (SaveExists)
        {
            return File.GetLastWriteTime(SavePath);
        }
        return DateTime.MinValue;
    }
}