using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class DamageCollider : MonoBehaviour
{
    public UnityEvent<BulletScript> onBulletHit;
    public void Hit(BulletScript bullet)
    {
        onBulletHit.Invoke(bullet);
    }
}
