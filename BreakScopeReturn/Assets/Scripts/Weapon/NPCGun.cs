using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NPCGun : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    bool _realtimeIK;

    [Header("Misc")]
    [SerializeField]
    float damage;
    [SerializeField]
    float speed;
    [SerializeField]
    Transform _headAnchor;
    [SerializeField]
    Transform _rightHandAnchor;
    [SerializeField]
    Transform _leftHandAnchor;
    [SerializeField]
    Transform muzzleAnchor;
    [SerializeField]
    Bullet bulletPrefab;
    [SerializeField]
    Cooldown fireCD = new(0.2F);
    [SerializeField]
    List<GameObject> _muzzleFlushes;
    [SerializeField]
    AudioSource _fireSESouce;

    Vector3 aimPosition;

    public Unit OwnerUnit { get; private set; }
    public TransformRelator CentreRelHead {  get; private set; }
    public Transform RightHandAnchor => _rightHandAnchor;
    public Transform LeftHandAnchor => _leftHandAnchor;
    public Transform MuzzleAnchor => muzzleAnchor;
    public Cooldown FireCD => fireCD;
    private void Awake()
    {
        UpdateAnchorRelation();
    }
    private void Update()
    {
        if (_realtimeIK)
            UpdateAnchorRelation();
        fireCD.AddDeltaTime();
    }
    public void UpdateAnchorRelation()
    {
        if (_headAnchor == null)
            return;
        CentreRelHead = new TransformRelator(transform, _headAnchor);
    }
    public void Init(Unit owner)
    {
        OwnerUnit = owner;
        SetCollision(owner == null); //only collides during drop item state
    }
    public void Aim(Vector3 aimPosition)
    {
        this.aimPosition = aimPosition;
    }
    public void Trigger()
    {
        if (fireCD.Eat())
            Fire();
    }
    public void Fire()
    {
        Bullet bullet = Instantiate(bulletPrefab);
        bullet.transform.SetParent(GameManager.Instance.Stage.transform);
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
    public bool EnsureBulletLineClear(Unit targetUnit, out Vector3 recommendAimPosition)
    {
        return OwnerUnit.TryDetect(MuzzleAnchor.position, targetUnit, out recommendAimPosition);
    }
    public void SetCollision(bool cond)
    {
        GetComponentInChildren<Collider>().enabled = cond;
    }
}
