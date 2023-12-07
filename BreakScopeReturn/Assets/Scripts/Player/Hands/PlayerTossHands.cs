using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTossHands : PlayerHands
{
    private Player Player => GameManager.Instance.Player;
    private Camera MainCamera => Player.Camera;
    private Camera HUDCamera => Player.HUDCamera;
    private PlayerMovement PlayerMovement => Player.Movement;
    private MouseLook PlayerLook => Player.MouseLook;

    private Vector2 velocityGunRotate;
    private Vector2 angularVelocity;
    private Vector2 rotationLast;
    private Vector2 gunWeight;
    private void Update()
    {
        transform.position = MainCamera.transform.position;

        Vector2 rotationDelta = PlayerLook.currentRotation - rotationLast;

        rotationLast = PlayerLook.currentRotation;

        angularVelocity = Vector3.Lerp(angularVelocity, rotationDelta, Time.deltaTime * 5);

        gunWeight = Vector2.SmoothDamp(gunWeight, PlayerLook.currentRotation, ref velocityGunRotate, 0);

        transform.rotation = Quaternion.Euler(gunWeight + angularVelocity);
    }
}
