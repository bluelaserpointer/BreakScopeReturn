using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ObjectiveUI : SaveTarget
{
    [SerializeField]
    Camera _camera;
    [SerializeField]
    Transform _graphicRoot;
    [SerializeField]
    IzumiTools.ReuseNest<WorldPositionPin> pins;
    [SerializeField]
    TextMeshProUGUI _objectiveNameText;
    [SerializeField]
    GameObject _completeCheckMark;
    [SerializeField]
    Animator _highlightPulseAnimator;
    [SerializeField]
    AudioSource _newObjectiveSESource;
    [SerializeField]
    AudioSource _completeObjectiveSESource;

    public DialogObjective SourceDialogObjective { get; private set; }
    public string ObjectiveName { get; private set; }
    public bool Completed { get; private set; }
    private void Awake()
    {
        Init();
    }
    public void Init()
    {
        SourceDialogObjective = null;
        Completed = false;
        _objectiveNameText.text = "";
        _completeCheckMark.SetActive(false);
        pins.DisableAll();
    }
    private void Update()
    {
        if (GameManager.Instance.PlayingCutscene)
        {
            _graphicRoot.gameObject.SetActive(false);
            return;
        }
        _graphicRoot.gameObject.SetActive(true);
    }
    public void SetObjective(DialogObjective dialogObjective)
    {
        Init();
        SourceDialogObjective = dialogObjective;
        SourceDialogObjective.onSetObjective.Invoke();
    }
    public void CompleteObjective(DialogObjective dialogObjective)
    {
        if (SourceDialogObjective == dialogObjective)
            Internal_CompleteObjective();
    }
    public void CompleteObjective(string objectiveName)
    {
        if (!ObjectiveName.Equals(objectiveName))
        {
            print("<!>Completed objective name mismatch with current: " + ObjectiveName + "(current), " + objectiveName + "(completed)");
            return;
        }
        Internal_CompleteObjective();
    }
    private void Internal_CompleteObjective()
    {
        Completed = true;
        _highlightPulseAnimator.SetTrigger("Pulse");
        _completeCheckMark.SetActive(true);
        pins.DisableAll();
        _completeObjectiveSESource.Play();
    }
    public void SetObjectiveNameWithHighlight(string name)
    {
        SetObjectiveName(name);
        _highlightPulseAnimator.SetTrigger("Pulse");
        _newObjectiveSESource.Play();
    }
    public void SetObjectiveName(string name)
    {
        ObjectiveName = name;
        _objectiveNameText.text = name;
    }
    public void AddTransformTarget(Transform target)
    {
        pins.EnableOne().Init(target, WorldPositionPin.PinMode.TransformPivot);
    }
    public void AddColliderTarget(Transform target)
    {
        pins.EnableOne().Init(target, WorldPositionPin.PinMode.ColliderBoundsCenter);
    }
    public void AddRigidbodyTarget(Transform target)
    {
        pins.EnableOne().Init(target, WorldPositionPin.PinMode.RigidbodyCenterOfMass);
    }
    public void AddUnitTarget(Transform target)
    {
        pins.EnableOne().Init(target, WorldPositionPin.PinMode.Unit);
    }
    [System.Serializable]
    struct ObjectiveSave
    {
        public string sourceIdentifyName;
        public bool completed;
    }

    public override string Serialize()
    {
        return JsonUtility.ToJson(new ObjectiveSave()
        {
            sourceIdentifyName = SourceDialogObjective == null ? "" : SourceDialogObjective.SaveProperty.identifyName,
            completed = Completed
        });
    }

    public override void Deserialize(string data)
    {
        Init();
        var save = JsonUtility.FromJson<ObjectiveSave>(data);
        if (save.sourceIdentifyName.Equals("") || save.completed)
        {
            return;
        }
        GameManager.Instance.DoAfterInit(() =>
        {
            SourceDialogObjective = (DialogObjective)GameManager.Instance.FindSaveTarget(save.sourceIdentifyName);
            SourceDialogObjective.onSetObjective.Invoke();
        }
        );
    }
}
