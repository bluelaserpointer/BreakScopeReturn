using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public class Player : Unit
{
    [Header("Ability")]
    [SerializeField]
    Transform _abilityContainer;
    [SerializeField]
    ProjectRicochetMirror _projectRicochetMirror;

    [Header("Accessiblity")]
    [SerializeField]
    float _respawnWaitTime;
    [SerializeField]
    float _respawnStealthTime;
    [SerializeField]
    float _interactionDistance;

    [Header("Graphic")]
    [SerializeField]
    Transform _cameraPositionTarget;
    [SerializeField]
    float _aimModelYRotationFix;
    [SerializeField]
    VolumeProfile _cameraVolumeProfile;
    [SerializeField]
    [Range(0f, 1f)]
    float _bloodVignetteMaxIntencity;

    [Header("Internal Reference")]
    [SerializeField] Camera _hudCamera;
    [SerializeField] EquipmentSidePreview _equipmentSidePreview;
    [SerializeField] AnimatorIKEventExposure _IKEventExposure;
    [SerializeField] PlayerMovement _movement;
    [SerializeField] MouseLook _mouseLook;
    [SerializeField] GunInventory _gunInventory;
    [SerializeField] Transform _gunEyeAnchor;
    [SerializeField] Transform _gunEyeNoZRotAnchor;

    [Header("SE")]
    [SerializeField]
    AudioClip _deathSE;

    public Camera Camera => MouseLook.Camera;
    public Camera HUDCamera => _hudCamera;
    public ProjectRicochetMirror ProjectRicochetMirror => _projectRicochetMirror;
    public EquipmentSidePreview EquipmentSidePreview => _equipmentSidePreview;
    public AnimatorIKEventExposure IKEventExposure => _IKEventExposure;
    public PlayerMovement Movement => _movement;
    public MouseLook MouseLook => _mouseLook;
    public GunInventory GunInventory => _gunInventory;
    public Transform GunEyeAnchor => _gunEyeAnchor;
    public Transform GunEyeNoZRotAnchor => _gunEyeNoZRotAnchor;
    public Collider AimCollider { get; private set; }
    public Vector3 AimPosition { get; private set; }
    public float AimDistance { get; private set; }
    public IInteractable AimInteractable { get; private set; }
    public Vector3 FootPosition => transform.position;
    public Transform AbilityContainer => _abilityContainer;
    private Vignette _bloodVignette;
    private float _respawnWaitedTime;
    private float _remainRespawnStealthTime;
    protected override void Internal_Init(bool isInitialInit)
    {
        stealth = true;
        _remainRespawnStealthTime = _respawnStealthTime;
        if (isInitialInit)
        {
            _bloodVignette = (Vignette)_cameraVolumeProfile.components.Find(component => component.GetType() == typeof(Vignette));
            onDamage.AddListener(damageSource => {
                OnHealthChange();
                if (damageSource.GetType() == typeof(DamageSource.BulletDamage))
                    GameManager.Instance.DirectionIndicator.SetHitDirection((DamageSource.BulletDamage)damageSource);
            });
            onHeal.AddListener(healAmount => OnHealthChange());
            onDead.AddListener(() =>
            {
                GameManager.Instance.SetBlackout(true);
                Movement.enabled = false;
                MouseLook.enabled = false;
                _respawnWaitedTime = 0;
                AudioSource.PlayClipAtPoint(_deathSE, Camera.transform.position);
            });
        }
        base.Internal_Init(isInitialInit);
        if (isInitialInit)
            GunInventory.InitialInit();
        OnHealthChange();
        GameManager.Instance.SetBlackout(false); //TODO: dont do this in player script
        gameObject.SetActive(true);
    }
    private void Update()
    {
        if (GameManager.Instance.PlayerAlwaysStealth)
        {
            stealth = true;
        }
        else if (_remainRespawnStealthTime > 0)
        {
            if ((_remainRespawnStealthTime -= Time.deltaTime) <= 0)
            {
                stealth = false;
            }
        }
        Animator.transform.localEulerAngles = Vector3.up * _aimModelYRotationFix;
        _gunEyeNoZRotAnchor.SetPositionAndRotation(_gunEyeAnchor.position, Camera.transform.rotation);
        Camera.transform.position = _cameraPositionTarget.position;
        if (IsDead)
        {
            _respawnWaitedTime += Time.deltaTime;
            if (_respawnWaitedTime > _respawnWaitTime)
            {
                GameManager.Instance.LoadStage();
            }
            return;
        }
        AimPositionUpdate();
        InteractUpdate();
    }
    public override bool IsMyCollider(Collider collider)
    {
        return collider.gameObject.Equals(Movement.CharacterController.gameObject);
    }
    public void OnHealthChange()
    {
        _bloodVignette.intensity.value = Mathf.Lerp(0, _bloodVignetteMaxIntencity, 1 - Health.Ratio);
    }
    private void AimPositionUpdate()
    {
        Ray ray = new(Camera.transform.position, Camera.transform.forward);
        Collider latestReflectedMirrorCollider = null;
        bool rayHitFilter(RaycastHit hitInfo, out bool isMirror)
        {
            isMirror = false;
            if (hitInfo.collider == latestReflectedMirrorCollider)
                return false;
            if (RicochetMirror.IsRicochetMirrorCollider(hitInfo.collider))
            {
                isMirror = true;
                return true;
            }
            if (hitInfo.collider.isTrigger)
                return false;
            return !IsMyCollider(hitInfo.collider);
        }
        Vector3 firstRayOrigin = ray.origin;
        float totalRayDistance = 0;
        List<Vector3> aimLinePositions = new() { Camera.transform.position };
        do
        {
            RaycastHit closestHit = new() { distance = float.MaxValue };
            bool isMirror = false;
            foreach (var hitInfo in Physics.RaycastAll(ray))
            {
                if (hitInfo.distance > closestHit.distance)
                    continue;
                if (!rayHitFilter(hitInfo, out bool _isMirror))
                    continue;
                closestHit = hitInfo;
                isMirror = _isMirror;
            }
            if (closestHit.collider == null)
            {
                AimCollider = null;
                aimLinePositions.Add(ray.origin + ray.direction * 100);
                totalRayDistance += 100;
                break;
            }
            aimLinePositions.Add(closestHit.point);
            totalRayDistance += closestHit.distance;
            if (!isMirror)
            {
                AimCollider = closestHit.collider;
                break;
            }
            latestReflectedMirrorCollider = closestHit.collider;
            ray.origin = closestHit.point;
            ray.direction = Vector3.Reflect(ray.direction, closestHit.normal);
        } while (true);
        AimDistance = totalRayDistance;
        AimPosition = firstRayOrigin + Camera.transform.forward * AimDistance;
        GameManager.Instance.MinimapUI.SetAimLine(aimLinePositions.ToArray());
    }
    private void InteractUpdate()
    {
        if (AimDistance < _interactionDistance && AimCollider.TryGetComponent(out IInteractable interactable))
        {
            AimInteractable = interactable;
            GameManager.Instance.InteractUI.SetInfo(interactable);
            if (Input.GetKeyDown(KeyCode.E))
            {
                AimInteractable.Interact();
            }
            GameManager.Instance.InteractUI.gameObject.SetActive(true);
        }
        else
        {
            AimInteractable = null;
            GameManager.Instance.InteractUI.gameObject.SetActive(false);
        }
    }
    protected override void OnAIEnableChange()
    {
        Cursor.lockState = AIEnable ? CursorLockMode.Locked : CursorLockMode.None;
        MouseLook.enabled = Movement.enabled = AIEnable;
        GunInventory.enabled = AIEnable;
        if (GunInventory.Hands)
        {
            GunInventory.Hands.enabled = AIEnable;
        }
    }
    /*
    public void OrderAimAction(Transform aimTarget)
    {
        Vector3 lookRotationEuler = Quaternion.LookRotation(aimTarget.position - MouseLook.transform.position).eulerAngles;
        GameManager.Instance.Player.transform.rotation = Quaternion.Euler(0, lookRotationEuler.y, 0);
        MouseLook.transform.localRotation = Quaternion.Euler(lookRotationEuler.x, 0, 0);
    }
    public void OrderFireAction()
    {
        ((PlayerGunHands)GunInventory.Hands).Trigger();
    }
    */
    struct PlayerSave
    {
        public List<PrefabCloneSave> equipmentSaves;
        public int holdingEquipmentIndex;
        public string commonUnitSave;
    }
    public override string Serialize()
    {
        string json = JsonUtility.ToJson(new PlayerSave()
        {
            equipmentSaves = _gunInventory.equipments.ConvertAll(equipment => new PrefabCloneSave(equipment.saveProperty.prefabRoot)),
            holdingEquipmentIndex = GunInventory.HoldingEquipmentIndex,
            commonUnitSave = base.Serialize()
        });
        return json;
    }
    protected override void Internal_Deserialize(string json)
    {
        PlayerSave save = JsonUtility.FromJson<PlayerSave>(json);
        base.Internal_Deserialize(save.commonUnitSave);
        List<SaveTargetPrefabRoot> reuseCandidates = _gunInventory.equipments.ConvertAll(equpiment => equpiment.saveProperty.prefabRoot);
        _gunInventory.equipments.Clear();
        foreach (var equipmentSave in save.equipmentSaves)
        {
            SaveTargetPrefabRoot equipmentPrefabRoot = equipmentSave.Deserialize(reuseCandidates, out bool reused);
            HandEquipment equipment = equipmentPrefabRoot.GetComponent<HandEquipment>();
            _gunInventory.AddEquipment(equipment);
        }
        _gunInventory.InitHoldingWeaponIndex(save.holdingEquipmentIndex);
        _gunInventory.LoadInit();
        reuseCandidates.ForEach(candidate => Destroy(candidate.gameObject));
        MouseLook.LoadInit();
    }
}
