using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public AudioClip _jumpSE, _walkSE, _runSE;

    public Rigidbody Rigidbody {get; private set; }
    public CharacterController CharacterController { get; private set; }
    public bool IsGrounded { get; private set; }
	public MovingStateEnum MovingState { get; private set; }
    public Vector2 InputXZ { get; private set; }
    public bool HasInputXZ { get; private set; }
	public Vector3 Movement { get; private set; }
    public bool JumpInput { get; private set; }
    public bool IsMoving => Movement.sqrMagnitude > 0;

    private LayerMask ignorePlayerLayer;
    float _fallVelocity;

    private Player Player => GameManager.Instance.Player;

    /*
	 * Getting the Players rigidbody component.
	 * And grabbing the mainCamera from Players child transform.
	 */
    void Awake(){
		Rigidbody = GetComponent<Rigidbody>();
        CharacterController = GetComponent<CharacterController>();
		ignorePlayerLayer = 1 << LayerMask.NameToLayer ("Player");
		MovingState = MovingStateEnum.Walk;
    }
	void FixedUpdate() {
		MovementUpdate();
	}
	void MovementUpdate()
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
        JumpInput = Input.GetButtonDown("Jump");
        if (IsGrounded && JumpInput)
        {
            IsGrounded = false;
            _fallVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            AudioSource.PlayClipAtPoint(_jumpSE, transform.position);
            _moveAudioSource.Stop();
            Player.Animator.SetTrigger("jump");
        }
        Player.Animator.SetBool("isGrounded", IsGrounded);
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
            if (Input.GetAxisRaw("Fire1") != 0 || Input.GetAxisRaw("Fire2") != 0)
            {
                MovingState = MovingStateEnum.Walk;
            }
        }
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
		if(Physics.Raycast(transform.position, transform.up *-1f, out groundedInfo, 1, ~ignorePlayerLayer)){
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
}

