using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] bool _logSaveLoad;
    [SerializeField] bool _playerStealth;
    [SerializeField] Stage _stage;

    [Header("Reference")]
    [SerializeField] Player playerPrefab;
    [SerializeField] DialogUI dialogUI;
    [SerializeField] GameObject playerDeathBlackout;
    [SerializeField] CheckPointNotifiactionUI checkPointNotification;
    [SerializeField] DirectionIndicator directionIndicator;
    [SerializeField] GameObject interactIconViewer;
    [SerializeField] MinimapUI minimapUI;
    [SerializeField] EquipmentSidePreview equipmentSidePreview;

    public static GameManager Instance { get; private set; }
    public Stage CurrentStage => _stage;
    public Player Player => CurrentStage.Player;
    public DialogUI DialogUI => dialogUI;
    public GameObject InteractIconViewer => interactIconViewer;
    public MinimapUI MinimapUI => minimapUI;
    public CheckPointNotifiactionUI CheckPointNotification => checkPointNotification;
    public DirectionIndicator DirectionIndicator => directionIndicator;
    public EquipmentSidePreview EquipmentSidePreview => equipmentSidePreview;
    private void Awake()
    {
        Instance = this;
        InitStage();
        Player.stealth = _playerStealth;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveStage();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadStage();
        }
    }
    public void InitStage()
    {
        CurrentStage.Init(playerPrefab);
        SaveStage();
    }
    string savedStageData;
    public void SaveStage()
    {
        CollectSaveTargets(out List<SaveTargetPrefabRoot> prefabClones, out List<SaveTarget> sceneBasedComponents);
        StageSave stageSave = new()
        {
            stageProgressData = CurrentStage.Serialize(),
            prefabClones = new(),
            sceneBasedComponents = new()
        };
        foreach (var prefabClone in prefabClones)
        {
            if (!prefabClone.excludeFromSave)
                stageSave.prefabClones.Add(new PrefabCloneSave(prefabClone));
        }
        foreach (var sceneBasedComponent in sceneBasedComponents)
        {
            if (!sceneBasedComponent.saveProperty.excludeFromSave)
                stageSave.sceneBasedComponents.Add(new ComponentSave(sceneBasedComponent));
        }
        savedStageData = JsonConvert.SerializeObject(stageSave, Formatting.Indented);
        if (_logSaveLoad)
            print("savedStageData: " + savedStageData);
    }
    struct StageSave
    {
        public string stageProgressData;
        public List<PrefabCloneSave> prefabClones;
        public List<ComponentSave> sceneBasedComponents;
    }
    void CollectSaveTargets(out List<SaveTargetPrefabRoot> prefabClones, out List<SaveTarget> sceneBasedComponents)
    {
        prefabClones = new();
        foreach (var prefabRoot in CurrentStage.GetComponentsInChildren<SaveTargetPrefabRoot>(true))
        {
            if (!prefabRoot.excludeFromSave)
            {
                prefabClones.Add(prefabRoot);
            }
        }
        sceneBasedComponents = new();
        foreach (var saveTarget in CurrentStage.GetComponentsInChildren<SaveTarget>(true))
        {
            if (!saveTarget.saveProperty.excludeFromSave && !saveTarget.saveProperty.BasedOnPrefab)
                sceneBasedComponents.Add(saveTarget);
        }
    }
    public void LoadStage()
    {
        //remove objects should delete during stage load. (ex. VFX)
        foreach (var destoryTarget in CurrentStage.GetComponentsInChildren<DestoryOnStageLoad>(true))
        {
            Destroy(destoryTarget.gameObject);
        }
        //collect reusable candidates
        CollectSaveTargets(out List<SaveTargetPrefabRoot> reusablePrefabClones, out List<SaveTarget> reusableSceneBasedComponents);
        foreach (var prefabRoot in reusablePrefabClones)
        {
            reusableSceneBasedComponents.RemoveAll(component => prefabRoot.SaveTargets.Contains(component));
        }
        StageSave stageSave = JsonConvert.DeserializeObject<StageSave>(savedStageData);
        //deserialize stage progress / records / events
        CurrentStage.Deserialize(stageSave.stageProgressData);
        //deserialize prefab based components
        stageSave.prefabClones.ForEach(prefabCloneSave => prefabCloneSave.Deserialize(reusablePrefabClones, out bool _));
        //deserialize scene based components
        foreach (var componentSave in stageSave.sceneBasedComponents)
        {
            string identifyName = componentSave.identifyName;
            if (identifyName.Length == 0)
            {
                Debug.Log("<!> Save data contains empty name component with data + \"" + componentSave.data + "\"");
                continue;
            }
            SaveTarget component = reusableSceneBasedComponents.Find(candidate => candidate.saveProperty.Match(identifyName));
            if (component == null)
            {
                Debug.Log("<!> A scene based save target is unable to find reuse target with name + \"" + identifyName + "\"");
                continue;
            }
            reusableSceneBasedComponents.Remove(component);
            component.Deserialize(componentSave.data);
            if (_logSaveLoad)
                Debug.Log("reused existing object: " + component);
        }
        foreach (var dump in reusablePrefabClones)
            Destroy(dump.gameObject);
        foreach (var abonormalDump in reusableSceneBasedComponents)
        {
            Debug.Log("<!> A scene based reusable component is not reused: + \"" + abonormalDump + "\"");
            Destroy(abonormalDump.gameObject);
        }
        //refresh ui
        dialogUI.SetDialog(null);
    }
    public void SetBlackout(bool cond)
    {
        playerDeathBlackout.SetActive(cond);
    }
}
