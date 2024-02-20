using System.Collections.Generic;
using UnityEngine;

public interface ISaveTarget
{
    public SaveProperty SaveProperty { get; set; }
    public abstract string Serialize();
    public abstract void Deserialize(string data);
}
public abstract class SaveTarget : MonoBehaviour, ISaveTarget
{
    public SaveProperty saveProperty;

    public SaveProperty SaveProperty { get => saveProperty; set => saveProperty = value; }
    public abstract string Serialize();
    public abstract void Deserialize(string data);
}
[System.Serializable]
public struct SaveProperty
{
    public bool excludeFromSave;
    public string identifyName;
    public SaveTargetPrefabRoot prefabRoot;
    public readonly bool BasedOnPrefab => prefabRoot != null;
    public readonly string PrefabPath => prefabRoot.prefabPath;
    public readonly bool Match(string identifyName)
    {
        return identifyName.Equals(identifyName);
    }
}
[System.Serializable]
public struct ComponentSave
{
    public string identifyName;
    public string data;
    public ComponentSave(ISaveTarget saveTarget)
    {
        identifyName = saveTarget.SaveProperty.identifyName;
        data = saveTarget.Serialize();
    }
    public readonly bool Match(string identifyName)
    {
        return identifyName.Equals(identifyName);
    }
    public readonly bool Match(ISaveTarget saveTarget)
    {
        return Match(saveTarget.SaveProperty.identifyName);
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
            if (!saveTarget.SaveProperty.excludeFromSave)
                components.Add(new ComponentSave(saveTarget));
        }
    }
    public readonly bool Match(SaveTargetPrefabRoot prefabRoot)
    {
        return prefabPath.Equals(prefabRoot.prefabPath);
    }
    public readonly SaveTargetPrefabRoot Deserialize(List<SaveTargetPrefabRoot> reuseCandidates, out bool reused)
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
        List<ISaveTarget> componentsInClone = new(prefabClone.GetComponentsInChildren<ISaveTarget>());
        foreach (ComponentSave componentSave in components)
        {
            ISaveTarget component = componentsInClone.Find(eachComponent => componentSave.Match(eachComponent));
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