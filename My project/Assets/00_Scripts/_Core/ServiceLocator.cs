// Assets/00_Scripts/_Core/ServiceLocator.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"Service of type {type} already registered. Overwriting.");
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
        }
    }

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
        {
            return (T)_services[type];
        }

        Debug.LogError($"Service of type {type} not registered.");
        return default(T);
    }

    public static bool TryGet<T>(out T service)
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
        {
            service = (T)_services[type];
            return true;
        }

        service = default(T);
        return false;
    }

    public static void Unregister<T>()
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
        }
    }

    public static void Clear()
    {
        _services.Clear();
    }
}