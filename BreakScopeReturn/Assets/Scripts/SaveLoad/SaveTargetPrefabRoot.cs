using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SaveTargetPrefabRoot : MonoBehaviour
{
    public bool excludeFromSave;
    public string prefabPath;
    [SerializeField]
    List<ISaveTarget> saveTargets = new();

    public List<ISaveTarget> SaveTargets => saveTargets;

    public static SaveTargetPrefabRoot Recreate(string prefabPath)
    {
        return Instantiate(Resources.Load<SaveTargetPrefabRoot>(prefabPath), GameManager.Instance.Stage.transform);
    }
}
