using UnityEngine;
using System.Collections;
//using UnityStandardAssets.ImageEffects;

public enum GunStyles{
	nonautomatic,automatic
}
[DisallowMultipleComponent]
public class GunScript : MonoBehaviour
{
    [Header("Test / Debug")]
	[SerializeField]
	CompositeCrosshair _crosshair;

	public float damage;
	public float bulletSpeed;
	[Tooltip("Selects type of waepon to shoot rapidly or one bullet per click.")]
	public GunStyles currentStyle;

	[Header("Player movement properties")]
	[Tooltip("Speed is determined via gun because not every gun has same properties or weights so you MUST set up your speeds here")]
	public int walkingSpeed = 3;
	[Tooltip("Speed is determined via gun because not every gun has same properties or weights so you MUST set up your speeds here")]
	public int runningSpeed = 5;

	[Header("Bullet properties")]
	[Tooltip("Preset value to tell with how many bullets will our waepon spawn aside.")]
	public float bulletsIHave = 20;
	[Tooltip("Preset value to tell with how much bullets will our waepon spawn inside rifle.")]
	public float bulletsInTheGun = 5;
	[Tooltip("Preset value to tell how much bullets can one magazine carry.")]
	public float amountOfBulletsPerLoad = 5;

    [Header("Shooting setup")]
    [SerializeField] GameObject bulletSpawnPlace;
    [Tooltip("Bullet prefab that this waepon will shoot.")]
    public BulletScript bulletPrefab;
    [Tooltip("Rounds per second if weapon is set to automatic rafal.")]
    public float roundsPerSecond;
    private float waitTillNextFire;

    [Header("Gun Positioning")]
    [Tooltip("Vector 3 position from player SETUP for NON AIMING values")]
    public Vector3 restPlacePosition;
    [Tooltip("Vector 3 position from player SETUP for AIMING values")]
    public Vector3 aimPlacePosition;
    [Tooltip("Time that takes for gun to get into aiming stance.")]
    public float gunAimTime = 0.1f;

	private Player Player => GameManager.Instance.Player;
    private Camera MainCamera => Player.Camera;
    private Camera HUDCamera => Player.HUDCamera;
    private PlayerMovementScript PlayerMovement => Player.Movement;
	private MouseLookScript PlayerLook => Player.MouseLook;

	/*
	 * Collection the variables upon awake that we need.
	 */
	void Awake(){

		hitMarker = transform.Find ("hitMarkerSound").GetComponent<AudioSource> ();

		startLook = mouseSensitvity_notAiming;
		startAim = mouseSensitvity_aiming;
		startRun = mouseSensitvity_running;

		rotationLastY = PlayerLook.currentYRotation;
		rotationLastX= PlayerLook.currentCameraXRotation;
	}


	[HideInInspector]
	public Vector3 currentGunPosition;

	[HideInInspector]
	public bool reloading;

	private Vector3 gunPosVelocity;
	private float cameraZoomVelocity;
	private float secondCameraZoomVelocity;

	/*
	Update loop calling for methods that are descriped below where they are initiated.
	*/
	void Update(){

		Animations();

		GiveCameraScriptMySensitvity();

		PositionGun();

		Shooting();
		MeeleAttack();
		LockCameraWhileMelee ();

		CrossHairExpansionOfWalking();

		_crosshair.expandDistance = 100 + crosshairExpand;
		_crosshair.SetCanvasGroupAlpha(fadeout_value);
    }

