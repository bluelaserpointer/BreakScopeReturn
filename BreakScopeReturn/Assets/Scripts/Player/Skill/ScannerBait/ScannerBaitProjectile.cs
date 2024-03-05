using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ScannerBaitProjectile : MonoBehaviour
{
    public float speed;
    [SerializeField]
    AudioSource _detectSE;
    [SerializeField]
    LayerMask _ignoreLayer;

    void FixedUpdate()
    {
        Travel();
    }
    public void Travel()
    {
        RaycastHit closestValidHit = new();
        closestValidHit.distance = float.MaxValue;
        RicochetMirror hitMirror = null;
        RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, transform.forward, speed * Time.fixedDeltaTime, ~_ignoreLayer);
        foreach (var hit in raycastHits)
        {
            if (PenetratableCollider.IgnoreThis(hit, raycastHits))
                continue;
            if (hit.distance > closestValidHit.distance)
                continue;
            if (RicochetMirror.TryGetRicochetMirror(hit.collider, out RicochetMirror mirror) && mirror != hitMirror)
            {
                hitMirror = mirror;
            }
            else
            {
                if (hit.collider.isTrigger)
                    continue;
                hitMirror = null;
            }
            closestValidHit = hit;
        }
        if (closestValidHit.collider != null)
        {
            transform.position = closestValidHit.point;
            if (hitMirror != null)
            {
                transform.forward = Vector3.Reflect(transform.forward, closestValidHit.normal);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            transform.position += speed * Time.fixedDeltaTime * transform.forward;
        }
    }
}
