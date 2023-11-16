using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public abstract class SaveTarget : MonoBehaviour
{
    public SaveProperty saveProperty;
    public abstract string Serialize();
    public abstract void Deserialize(string data);
}
[System.Serializable]
public struct SaveProperty
{
    public bool excludeFromSave;
    public string identifyName;
    public SaveTargetPrefabRoot prefabRoot;
    public bool BasedOnPrefab => prefabRoot != null;
    public string PrefabPath => prefabRoot.prefabPath;
    public bool Match(string identifyName)
    {
        return identifyName.Equals(identifyName);
    }
}
[System.Serializable]
public struct ComponentSave
{
    public string identifyName;
    public string data;
    public ComponentSave(SaveTarget saveTarget)
    {
        identifyName = saveTarget.saveProperty.identifyName;
        data = saveTarget.Serialize();
    }
    public bool Match(string identifyName)
    {
        return identifyName.Equals(identifyName);
    }
    public bool Match(SaveTarget saveTarget)
    {
        return Match(saveTarget.saveProperty.identifyName);
    }
}
[System.Serializable]
struct PrefabCloneSave
{
    public string prefabPath;
    public List<ComponentSave> components;
    public PrefabCloneSave(SaveTargetPrefabRoot prefabRoot)
    {
        prefabPath = prefabRoot.prefabPath;
        components = new();
        foreach (var saveTarget in prefabRoot.SaveTargets)
        {
            if (saveTarget == null)
            {
                Debug.Log("<!>" + prefabRoot.name + "(path: " + prefabPath + ") contains empty saveTarget");
                continue;
            }
            if (!saveTarget.saveProperty.excludeFromSave)
                components.Add(new ComponentSave(saveTarget));
        }
    }
    public bool Match(SaveTargetPrefabRoot prefabRoot)
    {
        return prefabPath.Equals(prefabRoot.prefabPath);
    }
    public SaveTargetPrefabRoot Deserialize(List<SaveTargetPrefabRoot> reuseCandidates, out bool reused)
    {
        PrefabCloneSave myself = this;
        SaveTargetPrefabRoot prefabClone = reuseCandidates.Find(clone => myself.Match(clone));
        if (prefabClone != null)
        {
            reuseCandidates.Remove(prefabClone);
            reused = true;
        }
        else
        {
            prefabClone = SaveTargetPrefabRoot.Recreate(prefabPath);
            reused = false;
        }
        List<SaveTarget> componentsInClone = new(prefabClone.GetComponentsInChildren<SaveTarget>());
        foreach (ComponentSave componentSave in components)
        {
            SaveTarget component = componentsInClone.Find(eachComponent => componentSave.Match(eachComponent));
            if (component == null)
            {
                Debug.Log("<!> Missing component with identify name \"" + componentSave.identifyName + "\" in prefab \"" + prefabClone.name + "\"");
                continue;
            }
            componentsInClone.Remove(component);
            component.Deserialize(componentSave.data);
        }
        return prefabClone;
    }
}