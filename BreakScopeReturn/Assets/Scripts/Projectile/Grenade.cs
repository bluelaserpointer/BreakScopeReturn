using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Grenade : MonoBehaviour
{
    public float explosionTimeLeft;
    public float explosionDamage;
    public float explosionDistance;
    public float explosionSoundDistance;

    [SerializeField]
    SphereCollider _sphereCollider;
    [SerializeField]
    GameObject _explosionEffect;
    [SerializeField]
    AudioClip _explosionSound;

    [HideInInspector]
    public Vector3 velocity;
    public SphereCollider SphereCollider => _sphereCollider;

    private void Update()
    {
        if((explosionTimeLeft -= Time.deltaTime) < 0)
        {
            Explode();
        }
    }
    private void FixedUpdate()
    {
        Vector3 position = transform.position;
        ProjectileUpdate(this, ref position, ref velocity, SphereCollider.radius, SphereCollider.sharedMaterial);
        transform.position = position;
    }
    public static void ProjectileUpdate(Grenade grenade, ref Vector3 position, ref Vector3 velocity, float colliderRadius, PhysicMaterial physicMaterial = null)
    {
        Vector3 oldPosition = position;
        position += velocity * Time.fixedDeltaTime;
        velocity += Physics.gravity * Time.fixedDeltaTime;
        foreach (var hitInfo in Physics.SphereCastAll(oldPosition, colliderRadius, velocity, velocity.magnitude * Time.fixedDeltaTime))
        {
            if (hitInfo.collider.isTrigger)
                continue;
            if (hitInfo.collider.transform.IsChildOf(grenade.transform))
                continue;
            if (hitInfo.point == Vector3.zero && hitInfo.distance == 0)
            {
                if (Vector3.Dot(hitInfo.point - position, velocity) < 0)
                    continue; //ignore backward overlap
                position = oldPosition;
            }
            else
            {
                position = oldPosition;
                //position = hitInfo.point;
            }
            velocity = Vector3.Reflect(velocity, hitInfo.normal) * (physicMaterial ? physicMaterial.bounciness : 1);
            position = oldPosition + velocity * Time.fixedDeltaTime;
            break;
        }
    }
    public void Explode()
    {
        foreach (Unit unit in new List<Unit>(GameManager.Instance.Stage.AliveNpcUnits))
        {
            float distance = Vector3.Distance(unit.transform.position, transform.position);

            if (distance > explosionDistance)
                continue;
            if (unit.HasExposedDetectAnchor(transform.position, out _))
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
            Instantiate(_explosionEffect, GameManager.Instance.Stage.transform).transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
        if (_explosionSound)
        {
            AudioSource.PlayClipAtPoint(_explosionSound, transform.position);
        }
    }
}