using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    public float explosionTimeLeft;
    public float explosionDamage;
    public float explosionDistance;
    public float explosionSoundDistance;

    [SerializeField]
    GameObject _explosionEffect;
    [SerializeField]
    AudioClip _explosionSound;

    public bool isGhost;
    public Rigidbody Rigidbody { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if((explosionTimeLeft -= Time.deltaTime) < 0)
        {
            if (!isGhost)
                Explode();
            else
                Destroy(gameObject);
        }
    }
    public void Explode()
    {
        foreach (Unit unit in new List<Unit>(GameManager.Instance.CurrentStage.aliveUnits))
        {
            float distance = Vector3.Distance(unit.transform.position, transform.position);

            if (distance > explosionDistance)
                continue;
            if (unit.RaycastableFrom(transform.position, out Vector3 raycastablePosition))
            {
                DamageSource damageSource = new DamageSource.ExplosionDamage(this);
                float distanceRatio = 1 - distance / explosionDistance;
                damageSource.damage = explosionDamage * distanceRatio * distanceRatio;
                unit.Damage(damageSource);
            }
        }
        Destroy(gameObject);
        if (_explosionEffect)
        {
            Instantiate(_explosionEffect, GameManager.Instance.CurrentStage.transform).transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
        if (_explosionSound)
        {
            AudioSource.PlayClipAtPoint(_explosionSound, transform.position);
        }
    }
}