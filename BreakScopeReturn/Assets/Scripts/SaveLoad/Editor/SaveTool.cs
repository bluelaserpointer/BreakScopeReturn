using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using Unity.VisualScripting.YamlDotNet.Serialization.TypeInspectors;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveTool
{
    [MenuItem("BreakScope/Save/RenameSaveIdentifyNames")]
    public static void RenameSaveIdentifyNames()
    {
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        int id = 0;
        if (prefabStage != null)
        {
            string assetPath = prefabStage.assetPath;
            int resourcesStrIndex = assetPath.IndexOf("Resources");
            if (resourcesStrIndex == -1)
            {
                Debug.Log("Save Target Prefab must placed under any Resouces folder.");
                return;
            }
            int resourcePathStartIndex = resourcesStrIndex + 10;
            assetPath = assetPath.Substring(resourcePathStartIndex, assetPath.Length - resourcePathStartIndex - 7); //erase ".../Resources/" and ".prefab"
            if (!prefabStage.prefabContentsRoot.TryGetComponent(out SaveTargetPrefabRoot prefabRoot))
            {
                Debug.Log("Prefab root should attach " + nameof(SaveTargetPrefabRoot) + " component.");
                return;
            }
            prefabRoot.prefabPath = assetPath;
            prefabRoot.SaveTargets.Clear();
            foreach (var saveTarget in prefabRoot.GetComponentsInChildren<ISaveTarget>(true))
            {
                prefabRoot.SaveTargets.Add(saveTarget);
                saveTarget.SaveProperty = new SaveProperty()
                {
                    excludeFromSave = saveTarget.SaveProperty.excludeFromSave,
                    prefabRoot = prefabRoot,
                    identifyName = saveTarget + "#Prefab-" + id
                };
                EditorUtility.SetDirty((MonoBehaviour)saveTarget);
                ++id;
            }
            EditorUtility.SetDirty(prefabRoot);
            Debug.Log("Renamed " + id + " save targets.");
        }
        else
        {
            foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (ISaveTarget saveTarget in obj.GetComponentsInChildren<ISaveTarget>(true))
                {
                    if (!saveTarget.SaveProperty.BasedOnPrefab)
                    {
                        saveTarget.SaveProperty = new SaveProperty()
                        {
                            identifyName = saveTarget + "#Scene-" + id
                        };
                        EditorUtility.SetDirty((MonoBehaviour)saveTarget);
                        ++id;
                    }
                }
            }
            Debug.Log("Renamed \"" + id + "\" save targets.");
        }
    }
}
