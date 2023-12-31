using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CommonGuard : Unit
{
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
    public float aimSmoothTime = 1;

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
    IKControl _leftHandIKControl;
    [SerializeField]
    LookAtIKControl _lookAtIKControl;
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
    AudioSource _voiceSource;
    [SerializeField]
    AudioClip _attackVoice;
    [SerializeField]
    AudioClip _deathVoice;

    public bool AIEnabled { get; private set; }
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
    public Vector3 AimPosition
    {
        get => _aimPosition;
        set
        {
            _aimPosition = value;
        }
    }
    public Vector3 MovePosition { get; private set; }
    public enum GuardStateEnum { Patrol, PatrolStay, Defend, Search, ObeyActionOrder }
    public GuardStateEnum GuardState { get; private set; }
    public bool NeverFoundEnemy { get; private set; }

    Vector3 _aimPosition;
    Vector3 _aimVelocity;
    Vector3 _currentAimPosition;
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
            if (found)
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
    }
    public override void LoadInit()
    {
        base.LoadInit();
        Gun = Instantiate(_gunPrefab, transform);
        Gun.Init(this);
        _leftHandIKControl.anchor = Gun.LeftHandAnchor;
        SetModelFiringCD(Gun.FireCD.Max);
        _currentAimPosition = AimPosition = viewAnchor.position + transform.forward;
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
            && RaycastableTo(ViewPosition, Player, out Vector3 raycastablePosition))
        {
            FoundEnemy = true;
            NeverFoundEnemy = false;
            _lastFoundPosition = raycastablePosition;
            _lastFoundTime = Time.timeSinceLevelLoad;
            _chaseLastFoundPositionWaitTime = Random.Range(chaseLastFoundPositionWaitTimeRange.x, chaseLastFoundPositionWaitTimeRange.y);
            //TODO: model spine vend vertically
            float horzGunAngleDifference = Vector3.Angle(Gun.MuzzleAnchor.forward.Set(y: 0), (Gun.MuzzleAnchor.position - ViewPosition).Set(y: 0));
            if (horzGunAngleDifference < _fireConeMaxAngle)
                Trigger();
            GuardState = GuardStateEnum.Defend;
        }
        else
        {
            FoundEnemy = false;
            _animator.SetBool("Fire", false);
        }
        Gun.transform.SetPositionAndRotation(_weaponAnchor.transform.position, _weaponAnchor.transform.rotation);
        Gun.Aim(AimPosition);
        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            _animator.SetBool("Walk", true);
        }
        else
        {
            _animator.SetBool("Walk", false);
        }
        float footDistance = Vector3.Distance(transform.position, _aimPosition);
        Vector3 aimPosition = _aimPosition;
        if (footDistance < 2)
        {
            //suppress head angle to horizontal
            aimPosition = Vector3.Lerp(aimPosition, viewAnchor.position + transform.forward, (2 - footDistance) / 2);
        }
        _currentAimPosition = Vector3.SmoothDamp(_currentAimPosition, aimPosition, ref _aimVelocity, aimSmoothTime);
        _lookAtIKControl.SetLookAtPosition(_currentAimPosition);
    }
    private void FixedUpdate()
    {
        if (IsDead)
            return;
        float oldYRotation = transform.eulerAngles.y;
        Vector3 horzDelta = (AimPosition - viewAnchor.position).Set(y: 0);
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

            transform.eulerAngles = transform.eulerAngles.Set(y: Mathf.SmoothDamp(oldYRotation, newYRotation, ref _modelYRotateVelocity, _modelRotateSmoothTime));
        }
    }
    public override void SetEnableAI(bool cond)
    {
        AIEnabled = cond;
        _leftHandIKControl.enabled = cond;
        _lookAtIKControl.enabled = cond;
        _ragdollRelax.enabled = cond;
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
            AimPosition = _orderAimPosition;
        }
        else if (GuardState == GuardStateEnum.Search)
        {
            MovePosition = _suspiciousPosition;
            navMeshAgent.destination = MovePosition;
            AimPosition = _suspiciousPosition;
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
                AimPosition = viewAnchor.position + transform.forward;
                return;
            }
            MovePosition = _currentPatrolAnchor.transform.position;
            navMeshAgent.destination = MovePosition;
            Vector3 moveDelta = (navMeshAgent.steeringTarget - navMeshAgent.nextPosition).Set(y: 0);
            if (moveDelta != Vector3.zero)
            {
                AimPosition = viewAnchor.position + moveDelta;
            }
            else
            {
                AimPosition = viewAnchor.position + transform.forward;
            }
            if (patrolAnchors.Count > 1)
            {
                if (navMeshAgent.remainingDistance < stoppingDistance)
                {
                    GuardState = GuardStateEnum.PatrolStay;
                    _patrolAnchorStayedTime = 0;
                    if (_currentPatrolAnchor.LookFowardOnReach)
                    {
                        AimPosition = viewAnchor.position + _currentPatrolAnchor.transform.forward.Set(y: 0);
                    }
                }
            }
        }
        else if (GuardState == GuardStateEnum.PatrolStay)
        {
            AimPosition = viewAnchor.position + transform.forward;
            _patrolAnchorStayedTime += Time.deltaTime;
            if (_patrolAnchorStayedTime > _currentPatrolAnchor.StayDuration)
            {
                GuardState = GuardStateEnum.Patrol;
                _currentPatrolAnchor = patrolAnchors[(patrolAnchors.IndexOf(_currentPatrolAnchor) + 1) % patrolAnchors.Count];
            }
        }
        else if (GuardState == GuardStateEnum.Defend)
        {
            AimPosition = viewAnchor.position + transform.forward;
            bool stay = true;
            if (FoundEnemy)
            {
                if (Gun.CheckRaycast(Player, out Vector3 gunRaycastablePosition))
                {
                    AimPosition = gunRaycastablePosition;
                    stay = true;
                }
                else
                {
                    AimPosition = _lastFoundPosition;
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
            else if (Time.timeSinceLevelLoad - _lastFoundTime > _chaseLastFoundPositionWaitTime)
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
                AimPosition = ViewPosition - bulletDamage.Bullet.transform.forward * viewRange;
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
        _animator.SetBool("Fire", true);
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
        public Vector3 lookAtPosition;
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(new NpcEnemySave()
        {
            commonUnitSave = base.Serialize(),
            neverFoundEnemy = NeverFoundEnemy,
            lookAtPosition = _lookAtIKControl.lookAtPosition
        });
    }
    public override void Deserialize(string json)
    {
        NpcEnemySave save = JsonUtility.FromJson<NpcEnemySave>(json);
        NeverFoundEnemy = save.neverFoundEnemy;
        _lookAtIKControl.SetLookAtPosition(save.lookAtPosition);
        base.Deserialize(save.commonUnitSave);
    }
}
