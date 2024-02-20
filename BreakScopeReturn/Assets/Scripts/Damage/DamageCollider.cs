using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class DamageCollider : MonoBehaviour
{
    public IDamageReceiver damageReceiver;
    public bool ignore;
    public GameObject hitVfxPrefab;
    public bool useBulletHitDefaultVFX;
    public float damageRatio = 1;
    public UnityEvent<Bullet> onBulletHit;

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
