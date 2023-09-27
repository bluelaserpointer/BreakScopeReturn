using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

[DisallowMultipleComponent]
public class NPCGun : MonoBehaviour
{
    [SerializeField]
    float damage;
    [SerializeField]
    float speed;
    [SerializeField]
    Transform muzzleAnchor;
    [SerializeField]
    BulletScript bulletPrefab;
    [SerializeField]
    Cooldown fireCD = new(0.2F);
    [SerializeField]
    List<Transform> penetrationCheckAnchors;
    [SerializeField]
    AudioSource _fireSESouce;

    Vector3 aimPosition;

    public Unit OwnerUnit { get; private set; }
    public Transform MuzzleAnchor => muzzleAnchor;
    public Cooldown FireCD => fireCD;
    public void Init(Unit owner)
    {
        OwnerUnit = owner;
    }
    public void Aim(Vector3 aimPosition)
    {
        this.aimPosition = aimPosition;
    }
    private void Update()
    {
        fireCD.AddDeltaTime();
    }
    public void Trigger()
    {
        if (fireCD.Eat())
            Fire();
    }
    public void Fire()
    {
        _fireSESouce.Play();
        BulletScript bullet = Instantiate(bulletPrefab);
        bullet.transform.SetParent(GameManager.Instance.CurrentStage.transform);
        bullet.damage = damage;
        bullet.speed = speed;
        bullet.transform.SetPositionAndRotation(muzzleAnchor.position, Quaternion.LookRotation(aimPosition - muzzleAnchor.position));
    }
    public bool CheckRaycast(Unit targetUnit, out Vector3 gunRaycastablePosition)
    {
        gunRaycastablePosition = targetUnit.transform.position; // no effect
        if (penetrationCheckAnchors.Count > 0)
        {
            foreach (var anchor in penetrationCheckAnchors)
            {
                if (!OwnerUnit.RaycastableTo(anchor.position, targetUnit, out gunRaycastablePosition))
                    return false;
            }
        }
        else
        {
            if (!OwnerUnit.RaycastableTo(MuzzleAnchor.position, targetUnit, out gunRaycastablePosition))
                return false;
        }
        return true;
    }
}
