using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ReloadAnimationType { NoCocking, LeftCocking }
[DisallowMultipleComponent]
public class Gun : HandEquipment
{
    [Header("Debug")]
    [SerializeField]
    bool _realtimeIK;

    [Header("Info")]
    [SerializeField]
    string _displayName;
    [SerializeField]
    Sprite _icon;

    [Header("Dynamic data")]
    public IzumiTools.CappedValue magazine;
    public int spareAmmo;

    [Header("Spec")]
    public GunSpec spec;

    [Header("Animation")]
    [SerializeField]
    Transform _leftHandGoal, _rightHandGoal;
    [SerializeField]
    ReloadAnimationType _reloadAnimationType;
    [SerializeField]
    AnimationClip _reloadAnimationClip;

    [Header("Positioning")]
    [SerializeField]
    Transform _muzzleAnchor;
    [SerializeField]
    Transform _armCameraAnchor;
    [SerializeField]
    Transform _aimCameraAnchor;
    [SerializeField]
    Transform _magazineRoot;

    [Header("Sound")]
    [SerializeField]
    AudioSource _shootSESource;
    [SerializeField]
    AudioSource _reloadSESource;

    [Header("VFX")]
    public GameObject[] muzzleFlashes;

    [Header("Event")]
    public UnityEvent onFire;
    public UnityEvent<float> onFireCDSet;

    [ContextMenu("Pose Gun At Arm")]
    private void PoseGunAtArm()
    {
        Player player = GetComponentInParent<Player>();
        if (player == null)
        {
            print("Cannot find " + nameof(Player) + " in parent");
            return;
        }
        new TransformRelator(transform, ArmCameraAnchor).ApplyChildTransform(transform, player.Camera.transform);
    }
    [ContextMenu("Pose Gun At Aim")]
    private void PoseGunAtAim()
    {
        Player player = GetComponentInParent<Player>();
        if (player == null)
        {
            print("Cannot find " + nameof(Player) + " in parent");
            return;
        }
        new TransformRelator(transform, AimCameraAnchor).ApplyChildTransform(transform, player.Camera.transform);
    }
    public string DisplayName => _displayName;
    public Sprite Icon => _icon;
    public Unit Owner { get; private set; }
    public Transform MuzzleAnchor => _muzzleAnchor;
    public Transform ArmCameraAnchor => _armCameraAnchor;
    public Transform AimCameraAnchor => _aimCameraAnchor;
    public Transform LeftHandGoal => _leftHandGoal;
    public Transform RightHandGoal => _rightHandGoal;
    public Transform MagazineRoot => _magazineRoot;
    public int ReloadAnimationID => (int)_reloadAnimationType;
    public float ReloadAnimationClipLength => _reloadAnimationClip.length;
    public AudioSource ShootSESource => _shootSESource;
    public AudioSource ReloadSESource => _reloadSESource;
    public TransformRelator CentreRelRightHand { get; private set; }
    public TransformRelator CentreRelArmCamera { get; private set; }
    public TransformRelator CentreRelAimCamera { get; private set; }

    private void Awake()
    {
        magazine.Fill();
        UpdateAnchorRelation();
    }
    private void Update()
    {
        if (_realtimeIK)
            UpdateAnchorRelation();
    }
    void UpdateAnchorRelation()
    {
        CentreRelRightHand = new TransformRelator(transform, RightHandGoal);
        CentreRelArmCamera = new TransformRelator(transform, ArmCameraAnchor);
        CentreRelAimCamera = new TransformRelator(transform, AimCameraAnchor);
    }
    struct GunSave
    {
        public int magazineSize;
        public int magazineAmmo;
        public int spareAmmo;
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(new GunSave()
        {
            magazineSize = (int)magazine.Capacity,
            magazineAmmo = (int)magazine.Value,
            spareAmmo = spareAmmo
        });
    }
    public override void Deserialize(string json)
    {
        GunSave save = JsonUtility.FromJson<GunSave>(json);
        magazine.Capacity = save.magazineSize;
        magazine.Value = save.magazineAmmo;
        spareAmmo = save.spareAmmo;
    }
}
