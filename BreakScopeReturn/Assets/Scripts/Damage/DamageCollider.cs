using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class DamageCollider : MonoBehaviour
{
    public UnityEvent<Bullet> onBulletHit;
    public void Hit(Bullet bullet)
    {
        onBulletHit.Invoke(bullet);
    }
}
