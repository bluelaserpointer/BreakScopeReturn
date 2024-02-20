using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogEvent : DialogNode
{
    //public bool dontSkip; //wip
    public UnityEvent onEnable;

    private void OnEnable()
    {
        onEnable.Invoke();
    }
    public override float LifeTime()
    {
        return 0;
    }
}
