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
    [SerializeField] bool _disableAIOnAwake;
    [SerializeField] IzumiTools.CappedValue _health;
    [Range(0f, 1f)]
    [SerializeField] float _initialHealthRatio = 1;
    public bool stealth;
    [SerializeField] protected Transform viewAnchor;
    [SerializeField] List<Transform> _detectTargets;
    [SerializeField] protected Animator _animator;
    [SerializeField] AnimationClip _firingAnimationClip;

    public Animator Animator => _animator;
    public IzumiTools.CappedValue Health => _health;
    public Vector3 ViewPosition => viewAnchor.position;
    public bool IsDead { get; private set; }
    public readonly List<UnitDamageCollider> damageColliders = new List<UnitDamageCollider>();

    public UnityEvent<DamageSource> onDamage = new();
    public UnityEvent<float> onHeal = new();
    public UnityEvent onDead = new();

    /// <summary>
    /// Called on initial stgage load after <see cref="GameManager.Instance"/> is loaded
    /// </summary>
    public virtual void InitialInit()
    {
        damageColliders.AddRange(GetComponentsInChildren<UnitDamageCollider>());
        damageColliders.ForEach(collider => collider.Init(this));
        Health.Ratio = _initialHealthRatio;
        IsDead = false;
        if (Health.Value <= 0)
        {
            Dead();
        }
        if (_disableAIOnAwake)
            SetEnableAI(false);
        LoadInit();
    }
    /// <summary>
    /// Called on initial stage load and reuse
    /// </summary>
    public virtual void LoadInit()
    {
        IsDead = Health.Value <= 0;
    }
    public void SetModelFiringCD(float time)
    {
        if (time <= 0)
            time = 0.1F;
        if (_animator)
        {
            //_animator.SetFloat("FiringSpeedMultiplier", _firingAnimationClip.length / time);
        }
    }
    public virtual void Damage(DamageSource damageSource)
    {
        if (IsDead)
            return;
        Health.Value -= damageSource.damage;
        onDamage.Invoke(damageSource);
        if (Health.Value == 0)
        {
            Dead();  //TODO: immediately finish posing of dead body
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
    public virtual void SetEnableAI(bool cond)
    {
        enabled = false;
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

    public override void Deserialize(string json)
    {
        var save = JsonUtility.FromJson<UnitCommonSave>(json);
        transform.SetPositionAndRotation(save.position, save.rotation);
        Health.Value = save.health;
        LoadInit();
    }
}
