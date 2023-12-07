using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterControllerExtension
{
    public static bool CheckGround(this CharacterController characterController, float depth)
    {
        bool grounded = false;
        foreach (var groundCollider in Physics.OverlapSphere(characterController.transform.position + Vector3.up * (characterController.center.y + (-characterController.height + characterController.radius) /2 - depth), characterController.radius))
        {
            if (!groundCollider.isTrigger && groundCollider.gameObject.GetComponentInParent<CharacterController>() != characterController)
            {
                grounded = true;
                break;
            }
        }
        return grounded;
    }
}
