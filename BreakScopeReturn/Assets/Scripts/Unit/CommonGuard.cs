using System.Collections;
using System.Collections.Generic;
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
    float rotateLerpFactor = 10;
    [SerializeField]
    float rotateQuickAngle = 1F;
    [SerializeField]
    float rotateMaxSpeed = 10;
    [SerializeField]
    float aimModelYRotationFix;

    [Header("Sound")]
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
    public bool UseAimRotationOrPosition { get; private set; }
    public Quaternion AimRotation
    {
        get => _aimRotation;
        set
        {
            UseAimRotationOrPosition = true;
            _aimRotation = value;
        }
    }
    public Vector3 AimPosition
    {
        get => _aimPosition;
        set
        {
            UseAimRotationOrPosition = false;
            _aimPosition = value;
        }
    }
    public Vector3 MovePosition { get; private set; }
    public enum GuardStateEnum { Patrol, PatrolStay, Defend, Search }
    public GuardStateEnum GuardState { get; private set; }
    public bool NeverFoundEnemy { get; private set; }

    Vector3 _aimPosition;
    Quaternion _aimRotation;
    Vector3 _lastFoundPosition;
    Vector3 _suspiciousPosition; //TODO: merge into above?
    float _suspiciousPositionSearchedTime;
    float _lastFoundTime;
    float _chaseLastFoundPositionWaitTime;
    float _patrolAnchorStayedTime;
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
        UseAimRotationOrPosition = true;
        AimRotation = Quaternion.LookRotation(transform.forward);
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
        if (Vector3.Distance(Player.ViewPosition, ViewPosition) < viewRange
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
        Vector3 lookAtPosition;
        if (UseAimRotationOrPosition)
        {
            lookAtPosition = viewAnchor.position + AimRotation * Vector3.forward * 100;
        }
        else
        {
            lookAtPosition = _aimPosition;
            float footDistance = Vector3.Distance(transform.position, _aimPosition);
            if (footDistance < 2)
            {
                //suppress head angle to horizontal
                lookAtPosition = Vector3.Lerp(lookAtPosition, viewAnchor.position + transform.forward, (2 - footDistance) / 2);
            }
        }
        _lookAtIKControl.SetLookAtPosition(lookAtPosition);
    }
    private void FixedUpdate()
    {
        if (IsDead)
            return;
        Quaternion oldRotation = transform.rotation;
        Quaternion targetRotation;
        if (UseAimRotationOrPosition)
        {
            targetRotation = AimRotation;
        }
        else
        {
            Vector3 horzDelta = (AimPosition - transform.position).Set(y: 0);
            targetRotation = horzDelta != Vector3.zero ? Quaternion.LookRotation(horzDelta) : oldRotation;
        }
        if (Quaternion.Angle(oldRotation, targetRotation) < rotateQuickAngle)
        {
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateLerpFactor * Time.fixedDeltaTime);
            if (Quaternion.Angle(oldRotation, transform.rotation) > rotateMaxSpeed * Time.fixedDeltaTime)
            {
                transform.rotation = Quaternion.RotateTowards(oldRotation, targetRotation, rotateMaxSpeed * Time.fixedDeltaTime);
            }
        }
    }
    private void GuardStateUpdate()
    {
        if (GuardState == GuardStateEnum.Search)
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
                AimRotation = Quaternion.LookRotation(viewAnchor.forward);
                return;
            }
            MovePosition = _currentPatrolAnchor.transform.position;
            navMeshAgent.destination = MovePosition;
            Vector3 moveDelta = (navMeshAgent.steeringTarget - navMeshAgent.nextPosition).Set(y: 0);
            if (moveDelta != Vector3.zero)
            {
                AimRotation = Quaternion.LookRotation(moveDelta);
            }
            if (patrolAnchors.Count > 1)
            {
                if (navMeshAgent.remainingDistance < stoppingDistance)
                {
                    GuardState = GuardStateEnum.PatrolStay;
                    _patrolAnchorStayedTime = 0;
                    if (_currentPatrolAnchor.LookFowardOnReach)
                    {
                        AimRotation = Quaternion.LookRotation(_currentPatrolAnchor.transform.forward.Set(y: 0));
                    }
                }
            }
        }
        else if (GuardState == GuardStateEnum.PatrolStay)
        {
            _patrolAnchorStayedTime += Time.deltaTime;
            if (_patrolAnchorStayedTime > _currentPatrolAnchor.StayDuration)
            {
                GuardState = GuardStateEnum.Patrol;
                _currentPatrolAnchor = patrolAnchors[(patrolAnchors.IndexOf(_currentPatrolAnchor) + 1) % patrolAnchors.Count];
            }
        }
        else if (GuardState == GuardStateEnum.Defend)
        {
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
                AimRotation = transform.rotation;
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
