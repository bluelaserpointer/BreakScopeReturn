using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class UnitDamageCollider : DamageCollider
{
    public Unit Unit { get; private set; }
    
    public void Init(Unit unit)
    {
        Unit = unit;
        onBulletHit.AddListener(bullet =>
        {
            Damage(new DamageSource.BulletDamage(bullet));
        });
    }
    public void Damage(DamageSource damageSource)
    {
        damageSource.damage *= DamageRatio;
        Unit.Damage(damageSource);
    }
}
