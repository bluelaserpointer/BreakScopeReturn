using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Mobility")]
    public float speed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float groundCheckDepth = 0.05F;

    [Header("SE")]
    public AudioClip footStepSound;
    public float footStepDelay;

    public CharacterController CharacterController { get; private set; }
    public bool IsGrounded { get; private set; }
    Vector3 moveVelocity;
    float nextFootstep;

    private void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        IsGrounded = CharacterController.CheckGround(groundCheckDepth);
        if (IsGrounded && moveVelocity.y < 0)
        {
            moveVelocity.y = -2;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 motion = transform.right * x + transform.forward * z;
        CharacterController.Move(motion * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            moveVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        moveVelocity.y += gravity * Time.deltaTime;
        CharacterController.Move(moveVelocity * Time.deltaTime);

        if (x != 0 || z != 0 && IsGrounded)
        {
            nextFootstep -= Time.deltaTime;
            if (nextFootstep <= 0)
            {
                GetComponent<AudioSource>().PlayOneShot(footStepSound, 0.7f);
                nextFootstep += footStepDelay;
            }
        }
    }
}


