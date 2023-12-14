using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Player : Unit
{
    [Header("Ability")]
    [SerializeField]
    float _respawnWaitTime;

    [Header("Accessiblity")]
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
    [SerializeField] AnimatorIKEventExposure _IKEventExposure;
    [SerializeField] PlayerMovement movement;
    [SerializeField] MouseLook mouseLook;
    [SerializeField] GunInventory gunInventory;

    [Header("SE")]
    [SerializeField]
    AudioClip _deathSE;

    public Camera Camera => MouseLook.Camera;
    public Camera HUDCamera => _hudCamera;
    public AnimatorIKEventExposure IKEventExposure => _IKEventExposure;
    public PlayerMovement Movement => movement;
    public MouseLook MouseLook => mouseLook;
    public GunInventory GunInventory => gunInventory;
    public bool HasAimRaycastHit { get; private set; }
    public Vector3 AimPosition { get; private set; }
    public Interactable AimingInteractable { get; private set; }
    public Vector3 FootPosition => transform.position + Vector3.down * movement.CharacterController.height / 2;
    private Vignette _bloodVignette;
    private float _respawnWaitedTime;
    private void Awake()
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
    public override bool IsMyCollider(Collider collider)
    {
        return collider.gameObject.Equals(Movement.CharacterController.gameObject);
    }
    public void OnHealthChange()
    {
        _bloodVignette.intensity.value = Mathf.Lerp(0, _bloodVignetteMaxIntencity, 1 - Health.Ratio);
    }
    public override void LoadInit()
    {
        base.LoadInit();
        GameManager.Instance.SetBlackout(false);
        Movement.enabled = true;
        MouseLook.enabled = true;
        OnHealthChange();
    }
    private void Update()
    {
        Animator.transform.localEulerAngles = Vector3.up * _aimModelYRotationFix;
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
        RaycastTargetUpdate();
        InteractUpdate();
    }
    public void RaycastTargetUpdate()
    {
        HasAimRaycastHit = false;
        Ray ray = new(Camera.transform.position, Camera.transform.forward);
        RicochetMirror latestReflectedMirror = null;
        bool rayHitPredicate(RaycastHit hitInfo)
        {
            if (hitInfo.collider.isTrigger)
            {
                RicochetMirror mirror = hitInfo.collider.GetComponentInParent<RicochetMirror>();
                return mirror != null && mirror != latestReflectedMirror;
            }
            else
            {
                return hitInfo.collider.GetComponentInParent<Player>() == null;
            }
        }
        Vector3 virtualAimPosition = ray.origin;
        List<Vector3> aimLinePositions = new()
        {
            Camera.transform.position
        };
        do
        {
            if (ClosestRaycastHit(ray, rayHitPredicate, out var hit))
            {
                aimLinePositions.Add(hit.point);
                virtualAimPosition += Camera.transform.forward * hit.distance;
                RicochetMirror mirror = hit.collider.GetComponentInParent<RicochetMirror>();
                if (mirror == null)
                {
                    HasAimRaycastHit = true;
                    AimPosition = virtualAimPosition;
                    GameManager.Instance.MinimapUI.SetAimLine(aimLinePositions.ToArray());
                    break;
                }
                else
                {
                    latestReflectedMirror = mirror;
                    ray.origin = hit.point;
                    ray.direction = Vector3.Reflect(ray.direction, hit.normal);
                }
            }
            else
            {
                HasAimRaycastHit = false;
                AimPosition = Camera.transform.position + Camera.transform.forward * 100;
                GameManager.Instance.MinimapUI.SetAimLine(aimLinePositions[0], AimPosition);
                break;
            }
        } while (true);
    }
    public void InteractUpdate()
    {
        Interactable closestInteractable = null;
        RaycastHit closestValidHit = new()
        {
            distance = float.MaxValue
        };
        foreach (var hit in Physics.RaycastAll(Camera.transform.position, Camera.transform.forward, _interactionDistance))
        {
            if (hit.distance > closestValidHit.distance)
                continue;
            if (hit.collider.TryGetComponent(out Interactable interactable) && interactable.ContainsActiveInteract)
            {
                closestInteractable = interactable;
                closestValidHit = hit;
            }
            if (!hit.collider.isTrigger)
            {
                closestValidHit = hit; // block interaction raycast
            }
        }
        if (closestInteractable != null && closestInteractable.gameObject.Equals(closestValidHit.collider.gameObject))
        {
            AimingInteractable = closestInteractable;
            GameManager.Instance.InteractIconViewer.SetActive(true);
            //GameManager.Instance.InteractIconViewer.transform.position = Camera.WorldToScreenPoint(closestValidHit.collider.bounds.center);
            GameManager.Instance.InteractIconViewer.transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
            GameManager.Instance.InteractIconViewer.GetComponentInChildren<Image>().sprite = AimingInteractable.InteractIcon;
            if (Input.GetKeyDown(KeyCode.E))
            {
                AimingInteractable.Interact();
            }
        }
        else
        {
            AimingInteractable = null;
            GameManager.Instance.InteractIconViewer.SetActive(false);
        }
    }
    private bool ClosestRaycastHit(Ray ray, Predicate<RaycastHit> predicate, out RaycastHit closestHit)
    {
        closestHit = new()
        {
            distance = float.MaxValue
        };
        foreach (var hitInfo in Physics.RaycastAll(ray))
        {
            if (!predicate.Invoke(hitInfo))
                continue;
            if (hitInfo.distance < closestHit.distance)
            {
                closestHit = hitInfo;
            }
        }
        return closestHit.collider != null;
    }
    public override void SetAIActive(bool cond)
    {
        base.SetAIActive(cond);
        enabled = cond;
        MouseLook.enabled = cond;
        Movement.enabled = cond;
        //Movement.Rigidbody.isKinematic = !cond;
        GunInventory.enabled = cond;
        if (GunInventory.Hands)
        {
            GunInventory.Hands.enabled = cond;
        }
        GetComponent<ProjectRicochetMirror>().enabled = cond;
    }
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
            equipmentSaves = gunInventory.equipments.ConvertAll(equipment => new PrefabCloneSave(equipment.saveProperty.prefabRoot)),
            holdingEquipmentIndex = GunInventory.HoldingEquipmentIndex,
            commonUnitSave = base.Serialize()
        });
        return json;
    }
    public override void Deserialize(string json)
    {
        PlayerSave save = JsonUtility.FromJson<PlayerSave>(json);
        base.Deserialize(save.commonUnitSave);
        List<SaveTargetPrefabRoot> reuseCandidates = gunInventory.equipments.ConvertAll(equpiment => equpiment.saveProperty.prefabRoot);
        gunInventory.equipments.Clear();
        foreach (var equipmentSave in save.equipmentSaves)
        {
            SaveTargetPrefabRoot equipmentPrefabRoot = equipmentSave.Deserialize(reuseCandidates, out bool reused);
            HandEquipment equipment = equipmentPrefabRoot.GetComponent<HandEquipment>();
            gunInventory.AddEquipment(equipment);
        }
        gunInventory.InitHoldingWeaponIndex(save.holdingEquipmentIndex);
        gunInventory.LoadInit();
        reuseCandidates.ForEach(candidate => Destroy(candidate.gameObject));
        MouseLook.LoadInit();
    }
}
