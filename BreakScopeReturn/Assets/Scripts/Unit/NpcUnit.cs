using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class NpcUnit : Unit, IHasCatalog
{
    [SerializeField]
    Catalog _catalog;
    public bool NeverFoundEnemy { get; protected set; }

    public Catalog Catalog => _catalog;

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
        base.Internal_Init(isInitialInit);
        if (isInitialInit)
        {
            GameManager.Instance.Stage.OnNewNpcUnitAdded(this);
            NeverFoundEnemy = true;
        }
        else if (IsAlive)
        {
            if (!GameManager.Instance.Stage.AliveNpcUnits.Contains(this))
            {
                GameManager.Instance.Stage.AliveNpcUnits.Add(this);
            }
        }
        else
        {
            if (GameManager.Instance.Stage.AliveNpcUnits.Contains(this))
            {
                GameManager.Instance.Stage.AliveNpcUnits.Remove(this);
            }
        }
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
