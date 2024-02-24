using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[DisallowMultipleComponent]
[RequireComponent(typeof(VisualEffect))]
public class LaserSightVFX : MonoBehaviour
{
    public Vector3 aimPosition;
    public LayerMask _excludeMask;
    public float maxDistance = 50;

    private VisualEffect _visualEffect;

    private void Awake()
    {
        _visualEffect = GetComponent<VisualEffect>();
    }
    private void Update()
    {
        Vector3 direction = (aimPosition - transform.position).normalized;
        RaycastHit closestHit = new RaycastHit() { distance = maxDistance };
        foreach (var hitInfo in Physics.RaycastAll(transform.position, direction, maxDistance, ~_excludeMask))
        {
            //any predicate?
            if (hitInfo.distance < closestHit.distance)
            {
                closestHit = hitInfo;
            }
        }
        aimPosition = (closestHit.collider != null) ? closestHit.point : transform.position + direction * maxDistance;
        _visualEffect.SetVector3("LaserStartPosition", transform.position);
        _visualEffect.SetVector3("LaserEndPosition", aimPosition);
    }
}
