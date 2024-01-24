using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EventDialogNode : DialogNode
{
    public float nodeLifetime = 0;
    public UnityEvent onAwake;

    public override float LifeTime()
    {
        return nodeLifetime;
    }

    private void Awake()
    {
        onAwake.Invoke();
    }
}
