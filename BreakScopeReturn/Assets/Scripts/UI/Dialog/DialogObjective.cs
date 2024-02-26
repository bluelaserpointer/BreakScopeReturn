using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogObjective : DialogNode, ISaveTarget
{
    [SerializeField]
    SaveProperty _saveProperty;
    [SerializeField]
    TranslatableSentence objectiveNameTS;
    //public bool dontSkip; //wip
    [SerializeField]
    UnityEvent onSetObjective;

    public TranslatableSentence ObjectiveNameTS => objectiveNameTS;
    public SaveProperty SaveProperty { get => _saveProperty; set => _saveProperty = value; }

    private void OnEnable()
    {
        GameManager.Instance.ObjectiveUI.SetObjective(this);
        onSetObjective.Invoke();
    }
    /// <summary>
    /// For stage load
    /// </summary>
    public void InvokeOnSetObjective()
    {
        onSetObjective.Invoke();
    }
    public override float LifeTime()
    {
        return 0;
    }

    public string Serialize()
    {
        return "";
    }

    public void Deserialize(string data)
    {
    }
}
