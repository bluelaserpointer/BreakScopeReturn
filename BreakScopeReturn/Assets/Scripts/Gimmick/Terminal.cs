using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Terminal : SaveTarget
{
    [SerializeField]
    MeshRenderer _modelMainRenderer;
    [SerializeField]
    Material _enabledMaterial, _disabledMaterial;
    [SerializeField]
    GameObject _destroyedGroup;
    [SerializeField]
    CappedValue _hitPoint;
    [SerializeField]
    UnityEvent _onDestroy;

    public CappedValue HitPoint => _hitPoint;
    public UnityEvent OnDestroy => _onDestroy;
    public bool Destroyed { get; private set; }

    private void Awake()
    {
        _hitPoint.Fill();
        foreach (var hitbox in GetComponentsInChildren<DamageCollider>())
        {
            hitbox.onBulletHit.AddListener(bullet => Damage(bullet.damage * hitbox.damageRatio));
            //TODO: make universal damage source to abstract melee, explosion damages
        }
        UpdateVisual();
    }
    public void Damage(float damage)
    {
        if (Destroyed)
            return;
        _hitPoint.Value -= damage;
        if (_hitPoint.Empty)
        {
            Destroy();
        }
    }
    public void Destroy()
    {
        if (Destroyed)
            return;
        Destroyed = true;
        UpdateVisual();
        _onDestroy.Invoke();
    }
    private void UpdateVisual()
    {
        _modelMainRenderer.material = Destroyed ? _disabledMaterial : _enabledMaterial;
        _destroyedGroup.gameObject.SetActive(Destroyed);
    }

    public override string Serialize()
    {
        return _hitPoint.Value.ToString();
    }

    public override void Deserialize(string data)
    {
        _hitPoint.Value = float.Parse(data);
        Destroyed = _hitPoint.Empty;
        UpdateVisual();
    }
}
