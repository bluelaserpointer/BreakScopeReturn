using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class UnitDamageCollider : DamageCollider
{
    [SerializeField] float damageRatio = 1;
    [SerializeField] GameObject bloodEffect;

    public float DamageRatio => damageRatio;
    public Unit Unit { get; private set; }
    
    public void Init(Unit unit)
    {
        Unit = unit;
        onBulletHit.AddListener(bullet =>
        {
            Damage(new DamageSource.BulletDamage(bullet));
            if (bloodEffect && damageRatio > 0)
            {
                GameObject generatedEffect = Instantiate(bloodEffect);
                generatedEffect.transform.SetParent(GameManager.Instance.CurrentStage.transform);
                generatedEffect.transform.position = bullet.transform.position;
            }
        });
    }
    public void Damage(DamageSource damageSource)
    {
        damageSource.damage *= damageRatio;
        Unit.Damage(damageSource);
    }
}