	/*
	*Update loop calling for methods that are descriped below where they are initiated.
	*+
	*Calculation of weapon position when aiming or not aiming.
	*/
	void FixedUpdate(){
		RotationGun ();

		MeeleAnimationsStates ();

		/*
		 * Changing some values if we are aiming, like sensitity, zoom racion and position of the waepon.
		 */
		//if aiming
		if(Input.GetAxis("Fire2") != 0 && !reloading && !meeleAttack){
			recoilPenalty = 1;
			currentGunPosition = Vector3.SmoothDamp(currentGunPosition, aimPlacePosition, ref gunPosVelocity, gunAimTime);
			MainCamera.fieldOfView = Mathf.SmoothDamp(MainCamera.fieldOfView, cameraZoomRatio_aiming, ref cameraZoomVelocity, gunAimTime);
			HUDCamera.fieldOfView = Mathf.SmoothDamp(HUDCamera.fieldOfView, secondCameraZoomRatio_aiming, ref secondCameraZoomVelocity, gunAimTime);
		}
		//if not aiming
		else{
			recoilPenalty = recoilPenaltyNotAiming;
			currentGunPosition = Vector3.SmoothDamp(currentGunPosition, restPlacePosition, ref gunPosVelocity, gunAimTime);
            MainCamera.fieldOfView = Mathf.SmoothDamp(MainCamera.fieldOfView, cameraZoomRatio_notAiming, ref cameraZoomVelocity, gunAimTime);
			HUDCamera.fieldOfView = Mathf.SmoothDamp(HUDCamera.fieldOfView, secondCameraZoomRatio_notAiming, ref secondCameraZoomVelocity, gunAimTime);
		}

	}

	[Header("Sensitvity of the gun")]
	[Tooltip("Sensitvity of this gun while not aiming.")]
	public float mouseSensitvity_notAiming = 10;
	//[HideInInspector]
	[Tooltip("Sensitvity of this gun while aiming.")]
	public float mouseSensitvity_aiming = 5;
	//[HideInInspector]
	[Tooltip("Sensitvity of this gun while running.")]
	public float mouseSensitvity_running = 4;
	/*
	 * Used to give our main camera different sensivity options for each gun.
	 */
	void GiveCameraScriptMySensitvity(){
		PlayerLook.mouseSensitvity_notAiming = mouseSensitvity_notAiming;
		PlayerLook.mouseSensitvity_aiming = mouseSensitvity_aiming;
	}

	/*
	 * Used to expand position of the crosshair or make it dissapear when running
	 */
	void CrossHairExpansionOfWalking(){
		if(Player.GetComponent<Rigidbody>().velocity.magnitude > 1 && Input.GetAxis("Fire1") == 0){
			crosshairExpand += 300 * Time.deltaTime;
			if(Player.GetComponent<PlayerMovementScript>().walkSpeed < runningSpeed){ //not running
				crosshairExpand = Mathf.Clamp(crosshairExpand, 0, 150);
				fadeout_value = Mathf.Lerp(fadeout_value, 1, Time.deltaTime * 2);
			}
			else{//running
				fadeout_value = Mathf.Lerp(fadeout_value, 0, Time.deltaTime * 10);
                crosshairExpand = Mathf.Clamp(crosshairExpand, 0, 300);
            }
		}
		else
		{
			crosshairExpand = Mathf.Lerp(crosshairExpand, 0, Time.deltaTime * 5);
            crosshairExpand = Mathf.Clamp(crosshairExpand, 0, 150);
            fadeout_value = Mathf.Lerp(fadeout_value, 1, Time.deltaTime * 2);

		}
	}

	[HideInInspector]
	public bool meeleAttack;
	[HideInInspector]
	public bool aiming;
	/*
	 * Checking if meeleAttack is already running.
	 * If we are not reloading we can trigger the MeeleAttack animation from the IENumerator.
	 */
	void MeeleAnimationsStates(){
		if (handsAnimator) {
			meeleAttack = handsAnimator.GetCurrentAnimatorStateInfo (0).IsName (meeleAnimationName);
			aiming = handsAnimator.GetCurrentAnimatorStateInfo (0).IsName (aimingAnimationName);	
		}
	}
	void MeeleAttack(){	

		if(Input.GetKeyDown(KeyCode.V) && !meeleAttack){			
			StartCoroutine("AnimationMeeleAttack");
		}
	}
	/*
	* Sets meele animation to play.
	*/
	IEnumerator AnimationMeeleAttack(){
		handsAnimator.SetBool("meeleAttack",true);
		//yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(0.1f);
		handsAnimator.SetBool("meeleAttack",false);
	}

