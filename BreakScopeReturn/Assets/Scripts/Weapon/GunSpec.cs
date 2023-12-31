using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GunSpec
{
    public enum FireMode
    {
        Nonautomatic, Automatic
    }
    [Header("Aiming")]
    public float aimTime;
    public float cameraZoomRatio_notAiming;
    public float cameraZoomRatio_aiming;
    public float mouseSensitivity_aiming;

    [Header("Firing")]
    public FireMode fireMode;
    public Bullet bulletPrefab;
    public float fireCD;
    public float damage;
    public float bulletSpeed;
    public float accuracy;

    [Header("Recoil")]
    public Vector3 recoilAmount_aiming;
    public float recoilPenaltyNotAiming;
    public float recoilOverTime;
    public float recoilRotateRatio;

    [Header("Reload")]
    public float reloadCD;
}
