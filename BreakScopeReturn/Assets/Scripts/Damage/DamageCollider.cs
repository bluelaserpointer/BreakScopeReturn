using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class DamageCollider : MonoBehaviour, IHitSound
{
    public IDamageReceiver damageReceiver;
    public bool ignore;
    [Header("VFX")]
    public GameObject hitVfxPrefab;
    public bool useBulletHitDefaultVFX;
    [Header("SFX")]
    public SoundSetSO hitSoundSet;
    [Header("Damage")]
    public float damageRatio = 1;
    public UnityEvent<Bullet> onBulletHit;

    public SoundSetSO HitSoundSet => hitSoundSet;

    public void BulletHit(Bullet bullet, RaycastHit hitInfo)
    {
        onBulletHit.Invoke(bullet);
        if (useBulletHitDefaultVFX)
        {
            GameObject vfx = Instantiate(bullet._destoryVFXPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            vfx.transform.SetParent(transform);
        }
        if (hitVfxPrefab)
        {
            GameObject vfx = Instantiate(hitVfxPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            vfx.transform.SetParent(transform);
        }
    }
    [ContextMenu(nameof(LinkReceiverInParent))]
    public void LinkReceiverInParent()
    {
        damageReceiver = GetComponentInParent<IDamageReceiver>();
    }
}
