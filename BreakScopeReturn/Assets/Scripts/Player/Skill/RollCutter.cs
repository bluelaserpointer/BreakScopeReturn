using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RollCutter : MonoBehaviour
{
    [SerializeField]
    Transform _rollAnchor;
    [SerializeField]
    float _sawRollSpeed;
    [SerializeField]
    float _moveSpeed;

    [SerializeField]
    LineRenderer _laserLine;
    [SerializeField]
    float _laserLength;
    [SerializeField]
    float _ColliderRadius;

    [SerializeField]
    StickyProjectile _stickyProjectile;

    public StickyProjectile StickyProjectile => _stickyProjectile;

    void Start()
    {
        _laserLine.SetPosition(1, Vector3.forward * _laserLength);
    }

    private void FixedUpdate()
    {
        _rollAnchor.Rotate(-Vector3.forward, _sawRollSpeed * Time.fixedDeltaTime, Space.Self);
        transform.position += transform.up * _moveSpeed * Time.fixedDeltaTime;
        if(Raycast(transform.position - transform.up * _ColliderRadius * 1.5F, transform.up, _ColliderRadius * 2.2F, out RaycastHit frontHit))
        {
            Curve(frontHit.normal);
        }
        else if (!SphereCast(transform.position + transform.forward * _ColliderRadius * 1.5F, -transform.forward, _ColliderRadius * 2.2F, out RaycastHit beneathHit))
        {
            if (Raycast(transform.position - _ColliderRadius * 1.5F * transform.forward, -transform.up, _ColliderRadius * 2, out beneathHit))
            {
                Curve(beneathHit.normal);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            transform.position = beneathHit.point + beneathHit.normal * _ColliderRadius;
        }
    }
    public void Curve(Vector3 newNormal)
    {
        Vector3 oldNormal = transform.forward;
        Vector3 axis = Vector3.Cross(oldNormal, newNormal);
        transform.rotation = Quaternion.LookRotation(newNormal, Quaternion.AngleAxis(Vector3.SignedAngle(oldNormal, newNormal, axis), axis) * transform.up);
    }
    public bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
    {
        hit = new();
        hit.distance = float.MaxValue;
        foreach (var hitInfo in Physics.RaycastAll(origin, direction, distance))
        {
            //print("hit " + hitInfo.collider.name + ", trigger: " + hitInfo.collider.isTrigger + ", distance " + (hitInfo.distance > hit.distance) + ", parent " + (hitInfo.collider.GetComponentInParent<RollCutter>() == this));
            if (hitInfo.collider.isTrigger || hitInfo.distance > hit.distance || hitInfo.collider.GetComponentInParent<RollCutter>() == this)
                continue;
            hit = hitInfo;
            return true;
        }
        return false;
    }
    public bool SphereCast(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
    {
        hit = new();
        hit.distance = float.MaxValue;
        foreach (var hitInfo in Physics.SphereCastAll(origin, _ColliderRadius, direction, distance))
        {
            //print("hit " + hitInfo.collider.name + ", trigger: " + hitInfo.collider.isTrigger + ", distance " + (hitInfo.distance > hit.distance) + ", parent " + (hitInfo.collider.GetComponentInParent<RollCutter>() == this));
            if (hitInfo.collider.isTrigger || hitInfo.distance > hit.distance
                || hitInfo.collider.GetComponentInParent<RollCutter>() == this
                || (hitInfo.point == Vector3.zero && hitInfo.distance == 0 && hitInfo.normal == -direction))
                continue;
            hit = hitInfo;
            return true;
        }
        return false;
    }
}
