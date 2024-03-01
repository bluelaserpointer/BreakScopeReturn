using UnityEngine;
using System.Collections;
using IzumiTools;
using UnityEngine.UI;

public class PlayerGunHands : PlayerHands
{
    [Header("Crosshair")]
    [SerializeField]
    Transform _crosshairContainer;
    [SerializeField]
    CompositeCrosshair _crosshair;
    [SerializeField]
	CanvasGroup _scopeCanvasGroup;
    [SerializeField]
    Image _magazineGaugeFill;

    [Header("Bash")]
    [SerializeField]
    float _bashDamage;
    [SerializeField]
    float _bashActionOccupyTime;

    [Header("Rotation")]
    public float rotationLagTime = 0f;
    public Vector2 forwardRotationAmount = Vector2.one;

    public override HandsType HandsType => HandsType.Rifle;
    public Gun Gun { get; private set; }
	public GunSpec GunSpec => Gun.spec;
	[HideInInspector]
	public SmoothDampTransition aimTransition;
    [HideInInspector] public Vector3 velocity_recoil;
    public Cooldown FireCD { get; private set; }
    public Cooldown ReloadCD { get; private set; }
    public bool IsReloading { get; private set; }
    public bool IsBashing => BashActionOccupyRemainTime > 0;
    public float BashActionOccupyRemainTime { get; private set; }
    public float RecoilPenalty { get; private set; }
    public Vector3 BulletSpawnPosition => Gun.MuzzleAnchor.position;
    private Player Player => GameManager.Instance.Player;
    private Camera MainCamera => Player.Camera;
    private PlayerMovement PlayerMovement => Player.Movement;
    private MouseLook PlayerLook => Player.MouseLook;

    private Vector3 currentRecoil;
    private Vector3 gunModelTargetRecoil, gunModelCurrentRecoil;
    float _crosshairExpand;
    Transform _rightHandBone;

    bool _gunFirstRendered;
    bool _rightHandIKRotationOffsetAssigned;
    Quaternion _rightHandIKRotationOffset;

