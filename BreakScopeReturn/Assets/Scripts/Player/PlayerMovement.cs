using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public enum MovingStateEnum
{
    Walk, Run, Crouch
}
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour {
    [Header("Mobility")]
	public float crouchSpeed = 2;
    public float walkSpeed = 3;
    public float runSpeed = 5;
    public float _groundCheckDepth = 0.05F;
    [Range(0, 1)]
    public float accelLerp = 0.8F;
    [Tooltip("Force that moves player into jump")]
    public float jumpHeight = 3;

    [Header("SE")]
    public AudioSource _moveAudioSource;
    public AudioSource _oralAudioSource;
    public AudioClip _jumpSE, _walkSE, _runSE;

    public Rigidbody Rigidbody {get; private set; }
    public CharacterController CharacterController { get; private set; }
    public bool IsGrounded { get; private set; }
	public MovingStateEnum MovingState { get; private set; }
    public Vector2 InputXZ { get; private set; }
    public bool HasInputXZ { get; private set; }
	public Vector3 Movement { get; private set; }
	public bool IsMoving => Movement.sqrMagnitude > 0;

    float _fallVelocity;
    /*
	 * Getting the Players rigidbody component.
	 * And grabbing the mainCamera from Players child transform.
	 */
    void Awake(){
		Rigidbody = GetComponent<Rigidbody>();
        CharacterController = GetComponent<CharacterController>();
		ignoreLayer = 1 << LayerMask.NameToLayer ("Player");
		MovingState = MovingStateEnum.Walk;
    }
	/*
	* Raycasting for meele attacks and input movement handling here.
	*/
	void FixedUpdate() {
		RaycastForMeleeAttacks();
		FixedUpdateMovement();
	}
	/*
	* Accordingly to input adds force and if magnitude is bigger it will clamp it.
	* If player leaves keys it will deaccelerate
	*/
	void FixedUpdateMovement()
    {
		Vector3 movement = Vector3.zero;
        IsGrounded = CharacterController.CheckGround(_groundCheckDepth);
        if (IsGrounded && _fallVelocity < 0)
        {
            _fallVelocity = -2;
        }
        if (HasInputXZ)
        {
            float targetSpeed = Time.fixedDeltaTime;
            switch (MovingState)
            {
                case MovingStateEnum.Walk:
                    targetSpeed = walkSpeed;
                    break;
                case MovingStateEnum.Run:
                    targetSpeed = runSpeed;
                    break;
                case MovingStateEnum.Crouch:
                    targetSpeed = crouchSpeed;
                    break;
            }
            if (!IsGrounded)
                targetSpeed /= 2;
            Vector3 motion = transform.right * InputXZ.x + transform.forward * InputXZ.y;
			Vector3 walkMovement = motion * targetSpeed * Time.deltaTime;
			movement += walkMovement;
            CharacterController.Move(walkMovement);
        }
        _fallVelocity += Physics.gravity.y * Time.deltaTime;
        Vector3 fallMovement = Vector3.up * _fallVelocity * Time.deltaTime;
        movement += fallMovement;
        CharacterController.Move(fallMovement);
		Movement = movement;
    }
    void ListenMoveInput()
	{
        InputXZ = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        HasInputXZ = InputXZ != Vector2.zero;
        if (IsGrounded && Input.GetButtonDown("Jump"))
        {
            _fallVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            SpeakSE(_jumpSE);
            _moveAudioSource.Stop();
        }
        switch (MovingState)
        {
            case MovingStateEnum.Walk:
                if (Input.GetKey(KeyCode.LeftShift))
                    MovingState = MovingStateEnum.Run;
                else if (Input.GetKeyDown(KeyCode.C))
                    MovingState = MovingStateEnum.Crouch;
                break;
            case MovingStateEnum.Run:
                if (Input.GetKeyUp(KeyCode.LeftShift))
                    MovingState = MovingStateEnum.Walk;
                break;
            case MovingStateEnum.Crouch:
                if (Input.GetKey(KeyCode.LeftShift))
                    MovingState = MovingStateEnum.Run;
                else if (Input.GetKeyDown(KeyCode.C))
                    MovingState = MovingStateEnum.Walk;
                break;
        }
        if (MovingState == MovingStateEnum.Run)
        {
            if (Input.GetAxisRaw("Fire1") != 0 || Input.GetAxisRaw("Fire2") != 0/* && meeleAttack == false */)
            {
                MovingState = MovingStateEnum.Walk;
            }
        }
    }
	public void SpeakSE(AudioClip se)
    {
        _oralAudioSource.clip = se;
        _oralAudioSource.Play();
    }
	void Update()
    {
        ListenMoveInput();
		Crouching();
		WalkingSound();
	}

	/*
	* Checks if player is grounded and plays the sound accorindlgy to his speed
	*/
	void WalkingSound(){
		if (HasInputXZ && RayCastGrounded()) { //for walk sounsd using this because suraface is not straigh
            if (MovingState == MovingStateEnum.Walk)
            {
				_moveAudioSource.clip = _walkSE;
            }
            else if (MovingState == MovingStateEnum.Run)
            {
                _moveAudioSource.clip = _runSE;
            }
			else
			{
				_moveAudioSource.clip = null;
            }
			if (_moveAudioSource.clip != null && _moveAudioSource.isPlaying)
				_moveAudioSource.Play();
        } else {
			_moveAudioSource.Stop();
		}
	}
	/*
	* Raycasts down to check if we are grounded along the gorunded method() because if the
	* floor is curvy it will go ON/OFF constatly this assures us if we are really grounded
	*/
	private bool RayCastGrounded(){
		RaycastHit groundedInfo;
		if(Physics.Raycast(transform.position, transform.up *-1f, out groundedInfo, 1, ~ignoreLayer)){
			Debug.DrawRay (transform.position, transform.up * -1f, Color.red, 0.0f);
			if(groundedInfo.transform != null){
				//print ("vracam true");
				return true;
			}
			else{
				//print ("vracam false");
				return false;
			}
		}
		//print ("nisam if dosao");

		return false;
	}

	/*
	* If player toggle the crouch it will scale the player to appear that is crouching
	*/
	void Crouching() {
        if (MovingState == MovingStateEnum.Crouch){
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1,0.6f,1), Time.deltaTime * 15);
		}
		else{
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1,1,1), Time.deltaTime * 15);
		}
	}

	private string currentWeapon;
	[Tooltip("Put 'Player' layer here")]
	[Header("Shooting Properties")]
	private LayerMask ignoreLayer;//to ignore player layer
	/*
	* This method casts 9 rays in different directions. ( SEE scene tab and you will see 9 rays differently coloured).
	* Used to widley detect enemy infront and increase meele hit detectivity.
	* Checks for cooldown after last preformed meele attack.
	*/


	public bool been_to_meele_anim = false;
	private void RaycastForMeleeAttacks(){
		/*
		if (meleeAttack_cooldown > -5) {
			meleeAttack_cooldown -= 1 * Time.deltaTime;
		}


		if (GetComponent<GunInventory> ().currentGun) {
			if (GetComponent<GunInventory> ().currentGun.GetComponent<GunScript> ()) 
				currentWeapon = "gun";
		}

		//middle row
		ray1 = new Ray (bulletSpawn.position + (bulletSpawn.right*offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace));
		ray2 = new Ray (bulletSpawn.position - (bulletSpawn.right*offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace));
		ray3 = new Ray (bulletSpawn.position, bulletSpawn.forward);
		//upper row
		ray4 = new Ray (bulletSpawn.position + (bulletSpawn.right*offsetStart) + (bulletSpawn.up*offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace) + (bulletSpawn.up * rayDetectorMeeleSpace));
		ray5 = new Ray (bulletSpawn.position - (bulletSpawn.right*offsetStart) + (bulletSpawn.up*offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace) + (bulletSpawn.up * rayDetectorMeeleSpace));
		ray6 = new Ray (bulletSpawn.position + (bulletSpawn.up*offsetStart), bulletSpawn.forward + (bulletSpawn.up * rayDetectorMeeleSpace));
		//bottom row
		ray7 = new Ray (bulletSpawn.position + (bulletSpawn.right*offsetStart) - (bulletSpawn.up*offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace) - (bulletSpawn.up * rayDetectorMeeleSpace));
		ray8 = new Ray (bulletSpawn.position - (bulletSpawn.right*offsetStart) - (bulletSpawn.up*offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace) - (bulletSpawn.up * rayDetectorMeeleSpace));
		ray9 = new Ray (bulletSpawn.position -(bulletSpawn.up*offsetStart), bulletSpawn.forward - (bulletSpawn.up * rayDetectorMeeleSpace));

		Debug.DrawRay (ray1.origin, ray1.direction, Color.cyan);
		Debug.DrawRay (ray2.origin, ray2.direction, Color.cyan);
		Debug.DrawRay (ray3.origin, ray3.direction, Color.cyan);
		Debug.DrawRay (ray4.origin, ray4.direction, Color.red);
		Debug.DrawRay (ray5.origin, ray5.direction, Color.red);
		Debug.DrawRay (ray6.origin, ray6.direction, Color.red);
		Debug.DrawRay (ray7.origin, ray7.direction, Color.yellow);
		Debug.DrawRay (ray8.origin, ray8.direction, Color.yellow);
		Debug.DrawRay (ray9.origin, ray9.direction, Color.yellow);

		if (GetComponent<GunInventory> ().currentGun) {
			if (GetComponent<GunInventory> ().currentGun.GetComponent<GunScript> ().meeleAttack == false) {
				been_to_meele_anim = false;
			}
			if (GetComponent<GunInventory> ().currentGun.GetComponent<GunScript> ().meeleAttack == true && been_to_meele_anim == false) {
				been_to_meele_anim = true;
				//	if (isRunning == false) {
				StartCoroutine ("MeeleAttackWeaponHit");
				//	}
			}
		}
		*/
	}

    /*
	 *Method that is called if the waepon hit animation has been triggered the first time via Q input
	 *and if is, it will search for target and make damage
	IEnumerator MeeleAttackWeaponHit(){
		RaycastHit hitInfo;
		if (Physics.Raycast (ray1, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray2, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray3, out hitInfo, 2f, ~ignoreLayer)
			|| Physics.Raycast (ray4, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray5, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray6, out hitInfo, 2f, ~ignoreLayer)
			|| Physics.Raycast (ray7, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray8, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray9, out hitInfo, 2f, ~ignoreLayer)) {
			//Debug.DrawRay (bulletSpawn.position, bulletSpawn.forward + (bulletSpawn.right*0.2f), Color.green, 0.0f);
			if (hitInfo.transform.tag == "Dummie") {
				Transform _other = hitInfo.transform.root.transform;
				if (_other.transform.tag == "Dummie") {
					print ("hit a dummie");
				}
				InstantiateBlood(hitInfo,false);
			}
		}
		yield return new WaitForEndOfFrame ();
	}
	 */

    [Header("BloodForMelleAttaacks")]
	RaycastHit hit;//stores info of hit;
	[Tooltip("Put your particle blood effect here.")]
	public GameObject bloodEffect;//blod effect prefab;
	/*
	* Upon hitting enemy it calls this method, gives it raycast hit info 
	* and at that position it creates our blood prefab.
	*/
	void InstantiateBlood (RaycastHit _hitPos,bool swordHitWithGunOrNot) {		

		if (currentWeapon == "gun") {
			if (!swordHitWithGunOrNot) {
				if (bloodEffect)
					Instantiate (bloodEffect, _hitPos.point, Quaternion.identity);
				else
					print ("Missing blood effect prefab in the inspector.");
			}
		} 
	}
}