	private float startLook, startAim, startRun;
	/*
	* Setting the mouse sensitvity lower when meele attack and waits till it ends.
	*/
	void LockCameraWhileMelee(){
		if (meeleAttack) {
			mouseSensitvity_notAiming = 2;
			mouseSensitvity_aiming = 1.6f;
			mouseSensitvity_running = 1;
		} else {
			mouseSensitvity_notAiming = startLook;
			mouseSensitvity_aiming = startAim;
			mouseSensitvity_running = startRun;
		}
	}
	private Vector3 velV;
	/*
	 * Calculatin the weapon position accordingly to the player position and rotation.
	 * After calculation the recoil amount are decreased to 0.
	 */
	void PositionGun(){
		transform.position = Vector3.SmoothDamp(transform.position, MainCamera.transform.TransformPoint(currentGunPosition + currentRecoil), ref velV, 0);
		PlayerMovement.cameraPosition = new Vector3(currentRecoil.x,currentRecoil.y, 0);
		currentRecoil.z = Mathf.SmoothDamp(currentRecoil.z, 0, ref velocity_recoil.z, recoilOverTime.z);
		currentRecoil.x = Mathf.SmoothDamp(currentRecoil.x, 0, ref velocity_recoil.x, recoilOverTime.x);
		currentRecoil.y = Mathf.SmoothDamp(currentRecoil.y, 0, ref velocity_recoil.y, recoilOverTime.y);
	}

	[Header("Rotation")]
	private Vector2 velocityGunRotate;
	private float gunWeightX,gunWeightY;
	[Tooltip("The time waepon will lag behind the camera view best set to '0'.")]
	public float rotationLagTime = 0f;
	private float rotationLastY;
	private float rotationDeltaY;
	private float angularVelocityY;
	private float rotationLastX;
	private float rotationDeltaX;
	private float angularVelocityX;
	[Tooltip("Value of forward rotation multiplier.")]
	public Vector2 forwardRotationAmount = Vector2.one;
	/*
	* Rotatin the weapon according to mouse look rotation.
	* Calculating the forawrd rotation like in Call Of Duty weapon weight
	*/
	void RotationGun(){

		rotationDeltaY = PlayerLook.currentYRotation - rotationLastY;
		rotationDeltaX = PlayerLook.currentCameraXRotation - rotationLastX;

		rotationLastY= PlayerLook.currentYRotation;
		rotationLastX= PlayerLook.currentCameraXRotation;

		angularVelocityY = Mathf.Lerp (angularVelocityY, rotationDeltaY, Time.deltaTime * 5);
		angularVelocityX = Mathf.Lerp (angularVelocityX, rotationDeltaX, Time.deltaTime * 5);

		gunWeightX = Mathf.SmoothDamp (gunWeightX, PlayerLook.currentCameraXRotation, ref velocityGunRotate.x, rotationLagTime);
		gunWeightY = Mathf.SmoothDamp (gunWeightY, PlayerLook.currentYRotation, ref velocityGunRotate.y, rotationLagTime);

		transform.rotation = Quaternion.Euler (gunWeightX + (angularVelocityX*forwardRotationAmount.x), gunWeightY + (angularVelocityY*forwardRotationAmount.y), 0);
	}

	private Vector3 currentRecoil;
	/*
	 * Called from ShootMethod();, upon shooting the recoil amount will increase.
	 */
	public void RecoilMath(){
		recoilAmount = recoilAmount_aiming * recoilPenalty;
        currentRecoil.z -= recoilAmount.z;
		currentRecoil.x -= (Random.value - 0.5f) * recoilAmount.x;
		currentRecoil.y -= (Random.value - 0.5f) * recoilAmount.y;
		PlayerLook.wantedCameraXRotation -= Mathf.Abs(currentRecoil.y);
		PlayerLook.wantedYRotation -= (currentRecoil.x);
		crosshairExpand += 90;
	}

