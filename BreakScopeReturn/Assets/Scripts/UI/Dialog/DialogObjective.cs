using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogObjective : DialogNode, ISaveTarget
{
    [SerializeField]
    SaveProperty _saveProperty;
    //public bool dontSkip; //wip
    public UnityEvent onSetObjective;

    public SaveProperty SaveProperty { get => _saveProperty; set => _saveProperty = value; }

    private void OnEnable()
    {
        GameManager.Instance.ObjectiveUI.SetObjective(this);
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
