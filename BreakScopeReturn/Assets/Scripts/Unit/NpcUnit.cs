using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NpcUnit : Unit
{
    public bool NeverFoundEnemy { get; protected set; }

    [Serializable]
    struct NpcUnitSave
    {
        public string unitSave;
        public bool neverFoundEnemy;
    }
    protected virtual void Awake()
    {
        GameManager.DoAfterInit(() => Init(true));
    }
    protected override void Internal_Init(bool isInitialInit)
    {
        if (isInitialInit)
        {
            GameManager.Instance.Stage.OnNewNpcUnitAdded(this);
            NeverFoundEnemy = true;
        }
        base.Internal_Init(isInitialInit);
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(new NpcUnitSave() {
            unitSave = base.Serialize(),
            neverFoundEnemy = NeverFoundEnemy,
        });
    }
    protected override void Internal_Deserialize(string json)
    {
        NpcUnitSave save = JsonUtility.FromJson<NpcUnitSave>(json);
        base.Internal_Deserialize(save.unitSave);
        NeverFoundEnemy = save.neverFoundEnemy;
    }
}
