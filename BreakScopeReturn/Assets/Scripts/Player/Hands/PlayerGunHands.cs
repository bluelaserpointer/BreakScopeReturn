using UnityEngine;
using System.Collections;
using IzumiTools;

public class PlayerGunHands : PlayerHands
{
	[Header("HUD")]
    [SerializeField]
    CompositeCrosshair _crosshair;
    [SerializeField]
	CanvasGroup _scopeCanvasGroup;

    [Header("Reload")]
    [SerializeField]
    AnimationClip _reloadAnimationClip;

    [Header("Animation Positioning")]
	[SerializeField]
    AnimatorIKEventExposure _IKEventExposure;

    [Header("Rotation")]
    public float rotationLagTime = 0f;
    public Vector2 forwardRotationAmount = Vector2.one;

    public Gun Gun { get; private set; }
	public GunSpec GunSpec => Gun.spec;
	[HideInInspector]
	public SmoothDampTransition aimTransition;
    [HideInInspector] public Vector3 velocity_recoil;
    public Cooldown FireCD { get; private set; }
    public Cooldown ReloadCD { get; private set; }
    public bool IsReloading { get; private set; }
    public float RecoilPenalty { get; private set; }
    public Vector3 BulletSpawnPosition => Gun.MuzzleAnchor.position;
    private Player Player => GameManager.Instance.Player;
    private Camera MainCamera => Player.Camera;
    private Camera HUDCamera => Player.HUDCamera;
    private PlayerMovementScript PlayerMovement => Player.Movement;
    private MouseLookScript PlayerLook => Player.MouseLook;

	Transform _rightHandBone;

    private Vector3 currentRecoil;
    float _crosshairExpand;

    bool _gunFirstRendered;