	/*
	 * Checking if the gun is automatic or nonautomatic and accordingly runs the ShootMethod();.
	 */
	void Shooting(){
		if (!meeleAttack) {
			if (currentStyle == GunStyles.nonautomatic) {
				if (Input.GetButtonDown ("Fire1")) {
					ShootMethod ();
				}
			}
			if (currentStyle == GunStyles.automatic) {
				if (Input.GetButton ("Fire1")) {
					ShootMethod ();
				}
			}
		}
		waitTillNextFire -= roundsPerSecond * Time.deltaTime;
	}

	[HideInInspector]	public Vector3 recoilAmount = new Vector3(0.5f, 0.5f, 0.5f);
	[Header("Recoil")]
	[Tooltip("Recoil amount while aiming")]
	public Vector3 recoilAmount_aiming = new Vector3(0.5f, 0.5f, 0.5f);
    [Tooltip("Recoil rate when player is not aiming. This is calculated with recoil.")]
    public float recoilPenaltyNotAiming = 2f;
	[Tooltip("The time that takes weapon to get back on its original axis after recoil.(The smaller number the faster it gets back to original position)")]
	public Vector3 recoilOverTime = new Vector3(0.5f, 0.5f, 0.5f);
    [HideInInspector] public Vector3 velocity_recoil;

    [Header("Gun Precision")]
	[Tooltip("FOV of first camera when NOT aiming(ONLY SECOND CAMERA RENDERS WEAPONS")]
	public float cameraZoomRatio_notAiming = 60;
	[Tooltip("FOV of first camera when aiming(ONLY SECOND CAMERA RENDERS WEAPONS")]
	public float cameraZoomRatio_aiming = 40;
	[Tooltip("FOV of second camera when NOT aiming(ONLY SECOND CAMERA RENDERS WEAPONS")]
	public float secondCameraZoomRatio_notAiming = 60;
	[Tooltip("FOV of second camera when aiming(ONLY SECOND CAMERA RENDERS WEAPONS")]
	public float secondCameraZoomRatio_aiming = 40;
	[HideInInspector]
	public float recoilPenalty;

	[Tooltip("Audios for shootingSound, and reloading.")]
	public AudioSource shoot_sound_source, reloadSound_source;
	[SerializeField]
    AudioClip _reloadFinishSE;
    [Tooltip("Sound that plays after successful attack bullet hit.")]
	public static AudioSource hitMarker;

	/*
	* Sounds that is called upon hitting the target.
	*/
	public static void HitMarkerSound(){
		hitMarker.Play();
	}

	[Tooltip("Array of muzzel flashes, randmly one will appear after each bullet.")]
	public GameObject[] muzzelFlash;
	[Tooltip("Place on the gun where muzzel flash will appear.")]
	public GameObject muzzelSpawn;
	private GameObject holdFlash;
	/*
	 * Called from Shooting();
	 * Creates bullets and muzzle flashes and calls for Recoil.
	 */
	private void ShootMethod(){
		if(waitTillNextFire <= 0 && !reloading){
			if(bulletsInTheGun > 0){
                BulletScript bullet = Instantiate(bulletPrefab);
				bullet.transform.SetParent(GameManager.Instance.CurrentStage.transform);
                bullet.transform.position = bulletSpawnPlace.transform.position;
				Vector3 aimPosition;
				if (Player.HasRaycastPosition)
				{
					aimPosition = Player.RaycastPosition;
				}
				else
				{
					aimPosition = MainCamera.transform.position + MainCamera.transform.forward * 100;
				}
				bullet.transform.forward = aimPosition - bullet.transform.position;
                bullet.damage = damage;
				bullet.speed = bulletSpeed;
                holdFlash = Instantiate(muzzelFlash[Random.Range(0, 5)], muzzelSpawn.transform.position /*- muzzelPosition*/, muzzelSpawn.transform.rotation * Quaternion.Euler(0,0,90) ) as GameObject;
				holdFlash.transform.parent = muzzelSpawn.transform;
                shoot_sound_source?.Play();
                RecoilMath();
				waitTillNextFire = 1;
				bulletsInTheGun -= 1;
			}
			else
			{
				//if(!aiming)
				StartCoroutine("Reload_Animation");
				//if(emptyClip_sound_source)
				//	emptyClip_sound_source.Play();
			}

		}

	}



