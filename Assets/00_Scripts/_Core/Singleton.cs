// Assets/00_Scripts/_Core/Singleton.cs

using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // ИСПРАВЛЕНО: используем новые методы
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        // Альтернативный поиск если первый не нашел
                        _instance = FindAnyObjectByType<T>();
                    }

                    var objects = FindObjectsByType<T>(FindObjectsSortMode.None);
                    if (objects.Length > 1)
                    {
                        Debug.LogError($"[Singleton] Multiple instances of '{typeof(T)}' found!");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = $"[Singleton] {typeof(T).Name}";

                        Debug.Log($"[Singleton] Created new instance of {typeof(T).Name}");
                    }
                    else
                    {
                        Debug.Log($"[Singleton] Using existing instance: {_instance.gameObject.name}");
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Multiple instances of '{typeof(T)}' detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _applicationIsQuitting = true;
        }
    }
}