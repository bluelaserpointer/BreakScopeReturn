using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonGuard : GroundMoveNpcUnit
{
    [Header("Debug")]
    [SerializeField]
    LineRenderer _lookLineDebug;

    [Header("Dectection")]
    [SerializeField]
    float viewRange = 50;
    [SerializeField]
    float viewAngle = 100;

    [Header("Performance")]
    [SerializeField]
    float _lookSmoothDampTime = 1;
    [SerializeField]
    float _aimSmoothDampTime = 1;

    [Header("Drop")]
    [Tooltip("Duplicate prefabs and moves here")]
    [SerializeField]
    List<GameObject> dropPrefabs;
    [Tooltip("Set Active GameObjects and moves here")]
    [SerializeField]
    List<GameObject> dropObjects;

    [Header("Model")]
    [SerializeField]
    RagdollRelax _ragdollRelax;
    [SerializeField]
    Transform _weaponAnchor;
    [SerializeField]
    NPCGun _gunPrefab;
    [SerializeField]
    List<NPCGun> _fixedGuns;
    [SerializeField]
    AnimatorIKEventExposure _IKControl;
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
    bool _bulletLineClear;
    protected Quaternion _currentLookRotation, _currentAimRotation;
    protected float _lookAngleVelocity, _aimAngleVelocity;
    protected override void Awake()
    {
        base.Awake();
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
            Vector3 itemDropPosition = transform.position + Vector3.up * 1; //TODO: read this from empty gameobject's position in inspector
            dropPrefabs.ForEach(dropPrefab =>
            {
                GameObject drop = Instantiate(dropPrefab, GameManager.Instance.Stage.transform);
                drop.transform.position = itemDropPosition;
                drop.SetActive(true);
            });
            dropObjects.ForEach(dropObject =>
            {
                dropObject.transform.position = itemDropPosition;
                dropObject.SetActive(true);
            });
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
                Gun.transform.SetPositionAndRotation(Gun.CentreRelHead.GetChildPosition(viewAnchor), _currentAimRotation);
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
        if (isInitialInit)
            _currentAimRotation = _currentLookRotation = transform.rotation;
        _ragdollRelax.relax = IsDead;
        _ragdollRelax.Check();
    }
    private void Update()
    {
        if (IsDead)
            return;
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
            _bulletLineClear = Gun.EnsureBulletLineClear(Player, out Vector3 gunRaycastablePosition);
            TargetAimPosition = _bulletLineClear ? gunRaycastablePosition : raycastablePosition;
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
        }
        Gun.Aim(TargetAimPosition);
        if (Vector3.Distance(navMeshAgent.pathEndPosition, transform.position) > navMeshAgent.stoppingDistance)
        {
            _animator.SetBool("isMoving", true);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }
        float footDistance = Vector3.Distance(transform.position, TargetAimPosition);
        Vector3 targetlookPosition = TargetAimPosition;
        /* //suppress head angle to horizontal
        if (footDistance < 2)
        {
            targetlookPosition = Vector3.Lerp(targetlookPosition, viewAnchor.position + transform.forward, (2 - footDistance) / 2);
        }*/
        _currentLookRotation = ExtendedMath.SmoothDampQuaternion(_currentLookRotation,
            Quaternion.LookRotation(targetlookPosition - viewAnchor.position), ref _lookAngleVelocity, _lookSmoothDampTime * Time.deltaTime);
        _currentAimRotation = ExtendedMath.SmoothDampQuaternion(_currentAimRotation,
            Quaternion.LookRotation(TargetAimPosition - Gun.transform.position), ref _aimAngleVelocity, _aimSmoothDampTime * Time.deltaTime);
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
            transform.eulerAngles = transform.eulerAngles.Set(y: Mathf.SmoothDampAngle(oldYRotation, newYRotation, ref _modelYRotateVelocity, _modelRotateSmoothTime * Time.fixedDeltaTime));
        }
    }
    public void Trigger()
    {
        Gun.Trigger();
    }
    public void VoicePlay(AudioClip clip)
    {
        _voiceSource.clip = clip;
        _voiceSource.Play();
    }
    protected override void DefendAction()
    {
        bool stay = true;
        if (FoundEnemy)
        {
            stay = _bulletLineClear;
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
    struct NpcEnemySave
    {
        public string npcUnitSave;
        public bool neverFoundEnemy;
        public Quaternion lookEulerAngle;
        public Quaternion aimEulerAngle;
        public Vector3 targetAimPosition;
        public Vector3 movePosition;
        public Vector3 lastFoundPosition;
        public float lastFoundTime;
        public Vector3 suspiciousPosition;
        public float suspiciousPositionSearchedTime;
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(new NpcEnemySave()
        {
            npcUnitSave = base.Serialize(),
            lookEulerAngle = _currentLookRotation,
            aimEulerAngle = _currentAimRotation,
            targetAimPosition = TargetAimPosition,
            movePosition = MovePosition,
            lastFoundPosition = _lastFoundPosition,
            lastFoundTime = _lastFoundTime,
            suspiciousPosition = _suspiciousPosition,
            suspiciousPositionSearchedTime = _suspiciousPositionSearchedTime,

        });
    }
    protected override void Internal_Deserialize(string json)
    {
        NpcEnemySave save = JsonUtility.FromJson<NpcEnemySave>(json);
        _currentLookRotation = save.lookEulerAngle;
        _currentAimRotation = save.aimEulerAngle;
        TargetAimPosition = save.targetAimPosition;
        MovePosition = save.movePosition;
        _lastFoundPosition = save.lastFoundPosition;
        _lastFoundTime = save.lastFoundTime;
        _suspiciousPosition = save.suspiciousPosition;
        _suspiciousPositionSearchedTime = save.suspiciousPositionSearchedTime;
        base.Internal_Deserialize(save.npcUnitSave);
    }
}
