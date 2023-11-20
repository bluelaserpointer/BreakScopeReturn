using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Gun : HandEquipment
{
    [Header("Info")]
    [SerializeField]
    string _displayName;
    [SerializeField]
    Sprite _icon;

    [Header("IK")]
    [SerializeField]
    bool _relatimeIK;
    [SerializeField]
    Transform _leftHandAnchor, _rightHandAnchor;

    [Header("Spec")]
    public GunSpec spec;

    [Header("Dynamic data")]
    public IzumiTools.CappedValue magazine;
    public int spareAmmo;

    [Header("Handling")]
    [SerializeField]
    PlayerGunHands _playerGunHandsPrefab;
    [SerializeField]
    AnimationCurve _reloadLeftHandIKWeightCurve;
    [SerializeField]
    Transform _muzzleAnchor;
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

    public string DisplayName => _displayName;
    public Sprite Icon => _icon;
    public Unit Owner { get; private set; }
    public Transform MuzzleAnchor => _muzzleAnchor;
    public Transform LeftHandAnchor => _leftHandAnchor;
    public Transform RightHandAnchor => _rightHandAnchor;
    public Transform AimCameraAnchor => _aimCameraAnchor;
    public Transform MagazineRoot => _magazineRoot;
    public AnimationCurve ReloadLeftHandIKWeightCurve => _reloadLeftHandIKWeightCurve;
    public AudioSource ShootSESource => _shootSESource;
    public AudioSource ReloadSESource => _reloadSESource;
    public TransformRelator CentreRelRightHand { get; private set; }
    public TransformRelator CentreRelAimCamera { get; private set; }

    private void Awake()
    {
        magazine.Maximize();
        UpdateAnchorRelation();
    }
    private void Update()
    {
        if (_relatimeIK)
            UpdateAnchorRelation();
    }
    void UpdateAnchorRelation()
    {
        CentreRelRightHand = new TransformRelator(transform, RightHandAnchor);
        CentreRelAimCamera = new TransformRelator(transform, AimCameraAnchor);
    }
    public void SetCentreByRightHand(Transform currentRightHandTransform)
    {
        CentreRelRightHand.ApplyChildTransform(transform, currentRightHandTransform);
    }
    public void SetCentreByAimCamera(Transform currentCameraTransform)
    {
        CentreRelAimCamera.ApplyChildTransform(transform, currentCameraTransform);
    }
    public override PlayerHands GeneratePlayerHands()
    {
        PlayerGunHands hands = Instantiate(_playerGunHandsPrefab);
        hands.Init(this);
        return hands;
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
            magazineSize = (int)magazine.Max,
            magazineAmmo = (int)magazine.Value,
            spareAmmo = spareAmmo
        });
    }
    public override void Deserialize(string json)
    {
        GunSave save = JsonUtility.FromJson<GunSave>(json);
        magazine.Max = save.magazineSize;
        magazine.Value = save.magazineAmmo;
        spareAmmo = save.spareAmmo;
    }
}