	/*
	* Reloading, setting the reloading to animator,
	* Waiting for 2 seconds and then seeting the reloaded clip.
	*/
	[Header("reload time after anima")]
	[Tooltip("Time that passes after reloading. Depends on your reload animation length, because reloading can be interrupted via meele attack or running. So any action before this finishes will interrupt reloading.")]
	public float reloadChangeBulletsTime;
	IEnumerator Reload_Animation(){
		if(bulletsIHave > 0 && bulletsInTheGun < amountOfBulletsPerLoad && !reloading/* && !aiming*/){

			if (reloadSound_source.isPlaying == false && reloadSound_source != null) {
				if (reloadSound_source)
					reloadSound_source.Play ();
				else
					print ("'Reload Sound Source' missing.");
			}
		

			handsAnimator.SetBool("reloading",true);
			yield return new WaitForSeconds(0.5f);
			handsAnimator.SetBool("reloading",false);



			yield return new WaitForSeconds (reloadChangeBulletsTime - 0.5f);//minus ovo vrijeme cekanja na yield
			if (meeleAttack == false && PlayerMovement.walkSpeed != runningSpeed) {
				//print ("tu sam");
				//Player.GetComponent<PlayerMovementScript>().SpeakSE(_reloadFinishSE);

                if (bulletsIHave - amountOfBulletsPerLoad >= 0) {
					bulletsIHave -= amountOfBulletsPerLoad - bulletsInTheGun;
					bulletsInTheGun = amountOfBulletsPerLoad;
				} else if (bulletsIHave - amountOfBulletsPerLoad < 0) {
					float valueForBoth = amountOfBulletsPerLoad - bulletsInTheGun;
					if (bulletsIHave - valueForBoth < 0) {
						bulletsInTheGun += bulletsIHave;
						bulletsIHave = 0;
					} else {
						bulletsIHave -= valueForBoth;
						bulletsInTheGun += valueForBoth;
					}
				}
			} else {
				reloadSound_source.Stop ();

				print ("Reload interrupted via meele attack");
			}

		}
	}

	/*
	 * Setting the number of bullets to the hud UI gameobject if there is one.
	 * And drawing CrossHair from here.
	 */
	[Tooltip("HUD bullets to display bullet count on screen. Will be find under name 'HUD_bullets' in scene.")]
	public TextMesh HUD_bullets;
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
			HUD_bullets.text = bulletsIHave.ToString() + " - " + bulletsInTheGun.ToString();
	}

	[Header("Crosshair properties")]
	[HideInInspector]
	public float crosshairExpand;
	private float fadeout_value = 1;

	public Animator handsAnimator;
	/*
	* Fetching if any current animation is running.
	* Setting the reload animation upon pressing R.
	*/
	void Animations(){

		if(handsAnimator){

			reloading = handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(reloadAnimationName);

			handsAnimator.SetBool("isMoving", PlayerMovement.HasInputXZ);
			handsAnimator.SetBool("aiming", Input.GetButton("Fire2"));
			handsAnimator.SetBool("running", PlayerMovement.HasInputXZ && PlayerMovement.MovingState == MovingStateEnum.Run);
			if(Input.GetKeyDown(KeyCode.R) && PlayerMovement.walkSpeed < 5 && !reloading && !meeleAttack/* && !aiming*/){
				StartCoroutine("Reload_Animation");
			}
		}

	}

	[Header("Animation names")]
	public string reloadAnimationName = "Player_Reload";
	public string aimingAnimationName = "Player_AImpose";
	public string meeleAnimationName = "Character_Malee";
}
