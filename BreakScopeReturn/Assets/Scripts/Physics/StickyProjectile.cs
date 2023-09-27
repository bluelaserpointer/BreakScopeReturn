using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class StickyProjectile : MonoBehaviour
{
    public Predicate<Collider> hitCondition;
    public UnityEvent<RaycastHit> onHit;

    Vector3 velocity;
    public bool IsHit { get; private set; }
    public void Eject(Vector3 velocity)
    {
        transform.forward = this.velocity = velocity;
    }
    private void FixedUpdate()
    {
        if (IsHit)
            return;
        transform.position += velocity * Time.fixedDeltaTime;
        RaycastHit closestHit = new();
        closestHit.distance = Mathf.Infinity;
        foreach(var hit in Physics.RaycastAll(transform.position, transform.forward, velocity.magnitude * Time.fixedDeltaTime))
        {
            if (hit.distance > closestHit.distance || (hitCondition != null && !hitCondition.Invoke(hit.collider)))
                continue;
            closestHit = hit;
        }
        if (closestHit.collider != null)
        {
            transform.position = closestHit.point;
            IsHit = true;
            onHit.Invoke(closestHit);
        }
    }
}
