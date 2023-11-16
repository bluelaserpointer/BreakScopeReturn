using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageSource
{
    public float damage;
    public class BulletDamage : DamageSource
    {
        public Bullet Bullet { get; private set; }
        public BulletDamage(Bullet bullet)
        {
            Bullet = bullet;
            damage = bullet.damage;
        }
    }
    public class ExplosionDamage : DamageSource
    {
        public Grenade Grenade { get; private set; } //TODO: need abstraction ?
        public ExplosionDamage(Grenade grenade)
        {
            Grenade = grenade;
        }
    }
}
