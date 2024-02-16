using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CommonGuard : Unit
{
    [Header("Debug")]
    [SerializeField]
    LineRenderer _lookLineDebug;

    [Header("Dectection")]
    [SerializeField]
    float viewRange = 50;
    [SerializeField]
    float viewAngle = 100;

    [Header("ActionOrder")]
    [SerializeField]
    bool _obeyActionOrder;
    [SerializeField]
    Vector3 _orderMovePosition;
    [SerializeField]
    Vector3 _orderAimPosition;

    [Header("Performance")]
    [SerializeField]
    float _lookSmoothDampTime = 1;
    [SerializeField]
    float _aimSmoothDampTime = 1;

    [Header("Strategy")]
    [SerializeField]
    bool _mustHoldPosition;
    [SerializeField]
    float _suspiciousPositionSearchTime;
    [SerializeField]
    float _fireConeMaxAngle;
    [SerializeField]
    float stoppingDistance;
    [SerializeField]
    NavMeshAgent navMeshAgent;
    [SerializeField]
    Vector2 chaseLastFoundPositionWaitTimeRange;
    [SerializeField]
    float quickChaseDistance;
    [SerializeField]
    List<PatrolAnchor> patrolAnchors;

    [Header("Drop")]
    [SerializeField]
    List<GameObject> dropPrefabs;

    [Header("Model")]
    [SerializeField]
    RagdollRelax _ragdollRelax;
    [SerializeField]
    Transform _weaponAnchor;
    [SerializeField]
    NPCGun _gunPrefab;
    [SerializeField]
    AnimatorIKEventExposure _IKControl;
    [SerializeField]
    GameObject _aliveDevice;
    [SerializeField]
    float rotateQuickAngle = 1F;
    [SerializeField]
    float _modelRotateSmoothTime = 10;
    [SerializeField]
    float aimModelYRotationFix;

    [Header("Sound")]
    [SerializeField]
    float _foundVoiceCD = 5F;
    [SerializeField]
    AudioSource _voiceSource;
    [SerializeField]
    AudioClip _attackVoice;
    [SerializeField]
    AudioClip _deathVoice;

    public NPCGun Gun { get; private set; }
    public readonly UnityEvent<bool> onFoundEnemyChangedTo = new UnityEvent<bool>();
    Player Player => GameManager.Instance.Player;
    PatrolAnchor _currentPatrolAnchor;

    public bool FoundEnemy
    {
        get => _foundEnemy;
        set
        {
            if (_foundEnemy != value)
            {
                _foundEnemy = value;
                onFoundEnemyChangedTo.Invoke(value);
            }
        }
    }
    private bool _foundEnemy;
    public Vector3 TargetAimPosition
    {
        get => _targetAimPosition;
        set
        {
            _targetAimPosition = value;
        }
    }
    public Vector3 MovePosition { get; private set; }
    public enum GuardStateEnum { Patrol, PatrolStay, Defend, Search, ObeyActionOrder }
    public GuardStateEnum GuardState { get; private set; }
    public bool NeverFoundEnemy { get; private set; }
    public float TimePassedAfterLastFound => Time.timeSinceLevelLoad - _lastFoundTime;

    Vector3 _targetAimPosition;
    Quaternion _currentLookRotation, _currentAimRotation;
    float _lookAngleVelocity, _aimAngleVelocity;
    Vector3 _lastFoundPosition;
    Vector3 _suspiciousPosition; //TODO: merge into above?
    float _suspiciousPositionSearchedTime;
    float _lastFoundTime;
    float _chaseLastFoundPositionWaitTime;
    float _patrolAnchorStayedTime;
    float _modelYRotateVelocity;
    private void Awake()
    {
        NeverFoundEnemy = true;
        navMeshAgent.updateRotation = false;
        navMeshAgent.stoppingDistance = stoppingDistance;
        onFoundEnemyChangedTo.AddListener(found =>
        {
            if (found && TimePassedAfterLastFound > _foundVoiceCD)
            {
                VoicePlay(_attackVoice);
            }
        });
        onDead.AddListener(() =>
        {
            VoicePlay(_deathVoice);
            Gun.GetComponent<ArmDrop>().Drop(); //TODO: combine the function to NPCGun
            navMeshAgent.enabled = false;
            dropPrefabs.ForEach(dropPrefab =>
            {
                GameObject drop = Instantiate(dropPrefab);
                drop.transform.SetParent(GameManager.Instance.CurrentStage.transform);
                drop.transform.position = transform.position + Vector3.up * 1;
            });
            if (_aliveDevice)
                _aliveDevice.gameObject.SetActive(false);
            if (_ragdollRelax)
                _ragdollRelax.relax = true;
        });
        _IKControl.onAnimatorIK.AddListener(layer =>
        {
            if (layer == Animator.GetLayerIndex("LegsLayer"))
            {
                Animator.SetLookAtPosition(viewAnchor.position + _currentLookRotation * Vector3.forward);
                return;
            }
            if (layer == Animator.GetLayerIndex("HandsLayer"))
            {
                Gun.transform.position = Gun.CentreRelHead.GetChildPosition(viewAnchor);
                Gun.transform.rotation = _currentAimRotation;
                Animator.SetIKPosition(AvatarIKGoal.RightHand, Gun.RightHandAnchor.position);
                Animator.SetIKRotation(AvatarIKGoal.RightHand, Gun.RightHandAnchor.rotation);
                Animator.SetIKPosition(AvatarIKGoal.LeftHand, Gun.LeftHandAnchor.position);
                Animator.SetIKRotation(AvatarIKGoal.LeftHand, Gun.LeftHandAnchor.rotation);
                return;
            }
        });
    }
    protected override void Internal_Init(bool isInitialInit)
    {
        base.Internal_Init(isInitialInit);
        Gun = Instantiate(_gunPrefab, transform);
        Gun.Init(this);
        SetModelFiringCD(Gun.FireCD.Capacity);
        TargetAimPosition = viewAnchor.position + transform.forward;
        _currentLookRotation = transform.rotation;
        _currentAimRotation = Quaternion.LookRotation(TargetAimPosition - Gun.transform.position, viewAnchor.up);
        MovePosition = transform.position;
        navMeshAgent.enabled = true;
        navMeshAgent.destination = transform.position;
        FoundEnemy = false;
        if (!NeverFoundEnemy)
        {
            _lastFoundPosition = _suspiciousPosition = transform.position; //stop chasing after stage saving
        }
        _ragdollRelax.relax = IsDead;
        _ragdollRelax.Check();
        if (patrolAnchors.Count > 0)
        {
            GuardState = GuardStateEnum.Patrol;
            _currentPatrolAnchor = patrolAnchors[0];
        }
        else
        {
            GuardState = GuardStateEnum.Defend;
        }
    }
    private void Update()
    {
        if (IsDead)
        {
            return;
        }
        GuardStateUpdate();
        _animator.transform.localEulerAngles = Vector3.up * aimModelYRotationFix;
        float viewAngleDifference = Vector3.Angle(viewAnchor.forward, Player.ViewPosition - ViewPosition);
        if (!Player.stealth
            && Vector3.Distance(Player.ViewPosition, ViewPosition) < viewRange
            && viewAngleDifference < viewAngle / 2
            && TryDetect(ViewPosition, Player, out Vector3 raycastablePosition))
        {
            FoundEnemy = true;
            NeverFoundEnemy = false;
            _lastFoundPosition = raycastablePosition;
            _lastFoundTime = Time.timeSinceLevelLoad;
            _chaseLastFoundPositionWaitTime = Random.Range(chaseLastFoundPositionWaitTimeRange.x, chaseLastFoundPositionWaitTimeRange.y);
            float horzGunAngleDifference = Vector3.Angle(Gun.transform.forward, TargetAimPosition - Gun.transform.position);
            if (horzGunAngleDifference < _fireConeMaxAngle)
                Trigger();
            GuardState = GuardStateEnum.Defend;
        }
        else
        {
            FoundEnemy = false;
            //_animator.SetBool("Fire", false);
        }
        Gun.Aim(TargetAimPosition);
        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            _animator.SetBool("isMoving", true);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }
        float footDistance = Vector3.Distance(transform.position, _targetAimPosition);
        Vector3 targetlookPosition = _targetAimPosition;
        /* //suppress head angle to horizontal
        if (footDistance < 2)
        {
            targetlookPosition = Vector3.Lerp(targetlookPosition, viewAnchor.position + transform.forward, (2 - footDistance) / 2);
        }*/
        _currentLookRotation = ExtendedMath.SmoothDampQuaternion(_currentLookRotation,
            Quaternion.LookRotation(targetlookPosition - viewAnchor.position), ref _lookAngleVelocity, _lookSmoothDampTime * Time.deltaTime);
        _currentAimRotation = ExtendedMath.SmoothDampQuaternion(_currentAimRotation,
            Quaternion.LookRotation(_targetAimPosition - Gun.transform.position), ref _aimAngleVelocity, _aimSmoothDampTime * Time.deltaTime);
        _lookLineDebug.SetPositions(new Vector3[] { viewAnchor.position, targetlookPosition });
    }
    private void FixedUpdate()
    {
        if (IsDead)
            return;
        float oldYRotation = transform.eulerAngles.y;
        Vector3 horzDelta = (TargetAimPosition - viewAnchor.position).Set(y: 0);
        float newYRotation = horzDelta != Vector3.zero ? Quaternion.LookRotation(horzDelta).eulerAngles.y : oldYRotation;
        if (Mathf.DeltaAngle(oldYRotation, newYRotation) < rotateQuickAngle)
        {
            transform.eulerAngles = transform.eulerAngles.Set(y: newYRotation);
        }
        else
        {
            /*
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateLerpFactor * Time.fixedDeltaTime);
            if (Quaternion.Angle(oldRotation, transform.rotation) > rotateMaxSpeed * Time.fixedDeltaTime)
            {
            }*/

            transform.eulerAngles = transform.eulerAngles.Set(y: Mathf.SmoothDamp(oldYRotation, newYRotation, ref _modelYRotateVelocity, _modelRotateSmoothTime * Time.fixedDeltaTime));
        }
    }
    protected override void OnAIEnableChange()
    {
        base.OnAIEnableChange();
    }
    public void OrderAction(UnitActionOrder order)
    {
        _obeyActionOrder = true;
        _orderMovePosition = order.transform.position;
        _orderAimPosition = order.aimTarget.position;
    }
    private void GuardStateUpdate()
    {
        if (_obeyActionOrder)
            GuardState = GuardStateEnum.ObeyActionOrder;
        if (GuardState == GuardStateEnum.ObeyActionOrder)
        {
            navMeshAgent.destination = _orderMovePosition;
            TargetAimPosition = _orderAimPosition;
        }
        else if (GuardState == GuardStateEnum.Search)
        {
            MovePosition = _suspiciousPosition;
            navMeshAgent.destination = MovePosition;
            TargetAimPosition = _suspiciousPosition;
            if (navMeshAgent.remainingDistance < stoppingDistance)
            {
                _suspiciousPositionSearchedTime += Time.deltaTime;
                if (_suspiciousPositionSearchedTime > _suspiciousPositionSearchTime)
                {
                    GuardState = NeverFoundEnemy ? GuardStateEnum.Patrol : GuardStateEnum.Defend;
                }
            }
        }
        else if (GuardState == GuardStateEnum.Patrol)
        {
            if (_currentPatrolAnchor == null)
            {
                GuardState = GuardStateEnum.Defend;
                TargetAimPosition = viewAnchor.position + transform.forward;
                return;
            }
            MovePosition = _currentPatrolAnchor.transform.position;
            navMeshAgent.destination = MovePosition;
            Vector3 moveDelta = (navMeshAgent.steeringTarget - navMeshAgent.nextPosition).Set(y: 0);
            if (moveDelta != Vector3.zero)
            {
                TargetAimPosition = viewAnchor.position + moveDelta;
            }
            else
            {
                TargetAimPosition = viewAnchor.position + transform.forward;
            }
            if (patrolAnchors.Count > 1)
            {
                if (navMeshAgent.remainingDistance < stoppingDistance)
                {
                    GuardState = GuardStateEnum.PatrolStay;
                    _patrolAnchorStayedTime = 0;
                    if (_currentPatrolAnchor.LookFowardOnReach)
                    {
                        TargetAimPosition = viewAnchor.position + _currentPatrolAnchor.transform.forward.Set(y: 0);
                    }
                }
            }
        }
        else if (GuardState == GuardStateEnum.PatrolStay)
        {
            TargetAimPosition = viewAnchor.position + transform.forward;
            _patrolAnchorStayedTime += Time.deltaTime;
            if (_patrolAnchorStayedTime > _currentPatrolAnchor.StayDuration)
            {
                GuardState = GuardStateEnum.Patrol;
                _currentPatrolAnchor = patrolAnchors[(patrolAnchors.IndexOf(_currentPatrolAnchor) + 1) % patrolAnchors.Count];
            }
        }
        else if (GuardState == GuardStateEnum.Defend)
        {
            TargetAimPosition = viewAnchor.position + transform.forward;
            bool stay = true;
            if (FoundEnemy)
            {
                if (Gun.EnsureBulletLineClear(Player, out Vector3 gunRaycastablePosition))
                {
                    TargetAimPosition = gunRaycastablePosition;
                    stay = true;
                }
                else
                {
                    TargetAimPosition = _lastFoundPosition;
                    stay = false;
                }
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
                navMeshAgent.destination = _lastFoundPosition;
            }
        }
    }
    public override void Damage(DamageSource damageSource)
    {
        base.Damage(damageSource);
        if (!FoundEnemy)
        {
            GuardState = GuardStateEnum.Defend;
            if (damageSource.GetType() == typeof(DamageSource.BulletDamage))
            {
                DamageSource.BulletDamage bulletDamage = (DamageSource.BulletDamage)damageSource;
                TargetAimPosition = ViewPosition - bulletDamage.Bullet.transform.forward * viewRange;
            }
        }
    }
    public override void ListenSound(SoundSource soundSouce)
    {
        base.ListenSound(soundSouce);
        bool shouldSearch = false;
        if (FoundEnemy)
        {
            shouldSearch = false;
        }
        else if (soundSouce.emergence)
        {
            shouldSearch = true;
        }
        else if (soundSouce.suspicious && !_mustHoldPosition)
        {
            shouldSearch = true;
        }
        if (shouldSearch)
        {
            GuardState = GuardStateEnum.Search;
            _suspiciousPositionSearchedTime = 0;
            _suspiciousPosition = soundSouce.position;
        }
    }
    public void Trigger()
    {
        //_animator.SetBool("Fire", true);
        Gun.Trigger();
    }
    public void VoicePlay(AudioClip clip)
    {
        _voiceSource.clip = clip;
        _voiceSource.Play();
    }
    struct NpcEnemySave
    {
        public string commonUnitSave;
        public bool neverFoundEnemy;
        public Quaternion lookEulerAngle;
        public Quaternion aimEulerAngle;
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(new NpcEnemySave()
        {
            commonUnitSave = base.Serialize(),
            neverFoundEnemy = NeverFoundEnemy,
            lookEulerAngle = _currentLookRotation,
            aimEulerAngle = _currentAimRotation
        });
    }
    protected override void Internal_Deserialize(string json)
    {
        NpcEnemySave save = JsonUtility.FromJson<NpcEnemySave>(json);
        NeverFoundEnemy = save.neverFoundEnemy;
        _currentLookRotation = save.lookEulerAngle;
        _currentAimRotation = save.aimEulerAngle;
        base.Internal_Deserialize(save.commonUnitSave);
    }
}
