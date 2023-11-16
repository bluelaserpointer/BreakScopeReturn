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
            var prefabRoot = prefabStage.prefabContentsRoot.GetComponent<SaveTargetPrefabRoot>();
            if (prefabRoot == null)
            {
                Debug.Log("Prefab root should attach " + nameof(SaveTargetPrefabRoot) + " component.");
                return;
            }
            prefabRoot.prefabPath = assetPath;
            prefabRoot.SaveTargets.Clear();
            foreach (var saveTarget in prefabRoot.GetComponentsInChildren<SaveTarget>(true))
            {
                prefabRoot.SaveTargets.Add(saveTarget);
                saveTarget.saveProperty = new SaveProperty()
                {
                    excludeFromSave = saveTarget.saveProperty.excludeFromSave,
                    prefabRoot = prefabRoot,
                    identifyName = saveTarget + "#Prefab-" + id
                };
                EditorUtility.SetDirty(saveTarget);
                ++id;
            }
            EditorUtility.SetDirty(prefabRoot);
            Debug.Log("Renamed " + id + " save targets.");
        }
        else
        {
            foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (SaveTarget saveTarget in obj.GetComponentsInChildren<SaveTarget>(true))
                {
                    if (!saveTarget.saveProperty.BasedOnPrefab)
                    {
                        saveTarget.saveProperty = new SaveProperty()
                        {
                            identifyName = saveTarget + "#Scene-" + id
                        };
                        EditorUtility.SetDirty(saveTarget);
                        ++id;
                    }
                }
            }
            Debug.Log("Renamed \"" + id + "\" save targets.");
        }
    }
}
