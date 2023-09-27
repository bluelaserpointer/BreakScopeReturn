using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class DirectionIndicator : MonoBehaviour
{
    [SerializeField]
    HitDiractionIndicatorPiece hitDirectionIndicatorPrefab;

    Player Player => GameManager.Instance.Player;
    public void SetHitDirection(DamageSource.BulletDamage bulletDamage)
    {
        Vector3 attackDirection = -bulletDamage.Bullet.transform.forward;
        HitDiractionIndicatorPiece hitIndicator = Instantiate(hitDirectionIndicatorPrefab, transform);
        hitIndicator.gameObject.SetActive(true);
        hitIndicator.transform.localEulerAngles = Vector3.forward * Vector3.SignedAngle(attackDirection, Vector3.forward, Vector3.up);
    }
    private void Update()
    {
        transform.eulerAngles = Vector3.forward * Player.Camera.transform.eulerAngles.y;
    }
}
