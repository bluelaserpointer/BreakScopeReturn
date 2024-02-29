using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : NpcUnit
{
    [SerializeField]
    Transform _xRotationJoint;
    [SerializeField]
    Transform _yRotationJoint;

    [Header("View")]
    public float maxViewDistance = 10;
    public float viewAngle = 100;

    [Header("Firearm")]
    public Bullet bulletPrefab;
    public List<Transform> _launchAnchors;
    public float damage;
    public float bulletVelocity;
    public IzumiTools.Cooldown fireCD = new(0.25F);
    public GameObject fireVfxPrefab;
    public AudioSource fireSE;

    [Header("Mobility")]
    [SerializeField]
    float _rotateSmoothTime = 10;
    [SerializeField]
    float _fireConeAngle = 2F;
    [SerializeField]
    Vector2 _xRotateRange = new Vector2(-60, 75), _yRotateRange = new Vector2(-180, 180);

    [Header("Animation")]
    [SerializeField]
    AnimationClip _fireAnimationClip;
    [SerializeField]
    GameObject _destoryVFX;
    [SerializeField]
    Renderer _laserSightRenderer;
    [SerializeField]
    List<GameObject> _aliveShowObjects;

    [Header("SE")]
    [SerializeField]
    AudioSource _detectSoundSource;
    [SerializeField]
    AudioClip _detectInSE, _detectOutSE;

    private Player Player => GameManager.Instance.Player;
    public bool FoundEnemy { get; private set; }
    public Vector3 TargetAimPosition { get; private set; }

    Vector3 _gunRotateVelocity;
    Material _laserSightMaterial;

    protected override void Internal_Init(bool isInitialInit)
    {
        base.Internal_Init(isInitialInit);
        _gunRotateVelocity = Vector2.zero;
        FoundEnemy = false;
        if (isInitialInit)
        {
            _laserSightMaterial = _laserSightRenderer.material;
            onDead.AddListener(() =>
            {
                _destoryVFX.SetActive(true);
                _aliveShowObjects.ForEach(each => each.SetActive(false));
            });
            TargetAimPosition = viewAnchor.position + viewAnchor.forward;
            _animator.SetFloat("FireSpeedMultiplier", fireCD.Capacity > 0 ? _fireAnimationClip.length / fireCD.Capacity : 1);
        }
        _destoryVFX.SetActive(IsDead);
        _aliveShowObjects.ForEach(each => each.SetActive(IsAlive));
    }
    private void Update()
    {
        if (IsDead)
            return;
        float viewAngleDifference = Vector3.Angle(viewAnchor.forward, Player.ViewPosition - ViewPosition);
        if (!Player.stealth
            && Vector3.Distance(Player.ViewPosition, ViewPosition) < maxViewDistance
            && viewAngleDifference < viewAngle / 2
            && TryDetect(ViewPosition, Player, out Vector3 raycastablePosition))
        {
            if (!FoundEnemy)
            {
                FoundEnemy = true;
                NeverFoundEnemy = false;
                print("found.");
                _detectSoundSource.clip = _detectInSE;
                _detectSoundSource.Play();
            }
            TargetAimPosition = raycastablePosition;
        }
        else if (FoundEnemy)
        {
            FoundEnemy = false;
            _detectSoundSource.clip = _detectOutSE;
            _detectSoundSource.Play();
        }
        fireCD.AddDeltaTime();
        if (FoundEnemy && viewAngleDifference < _fireConeAngle / 2)
        {
            Trigger();
        }
        LaserSightUpdate();
    }
    private void FixedUpdate()
    {
        if (IsDead)
            return;
        Vector3 horzDelta = (TargetAimPosition - viewAnchor.position).Set(y: 0);
        Vector2 newAngle = Vector2.zero;
        if (horzDelta != Vector3.zero)
        {
            newAngle.x = -Mathf.Rad2Deg * Mathf.Atan2(TargetAimPosition.y - viewAnchor.position.y, horzDelta.magnitude);
            newAngle.y = Quaternion.LookRotation(horzDelta).eulerAngles.y;
        }
        else
        {
            newAngle.x = _xRotationJoint.eulerAngles.x;
            newAngle.y = _yRotationJoint.eulerAngles.y;
        }
        _xRotationJoint.eulerAngles = _xRotationJoint.eulerAngles.Set(x: Mathf.SmoothDampAngle(_xRotationJoint.eulerAngles.x, newAngle.x, ref _gunRotateVelocity.x, _rotateSmoothTime * Time.fixedDeltaTime));
        _yRotationJoint.eulerAngles = _yRotationJoint.eulerAngles.Set(y: Mathf.SmoothDampAngle(_yRotationJoint.eulerAngles.y, newAngle.y, ref _gunRotateVelocity.y, _rotateSmoothTime * Time.fixedDeltaTime));
    }
    public void Trigger()
    {
        if (fireCD.Eat())
            Fire();
    }
    public void Fire()
    {
        Quaternion shotAngle = Quaternion.LookRotation(TargetAimPosition - _launchAnchors[0].position);
        foreach (var launchAnchor in _launchAnchors)
        {
            Bullet bullet = Instantiate(bulletPrefab, launchAnchor.position, shotAngle);
            bullet.damage = damage;
            bullet.speed = bulletVelocity;
            Instantiate(fireVfxPrefab, launchAnchor.position, launchAnchor.rotation).transform.SetParent(launchAnchor);
        }
        fireSE.Play();
        _animator.SetTrigger("Fire");
    }
    private void LaserSightUpdate()
    {
        Vector3 laserSightEndPosition;
        float laserSightLineLength;
        if (FoundEnemy)
        {
            laserSightEndPosition = TargetAimPosition;
            //TODO: remove distance recalcuration
            laserSightLineLength = Vector3.Distance(viewAnchor.position, laserSightEndPosition);
        }
        else
        {
            RaycastHit closestHit = new RaycastHit { distance = maxViewDistance };
            foreach (var hitInfo in Physics.RaycastAll(viewAnchor.position, viewAnchor.forward, maxViewDistance))
            {
                if (hitInfo.collider.isTrigger || IsMyCollider(hitInfo.collider))
                    continue;
                if (hitInfo.distance < closestHit.distance)
                {
                    closestHit = hitInfo;
                }
            }
            if (closestHit.collider != null)
            {
                laserSightEndPosition = closestHit.point;
                laserSightLineLength = closestHit.distance;
            }
            else
            {
                laserSightEndPosition = viewAnchor.position + viewAnchor.forward * maxViewDistance;
                laserSightLineLength = maxViewDistance;
            }
        }
        _laserSightRenderer.transform.position = (viewAnchor.position + laserSightEndPosition) / 2;
        float verticalStretch = laserSightLineLength / 2;
        _laserSightRenderer.transform.localScale = _laserSightRenderer.transform.localScale.Set(y: verticalStretch);
        _laserSightMaterial.SetFloat("_VScale", verticalStretch);
    }
    struct TurretSave
    {
        public string commonUnitSave;
        public Vector3 targetAimPosition;
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(new TurretSave()
        {
            commonUnitSave = base.Serialize(),
            targetAimPosition = TargetAimPosition
        });
    }
    protected override void Internal_Deserialize(string json)
    {
        TurretSave save = JsonUtility.FromJson<TurretSave>(json);
        TargetAimPosition = save.targetAimPosition;
        base.Internal_Deserialize(save.commonUnitSave);
    }
}
