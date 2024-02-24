using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] Language _editorLanguage;
    [SerializeField] bool _logSaveLoad;
    [SerializeField] bool _playerAlwaysStealth;

    [Header("Save / Load")]
    [SerializeField] Transform[] _saveTargetContainers;

    [Header("Reference")]
    [SerializeField] Stage _stage;
    [SerializeField] GameObject _cutsceneUI;
    [SerializeField] DialogUI _dialogUI;
    [SerializeField] GameObject _playerDeathBlackout;
    [SerializeField] FadingBlackout _cutsceneBlackout;
    [SerializeField] CheckPointUI _checkPointNotification;
    [SerializeField] DirectionIndicator _directionIndicator;
    [SerializeField] InteractUI _interactUI;
    [SerializeField] MinimapUI _minimapUI;
    [SerializeField] ObjectiveUI _objectiveUI;
    [SerializeField] PauseUI _pauseUI;
    public static GameManager Instance { get; private set; }
    public bool InitDone { get; private set; }
    public Cutscene ActiveCutscene { get; set; }
    public bool PlayingCutscene => ActiveCutscene != null && ActiveCutscene.Playing;
    public Stage Stage => _stage;
    public FadingBlackout CutsceneBlackout => _cutsceneBlackout;
    public Player Player => Stage.Player;
    public DialogUI DialogUI => _dialogUI;
    public GameObject CutsceneUI => _cutsceneUI;
    public InteractUI InteractUI => _interactUI;
    public MinimapUI MinimapUI => _minimapUI;
    public ObjectiveUI ObjectiveUI => _objectiveUI;
    public PauseUI PauseUI => _pauseUI;
    public bool MenuPause => PauseUI.Paused;
    public CheckPointUI CheckPointUI => _checkPointNotification;
    public DirectionIndicator DirectionIndicator => _directionIndicator;
    public bool PlayerAlwaysStealth => _playerAlwaysStealth;

    private readonly List<Action> _afterInitActions = new();
    private string _savedStageData;
    private readonly Dictionary<string, ISaveTarget> _nameMapSaveTarget = new();
    private void Awake()
    {
        Instance = this;
#if UNITY_EDITOR
        Cursor.SetCursor(Resources.Load<Texture2D>("Cursor/Cursor"), Vector2.zero, CursorMode.Auto);
        Setting.SetDefault();
        Setting.Set(Setting.LANGUAGE, _editorLanguage);
#endif
        Stage.Init();
        SaveStage();
        InitDone = true;
        _afterInitActions.ForEach(action => action.Invoke());
        _afterInitActions.Clear();
    }
    /// <summary>
    /// Ensure various init run after stage init.
    /// </summary>
    /// <param name="action"></param>
    public void DoAfterInit(Action action)
    {
        if (InitDone)
        {
            action.Invoke();
            return;
        }
        _afterInitActions.Add(action);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            SaveStage();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            LoadStage();
        }
    }
    public void SkipCutScene()
    {
        if (ActiveCutscene != null)
        {
            ActiveCutscene.Skip();
        }
    }
    public void SaveStage()
    {
        CollectSaveTargets(out List<SaveTargetPrefabRoot> prefabClones, out List<ISaveTarget> sceneBasedComponents);
        StageSave stageSave = new()
        {
            stageProgressData = Stage.Serialize(),
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
            if (!sceneBasedComponent.SaveProperty.excludeFromSave)
                stageSave.sceneBasedComponents.Add(new ComponentSave(sceneBasedComponent));
        }
        _savedStageData = JsonConvert.SerializeObject(stageSave, Formatting.Indented);
        if (_logSaveLoad)
            print("savedStageData: " + _savedStageData);
    }
    struct StageSave
    {
        public string stageProgressData;
        public List<PrefabCloneSave> prefabClones;
        public List<ComponentSave> sceneBasedComponents;
    }
    void CollectSaveTargets(out List<SaveTargetPrefabRoot> prefabClones, out List<ISaveTarget> sceneBasedComponents)
    {
        prefabClones = new();
        sceneBasedComponents = new();
        foreach (Transform saveTargetContainer in _saveTargetContainers)
        {
            foreach (var prefabRoot in saveTargetContainer.GetComponentsInChildren<SaveTargetPrefabRoot>(true))
            {
                if (!prefabRoot.excludeFromSave)
                {
                    prefabClones.Add(prefabRoot);
                }
            }
            foreach (var saveTarget in saveTargetContainer.GetComponentsInChildren<ISaveTarget>(true))
            {
                if (!saveTarget.SaveProperty.excludeFromSave && !saveTarget.SaveProperty.BasedOnPrefab)
                    sceneBasedComponents.Add(saveTarget);
            }
        }
    }
    public void LoadStage()
    {
        InitDone = false;
        _nameMapSaveTarget.Clear();
        //remove objects should delete during stage load. (ex. VFX)
        foreach (Transform saveTargetContainer in _saveTargetContainers)
        {
            foreach (var destoryTarget in saveTargetContainer.GetComponentsInChildren<DestoryOnStageLoad>(true))
            {
                Destroy(destoryTarget.gameObject);
            }
        }
        //collect reusable candidates
        CollectSaveTargets(out List<SaveTargetPrefabRoot> reusablePrefabClones, out List<ISaveTarget> reusableSceneBasedComponents);
        foreach (var prefabRoot in reusablePrefabClones)
        {
            //for those let prefab roots do deserialize
            reusableSceneBasedComponents.RemoveAll(component => prefabRoot.ContainsSaveTarget(component));
        }
        StageSave stageSave = JsonConvert.DeserializeObject<StageSave>(_savedStageData);
        //deserialize stage progress / records / events
        Stage.Deserialize(stageSave.stageProgressData);
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
            ISaveTarget component = reusableSceneBasedComponents.Find(candidate => candidate.SaveProperty.Match(identifyName));
            if (component == null)
            {
                Debug.Log("<!> A scene based save target is unable to find reuse target with name + \"" + identifyName + "\"");
                continue;
            }
            _nameMapSaveTarget.Add(identifyName, component);
            reusableSceneBasedComponents.Remove(component);
            component.Deserialize(componentSave.data);
            if (_logSaveLoad)
                Debug.Log("reused existing object: " + component);
        }
        foreach (var dump in reusablePrefabClones)
            Destroy(dump.gameObject);
        foreach (var abnormalDump in reusableSceneBasedComponents)
        {
            Debug.Log("<!> A scene based reusable component is not reused: + \"" + abnormalDump + "\"");
            Destroy(((MonoBehaviour)abnormalDump).gameObject);
        }
        _dialogUI.SetDialog(null);
        InitDone = true;
        _afterInitActions.ForEach(action => action.Invoke());
        _afterInitActions.Clear();
    }
    /// <summary>
    /// Must used in <see cref="DoAfterInit(Action)"/>.
    /// </summary>
    /// <param name="identifyName"></param>
    /// <returns></returns>
    public ISaveTarget FindSaveTarget(string identifyName)
    {
        return _nameMapSaveTarget[identifyName];
    }
    public void SetBlackout(bool cond)
    {
        _playerDeathBlackout.SetActive(cond);
    }
}
