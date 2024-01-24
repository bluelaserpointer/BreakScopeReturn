using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NPCGun : MonoBehaviour
{
    [SerializeField]
    float damage;
    [SerializeField]
    float speed;
    [SerializeField]
    Transform _leftHandAnchor;
    [SerializeField]
    Transform muzzleAnchor;
    [SerializeField]
    Bullet bulletPrefab;
    [SerializeField]
    Cooldown fireCD = new(0.2F);
    [SerializeField]
    List<Transform> penetrationCheckAnchors;
    [SerializeField]
    List<GameObject> _muzzleFlushes;
    [SerializeField]
    AudioSource _fireSESouce;

    Vector3 aimPosition;

    public Unit OwnerUnit { get; private set; }
    public Transform LeftHandAnchor => _leftHandAnchor;
    public Transform MuzzleAnchor => muzzleAnchor;
    public Cooldown FireCD => fireCD;
    public void Init(Unit owner)
    {
        OwnerUnit = owner;
        SetCollision(owner == null); //only collides during drop item state
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
        Bullet bullet = Instantiate(bulletPrefab);
        bullet.transform.SetParent(GameManager.Instance.CurrentStage.transform);
        bullet.damage = damage;
        bullet.speed = speed;
        bullet.transform.SetPositionAndRotation(muzzleAnchor.position, Quaternion.LookRotation(aimPosition - muzzleAnchor.position));
        if (_fireSESouce != null)
            _fireSESouce.Play();
        if (_muzzleFlushes.Count > 0)
        {
            GameObject flash = Instantiate(_muzzleFlushes.GetRandomElement(), muzzleAnchor.position, muzzleAnchor.rotation * Quaternion.Euler(0, 0, Random.value * 360));
            flash.transform.parent = muzzleAnchor.transform;
        }
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
    public void SetCollision(bool cond)
    {
        GetComponentInChildren<Collider>().enabled = cond;
    }
}
