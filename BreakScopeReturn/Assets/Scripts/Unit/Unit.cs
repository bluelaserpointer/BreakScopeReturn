using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Unit : SaveTarget
{
    public static string TAG_NPC_UNIT => "NpcUnit";
    [Header("Unit Common Stats")]
    [SerializeField] IzumiTools.CappedValue _health;
    [Range(0f, 1f)]
    [SerializeField] float _initialHealthRatio = 1;
    public bool stealth;
    [SerializeField] protected Transform viewAnchor;
    [SerializeField] List<Transform> _detectTargets;
    [SerializeField] protected Animator _animator;
    [SerializeField] AnimationClip _firingAnimationClip;

    public UnityEvent<DamageSource> onDamage = new();
    public UnityEvent<float> onHeal = new();
    public UnityEvent onDead = new();

    /// <summary>
    /// Set this true to always pass value change check during initialize phase. <br/>
    /// </summary>
    public bool Initializing { get; private set; }
    /// <summary>
    /// If an enemy AI or controllings of the player are avaliable. <br/>
    /// Set <see cref="CutscenePause"/> = false to stop its AI.
    /// </summary>
    public bool AIEnable { get; private set; }
    public bool CutscenePause
    {
        get => _cutscenePause;
        set
        {
            _cutscenePause = value;
            AIEnableUpdate();
        }
    }
    public bool MenuPause => GameManager.Instance.MenuPause;
    public Animator Animator => _animator;
    public IzumiTools.CappedValue Health => _health;
    public Vector3 ViewPosition => viewAnchor.position;
    public bool IsDead { get; private set; }
    public readonly List<UnitDamageCollider> damageColliders = new List<UnitDamageCollider>();

    private bool _cutscenePause;

    /// <summary>
    /// Called on initial stgage load after <see cref="GameManager.Instance"/> is loaded
    /// </summary>
    public void Init(bool isInitialInit)
    {
        Initializing = true;
        AIEnable = true;
        Internal_Init(isInitialInit);
        Initializing = false;
    }
    protected virtual void Internal_Init(bool isInitialInit)
    {
        damageColliders.AddRange(GetComponentsInChildren<UnitDamageCollider>());
        damageColliders.ForEach(collider => collider.Init(this));
        Health.Ratio = _initialHealthRatio;
        if (isInitialInit)
        {   
            IsDead = false;
            if (Health.Value <= 0)
            {
                Dead();
            }
        }
        else
        {
            IsDead = Health.Value <= 0;
        }
        _cutscenePause = false;
        AIEnableUpdate();
    }
    public void AIEnableUpdate()
    {
        bool newAIState = !CutscenePause && !MenuPause;
        if (AIEnable != newAIState || Initializing)
        {
            AIEnable = newAIState;
            OnAIEnableChange();
        }
    }
    protected virtual void OnAIEnableChange()
    {
        enabled = AIEnable;
    }

    public void SetModelFiringCD(float time)
    {
        if (time <= 0)
            time = 0.1F;
        if (_animator)
        {
            //TODO: add NPC shoot animation
            //_animator.SetFloat("FiringSpeedMultiplier", _firingAnimationClip.length / time);
        }
    }
    public virtual void Damage(DamageSource damageSource)
    {
        if (IsDead)
            return;
        Health.Value -= damageSource.damage;
        onDamage.Invoke(damageSource);
        if (Health.Empty)
        {
            Dead(); //TODO: immediately finish posing of dead body
        }
    }
    public virtual void Heal(float amount)
    {
        if (IsDead)
            return;
        Health.Value += amount;
        onHeal.Invoke(amount);
    }
    public virtual void Dead()
    {
        if (IsDead)
            return;
        IsDead = true;
        Health.Value = 0;
        onDead.Invoke();
    }
    public virtual void ListenSound(SoundSource soundSouce)
    {

    }
    public bool HasExposedDetectAnchor(Vector3 origin, out Vector3 detectPosition, Predicate<Collider> colliderPredicate = null)
    {
        foreach (Transform detectTarget in _detectTargets)
        {
            Vector3 _detectPosition = detectTarget.position;
            bool blocked = false;
            foreach (RaycastHit hit in Physics.RaycastAll(origin, _detectPosition - origin, Vector3.Distance(origin, _detectPosition)))
            {
                if (hit.collider.isTrigger)
                    continue;
                if (IsMyCollider(hit.collider))
                    continue;
                if (colliderPredicate != null && !colliderPredicate.Invoke(hit.collider))
                    continue;
                blocked = true;
                break;
            }
            if (!blocked)
            {
                detectPosition = _detectPosition;
                return true;
            }
        }
        detectPosition = Vector3.zero;
        return false;
    }
    public bool TryDetect(Vector3 raycastOrigin, Unit otherUnit, out Vector3 detectPosition)
    {
        return otherUnit.HasExposedDetectAnchor(raycastOrigin, out detectPosition, collider => !IsMyCollider(collider));
    }
    public virtual bool IsMyCollider(Collider collider)
    {
        return collider.transform.IsChildOf(transform);
    }
    protected struct UnitCommonSave
    {
        public float health;
        public Vector3 position;
        public Quaternion rotation;
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(new UnitCommonSave()
        {
            health = Health.Value,
            position = transform.position,
            rotation = transform.rotation
        });
    }

    public sealed override void Deserialize(string json)
    {
        Initializing = true;
        Internal_Deserialize(json);
        Internal_Init(false);
        Initializing = false;
    }
    protected virtual void Internal_Deserialize(string json)
    {
        var save = JsonUtility.FromJson<UnitCommonSave>(json);
        transform.SetPositionAndRotation(save.position, save.rotation);
        Health.Value = save.health;
    }
}
