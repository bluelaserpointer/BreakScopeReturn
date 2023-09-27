using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Unit : MonoBehaviour
{
    [Header("Unit Common Stats")]
    public float initialHealth;
    [SerializeField] protected Transform viewAnchor;
    [SerializeField] List<Transform> detectAnchors;
    [SerializeField] RagdollRelax _ragdollRelax;
    [SerializeField] protected Animator _animator;
    [SerializeField] AnimationClip _firingAnimationClip;

    public IzumiTools.CappedValue Health { get; private set; }
    public Vector3 ViewPosition => viewAnchor.position;
    public bool IsDead { get; private set; }
    public readonly List<UnitDamageCollider> damageColliders = new List<UnitDamageCollider>();

    public UnityEvent<DamageSource> onDamage = new();
    public UnityEvent<float> onHeal = new();
    public UnityEvent onDead = new();

    protected virtual void Start()
    {
        damageColliders.AddRange(GetComponentsInChildren<UnitDamageCollider>());
        damageColliders.ForEach(collider => collider.Init(this));
        Init();
    }
    public virtual void Init()
    {
        Health = new IzumiTools.CappedValue(initialHealth);
        Health.Maximize();
        IsDead = false;
    }
    public void SetModelFiringCD(float time)
    {
        if (time <= 0)
            time = 0.1F;
        if (_animator)
        {
            _animator.SetFloat("FiringSpeedMultiplier", _firingAnimationClip.length / time);
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
            Dead();
        }
    }
    public virtual void Heal(float amount)
    {
        //TODO: dead condition
        Health.Value += amount;
        onHeal.Invoke(amount);
    }
    public virtual void Dead()
    {
        if (IsDead)
            return;
        IsDead = true;
        if(_ragdollRelax)
            _ragdollRelax.relax = true;
        onDead.Invoke();
    }
    public virtual void Hear(SoundSource soundSouce)
    {

    }
    public bool RaycastableFrom(Vector3 raycastOrigin, out Vector3 raycastablePosition)
    {
        foreach (Transform detectAnchor in detectAnchors)
        {
            Vector3 detectPosition = detectAnchor.position;
            bool blocked = false;
            foreach (RaycastHit hit in Physics.RaycastAll(raycastOrigin, detectPosition - raycastOrigin, Vector3.Distance(raycastOrigin, detectPosition)))
            {
                if (hit.collider.isTrigger)
                    continue;
                if (IsMyCollider(hit.collider))
                    continue;
                blocked = true;
                break;
            }
            if (!blocked)
            {
                raycastablePosition = detectPosition;
                return true;
            }
        }
        raycastablePosition = Vector3.zero;
        return false;
    }
    public bool RaycastableTo(Vector3 raycastOrigin, Unit otherUnit, out Vector3 raycastablePosition)
    {
        foreach (Transform detectAnchor in otherUnit.detectAnchors)
        {
            Vector3 detectPosition = detectAnchor.position;
            bool blocked = false;
            foreach (RaycastHit hit in Physics.RaycastAll(raycastOrigin, detectPosition - raycastOrigin, Vector3.Distance(raycastOrigin, detectPosition)))
            {
                if (hit.collider.isTrigger)
                    continue;
                if (hit.collider.TryGetComponent(out UnitDamageCollider damageCollider)
                    && (damageCollider.Unit == this || damageCollider.Unit == otherUnit))
                    continue;
                blocked = true;
                break;
            }
            if (!blocked)
            {
                raycastablePosition = detectPosition;
                return true;
            }
        }
        raycastablePosition = Vector3.zero;
        return false;
    }
    public bool IsMyCollider(GameObject colliderObject)
    {
        return colliderObject.TryGetComponent(out UnitDamageCollider damageCollider) && damageCollider.Unit == this;
    }
    public bool IsMyCollider(Collider collider)
    {
        return IsMyCollider(collider.gameObject);
    }
}
