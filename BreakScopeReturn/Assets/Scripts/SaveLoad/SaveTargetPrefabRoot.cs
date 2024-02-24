using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SaveTargetPrefabRoot : MonoBehaviour
{
    public bool excludeFromSave;
    public string prefabPath;
    public List<GameObject> saveTargetGameObjects = new();

    public List<ISaveTarget> GetSaveTargets()
    {
        List<ISaveTarget> saveTargets = new();
        saveTargetGameObjects.ForEach(go => saveTargets.AddRange(go.GetComponents<ISaveTarget>()));
        return saveTargets;
    }
    public bool ContainsSaveTarget(ISaveTarget saveTarget) => saveTargetGameObjects.Contains(saveTarget.gameObject);

    public static SaveTargetPrefabRoot Recreate(string prefabPath)
    {
        return Instantiate(Resources.Load<SaveTargetPrefabRoot>(prefabPath), GameManager.Instance.Stage.transform);
    }
}