    public override void Init(HandEquipment equipment)
    {
        Gun = (Gun)equipment;
        _crosshair.CanvasGroup.alpha = 1;
        _scopeCanvasGroup.alpha = 0;
        PrepareTakeDown = false;
        aimTransition = new SmoothDampTransition(GunSpec.aimTime);
		FireCD = new Cooldown(GunSpec.fireCD);
        FireCD.IsReady = true;
        IsReloading = false;
		ReloadCD = new Cooldown(GunSpec.reloadCD);
        InitAnimatorParameters();
        //rotationLast = PlayerLook.currentRotation;
        _rightHandBone = Animator.GetBoneTransform(HumanBodyBones.RightHand);
        Gun.onFireCDSet.Invoke(FireCD.Capacity);
        Gun.gameObject.SetActive(true);
        Player.IKEventExposure.onAnimatorIK.AddListener(AnimatorIK);
    }
    private void OnEnable()
    {
        if (Gun == null)
            return;
        //animator parameters will be lost after gameobject disable.
        InitAnimatorParameters();
    }
    private void InitAnimatorParameters()
    {
        Animator.SetFloat("reloadSpeedMultiplier", Gun.ReloadAnimationClipLength / Mathf.Max(ReloadCD.Capacity, 0.1F));
        Animator.SetFloat("reloadAnimationType", Gun.ReloadAnimationID);
    }
    private void AnimatorIK(int layerIndex)
    {
        if (layerIndex == Animator.GetLayerIndex("LegsLayer")) //must done before heads layer
        {
            //camera defines head rotation
            //hands ik depends on head bone ik result, thus must do look at ik in prior layer 
            Animator.SetLookAtPosition(Player.GunEyeNoZRotAnchor.position + MainCamera.transform.forward);
            return;
        }
        if (layerIndex == Animator.GetLayerIndex("HandsLayer"))
        {
            //compute skeleton rotation offset from mechanim (Gun hands ik goal uses mechanim form)
            if (!_rightHandIKRotationOffsetAssigned)
            {
                _rightHandIKRotationOffsetAssigned = true;
                _rightHandIKRotationOffset = Quaternion.Inverse(_rightHandBone.rotation) * Animator.GetIKRotation(AvatarIKGoal.RightHand);
            }
            Quaternion rightHandIKRotationGoal = _rightHandBone.rotation * _rightHandIKRotationOffset;
            if (!aimTransition.NearZero)
            {
                //camera defines position of gun
                Gun.transform.SetPositionAndRotation(
                    aimTransition.Lerp(Gun.CentreRelArmCamera.GetChildPosition(Player.GunEyeNoZRotAnchor), Gun.CentreRelAimCamera.GetChildPosition(Player.GunEyeNoZRotAnchor)),
                    aimTransition.Lerp(Gun.CentreRelArmCamera.GetChildRotation(Player.GunEyeNoZRotAnchor), Gun.CentreRelAimCamera.GetChildRotation(Player.GunEyeNoZRotAnchor))
                    );
            }
            else
            {
                //camera and animation defines position of gun
                Gun.transform.SetPositionAndRotation(
                    Vector3.Lerp(Gun.CentreRelRightHand.GetChildPosition(_rightHandBone.position, rightHandIKRotationGoal),
                        Gun.CentreRelArmCamera.GetChildPosition(Player.GunEyeNoZRotAnchor),
                        Player.IKEventExposure.equipmentFollowRightHandPositionWeight), //TODO: the name meaning is reversed!!!
                    Quaternion.Lerp(Gun.CentreRelRightHand.GetChildRotation(rightHandIKRotationGoal),
                        Gun.CentreRelArmCamera.GetChildRotation(Player.GunEyeNoZRotAnchor),
                        Player.IKEventExposure.equipmentFollowRightHandRotationWeight)
                    );
            }
            Gun.transform.Translate(gunModelCurrentRecoil, Space.Self);
            //gun defines position of hands
            Animator.SetIKPosition(AvatarIKGoal.LeftHand, Gun.LeftHandGoal.position);
            Animator.SetIKRotation(AvatarIKGoal.LeftHand, Gun.LeftHandGoal.rotation);
            Animator.SetIKPosition(AvatarIKGoal.RightHand, Gun.RightHandGoal.position);
            Animator.SetIKRotation(AvatarIKGoal.RightHand, Gun.RightHandGoal.rotation);
            //(above ik weights are exposed in IKEventExposure for direct animation control)
            //Ensure first gun positioning is done before render
            if (!_gunFirstRendered)
            {
                _gunFirstRendered = true;
                Gun.gameObject.SetActive(true);
            }
        }
    }
    private void Update()
    {
        AimUpdate();
		PoseUpdate();
        RecoilUpdate();
        ShootingUpdate();
		ReloadUpdate();
        BashUpdate();
		CrossHairUpdate();
        _crosshairExpand = Mathf.Clamp(Mathf.Lerp(_crosshairExpand, 0, Time.deltaTime * 5), 0, 150);
        _crosshair.ExpandDistance = _crosshairExpand;
		_crosshair.CanvasGroup.alpha = 1 - aimTransition.value;
		_scopeCanvasGroup.alpha = aimTransition.value;
        AnimationUpdate();
    }
    public override void TakeDown()
    {
        base.TakeDown();
        Animator.SetTrigger("takeDown");
        Gun.ReloadSESource.Stop();
        IsReloading = false;
    }
    public override void Disable()
    {
        Gun.gameObject.SetActive(false);
        GameManager.Instance.Player.IKEventExposure.onAnimatorIK.RemoveListener(AnimatorIK);
        base.Disable();
    }
    void AimUpdate()
	{
		//TODO: consider meeleAttack state
		IsAiming = !PrepareTakeDown && Player.AIEnable && Input.GetButton("Fire2") && !IsReloading && !(PlayerMovement.HasInputXZ && PlayerMovement.MovingState == MovingStateEnum.Run);
        if (IsAiming)
        {
            aimTransition.SmoothTowardsOne();
            MouseSensitivityModify = GunSpec.mouseSensitivity_aiming;
            RecoilPenalty = 1;
        }
        else
        {
            aimTransition.SmoothTowardsZero();
            MouseSensitivityModify = 1;
            RecoilPenalty = GunSpec.recoilPenaltyNotAiming;
        }
        MainCamera.fieldOfView = aimTransition.Lerp(GunSpec.cameraZoomRatio_notAiming, GunSpec.cameraZoomRatio_aiming);
        GameManager.Instance.MinimapUI.SetAimLineVisibility(IsAiming);
    }

	/*
	 * Used to expand position of the crosshair or make it dissapear when running
	 */
	void CrossHairUpdate() {
        if (Player.AIEnable)
        {
            _crosshairContainer.gameObject.SetActive(true);
        }
        else
        {
            _crosshairContainer.gameObject.SetActive(false);
        }
		float playerSpeed = PlayerMovement.CharacterController.velocity.magnitude;
        if (playerSpeed > 0.1F)
        {
            _crosshairExpand += 0.4F * playerSpeed;
        }
        _magazineGaugeFill.fillAmount = 0.25F * Gun.magazine.Ratio;
    }

