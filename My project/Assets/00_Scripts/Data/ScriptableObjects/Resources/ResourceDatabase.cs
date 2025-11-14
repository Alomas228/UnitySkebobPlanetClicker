using Mono.Cecil;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceDatabase", menuName = "Game/Resources/Resource Database")]
public class ResourceDatabase : ScriptableObject
{
    public List<ResourceData> allResources;

    public ResourceData GetResourceData(ResourceType type)
    {
        return allResources.Find(r => r.type == type);
    }

    public void UnlockResource(ResourceType type)
    {
        var resource = GetResourceData(type);
        if (resource != null)
            resource.isUnlocked = true;
    }
}