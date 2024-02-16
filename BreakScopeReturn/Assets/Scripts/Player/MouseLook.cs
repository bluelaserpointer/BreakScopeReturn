using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class MouseLook : MonoBehaviour
{
    [Header("Internal Reference")]
	[SerializeField] public new Camera camera;
	public Camera Camera => camera;
	private Player Player => GameManager.Instance.Player;
    void Awake(){
		baseMouseSensitivity = PlayerPrefs.GetFloat(Setting.MOUSE_SENSITIVITY, 1F);
		LoadInit();
	}
	public void LoadInit()
    {
        currentRotation = baseRotation = Vector2.up * transform.eulerAngles.y;
		recoilRotation = Vector2.zero;
    }

	/*
	* Locking the mouse if pressing L.
	* Triggering the headbob camera omvement if player is faster than 1 of speed
	*/
	private void Update(){
        MouseSensitvity = baseMouseSensitivity;
        PlayerHands playerHands = Player.GunInventory.Hands;
        if (playerHands != null)
        {
            MouseSensitvity *= playerHands.MouseSensitivityModify;
        }
        MouseInputMovement();

		if(Player.Movement.HasInputXZ)
			HeadMovement ();
    }
    void FixedUpdate()
    {
        currentRotation = baseRotation + recoilRotation;
        Player.transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
        transform.localRotation = Quaternion.Euler(currentRotation.x, 0, zRotation);
    }

    [Header("Z Rotation Camera")]
	[HideInInspector] public float timer;
	[HideInInspector] public float zRotation;
	[HideInInspector] public float timeSpeed = 2;
	[HideInInspector] public float timerToRotateZ;
	/*
	* Switching Z rotation and applying to camera in camera Rotation().
	*/
	void HeadMovement(){
		timer += timeSpeed * Time.deltaTime;
		int int_timer = Mathf.RoundToInt (timer);
        float wantedZ;
		if (int_timer % 2 == 0) {
			wantedZ = -1;
		} else {
			wantedZ = 1;
		}
		zRotation = Mathf.Lerp (zRotation, wantedZ, Time.deltaTime * timerToRotateZ);
	}
	[Tooltip("Current mouse sensivity, changes in the weapon properties")]
    public float baseMouseSensitivity = 1;
    [HideInInspector]
    public float mouseSensitvity_aiming = 1;

    public float MouseSensitvity { get; private set; }

	[Tooltip("Speed that determines how much camera rotation will lag behind mouse movement.")]
	public float yRotationSpeed, xCameraSpeed;

	[HideInInspector]
	public Vector2 currentRotation;
    [HideInInspector]
    public Vector2 baseRotation;
    [HideInInspector]
    public Vector2 recoilRotation;

	[Tooltip("Top camera angle.")]
	public float topAngleView = 60;
	[Tooltip("Minimum camera angle.")]
	public float bottomAngleView = -45;
	/*
	 * Upon mouse movenet it increases/decreased wanted value. (not actually moving yet)
	 * Clamping the camera rotation X to top and bottom angles.
	 */
	void MouseInputMovement(){
		baseRotation.y += Input.GetAxis("Mouse X") * MouseSensitvity;
		baseRotation.x -= Input.GetAxis("Mouse Y") * MouseSensitvity;
		baseRotation.x = Mathf.Clamp(baseRotation.x, bottomAngleView, topAngleView);
	}
	public void OverrideRecoil()
	{
		baseRotation += recoilRotation;
		recoilRotation = Vector2.zero;
	}
}