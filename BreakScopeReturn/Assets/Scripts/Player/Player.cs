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
    VolumeProfile _cameraVolumeProfile;
    [SerializeField]
    [Range(0f, 1f)]
    float _bloodVignetteMaxIntencity;
    [SerializeField]
    GameObject _interactionIconViewer;

    [Header("Internal Reference")]
    [SerializeField] Camera _hudCamera;
    [SerializeField] PlayerMovementScript movement;
    [SerializeField] MouseLookScript mouseLook;
    [SerializeField] GunInventory gunInventory;

    [Header("SE")]
    [SerializeField]
    AudioClip _deathSE;

    public Camera Camera => MouseLook.Camera;
    public Camera HUDCamera => _hudCamera;
    public PlayerMovementScript Movement => movement;
    public MouseLookScript MouseLook => mouseLook;
    public GunInventory GunInventory => gunInventory;
    public bool HasRaycastPosition { get; private set; }
    public Vector3 RaycastPosition { get; private set; }
    public Interactable RaycastingInteractable { get; private set; }
    private Vignette _bloodVignette;
    private float _respawnWaitedTime;
    private void Awake()
    {
        _bloodVignette = (Vignette)_cameraVolumeProfile.components.Find(component => component.GetType() == typeof(Vignette));
    }
    protected override void Start()
    {
        base.Start();
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
    public void OnHealthChange()
    {
        _bloodVignette.intensity.value = Mathf.Lerp(0, _bloodVignetteMaxIntencity, 1 - Health.Ratio);
    }
    public override void Init()
    {
        base.Init();
        GameManager.Instance.SetBlackout(false);
        Movement.enabled = true;
        MouseLook.enabled = true;
        if (_bloodVignette)
            _bloodVignette.intensity.value = 0;
    }
    private void Update()
    {
        if (IsDead)
        {
            _respawnWaitedTime += Time.deltaTime;
            if (_respawnWaitedTime > _respawnWaitTime)
            {
                GameManager.Instance.RespawnPlayer();
            }
            return;
        }
        RaycastTargetUpdate();
        InteractUpdate();
    }
    public void RaycastTargetUpdate()
    {
        HasRaycastPosition = false;
        Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);
        RicochetMirror latestReflectedMirror = null;
        Predicate<RaycastHit> rayHitPredicate = hitInfo =>
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
        };
        Vector3 virtualAimPosition = ray.origin;
        do
        {
            if (ClosestRaycastHit(ray, rayHitPredicate, out var hit))
            {
                virtualAimPosition += Camera.transform.forward * hit.distance;
                RicochetMirror mirror = hit.collider.GetComponentInParent<RicochetMirror>();
                if (mirror == null)
                {
                    HasRaycastPosition = true;
                    RaycastPosition = virtualAimPosition;
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
                HasRaycastPosition = false;
                RaycastPosition = Camera.transform.position + Camera.transform.forward * 100;
                break;
            }
        } while (true);
    }
    public void InteractUpdate()
    {
        Interactable closestInteractable = null;
        RaycastHit closestValidHit = new();
        closestValidHit.distance = float.MaxValue;
        foreach(var hit in Physics.RaycastAll(Camera.transform.position, Camera.transform.forward, _interactionDistance))
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
            RaycastingInteractable = closestInteractable;
            GameManager.Instance.InteractIconViewer.SetActive(true);
            //GameManager.Instance.InteractIconViewer.transform.position = Camera.WorldToScreenPoint(closestValidHit.collider.bounds.center);
            GameManager.Instance.InteractIconViewer.transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
            GameManager.Instance.InteractIconViewer.GetComponentInChildren<Image>().sprite = RaycastingInteractable.InteractIcon;
            if (Input.GetKeyDown(KeyCode.E))
            {
                RaycastingInteractable.Interact();
            }
        }
        else
        {
            RaycastingInteractable = null;
            GameManager.Instance.InteractIconViewer.SetActive(false);
        }
    }
    private bool ClosestRaycastHit(Ray ray, Predicate<RaycastHit> predicate, out RaycastHit closestHit)
    {
        closestHit = new();
        closestHit.distance = float.MaxValue;
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
}
