using UnityEngine;

public enum ResourceType
{
    Weightome,  // "Весомое" из вашей схемы
    Food,
    Metal,
    Energy,
    Wood
}

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/Resources/Resource Data")]
public class ResourceData : ScriptableObject
{
    public ResourceType type;
    public string displayName;
    public Sprite icon;
    public Color color = Color.white;
    public bool isUnlocked = true;

    [TextArea]
    public string description;

    [Header("Starting Values")]
    public int startingAmount = 0;
    public bool isConsumable = true;
}