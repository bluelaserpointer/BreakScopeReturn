using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageReceiver
{
    public abstract bool IsAlive { get; }
    [ContextMenu(nameof(LinkDamageCollidersInChildren))]
    public void LinkDamageCollidersInChildren()
    {
        foreach (var damageCollider in ((MonoBehaviour)this).GetComponentsInChildren<DamageCollider>())
        {
            damageCollider.damageReceiver = this;
        }
    }
}