    public void Init(Gun gunModel)
    {
		Gun = gunModel;
        foreach (var child in Gun.GetComponentsInChildren<Renderer>())
            child.gameObject.layer = gameObject.layer;
        aimTransition = new SmoothDampTransition(GunSpec.aimTime);
		FireCD = new Cooldown(GunSpec.fireCD);
		ReloadCD = new Cooldown(GunSpec.reloadCD);
        //rotationLast = PlayerLook.currentRotation;
        HandsAnimator.SetFloat("reloadSpeedMultiplier", _reloadAnimationClip.length / Mathf.Max(ReloadCD.Max, 0.1F));
        _rightHandBone = HandsAnimator.GetBoneTransform(HumanBodyBones.RightHand);
    }
    private void Start()
    {
        _crosshair.CanvasGroup.alpha = 1;
        _scopeCanvasGroup.alpha = 0;
        Gun.onFireCDSet.Invoke(FireCD.Max);
        _IKEventExposure.onAnimatorIK.AddListener(layer =>
        {
            //RightHand
            if (aimTransition.NearZero)
            {
                //right hand's original animation defines position of gun
                Gun.SetCentreByRightHand(_rightHandBone);
            }
            else
            {
                //gun defines position of right hand using IK
                Gun.transform.SetPositionAndRotation(
                    aimTransition.Lerp(Gun.CentreRelRightHand.GetChildPosition(_rightHandBone.position, _rightHandBone.rotation), Gun.CentreRelAimCamera.GetChildPosition(MainCamera.transform)),
                    aimTransition.Lerp(Gun.CentreRelRightHand.GetChildRotation(_rightHandBone.rotation), Gun.CentreRelAimCamera.GetChildRotation(MainCamera.transform))
                    );
                HandsAnimator.SetIKPosition(AvatarIKGoal.RightHand, Gun.RightHandAnchor.position);
                HandsAnimator.SetIKRotation(AvatarIKGoal.RightHand, Gun.RightHandAnchor.rotation);
                HandsAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                HandsAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            }
            //Left Hand
            HandsAnimator.SetIKPosition(AvatarIKGoal.LeftHand, Gun.LeftHandAnchor.position);
            HandsAnimator.SetIKRotation(AvatarIKGoal.LeftHand, Gun.LeftHandAnchor.rotation);
            float leftHandReloadIKWeight = IsReloading ? Gun.ReloadLeftHandIKWeightCurve.Evaluate(this.ReloadCD.Ratio) : 1;
            HandsAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandReloadIKWeight);
            HandsAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandReloadIKWeight);
            //Ensure first gun positioning is done before render
            if (!_gunFirstRendered)
            {
                _gunFirstRendered = true;
                Gun.gameObject.SetActive(true);
            }
        });
    }
    private void Update()
    {
        AimUpdate();
		PoseUpdate();
        RecoilUpdate();
        ShootingUpdate();
		ReloadUpdate();
		MeeleAttack();
		CrossHairExpansionOfWalking();
        _crosshairExpand = Mathf.Clamp(Mathf.Lerp(_crosshairExpand, 0, Time.deltaTime * 5), 0, 150);
        _crosshair.ExpandDistance = _crosshairExpand;
		_crosshair.CanvasGroup.alpha = 1 - aimTransition.value;
		_scopeCanvasGroup.alpha = aimTransition.value;
        AnimationUpdate();
    }
    public override void WithdrawItemAndDestroy()
    {
		Gun.gameObject.SetActive(false);
		foreach(var child in Gun.GetComponentsInChildren<Renderer>())
			child.gameObject.layer = 0; //Default layer
		base.WithdrawItemAndDestroy();
    }
    void AimUpdate()
	{
		//TODO: consider meeleAttack state
		IsAiming = Input.GetButton("Fire2") && !IsReloading && !(PlayerMovement.HasInputXZ && PlayerMovement.MovingState == MovingStateEnum.Run);
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
        HUDCamera.fieldOfView = aimTransition.Lerp(GunSpec.hudCameraZoomRatio_notAiming, GunSpec.hudCameraZoomRatio_aiming);
        GameManager.Instance.MinimapUI.SetAimLineVisibility(IsAiming);
    }

	/*
	 * Used to expand position of the crosshair or make it dissapear when running
	 */
	void CrossHairExpansionOfWalking() {
		float playerSpeed = PlayerMovement.CharacterController.velocity.magnitude;
        if (playerSpeed > 0.1F)
        {
            _crosshairExpand += 0.4F * playerSpeed;
        }
    }

	[HideInInspector]
	public bool meeleAttack;
	void MeeleAttack(){	

		if(Input.GetKeyDown(KeyCode.V) && !meeleAttack)
        {
            HandsAnimator.SetTrigger("meeleAttack");
        }
	}
    //private Vector2 velocityGunRotate;
    //private Vector2 rotationLast;
    //private Vector2 angularVelocity;
    //private Vector2 gunWeight;
    private void PoseUpdate()
    {
        transform.position = MainCamera.transform.TransformPoint(currentRecoil);
        transform.rotation = Quaternion.Euler(PlayerLook.currentRotation);
		//TODO: smooth hands rotation accordings to equipment weight
        //Vector2 rotationDelta = PlayerLook.currentRotation - rotationLast;
        //rotationLast = PlayerLook.currentRotation;
        //angularVelocity = Vector3.Lerp (angularVelocity, rotationDelta, Time.deltaTime * 5);
        //gunWeight = Vector2.SmoothDamp (gunWeight, PlayerLook.currentRotation, ref velocityGunRotate, rotationLagTime);
        //transform.rotation = Quaternion.Euler (gunWeight + (angularVelocity * forwardRotationAmount.x));
    }
	private void RecoilUpdate()
    {
        currentRecoil = Vector3.SmoothDamp(currentRecoil, Vector3.zero, ref velocity_recoil, GunSpec.recoilOverTime);
        PlayerLook.recoilRotation.x = -Mathf.Abs(currentRecoil.y) * GunSpec.recoilRotateRatio;
        PlayerLook.recoilRotation.y = (currentRecoil.x) * GunSpec.recoilRotateRatio;
    }
	public void ApplyRecoil(Vector3 recoil)
	{
        currentRecoil.z -= recoil.z;
		currentRecoil.x -= (Random.value - 0.5f) * recoil.x;
		currentRecoil.y -= (0.9F + Random.value * 0.2f) * recoil.y;
		//PlayerLook.OverrideRecoil();
		_crosshairExpand += 90;
	}
	void ShootingUpdate(){
		FireCD.AddDeltaTime();
        if (!meeleAttack)
		{
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
	}
	/*
	 * Called from Shooting();
	 * Creates bullets and muzzle flashes and calls for Recoil.
	 */
	private void Trigger()
	{
		if(FireCD.IsReady && !IsReloading)
		{
			if(Gun.magazine.Value > 0)
			{
				FireCD.Eat();
				Gun.onFire.Invoke();
                Bullet bullet = Instantiate(GunSpec.bulletPrefab);
				bullet.transform.SetParent(GameManager.Instance.CurrentStage.transform);
                bullet.transform.position = BulletSpawnPosition;
				bullet.transform.forward = Player.AimPosition - bullet.transform.position;
				Vector2 randomAberation = GunSpec.accuracy * _crosshairExpand * Random.insideUnitCircle;
				bullet.transform.Rotate(randomAberation);
                bullet.damage = GunSpec.damage;
				bullet.speed = GunSpec.bulletSpeed;
                GameObject flash = Instantiate(Gun.muzzleFlashes[Random.Range(0, 5)], Gun.MuzzleAnchor.position, Gun.MuzzleAnchor.rotation * Quaternion.Euler(0, 0, Random.value * 360));
				flash.transform.parent = Gun.MuzzleAnchor;
                Gun.ShootSESource?.Play();
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
		if (IsReloading || Gun.spareAmmo <= 0 || Gun.magazine.Value == Gun.magazine.Max)
			return false;
		if (Gun.ReloadSESource.isPlaying == false && Gun.ReloadSESource != null)
		{
			if (Gun.ReloadSESource)
				Gun.ReloadSESource.Play ();
			else
				print ("'Reload Sound Source' missing.");
        }
        IsReloading = true;
        HandsAnimator.SetTrigger("reload");
		ReloadCD.Reset();
		return true;
	}
	void ReloadUpdate()
	{
		if (!IsReloading)
		{
            if (Input.GetKeyDown(KeyCode.R) && !meeleAttack)
            {
                TryReload();
            }
            return;
        }
        if (meeleAttack || (PlayerMovement.HasInputXZ && PlayerMovement.MovingState == MovingStateEnum.Run))
        {
			IsReloading = false;
        }
		if (ReloadCD.AddDeltaTimeAndEat())
        {
            IsReloading = false;
            Gun.spareAmmo = (int)Gun.magazine.AddAndGetOverflow(Gun.spareAmmo);
        }
	}

	/*
	 * Setting the number of bullets to the hud UI gameobject if there is one.
	 * And drawing CrossHair from here.
	 */
	TextMesh HUD_bullets;
	void OnGUI(){
		if(!HUD_bullets){
			try{
				HUD_bullets = GameObject.Find("HUD_bullets").GetComponent<TextMesh>();
			}
			catch(System.Exception ex){
				print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
			}
		}
		if(PlayerLook && HUD_bullets)
			HUD_bullets.text = Gun.spareAmmo.ToString() + " - " + Gun.magazine.Value.ToString();
	}
	/*
	* Fetching if any current animation is running.
	* Setting the reload animation upon pressing R.
	*/
	void AnimationUpdate()
	{
		if(HandsAnimator) {
			HandsAnimator.SetBool("isMoving", PlayerMovement.HasInputXZ);
			HandsAnimator.SetBool("aiming", IsAiming);
			HandsAnimator.SetBool("running", PlayerMovement.HasInputXZ && PlayerMovement.MovingState == MovingStateEnum.Run);
		}
	}
}
