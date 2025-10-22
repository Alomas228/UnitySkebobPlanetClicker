// Assets/00_Scripts/Data/Models/GameData.cs

using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public string saveVersion = "1.0";
    public DateTime lastSaveTime;

    // Основные данные игры
    public int colonyPopulation;
    public Dictionary<string, int> resources = new Dictionary<string, int>();
    public List<BuildingData> buildings = new List<BuildingData>();

    // Прогресс
    public int daysPassed;
    public bool[] completedQuests;

    public bool IsEmpty()
    {
        return colonyPopulation == 0 && resources.Count == 0 && buildings.Count == 0;
    }
}

[Serializable]
public class BuildingData
{
    public string buildingId;
    public float positionX;
    public float positionY;
    public bool isConstructed;
}