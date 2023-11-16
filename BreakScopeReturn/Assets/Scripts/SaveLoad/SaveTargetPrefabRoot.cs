using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SaveTargetPrefabRoot : MonoBehaviour
{
    public bool excludeFromSave;
    public string prefabPath;
    [SerializeField]
    List<SaveTarget> saveTargets = new();

    public List<SaveTarget> SaveTargets => saveTargets;

    public static SaveTargetPrefabRoot Recreate(string prefabPath)
    {
        return Instantiate(Resources.Load<SaveTargetPrefabRoot>(prefabPath), GameManager.Instance.CurrentStage.transform);
    }
}
