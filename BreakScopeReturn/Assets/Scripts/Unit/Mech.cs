using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : GroundMoveNpcUnit
{
    [Header("Dectection")]
    [SerializeField]
    float viewRange = 50;
    [SerializeField]
    float viewAngle = 100;
    [SerializeField]
    float findOutOfAngleTargetRotationSpeed = 25;

    [Header("Look Agility")]
    [SerializeField]
    float rotateQuickAngle = 1F;
    [SerializeField]
    float _modelRotateSmoothTime = 10;

    [Header("Weapons")]
    [SerializeField]
    List<BijointAim> _weaponJoints;
    [SerializeField]
    List<Transform> _weaponMuzzleAnchors;

    bool _lostTargetByViewAngle;
    float _findOutOfAngleTargetCurrentYAngle;
    float _findOutOfAngleTargetRotateDirection;
    float _findOutOfAngleTargetRemainTime;
    float _lastFoundDistance;

    protected override void Awake()
    {
        base.Awake();
        onFoundEnemyChangedTo.AddListener(found =>
        {
            if (found)
                return;
            if (!_lostTargetByViewAngle)
                return;
            _findOutOfAngleTargetRemainTime = 360 / findOutOfAngleTargetRotationSpeed;
            Vector3 localLastFoundPosition = transform.InverseTransformPoint(_lastFoundPosition);
            _findOutOfAngleTargetCurrentYAngle = Mathf.Rad2Deg * Mathf.Atan2(localLastFoundPosition.x, localLastFoundPosition.z);
            _findOutOfAngleTargetRotateDirection = localLastFoundPosition.x > 0 ? 1 : -1;
        });
    }
    protected override void Internal_Init(bool isInitialInit)
    {
        base.Internal_Init(isInitialInit);
        _lostTargetByViewAngle = false;
        _findOutOfAngleTargetRemainTime = 0;
    }
    private void Update()
    {
        if (IsDead)
            return;
        GuardStateUpdate();
        float viewAngleDifference = Vector3.Angle(viewAnchor.forward, Player.ViewPosition - ViewPosition);
        if (!Player.stealth
            && Vector3.Distance(Player.ViewPosition, ViewPosition) < viewRange
            && !(_lostTargetByViewAngle = viewAngleDifference > viewAngle / 2)
            && TryDetect(ViewPosition, Player, out Vector3 raycastablePosition))
        {
            FoundEnemy = true;
            NeverFoundEnemy = false;
            _lastFoundPosition = raycastablePosition;
            _lastFoundDistance = Vector3.Distance(transform.position, _lastFoundPosition);
            //_bulletLineClear = Gun.EnsureBulletLineClear(Player, out Vector3 gunRaycastablePosition);
            //TargetAimPosition = _bulletLineClear ? gunRaycastablePosition : raycastablePosition;
            TargetAimPosition = raycastablePosition;
            _lastFoundTime = Time.timeSinceLevelLoad;
            _chaseLastFoundPositionWaitTime = Random.Range(chaseLastFoundPositionWaitTimeRange.x, chaseLastFoundPositionWaitTimeRange.y);
            //float horzGunAngleDifference = Vector3.Angle(Gun.transform.forward, TargetAimPosition - Gun.transform.position);
            //if (horzGunAngleDifference < _fireConeMaxAngle)
                //Trigger();
            GuardState = GuardStateEnum.Defend;
            _findOutOfAngleTargetRemainTime = 0;
        }
        else
        {
            FoundEnemy = false;
            if (_findOutOfAngleTargetRemainTime > 0)
            {
                _findOutOfAngleTargetRemainTime -= Time.deltaTime;
                _findOutOfAngleTargetCurrentYAngle += _findOutOfAngleTargetRotateDirection * findOutOfAngleTargetRotationSpeed * Time.deltaTime;
                float rad = Mathf.Deg2Rad * _findOutOfAngleTargetCurrentYAngle;
                TargetAimPosition = viewAnchor.TransformPoint(_lastFoundDistance * new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)))
                    .Set(y: _lastFoundPosition.y);
            }
        }
        foreach (var weaponJoint in _weaponJoints)
        {
            weaponJoint.targetAimPosition = TargetAimPosition;
        }
        if (Vector3.Distance(navMeshAgent.pathEndPosition, transform.position) > navMeshAgent.stoppingDistance)
        {
            _animator.SetBool("isMoving", true);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }
        _animator.SetFloat("MoveDirection", viewAnchor.TransformPoint(TargetAimPosition).z > 0 ? 1 : -1);
    }
    private void FixedUpdate()
    {
        if (IsDead)
            return;
        float oldYRotation = transform.eulerAngles.y;
        Vector3 horzDelta = (TargetAimPosition - viewAnchor.position).Set(y: 0);
        float newYRotation = horzDelta != Vector3.zero ? Quaternion.LookRotation(horzDelta).eulerAngles.y : oldYRotation;
        transform.eulerAngles = transform.eulerAngles.Set(y: Mathf.SmoothDampAngle(oldYRotation, newYRotation, ref _modelYRotateVelocity, _modelRotateSmoothTime * Time.fixedDeltaTime));
    }
    public void Trigger()
    {
        
    }
    public void Fire()
    {

    }
    protected override void DefendAction()
    {
        bool stay = true;
        if (FoundEnemy)
        {
            stay = false;
        }
        else if (NeverFoundEnemy)
        {
            stay = true;
        }
        else if (Vector3.Distance(transform.position, _lastFoundPosition) < quickChaseDistance)
        {
            stay = false;
        }
        else if (TimePassedAfterLastFound > _chaseLastFoundPositionWaitTime)
        {
            stay = false;
        }
        if (_mustHoldPosition || stay)
        {
            navMeshAgent.destination = transform.position;
        }
        else
        {
            if (!FoundEnemy || Vector3.Distance(Player.transform.position, transform.position) > _keepAwayDistance)
            {
                navMeshAgent.destination = _lastFoundPosition;
            }
            else
            {
                navMeshAgent.destination = Player.transform.position + (transform.position - Player.transform.position).normalized * _keepAwayDistance;
                
            }
        }
    }
}