	void BashUpdate()
    {
        //wip
        /*
        if (BashActionOccupyRemainTime > 0)
        {
            BashActionOccupyRemainTime -= Time.deltaTime;
        }
        if (Player.AIEnable && Input.GetKeyDown(KeyCode.V) && !IsBashing)
        {
            Animator.SetTrigger("bash");
            BashActionOccupyRemainTime = _bashActionOccupyTime;
        }
        */
	}
    private void PoseUpdate()
    {
        //transform.position = MainCamera.transform.TransformPoint(currentRecoil);
        //transform.rotation = Quaternion.Euler(PlayerLook.currentRotation);
        //TODO: smooth hands rotation accordings to equipment weight
        //Vector2 rotationDelta = PlayerLook.currentRotation - rotationLast;
        //rotationLast = PlayerLook.currentRotation;
        //angularVelocity = Vector3.Lerp (angularVelocity, rotationDelta, Time.deltaTime * 5);
        //gunWeight = Vector2.SmoothDamp (gunWeight, PlayerLook.currentRotation, ref velocityGunRotate, rotationLagTime);
        //transform.rotation = Quaternion.Euler (gunWeight + (angularVelocity * forwardRotationAmount.x));
    }
    private void RecoilUpdate()
    {
        gunModelTargetRecoil = Vector3.Lerp(gunModelTargetRecoil, Vector3.zero, Gun.ModelRecoilReturnSpeed * Time.deltaTime);
        gunModelCurrentRecoil = Vector3.Slerp(gunModelCurrentRecoil, gunModelTargetRecoil, Gun.ModelRecoilSnappiness * Time.deltaTime);
        currentRecoil = Vector3.SmoothDamp(currentRecoil, Vector3.zero, ref velocity_recoil, GunSpec.recoilOverTime);
        PlayerLook.recoilRotation.x = -Mathf.Abs(currentRecoil.y) * GunSpec.recoilRotateRatio;
        PlayerLook.recoilRotation.y = (currentRecoil.x) * GunSpec.recoilRotateRatio;
    }
	public void ApplyRecoil(Vector3 recoil)
	{
        Vector3 modelRecoil = aimTransition.NearZero ? Gun.ModelRecoilHipAiming : Gun.ModelRecoilSightAiming;
        modelRecoil.x *= Random.Range(-1F, 1F);
        modelRecoil.y *= Random.Range(-1F, 1F);
        gunModelTargetRecoil += modelRecoil;
        currentRecoil.z -= recoil.z;
		currentRecoil.x -= (Random.value - 0.5f) * recoil.x;
		currentRecoil.y -= (0.9F + Random.value * 0.2f) * recoil.y;
		//PlayerLook.OverrideRecoil();
		_crosshairExpand += 90;
	}
	void ShootingUpdate()
    {
		FireCD.AddDeltaTime();
        if (PrepareTakeDown || !Player.AIEnable || IsBashing)
            return;
        if (GunSpec.fireMode == GunSpec.FireMode.Nonautomatic)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Trigger();
            }
        }
        if (GunSpec.fireMode == GunSpec.FireMode.Automatic)
        {
            if (Input.GetButton("Fire1"))
            {
                Trigger();
            }
        }
    }
	/*
	 * Called from Shooting();
	 * Creates bullets and muzzle flashes and calls for Recoil.
	 */
	public void Trigger()
	{
		if(FireCD.IsReady && !IsReloading)
		{
			if(Gun.magazine.Value > 0)
			{
				FireCD.Eat();
				Gun.onFire.Invoke();
                Bullet bullet = Instantiate(GunSpec.bulletPrefab);
				bullet.transform.SetParent(GameManager.Instance.Stage.transform);
                bullet.transform.position = BulletSpawnPosition;
				bullet.transform.forward = Player.AimPosition - bullet.transform.position;
				Vector2 randomAberation = GunSpec.accuracy * _crosshairExpand * Random.insideUnitCircle;
				bullet.transform.Rotate(randomAberation);
                bullet.damage = GunSpec.damage;
				bullet.speed = GunSpec.bulletSpeed;
                GameObject flash = Instantiate(Gun.muzzleFlashes.GetRandomElement(), Gun.MuzzleAnchor.position, Gun.MuzzleAnchor.rotation * Quaternion.Euler(0, 0, Random.value * 360));
				flash.transform.parent = Gun.MuzzleAnchor;
                if (Gun.ShootSESource != null)
                    Gun.ShootSESource.Play();
                ApplyRecoil(GunSpec.recoilAmount_aiming * RecoilPenalty);
				Gun.magazine.Value--;
			}
			else
			{
				//if(!aiming)
				TryReload();
				//if(emptyClip_sound_source)
				//	emptyClip_sound_source.Play();
			}
		}
	}
	bool TryReload()
	{
		if (IsReloading || Gun.spareAmmo <= 0 || Gun.magazine.Value == Gun.magazine.Capacity)
			return false;
        if (Gun.ReloadSESource != null)
            Gun.ReloadSESource.Play();
        IsReloading = true;
        Animator.SetTrigger("reload");
		ReloadCD.Clear();
		return true;
	}
	void ReloadUpdate()
	{
        if (PrepareTakeDown)
            return;
		if (!IsReloading)
		{
            if (Player.AIEnable && Input.GetKeyDown(KeyCode.R) && !IsBashing)
            {
                TryReload();
            }
            return;
        }
        if (IsBashing || (PlayerMovement.HasInputXZ && PlayerMovement.MovingState == MovingStateEnum.Run))
        {
			IsReloading = false;
        }
		if (ReloadCD.AddDeltaTimeAndEat())
        {
            IsReloading = false;
            Gun.spareAmmo = (int)Gun.magazine.AddAndGetOverflow(Gun.spareAmmo);
        }
	}
	private void AnimationUpdate()
    {
        Animator.SetFloat("ADSTransition", aimTransition.value);
    }
}
