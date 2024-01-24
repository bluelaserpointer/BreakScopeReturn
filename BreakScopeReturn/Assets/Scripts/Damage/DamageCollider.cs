using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class DamageCollider : MonoBehaviour
{
    [SerializeField] bool _ignore;
    [SerializeField] float damageRatio = 1;
    public UnityEvent<Bullet> onBulletHit;

    public bool Ignore => _ignore;
    public float DamageRatio => damageRatio;
    public void Hit(Bullet bullet)
    {
        onBulletHit.Invoke(bullet);
    }
}
