using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public abstract class GroundMoveNpcUnit : NpcUnit
{
    [Header("Moving Strategy")]
    [SerializeField]
    protected NavMeshAgent navMeshAgent;
    [SerializeField]
    protected bool _mustHoldPosition;
    [SerializeField]
    protected float _suspiciousPositionSearchTime;
    [SerializeField]
    protected float _fireConeMaxAngle;
    [SerializeField]
    protected float stoppingDistance;
    [SerializeField]
    protected float _keepAwayDistance;
    [SerializeField]
    protected Vector2 chaseLastFoundPositionWaitTimeRange;
    [SerializeField]
    protected float quickChaseDistance;
    [SerializeField]
    protected List<PatrolAnchor> patrolAnchors;

    [Header("ActionOrder")]
    [SerializeField]
    protected bool _obeyActionOrder;
    [SerializeField]
    protected Vector3 _orderMovePosition;
    [SerializeField]
    protected Vector3 _orderAimPosition;
    public readonly UnityEvent<bool> onFoundEnemyChangedTo = new();

    public Vector3 MovePosition { get; protected set; }
    public enum GuardStateEnum { Patrol, PatrolStay, Defend, Search, ObeyActionOrder }
    public GuardStateEnum GuardState { get; protected set; }
    public float TimePassedAfterLastFound => Time.timeSinceLevelLoad - _lastFoundTime;
    protected Player Player => GameManager.Instance.Player;
    protected PatrolAnchor _currentPatrolAnchor;

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
    public Vector3 TargetAimPosition { get; protected set; }
    protected Vector3 _lastFoundPosition;
    protected Vector3 _suspiciousPosition; //TODO: merge into above?
    protected float _suspiciousPositionSearchedTime;
    protected float _lastFoundTime;
    protected float _chaseLastFoundPositionWaitTime;
    protected float _patrolAnchorStayedTime;
    protected float _modelYRotateVelocity;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent.updateRotation = false;
        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.autoBraking = true;
        onDead.AddListener(() => navMeshAgent.enabled = false);
    }
    protected override void Internal_Init(bool isInitialInit)
    {
        base.Internal_Init(isInitialInit);
        navMeshAgent.enabled = true;
        if (gameObject.activeInHierarchy) //Suppress error of navMeshAgent
            navMeshAgent.destination = transform.position;
        FoundEnemy = false;
        if (!NeverFoundEnemy)
        {
            _lastFoundPosition = _suspiciousPosition = transform.position;
        }
        if (isInitialInit)
        {
            TargetAimPosition = transform.position + transform.forward * 100;
            MovePosition = transform.position;
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
        else
        {
            GuardState = GuardStateEnum.Defend; //TODO: save this data
        }
    }
    protected void GuardStateUpdate()
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
            SearchAction();
        }
        else if (GuardState == GuardStateEnum.Patrol)
        {
            PatrolAction();
        }
        else if (GuardState == GuardStateEnum.PatrolStay)
        {
            PatrolStayAction();
        }
        else if (GuardState == GuardStateEnum.Defend)
        {
            DefendAction();
        }
    }
    protected virtual void SearchAction()
    {
        bool waitNavMeshGeneretePath = false;
        if (MovePosition != _suspiciousPosition)
        {
            MovePosition = _suspiciousPosition;
            navMeshAgent.destination = _suspiciousPosition;
            waitNavMeshGeneretePath = true;
        }
        TargetAimPosition = _suspiciousPosition;
        if (!waitNavMeshGeneretePath && navMeshAgent.remainingDistance < stoppingDistance)
        {
            navMeshAgent.destination = transform.position;
            _suspiciousPositionSearchedTime += Time.deltaTime;
            if (_suspiciousPositionSearchedTime > _suspiciousPositionSearchTime)
            {
                GuardState = NeverFoundEnemy ? GuardStateEnum.Patrol : GuardStateEnum.Defend;
            }
        }
    }
    protected virtual void PatrolAction()
    {
        if (_currentPatrolAnchor == null)
        {
            GuardState = GuardStateEnum.Defend;
            TargetAimPosition = transform.position + transform.forward * 100;
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
            TargetAimPosition = transform.position + transform.forward * 100;
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
    protected virtual void PatrolStayAction()
    {
        TargetAimPosition = transform.position + transform.forward * 100;
        _patrolAnchorStayedTime += Time.deltaTime;
        if (_patrolAnchorStayedTime > _currentPatrolAnchor.StayDuration)
        {
            GuardState = GuardStateEnum.Patrol;
            _currentPatrolAnchor = patrolAnchors[(patrolAnchors.IndexOf(_currentPatrolAnchor) + 1) % patrolAnchors.Count];
        }
    }
    protected virtual void DefendAction()
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
    public void OrderAction(UnitActionOrder order)
    {
        _obeyActionOrder = true;
        _orderMovePosition = order.transform.position;
        _orderAimPosition = order.aimTarget.position;
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
                TargetAimPosition = ViewPosition - bulletDamage.Bullet.transform.forward * 100;
            }
        }
    }
    public override void ListenSound(SoundSource soundSouce)
    {
        base.ListenSound(soundSouce);
        bool shouldSearch = false;
        if (_mustHoldPosition || FoundEnemy)
        {
            shouldSearch = false;
        }
        else if (soundSouce.emergence)
        {
            shouldSearch = true;
        }
        else if (soundSouce.suspicious)
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
}
