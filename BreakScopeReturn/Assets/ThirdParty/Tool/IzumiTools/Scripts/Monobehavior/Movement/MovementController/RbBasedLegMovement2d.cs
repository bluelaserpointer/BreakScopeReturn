using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class RbBasedLegMovement2d : MonoBehaviour
{
    [Header("Input")]
    public Vector2 moveDirection;
    public float moveSpeedRatio = 1;

    [Header("Mobility")]
    public float moveSpeed;

    public Rigidbody2D Rigidbody { get; private set; }
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        if(moveSpeedRatio > 0 && moveDirection != Vector2.zero)
        {
            Rigidbody.MovePosition(Rigidbody.position + moveDirection * moveSpeed * moveSpeedRatio * Time.fixedDeltaTime);
        }
    }
}
